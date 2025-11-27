import { Injectable, Logger, OnModuleInit, Inject } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { GOT } from '../http/got.module';
import * as got from 'got';
import { RabbitService } from '../rabbit/rabbit.service';

export enum SensorType {
  AirQuality = 'air_quality',
  Energy = 'energy',
  Motion = 'motion',
}
export interface AirQualityPayload {
  co2: number;
  pm25: number;
  humidity: number;
}
export interface EnergyPayload {
  energy: number;
}
export interface MotionPayload {
  motionDetected: boolean;
}
export type Reading =
  | { type: SensorType.AirQuality; name: string; payload: AirQualityPayload }
  | { type: SensorType.Energy; name: string; payload: EnergyPayload }
  | { type: SensorType.Motion; name: string; payload: MotionPayload };
export type ApiResponse = Reading[] | { error: string };

function sleep(ms: number) {
  return new Promise((res) => setTimeout(res, ms));
}

@Injectable()
export class MetersService implements OnModuleInit {
  private readonly log = new Logger(MetersService.name);

  private pollIntervalMs!: number;
  private retryMaxDelayMs!: number;
  private isRequestInProgress = false;
  private readonly extraRetryDelayMs = 3000; // Extra delay to add to Retry-After

  constructor(
    private readonly cfg: ConfigService,
    @Inject(GOT) private readonly http: got.Got,
    private readonly rabbit: RabbitService,
  ) {}

  onModuleInit() {
    this.pollIntervalMs = this.cfg.get<number>('METERS_POLL_INTERVAL_MS', {
      infer: true,
    });
    this.retryMaxDelayMs = this.cfg.get<number>('RETRY_MAX_DELAY_MS', {
      infer: true,
    });

    this.log.log(`Polling /meters via got every ${this.pollIntervalMs} ms`);
    this.startLoop();
  }

  private async startLoop() {
    let attempt = 0;
    for (;;) {
      try {
        const delay = await this.pollOnce();
        attempt = 0;
        await sleep(delay);
      } catch (e) {
        attempt++;
        const delay = this.computeRetryDelay(e, attempt);
        this.log.warn(
          `pollOnce failed (#${attempt}): ${this.errMsg(e)}. Retry in ${delay} ms`,
        );
        await sleep(delay);
      }
    }
  }

  private async pollOnce(): Promise<number> {
    if (this.isRequestInProgress) {
      this.log.debug('Request already in progress, skipping this poll cycle');
      return this.pollIntervalMs;
    }

    this.isRequestInProgress = true;
    try {
      const data = await this.http.get('meters').json<ApiResponse>();

    if (this.isErrorBody(data)) {
      const err = new Error(`API error body: ${data.error}`);
      (err as any).__kind = 'api_error_body';
      throw err;
    }

    if (!Array.isArray(data)) {
      const err = new Error('Unexpected payload (not an array)');
      (err as any).__kind = 'unexpected_payload';
      throw err;
    }

      if (data.length > 0) {
        this.log.log(`Fetched ${data.length} readings from API`);
        await this.publishReadings(data);
      }

      return this.pollIntervalMs;
    } finally {
      this.isRequestInProgress = false;
    }
  }

  private async publishReadings(readings: Reading[]) {
    await this.rabbit.publish(readings);
    this.log.log(`Published ${readings.length} readings to RabbitMQ`);
  }

  private computeRetryDelay(err: unknown, attempt: number): number {
    // Check if this is a Got HTTPError with Retry-After header
    if (err && typeof err === 'object' && 'response' in err) {
      const httpError = err as any;
      const retryAfterHeader = httpError.response?.headers?.[
        'retry-after'
      ] as string | undefined;

      if (retryAfterHeader) {
        let delaySeconds = 0;

        // Parse Retry-After: can be either seconds (number) or HTTP date
        if (/^\d+$/.test(retryAfterHeader)) {
          // Numeric format: delay in seconds
          delaySeconds = parseInt(retryAfterHeader, 10);
        } else {
          // HTTP date format
          const retryDate = new Date(retryAfterHeader);
          if (!isNaN(retryDate.getTime())) {
            delaySeconds = Math.max(
              0,
              (retryDate.getTime() - Date.now()) / 1000,
            );
          }
        }

        if (delaySeconds > 0) {
          const totalDelayMs = delaySeconds * 1000 + this.extraRetryDelayMs;
          const cappedDelay = Math.min(totalDelayMs, this.retryMaxDelayMs);
          this.log.debug(
            `[Retry-After] Retry-After=${delaySeconds}s + ${this.extraRetryDelayMs}ms extra -> using ${cappedDelay}ms delay`,
          );
          return cappedDelay;
        }
      }
    }

    // No Retry-After: retry immediately without delay
    this.log.debug(
      `[No Retry-After] Retrying immediately (attempt #${attempt})`,
    );
    return 0;
  }

  private isErrorBody(x: unknown): x is { error: string } {
    return !!x && !Array.isArray(x) && typeof (x as any).error === 'string';
  }

  private errMsg(e: unknown): string {
    return e instanceof Error ? e.message : String(e);
  }
}

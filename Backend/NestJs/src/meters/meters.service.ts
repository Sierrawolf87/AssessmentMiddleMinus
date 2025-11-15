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
        await this.publishReadings(data);
      }

      return this.pollIntervalMs;
    } finally {
      this.isRequestInProgress = false;
    }
  }

  private async publishReadings(readings: Reading[]) {
    for (const r of readings) {
      const room = r.name.replace(/\s+/g, '_');
      const rk = `meters.${r.type}.${room}`;
      await this.rabbit.publish(r, rk);
    }
  }

  private computeRetryDelay(err: unknown, attempt: number): number {
    if (err && typeof err === 'object' && '__retryAfterDelay' in err) {
      const customDelay = (err as any).__retryAfterDelay;
      if (typeof customDelay === 'number' && customDelay > 0) {
        this.log.debug(`Using Retry-After delay: ${customDelay} ms`);
        return Math.min(customDelay, this.retryMaxDelayMs);
      }
    }

    const base = this.pollIntervalMs;
    const cap = this.retryMaxDelayMs;
    const exp = Math.min(cap, base * 2 ** (attempt - 1));
    const jitter = Math.floor(Math.random() * exp);
    return Math.max(250, jitter);
  }

  private isErrorBody(x: unknown): x is { error: string } {
    return !!x && !Array.isArray(x) && typeof (x as any).error === 'string';
  }

  private errMsg(e: unknown): string {
    return e instanceof Error ? e.message : String(e);
  }
}

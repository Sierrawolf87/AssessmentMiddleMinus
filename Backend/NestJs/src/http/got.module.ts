import { Module, Global } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { head } from 'axios';
import got, { Got } from 'got';

export const GOT = Symbol('GOT');

@Global()
@Module({
  providers: [
    {
      provide: GOT,
      inject: [ConfigService],
      useFactory: (cfg: ConfigService) => {
        const host = cfg.get<string>('WEAKAPP_HOST', { infer: true });
        const port = cfg.get<number>('WEAKAPP_PORT', { infer: true });
        const timeout = cfg.get<number>('METERS_TIMEOUT_MS', { infer: true });
        const retryCap = cfg.get<number>('RETRY_MAX_DELAY_MS', { infer: true });

        const client: Got = got.extend({
          prefixUrl: `http://${host}:${port}`,
          headers: {
            'X-Api-Key': 'supersecret',
          },
          throwHttpErrors: true,
          timeout: { request: timeout },
          retry: {
            limit: 5,
            methods: ['GET'],
            statusCodes: [429, 500, 502, 503, 504],
            errorCodes: [
              'ETIMEDOUT',
              'ECONNRESET',
              'ENOTFOUND',
              'EAI_AGAIN',
              'ECONNREFUSED',
              'EPIPE',
            ],
            maxRetryAfter: retryCap,
          },
          hooks: {
            beforeRetry: [
              (error, retryCount) => {
                const retryAfter = error.response?.headers['retry-after'];
                if (retryAfter) {
                  let delaySeconds = 0;
                  if (/^\d+$/.test(retryAfter)) {
                    delaySeconds = parseInt(retryAfter, 10);
                  } else {
                    const retryDate = new Date(retryAfter);
                    if (!isNaN(retryDate.getTime())) {
                      delaySeconds = Math.max(
                        0,
                        (retryDate.getTime() - Date.now()) / 1000,
                      );
                    }
                  }

                  const extraDelayMs = 2000;
                  const totalDelayMs = delaySeconds * 1000 + extraDelayMs;

                  (error as any).__retryAfterDelay = totalDelayMs;
                }
              },
            ],
          },
        });

        return client;
      },
    },
  ],
  exports: [GOT],
})
export class GotModule {}

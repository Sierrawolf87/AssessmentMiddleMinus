import { Module, Global } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
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
            limit: 0, // Disable Got's retry - MetersService handles all retries
          },
        });

        return client;
      },
    },
  ],
  exports: [GOT],
})
export class GotModule {}

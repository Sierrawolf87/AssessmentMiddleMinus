import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';
import { ConfigService } from '@nestjs/config';
import pino from 'pino';
import pinoHttp from 'pino-http';
import { PinoLoggerService } from './common/pino-logger.service';

async function bootstrap() {
  const app = await NestFactory.create(AppModule, {
    bufferLogs: true,
  });

  const configService = app.get(ConfigService);
  const lokiUrl = configService.get<string>('LOKI_URL', 'http://loki:3100');

  // Configure Pino with Loki transport
  const lokiTransport = pino.transport({
    target: 'pino-loki',
    options: {
      host: lokiUrl,
      interval: 5,
      labels: { service: 'NestJS' },
    },
  });

  const pinoLogger = pino(
    {
      level: process.env.LOG_LEVEL || 'info',
    },
    pino.multistream([
      { stream: process.stdout },
      { stream: lokiTransport },
    ])
  );

  // Create Pino logger adapter for NestJS
  const logger = new PinoLoggerService(pinoLogger);

  // Use Pino logger
  app.useLogger(logger);
  app.use(pinoHttp({ logger: pinoLogger }));

  await app.listen(process.env.PORT ?? 3000);
}
bootstrap();

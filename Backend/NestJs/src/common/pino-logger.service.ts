import { LoggerService } from '@nestjs/common';
import pino from 'pino';

export class PinoLoggerService implements LoggerService {
  constructor(private readonly logger: pino.Logger) {}

  log(message: any, ...optionalParams: any[]): void {
    this.logger.info(message, ...optionalParams);
  }

  error(message: any, ...optionalParams: any[]): void {
    this.logger.error(message, ...optionalParams);
  }

  warn(message: any, ...optionalParams: any[]): void {
    this.logger.warn(message, ...optionalParams);
  }

  debug(message: any, ...optionalParams: any[]): void {
    this.logger.debug(message, ...optionalParams);
  }

  verbose(message: any, ...optionalParams: any[]): void {
    this.logger.trace(message, ...optionalParams);
  }
}


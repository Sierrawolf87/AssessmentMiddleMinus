import {
  Injectable,
  OnModuleInit,
  OnModuleDestroy,
  Logger,
} from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import * as amqplib from 'amqplib';

@Injectable()
export class RabbitService implements OnModuleInit, OnModuleDestroy {
  private readonly log = new Logger(RabbitService.name);

  private conn!: amqplib.Connection;
  private ch!: amqplib.ConfirmChannel;

  private queue!: string;

  constructor(private readonly cfg: ConfigService) {}

  async onModuleInit() {
    this.queue = this.cfg.getOrThrow<string>('RABBITMQ_DATA_QUEUE', {
      infer: true,
    });

    const url = this.buildAmqpUrl();
    try {
      this.conn = await amqplib.connect(url + "?frameMax=131072");
    } catch (e) {
      this.log.error(`Failed to connect to RabbitMQ: ${e}`);
      throw e;
    }

    this.ch = await this.conn.createConfirmChannel();
    await this.ch.assertQueue(this.queue, {
      durable: true,
    });

    this.log.log(
      `Connected to RabbitMQ. Queue "${this.queue}" is ready`,
    );
  }

  async onModuleDestroy() {
    this.log.log('Closing RabbitMQ connection');
    await this.ch?.close().catch((e) => this.log.error(`Error closing channel: ${e}`));
    await this.conn?.close().catch((e) => this.log.error(`Error closing connection: ${e}`));
  }

  async publish(reading: unknown) {
    const payload = Buffer.from(JSON.stringify(reading));

    const ok = this.ch.sendToQueue(this.queue, payload, {
      contentType: 'application/json',
      persistent: true,
    });

    if (!ok) {
      this.log.warn('Channel backpressure: publish returned false');
    }

    await this.ch.waitForConfirms();
  }

  private buildAmqpUrl(): string {
    const user = this.cfg.getOrThrow<string>('RABBITMQ_USER', { infer: true });
    const pass = this.cfg.getOrThrow<string>('RABBITMQ_PASS', { infer: true });
    const host = this.cfg.getOrThrow<string>('RABBITMQ_HOST', { infer: true });
    const port = this.cfg.getOrThrow<number>('RABBITMQ_PORT', { infer: true });
    const vhost =
      this.cfg.get<string>('RABBITMQ_VHOST', { infer: true }) ?? '/';

    const vhostPath = `/${encodeURIComponent(vhost)}`;
    return `amqp://${encodeURIComponent(user)}:${encodeURIComponent(pass)}@${host}:${port}${vhostPath}`;
  }
}

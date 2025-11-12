export interface EnvVars {
  NODE_ENV: 'development' | 'test' | 'production';

  WEAKAPP_HOST: string;
  WEAKAPP_PORT: number;

  RABBITMQ_HOST: string;
  RABBITMQ_PORT: number;
  RABBITMQ_USER: string;
  RABBITMQ_PASS: string;
  RABBITMQ_VHOST: string;
  RABBITMQ_EXCHANGE: string;
  RABBITMQ_EXCHANGE_TYPE: 'topic' | 'direct' | 'fanout' | 'headers';

  METERS_POLL_INTERVAL_MS: number;
  METERS_TIMEOUT_MS: number;
  RETRY_MAX_DELAY_MS: number;
}

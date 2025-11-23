import Joi from 'joi';
import type { EnvVars } from './config.types';

export const envValidationSchema = Joi.object<EnvVars>({
  NODE_ENV: Joi.string()
    .valid('development', 'test', 'production')
    .default('development'),

  WEAKAPP_HOST: Joi.string().required(),
  WEAKAPP_PORT: Joi.number().port().required(),

  RABBITMQ_HOST: Joi.string().required(),
  RABBITMQ_PORT: Joi.number().port().required(),
  RABBITMQ_USER: Joi.string().required(),
  RABBITMQ_PASS: Joi.string().required(),
  RABBITMQ_VHOST: Joi.string().default('/'),
  RABBITMQ_DATA_QUEUE: Joi.string().required(),

  METERS_POLL_INTERVAL_MS: Joi.number().positive().default(5000),
  METERS_TIMEOUT_MS: Joi.number().positive().default(3000),
  RETRY_MAX_DELAY_MS: Joi.number().positive().default(30000),
});

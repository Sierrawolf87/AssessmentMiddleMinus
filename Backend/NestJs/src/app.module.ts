import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { AppConfigModule } from './config/config.module';
import { MetersModule } from './meters/meters.module';
import { RabbitModule } from './rabbit/rabbit.module';
import { GotModule } from './http/got.module';

@Module({
  imports: [AppConfigModule, MetersModule, RabbitModule, GotModule],
  controllers: [AppController],
  providers: [AppService],
})
export class AppModule {}

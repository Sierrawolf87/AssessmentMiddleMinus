import { Module } from '@nestjs/common';
import { HttpModule } from '@nestjs/axios';
import { MetersService } from './meters.service';
import { RabbitModule } from '../rabbit/rabbit.module';
import { GotModule } from 'src/http/got.module';

@Module({
  imports: [HttpModule, GotModule, RabbitModule],
  providers: [MetersService],
})
export class MetersModule {}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AppConfig } from '../models/app-config.model';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  private config: AppConfig | null = null;

  constructor(private http: HttpClient) {}

  async loadConfig(): Promise<AppConfig> {
    if (this.config) {
      return this.config;
    }

    try {
      this.config = await firstValueFrom(
        this.http.get<AppConfig>('/assets/config.json')
      );
      return this.config;
    } catch (error) {
      console.error('Failed to load configuration:', error);
      // Fallback to default configuration
      this.config = {
        baseUrl: 'http://localhost:8080',
        signalRPath: '/notifications/notificationHub',
        graphQLPath: '/graphql'
      };
      return this.config;
    }
  }

  getConfig(): AppConfig {
    if (!this.config) {
      throw new Error('Configuration not loaded. Call loadConfig() first.');
    }
    return this.config;
  }

  get baseUrl(): string {
    return this.getConfig().baseUrl;
  }

  get signalRUrl(): string {
    return this.getConfig().baseUrl + this.getConfig().signalRPath;
  }

  get graphQLUrl(): string {
    return this.getConfig().baseUrl + this.getConfig().graphQLPath;
  }
}

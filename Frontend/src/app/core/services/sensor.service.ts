import { Injectable } from '@angular/core';
import { Observable, Subject, interval, merge } from 'rxjs';
import { map } from 'rxjs/operators';
import { GraphqlService, PaginatedResult } from './graphql.service';
import { SignalrService } from './signalr.service';
import { SensorReading, SensorReadingStats, SensorMessageDto } from '../models/sensor-reading.model';
import { SensorType, SensorLocation } from '../models/enums';

@Injectable({
  providedIn: 'root'
})
export class SensorService {
  private realTimeUpdateSubject$ = new Subject<SensorMessageDto>();
  private pollingInterval$ = new Subject<SensorMessageDto>();

  constructor(
    private graphqlService: GraphqlService,
    private signalrService: SignalrService
  ) {
    // Subscribe to SignalR and forward messages to our subject
    this.signalrService.getSensorDataUpdates().subscribe(data => {
      if (data) {
        this.realTimeUpdateSubject$.next(data);
      }
    });
  }

  getReadings(take: number = 20, after?: string | null, where?: any, order?: any): Observable<PaginatedResult> {
    return this.graphqlService.getSensorReadings(take, after, where, order);
  }

  getStats(type?: SensorType, name?: SensorLocation, startDate?: Date, endDate?: Date): Observable<SensorReadingStats> {
    return this.graphqlService.getSensorStats(type, name, startDate, endDate);
  }

  /**
   * Subscribe to real-time sensor data updates
   * Combines SignalR updates with interval-based polling for reliable updates
   */
  getRealTimeUpdates(): Observable<SensorMessageDto> {
    // Emit update signal every 5 seconds for polling
    const polling$ = interval(5000).pipe(
      map(() => ({} as SensorMessageDto))
    );
    
    // Merge SignalR updates with polling updates
    return merge(this.realTimeUpdateSubject$.asObservable(), polling$);
  }
}

import { Injectable } from '@angular/core';
import { Apollo, gql } from 'apollo-angular';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { SensorReading, SensorReadingStats } from '../models/sensor-reading.model';
import { SensorLocation, SensorType } from '../models/enums';
import { ConfigService } from './config.service';

export interface PageInfo {
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  startCursor?: string;
  endCursor?: string;
}

export interface PaginatedResult {
  items: SensorReading[];
  totalCount: number;
  pageInfo: PageInfo;
}

const GET_SENSOR_READINGS = gql`
  query GetSensorReadings($first: Int, $after: String, $where: SensorReadingFilterInput, $order: [SensorReadingSortInput!]) {
    sensorReadings(first: $first, after: $after, where: $where, order: $order) {
      nodes {
        id
        type
        name
        co2
        pm25
        humidity
        motionDetected
        energy
        timestamp
      }
      pageInfo {
        hasNextPage
        hasPreviousPage
        startCursor
        endCursor
      }
      totalCount
    }
  }
`;

const GET_SENSOR_STATS = gql`
  query GetSensorStats($type: SensorType, $name: SensorLocation, $startDate: DateTime, $endDate: DateTime) {
    sensorReadingStats(type: $type, name: $name, startDate: $startDate, endDate: $endDate) {
      totalCount
      averageCo2
      averagePm25
      averageHumidity
      averageEnergy
      maxCo2
      minCo2
      maxPm25
      minPm25
      motionDetectedCount
    }
  }
`;

@Injectable({
  providedIn: 'root'
})
export class GraphqlService {

  constructor(
    private apollo: Apollo,
    private http: HttpClient,
    private configService: ConfigService
  ) { }

  /**
   * Get sensor readings using cursor-based pagination
   * @param take - Number of items to fetch
   * @param after - Cursor to fetch items after (for next page)
   * @param where - Filter conditions
   * @param order - Sort order
   */
  getSensorReadings(take: number = 20, after?: string | null, where?: any, order?: any): Observable<PaginatedResult> {
    return this.apollo.query<any>({
      query: GET_SENSOR_READINGS,
      variables: {
        first: take,
        after: after || null,
        where,
        order
      },
      fetchPolicy: 'network-only'
    }).pipe(
      map(result => ({
        items: result.data?.sensorReadings?.nodes ?? [],
        totalCount: result.data?.sensorReadings?.totalCount ?? 0,
        pageInfo: {
          hasNextPage: result.data?.sensorReadings?.pageInfo?.hasNextPage ?? false,
          hasPreviousPage: result.data?.sensorReadings?.pageInfo?.hasPreviousPage ?? false,
          startCursor: result.data?.sensorReadings?.pageInfo?.startCursor,
          endCursor: result.data?.sensorReadings?.pageInfo?.endCursor
        }
      }))
    );
  }

  getSensorStats(type?: SensorType, name?: SensorLocation, startDate?: Date, endDate?: Date): Observable<SensorReadingStats> {
    return this.apollo.query<any>({
      query: GET_SENSOR_STATS,
      variables: {
        type,
        name,
        startDate,
        endDate
      },
      fetchPolicy: 'network-only'
    }).pipe(
      map(result => result.data.sensorReadingStats)
    );
  }

  /**
   * Get all sensor types from REST API
   * @returns Observable array of sensor type strings in uppercase with underscores
   */
  getSensorTypes(): Observable<string[]> {
    const url = `${this.configService.restUrl}/api/sensortypes`;
    return this.http.get<string[]>(url);
  }
}

import { Injectable } from '@angular/core';
import { Apollo, gql } from 'apollo-angular';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { SensorReading, SensorReadingStats } from '../models/sensor-reading.model';
import { SensorLocation, SensorType } from '../models/enums';

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

  constructor(private apollo: Apollo) { }

  getSensorReadings(skip: number = 0, take: number = 20, where?: any, order?: any): Observable<{ items: SensorReading[], totalCount: number, endCursor?: string }> {
    // Backend uses cursor-based pagination, not offset-based
    // We'll fetch larger pages to reduce pagination issues
    // In a production app, we'd implement proper cursor management
    const actualTake = skip === 0 ? take : Math.min(100, take * 5); // Fetch more on subsequent pages
    
    return this.apollo.query<any>({
      query: GET_SENSOR_READINGS,
      variables: {
        first: actualTake,
        after: null,
        where,
        order
      },
      fetchPolicy: 'network-only'
    }).pipe(
      map(result => {
        const allItems = result.data?.sensorReadings?.nodes ?? [];
        const totalCount = result.data?.sensorReadings?.totalCount ?? 0;
        
        // Client-side pagination emulation
        const startIndex = skip;
        const endIndex = skip + take;
        const items = allItems.slice(startIndex, endIndex);
        
        return {
          items,
          totalCount,
          endCursor: result.data?.sensorReadings?.pageInfo?.endCursor
        };
      })
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
}

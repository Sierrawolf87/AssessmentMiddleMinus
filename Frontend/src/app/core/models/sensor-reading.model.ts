import { SensorLocation, SensorType } from './enums';

export interface SensorReading {
  id: string;
  type: SensorType;
  name: SensorLocation;
  co2?: number;
  pm25?: number;
  humidity?: number;
  motionDetected?: boolean;
  energy?: number;
  timestamp: string;
}

export interface SensorReadingStats {
  totalCount: number;
  averageCo2?: number;
  averagePm25?: number;
  averageHumidity?: number;
  averageEnergy?: number;
  maxCo2?: number;
  minCo2?: number;
  maxPm25?: number;
  minPm25?: number;
  motionDetectedCount: number;
}

export interface AggregatedSensorReading {
  timestamp: string;
  averageCo2?: number;
  averagePm25?: number;
  averageHumidity?: number;
  averageEnergy?: number;
  count: number;
}

export interface SensorMessageDto {
  type: SensorType;
  name: SensorLocation;
  payload?: SensorPayloadDto;
}

export interface SensorPayloadDto {
  co2?: number;
  pm25?: number;
  humidity?: number;
  motionDetected?: boolean;
  energy?: number;
}

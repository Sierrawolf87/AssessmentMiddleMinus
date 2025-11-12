export enum SensorType {
  AirQuality = 'air_quality',
  Energy = 'energy',
  Motion = 'motion',
}

export interface AirQualityPayload {
  co2: number;
  pm25: number;
  humidity: number;
}
export interface EnergyPayload {
  energy: number;
}
export interface MotionPayload {
  motionDetected: boolean;
}

export type Reading =
  | { type: SensorType.AirQuality; name: string; payload: AirQualityPayload }
  | { type: SensorType.Energy; name: string; payload: EnergyPayload }
  | { type: SensorType.Motion; name: string; payload: MotionPayload };

export type ApiResponse = Reading[] | { error: string };

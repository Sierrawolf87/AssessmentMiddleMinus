import { SensorType, SensorLocation } from '../models/enums';

/**
 * Converts SensorType enum value to user-friendly display label
 * @param type - The SensorType enum value
 * @returns A formatted, human-readable label
 */
export function getSensorTypeLabel(type: SensorType): string {
  const labels: Record<SensorType, string> = {
    [SensorType.AirQuality]: 'Air Quality',
    [SensorType.Energy]: 'Energy',
    [SensorType.Motion]: 'Motion'
  };
  return labels[type] || type;
}

/**
 * Converts SensorLocation enum value to user-friendly display label
 * @param location - The SensorLocation enum value
 * @returns A formatted, human-readable label
 */
export function getSensorLocationLabel(location: SensorLocation): string {
  const labels: Record<SensorLocation, string> = {
    [SensorLocation.Kitchen]: 'Kitchen',
    [SensorLocation.Garage]: 'Garage',
    [SensorLocation.Bedroom]: 'Bedroom',
    [SensorLocation.LivingRoom]: 'Living Room',
    [SensorLocation.Office]: 'Office',
    [SensorLocation.Corridor]: 'Corridor'
  };
  return labels[location] || location;
}

/**
 * Gets all sensor types as an array of objects with value and label
 */
export function getSensorTypeOptions(): Array<{ value: SensorType; label: string }> {
  return Object.values(SensorType).map(type => ({
    value: type,
    label: getSensorTypeLabel(type)
  }));
}

/**
 * Gets all sensor locations as an array of objects with value and label
 */
export function getSensorLocationOptions(): Array<{ value: SensorLocation; label: string }> {
  return Object.values(SensorLocation).map(location => ({
    value: location,
    label: getSensorLocationLabel(location)
  }));
}

/**
 * Time range configuration for analytics dashboard
 */

export enum TimeRange {
  THIRTY_MIN = '30min',
  THREE_HOURS = '3h',
  ONE_DAY = '1d',
  SEVEN_DAYS = '7d',
  ONE_MONTH = '1m'
}

export interface TimeRangeConfig {
  label: string;
  value: TimeRange;
  intervalMinutes: number;
  durationMinutes: number;
}

/**
 * Get all available time range configurations
 */
export function getTimeRangeConfigs(): TimeRangeConfig[] {
  return [
    {
      label: '30 minutes',
      value: TimeRange.THIRTY_MIN,
      intervalMinutes: 1,
      durationMinutes: 30
    },
    {
      label: '3 hours',
      value: TimeRange.THREE_HOURS,
      intervalMinutes: 5,
      durationMinutes: 180
    },
    {
      label: '1 day',
      value: TimeRange.ONE_DAY,
      intervalMinutes: 60,
      durationMinutes: 1440
    },
    {
      label: '7 days',
      value: TimeRange.SEVEN_DAYS,
      intervalMinutes: 360,
      durationMinutes: 10080
    },
    {
      label: '1 month',
      value: TimeRange.ONE_MONTH,
      intervalMinutes: 1440,
      durationMinutes: 43200
    }
  ];
}

/**
 * Get aggregation interval in minutes for a given time range
 */
export function getIntervalForTimeRange(timeRange: TimeRange): number {
  const config = getTimeRangeConfigs().find(c => c.value === timeRange);
  return config?.intervalMinutes ?? 1;
}

/**
 * Calculate start date based on time range from current time
 */
export function calculateStartDate(timeRange: TimeRange, fromDate: Date = new Date()): Date {
  const config = getTimeRangeConfigs().find(c => c.value === timeRange);
  if (!config) return new Date();
  
  const startDate = new Date(fromDate);
  startDate.setMinutes(startDate.getMinutes() - config.durationMinutes);
  return startDate;
}

/**
 * Get duration in minutes for a time range
 */
export function getDurationMinutes(timeRange: TimeRange): number {
  const config = getTimeRangeConfigs().find(c => c.value === timeRange);
  return config?.durationMinutes ?? 30;
}

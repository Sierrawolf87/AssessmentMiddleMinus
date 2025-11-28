import { Component, Output, EventEmitter, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SensorType, SensorLocation } from '../../../core/models/enums';
import { getSensorLocationOptions } from '../../../core/utils/enum.utils';
import { GraphqlService } from '../../../core/services/graphql.service';
import { TimeRange, getTimeRangeConfigs, TimeRangeConfig } from '../../../core/utils/time-range.utils';

export interface FilterOptions {
  type?: SensorType;
  location?: SensorLocation;
  startDate?: string;
  endDate?: string;
  timeRange?: TimeRange;
  resetSort?: boolean;
  // Sensor value filters
  co2Min?: number;
  co2Max?: number;
  pm25Min?: number;
  pm25Max?: number;
  humidityMin?: number;
  humidityMax?: number;
  motion?: boolean;
  energyMin?: number;
  energyMax?: number;
}

@Component({
  selector: 'app-filter-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './filter-panel.component.html',
  styleUrls: ['./filter-panel.component.css']
})
export class FilterPanelComponent implements OnInit {
  @Input() showSensorType: boolean = true;
  @Input() showLocation: boolean = true;
  @Input() showEndDate: boolean = true;
  @Input() showSensorFilters: boolean = false;
  @Input() showTimeRange: boolean = false;
  
  @Output() filterChange = new EventEmitter<FilterOptions>();
  @Output() refresh = new EventEmitter<void>();
  @Output() realTimeToggle = new EventEmitter<boolean>();

  filters: FilterOptions = {};
  realTimeEnabled: boolean = false;

  sensorTypeOptions: { label: string; value: SensorType }[] = [];
  sensorLocationOptions = getSensorLocationOptions();
  timeRangeOptions: TimeRangeConfig[] = getTimeRangeConfigs();

  constructor(private graphqlService: GraphqlService) {}

  ngOnInit() {
    // Fetch sensor types from REST API
    this.graphqlService.getSensorTypes().subscribe({
      next: (types) => {
        this.sensorTypeOptions = types.map(type => ({
          label: this.formatSensorTypeLabel(type),
          value: type as SensorType
        }));
      },
      error: (error) => {
        console.error('Failed to fetch sensor types:', error);
        // Fallback to empty array or default values
        this.sensorTypeOptions = [];
      }
    });
  }

  /**
   * Format sensor type from API format (AIR_QUALITY) to display format (Air Quality)
   */
  private formatSensorTypeLabel(type: string): string {
    return type
      .split('_')
      .map(word => word.charAt(0) + word.slice(1).toLowerCase())
      .join(' ');
  }

  onFilterChange() {
    this.filterChange.emit(this.filters);
  }

  clearFilters() {
    this.filters = { resetSort: true };
    this.filterChange.emit(this.filters);
    // Clear the resetSort flag after emitting
    setTimeout(() => {
      this.filters = {};
    }, 0);
  }

  onRefresh() {
    this.refresh.emit();
  }

  onRealTimeToggle() {
    this.realTimeToggle.emit(this.realTimeEnabled);
  }
}

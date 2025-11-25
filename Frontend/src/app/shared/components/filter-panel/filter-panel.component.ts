import { Component, Output, EventEmitter, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SensorType, SensorLocation } from '../../../core/models/enums';
import { getSensorTypeOptions, getSensorLocationOptions } from '../../../core/utils/enum.utils';

export interface FilterOptions {
  type?: SensorType;
  location?: SensorLocation;
  startDate?: string;
  endDate?: string;
  resetSort?: boolean;
}

@Component({
  selector: 'app-filter-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './filter-panel.component.html',
  styleUrls: ['./filter-panel.component.css']
})
export class FilterPanelComponent {
  @Input() showSensorType: boolean = true;
  @Input() showLocation: boolean = true;
  @Input() showEndDate: boolean = true;
  
  @Output() filterChange = new EventEmitter<FilterOptions>();
  @Output() refresh = new EventEmitter<void>();
  @Output() realTimeToggle = new EventEmitter<boolean>();

  filters: FilterOptions = {};
  realTimeEnabled: boolean = false;

  sensorTypeOptions = getSensorTypeOptions();
  sensorLocationOptions = getSensorLocationOptions();

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

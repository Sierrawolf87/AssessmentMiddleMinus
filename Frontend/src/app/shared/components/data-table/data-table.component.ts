import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SensorReading } from '../../../core/models/sensor-reading.model';
import { SensorType, SensorLocation } from '../../../core/models/enums';
import { getSensorTypeLabel, getSensorLocationLabel } from '../../../core/utils/enum.utils';

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './data-table.component.html',
  styleUrls: ['./data-table.component.css']
})
export class DataTableComponent {
  @Input() data: SensorReading[] = [];
  @Input() totalCount: number = 0;
  @Input() currentPage: number = 1;
  @Input() pageSize: number = 20;
  
  @Output() pageChange = new EventEmitter<number>();
  @Output() sortChange = new EventEmitter<{field: string, direction: 'asc' | 'desc'}>();

  sortField: string = 'timestamp';
  sortDirection: 'asc' | 'desc' = 'desc';

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  onSort(field: string) {
    if (this.sortField === field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortDirection = 'asc';
    }
    this.sortChange.emit({ field: this.sortField, direction: this.sortDirection });
  }

  onPageChange(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.pageChange.emit(page);
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString();
  }

  formatType(type: SensorType): string {
    return getSensorTypeLabel(type);
  }

  formatLocation(location: SensorLocation): string {
    return getSensorLocationLabel(location);
  }
}

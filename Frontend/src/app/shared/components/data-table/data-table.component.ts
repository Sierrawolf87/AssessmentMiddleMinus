import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SensorReading } from '../../../core/models/sensor-reading.model';
import { SensorType, SensorLocation } from '../../../core/models/enums';
import { getSensorTypeLabel, getSensorLocationLabel } from '../../../core/utils/enum.utils';
import { PageInfo } from '../../../core/services/graphql.service';

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
  @Input() pageInfo?: PageInfo;
  @Input() showLocationColumn: boolean = true;
  
  @Output() pageChange = new EventEmitter<'next' | 'prev'>();
  @Output() sortChange = new EventEmitter<{field: string, direction: 'asc' | 'desc'}>();

  sortField: string = 'timestamp';
  sortDirection: 'asc' | 'desc' = 'desc';

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  get canGoNext(): boolean {
    return this.pageInfo?.hasNextPage ?? false;
  }

  get canGoPrev(): boolean {
    return this.currentPage > 1;
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

  onNext() {
    if (this.canGoNext) {
      this.pageChange.emit('next');
    }
  }

  onPrev() {
    if (this.canGoPrev) {
      this.pageChange.emit('prev');
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString('en-GB', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false
    });
  }

  formatType(type: SensorType): string {
    return getSensorTypeLabel(type);
  }

  formatLocation(location: SensorLocation): string {
    return getSensorLocationLabel(location);
  }
}

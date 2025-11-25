import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { GlassCardComponent } from '../../shared/components/glass-card/glass-card.component';
import { DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { FilterPanelComponent, FilterOptions } from '../../shared/components/filter-panel/filter-panel.component';
import { SensorService } from '../../core/services/sensor.service';
import { SensorReading } from '../../core/models/sensor-reading.model';
import { PageInfo } from '../../core/services/graphql.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, GlassCardComponent, DataTableComponent, FilterPanelComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private realTimeSubscription: any;
  
  readings: SensorReading[] = [];
  totalCount: number = 0;
  currentPage: number = 1;
  pageSize: number = 20;
  pageInfo?: PageInfo;
  
  // Cursor history for navigation
  private cursorHistory: (string | null)[] = [null]; // Start with null for first page
  private currentCursorIndex: number = 0;
  
  filters: any = {};
  sortField: string = 'timestamp';
  sortDirection: 'asc' | 'desc' = 'desc';
  realTimeEnabled: boolean = false;

  constructor(private sensorService: SensorService) {}

  ngOnInit() {
    this.loadData();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.realTimeSubscription) {
      this.realTimeSubscription.unsubscribe();
    }
  }

  loadData() {
    const currentCursor = this.cursorHistory[this.currentCursorIndex];
    const where = this.buildWhereFilter();
    const order = this.buildOrderFilter();

    this.sensorService.getReadings(this.pageSize, currentCursor, where, order)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.readings = data?.items ?? [];
          this.totalCount = data?.totalCount ?? 0;
          this.pageInfo = data?.pageInfo;
        },
        error: (err) => console.error('Error loading data:', err)
      });
  }

  onFilterChange(filters: FilterOptions) {
    this.filters = filters;
    
    // Reset pagination when filters change
    this.currentPage = 1;
    this.cursorHistory = [null];
    this.currentCursorIndex = 0;
    
    // Reset sorting if requested
    if (filters.resetSort) {
      this.sortField = 'timestamp';
      this.sortDirection = 'desc';
    }
    
    this.loadData();
  }

  onPageChange(direction: 'next' | 'prev') {
    if (direction === 'next' && this.pageInfo?.hasNextPage) {
      // Moving forward
      this.currentCursorIndex++;
      this.currentPage++;
      
      // Add new cursor if we don't have it yet
      if (this.currentCursorIndex >= this.cursorHistory.length) {
        this.cursorHistory.push(this.pageInfo.endCursor || null);
      }
    } else if (direction === 'prev' && this.currentPage > 1) {
      // Moving backward
      this.currentCursorIndex--;
      this.currentPage--;
    }
    
    this.loadData();
  }

  onSortChange(sort: {field: string, direction: 'asc' | 'desc'}) {
    this.sortField = sort.field;
    this.sortDirection = sort.direction;
    
    // Reset pagination when sort changes
    this.currentPage = 1;
    this.cursorHistory = [null];
    this.currentCursorIndex = 0;
    
    this.loadData();
  }

  onRefresh() {
    this.loadData();
  }

  onRealTimeToggle(enabled: boolean) {
    this.realTimeEnabled = enabled;
    
    // Unsubscribe from previous subscription if exists
    if (this.realTimeSubscription) {
      this.realTimeSubscription.unsubscribe();
      this.realTimeSubscription = null;
    }

    // Subscribe to real-time updates if enabled
    if (enabled) {
      this.realTimeSubscription = this.sensorService.getRealTimeUpdates()
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.loadData();
        });
    }
  }

  private buildWhereFilter() {
    const where: any = {};
    if (this.filters.type) {
      where.type = { eq: this.filters.type };
    }
    if (this.filters.location) {
      where.name = { eq: this.filters.location };
    }
    if (this.filters.startDate) {
      // Convert datetime-local string to ISO format for GraphQL
      where.timestamp = { ...where.timestamp, gte: new Date(this.filters.startDate).toISOString() };
    }
    if (this.filters.endDate) {
      // Convert datetime-local string to ISO format for GraphQL
      where.timestamp = { ...where.timestamp, lte: new Date(this.filters.endDate).toISOString() };
    }
    return Object.keys(where).length > 0 ? where : undefined;
  }

  private buildOrderFilter() {
    return [{ [this.sortField]: this.sortDirection.toUpperCase() }];
  }
}

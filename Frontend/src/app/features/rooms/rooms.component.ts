import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { GlassCardComponent } from '../../shared/components/glass-card/glass-card.component';
import { DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { FilterPanelComponent, FilterOptions } from '../../shared/components/filter-panel/filter-panel.component';
import { SensorService } from '../../core/services/sensor.service';
import { SensorReading } from '../../core/models/sensor-reading.model';
import { SensorLocation } from '../../core/models/enums';

interface RoomData {
  location: SensorLocation;
  readings: SensorReading[];
  totalCount: number;
  currentPage: number;
}

@Component({
  selector: 'app-rooms',
  standalone: true,
  imports: [CommonModule, GlassCardComponent, DataTableComponent, FilterPanelComponent],
  templateUrl: './rooms.component.html',
  styleUrls: ['./rooms.component.css']
})
export class RoomsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private realTimeSubscription: any;
  
  rooms: RoomData[] = [];
  pageSize: number = 10;
  filters: any = {};
  realTimeEnabled: boolean = false;

  sensorLocations = Object.values(SensorLocation);

  constructor(private sensorService: SensorService) {}

  ngOnInit() {
    this.initializeRooms();
    this.loadAllRoomsData();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.realTimeSubscription) {
      this.realTimeSubscription.unsubscribe();
    }
  }

  initializeRooms() {
    this.rooms = this.sensorLocations.map(location => ({
      location,
      readings: [],
      totalCount: 0,
      currentPage: 1
    }));
  }

  loadAllRoomsData() {
    this.rooms.forEach(room => this.loadRoomData(room));
  }

  loadRoomData(room: RoomData) {
    const skip = (room.currentPage - 1) * this.pageSize;
    const where = this.buildWhereFilter(room.location);
    const order = [{ timestamp: 'DESC' }];

    this.sensorService.getReadings(skip, this.pageSize, where, order)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          room.readings = data?.items ?? [];
          room.totalCount = data?.totalCount ?? 0;
        },
        error: (err) => console.error(`Error loading data for ${room.location}:`, err)
      });
  }

  onFilterChange(filters: FilterOptions) {
    this.filters = filters;
    this.rooms.forEach(room => room.currentPage = 1);
    this.loadAllRoomsData();
  }

  onRefresh() {
    this.loadAllRoomsData();
  }

  onRealTimeToggle(enabled: boolean) {
    this.realTimeEnabled = enabled;
    
    if (this.realTimeSubscription) {
      this.realTimeSubscription.unsubscribe();
      this.realTimeSubscription = null;
    }

    if (enabled) {
      this.realTimeSubscription = this.sensorService.getRealTimeUpdates()
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.loadAllRoomsData();
        });
    }
  }

  onPageChange(room: RoomData, page: number) {
    room.currentPage = page;
    this.loadRoomData(room);
  }

  onSortChange(room: RoomData, sort: {field: string, direction: 'asc' | 'desc'}) {
    // Reload data with new sort
    this.loadRoomData(room);
  }

  private buildWhereFilter(location: SensorLocation) {
    const where: any = { name: { eq: location } };
    
    if (this.filters.type) {
      where.type = { eq: this.filters.type };
    }
    if (this.filters.startDate) {
      where.timestamp = { ...where.timestamp, gte: new Date(this.filters.startDate).toISOString() };
    }
    if (this.filters.endDate) {
      where.timestamp = { ...where.timestamp, lte: new Date(this.filters.endDate).toISOString() };
    }
    
    return where;
  }
}

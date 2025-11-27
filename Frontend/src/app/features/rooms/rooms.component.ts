import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { GlassCardComponent } from '../../shared/components/glass-card/glass-card.component';
import { DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { FilterPanelComponent, FilterOptions } from '../../shared/components/filter-panel/filter-panel.component';
import { SensorService } from '../../core/services/sensor.service';
import { SensorReading } from '../../core/models/sensor-reading.model';
import { SensorLocation } from '../../core/models/enums';
import { PageInfo } from '../../core/services/graphql.service';

interface RoomData {
  location: SensorLocation;
  readings: SensorReading[];
  totalCount: number;
  currentPage: number;
  pageInfo?: PageInfo;
  // Cursor history for pagination
  cursorHistory: (string | null)[];
  currentCursorIndex: number;
  // Sorting
  sortField: string;
  sortDirection: 'asc' | 'desc';
  // Loading state
  loading: boolean;
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
      currentPage: 1,
      cursorHistory: [null], // Start with null for first page
      currentCursorIndex: 0,
      sortField: 'timestamp',
      sortDirection: 'desc' as 'asc' | 'desc',
      loading: false
    }));
  }

  loadAllRoomsData() {
    this.rooms.forEach(room => this.loadRoomData(room));
  }

  loadRoomData(room: RoomData) {
    const currentCursor = room.cursorHistory[room.currentCursorIndex];
    const where = this.buildWhereFilter(room.location);
    const order = [{ [room.sortField]: room.sortDirection.toUpperCase() }];

    room.loading = true;
    this.sensorService.getReadings(this.pageSize, currentCursor, where, order)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          room.readings = data?.items ?? [];
          room.totalCount = data?.totalCount ?? 0;
          room.pageInfo = data?.pageInfo;
          room.loading = false;
        },
        error: (err) => {
          console.error(`Error loading data for ${room.location}:`, err);
          room.loading = false;
        }
      });
  }

  onFilterChange(filters: FilterOptions) {
    this.filters = filters;
    // Reset pagination for all rooms when filters change
    this.rooms.forEach(room => {
      room.currentPage = 1;
      room.cursorHistory = [null];
      room.currentCursorIndex = 0;
    });
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

  onPageChange(room: RoomData, direction: 'next' | 'prev') {
    if (direction === 'next' && room.pageInfo?.hasNextPage) {
      // Moving forward
      room.currentCursorIndex++;
      room.currentPage++;
      
      // Add new cursor if we don't have it yet
      if (room.currentCursorIndex >= room.cursorHistory.length) {
        room.cursorHistory.push(room.pageInfo.endCursor || null);
      }
    } else if (direction === 'prev' && room.currentPage > 1) {
      // Moving backward
      room.currentCursorIndex--;
      room.currentPage--;
    }
    
    this.loadRoomData(room);
  }

  onSortChange(room: RoomData, sort: {field: string, direction: 'asc' | 'desc'}) {
    // Update room's sort parameters
    room.sortField = sort.field;
    room.sortDirection = sort.direction;
    
    // Reset pagination when sort changes
    room.currentPage = 1;
    room.cursorHistory = [null];
    room.currentCursorIndex = 0;
    
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
    // Sensor value filters
    if (this.filters.co2Min !== undefined && this.filters.co2Min !== null && this.filters.co2Min !== '') {
      where.co2 = { ...where.co2, gte: this.filters.co2Min };
    }
    if (this.filters.co2Max !== undefined && this.filters.co2Max !== null && this.filters.co2Max !== '') {
      where.co2 = { ...where.co2, lte: this.filters.co2Max };
    }
    if (this.filters.pm25Min !== undefined && this.filters.pm25Min !== null && this.filters.pm25Min !== '') {
      where.pm25 = { ...where.pm25, gte: this.filters.pm25Min };
    }
    if (this.filters.pm25Max !== undefined && this.filters.pm25Max !== null && this.filters.pm25Max !== '') {
      where.pm25 = { ...where.pm25, lte: this.filters.pm25Max };
    }
    if (this.filters.humidityMin !== undefined && this.filters.humidityMin !== null && this.filters.humidityMin !== '') {
      where.humidity = { ...where.humidity, gte: this.filters.humidityMin };
    }
    if (this.filters.humidityMax !== undefined && this.filters.humidityMax !== null && this.filters.humidityMax !== '') {
      where.humidity = { ...where.humidity, lte: this.filters.humidityMax };
    }
    if (this.filters.motion !== undefined && this.filters.motion !== null) {
      where.motionDetected = { eq: this.filters.motion };
    }
    if (this.filters.energyMin !== undefined && this.filters.energyMin !== null && this.filters.energyMin !== '') {
      where.energy = { ...where.energy, gte: this.filters.energyMin };
    }
    if (this.filters.energyMax !== undefined && this.filters.energyMax !== null && this.filters.energyMax !== '') {
      where.energy = { ...where.energy, lte: this.filters.energyMax };
    }
    
    return where;
  }

  formatRoomName(location: SensorLocation): string {
    // Convert "LIVING_ROOM" to "Living room"
    return location
      .toLowerCase()
      .split('_')
      .map((word, index) => index === 0 ? word.charAt(0).toUpperCase() + word.slice(1) : word)
      .join(' ');
  }
}

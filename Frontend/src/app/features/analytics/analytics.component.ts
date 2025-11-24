import { Component, OnInit, OnDestroy, ViewChildren, QueryList } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { GlassCardComponent } from '../../shared/components/glass-card/glass-card.component';
import { LineChartComponent } from '../../shared/components/line-chart/line-chart.component';
import { FilterPanelComponent, FilterOptions } from '../../shared/components/filter-panel/filter-panel.component';
import { SensorService } from '../../core/services/sensor.service';
import { SignalrService } from '../../core/services/signalr.service';
import { SensorReading } from '../../core/models/sensor-reading.model';
import { SensorType, SensorLocation } from '../../core/models/enums';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule, GlassCardComponent, LineChartComponent, FilterPanelComponent],
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.css']
})
export class AnalyticsComponent implements OnInit, OnDestroy {
  @ViewChildren(LineChartComponent) chartComponents!: QueryList<LineChartComponent>;
  
  private destroy$ = new Subject<void>();
  private realTimeSubscription: any;
  
  airQualityChartData: any;
  energyChartData: any;
  motionChartData: any;
  
  filters: any = {};
  realTimeEnabled: boolean = false;

  constructor(
    private sensorService: SensorService,
    private signalrService: SignalrService
  ) {}

  ngOnInit() {
    // Load initial data
    this.loadChartData();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.realTimeSubscription) {
      this.realTimeSubscription.unsubscribe();
    }
  }

  loadChartData() {
    this.loadAirQualityData();
    this.loadEnergyData();
    this.loadMotionData();
  }

  loadAirQualityData() {
    const where = this.buildWhereFilter(SensorType.AirQuality);
    
    this.sensorService.getReadings(0, 200, where, [{ timestamp: 'ASC' }])
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.airQualityChartData = this.buildChartData(data?.items ?? [], ['co2', 'pm25', 'humidity']);
          this.updateChart(0);
        },
        error: (err) => console.error('Error loading air quality data:', err)
      });
  }

  loadEnergyData() {
    const where = this.buildWhereFilter(SensorType.Energy);
    
    this.sensorService.getReadings(0, 200, where, [{ timestamp: 'ASC' }])
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.energyChartData = this.buildChartData(data?.items ?? [], ['energy']);
          this.updateChart(1);
        },
        error: (err) => console.error('Error loading energy data:', err)
      });
  }

  loadMotionData() {
    const where = this.buildWhereFilter(SensorType.Motion);
    
    this.sensorService.getReadings(0, 200, where, [{ timestamp: 'ASC' }])
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.motionChartData = this.buildMotionChartData(data?.items ?? []);
          this.updateChart(2);
        },
        error: (err) => console.error('Error loading motion data:', err)
      });
  }

  private buildChartData(readings: SensorReading[], fields: string[]) {
    const locations = Object.values(SensorLocation);
    
    // Get unique timestamps for labels
    const labels = [...new Set(readings.map(r => new Date(r.timestamp).toLocaleTimeString()))];
    
    const datasets = locations.flatMap(location => {
      const locationReadings = readings.filter(r => r.name === location);
      
      return fields.map(field => ({
        label: `${location} - ${field}`,
        data: locationReadings.map(r => (r as any)[field] ?? 0),
        borderColor: this.getColorForLocation(location),
        backgroundColor: this.getColorForLocation(location) + '33',
        tension: 0.4
      }));
    });

    return {
      labels,
      datasets
    };
  }

  private buildMotionChartData(readings: SensorReading[]) {
    const locations = Object.values(SensorLocation);
    
    // Get unique timestamps for labels
    const labels = [...new Set(readings.map(r => new Date(r.timestamp).toLocaleTimeString()))];
    
    const datasets = locations.map(location => {
      const locationReadings = readings.filter(r => r.name === location);
      
      return {
        label: location,
        data: locationReadings.map(r => r.motionDetected ? 1 : 0),
        borderColor: this.getColorForLocation(location),
        backgroundColor: this.getColorForLocation(location) + '33',
        tension: 0.1,
        stepped: true
      };
    });

    return {
      labels,
      datasets
    };
  }

  private getColorForLocation(location: SensorLocation): string {
    const colors: Record<string, string> = {
      [SensorLocation.Kitchen]: '#ef4444',
      [SensorLocation.Garage]: '#f59e0b',
      [SensorLocation.Bedroom]: '#8b5cf6',
      [SensorLocation.LivingRoom]: '#10b981',
      [SensorLocation.Office]: '#3b82f6',
      [SensorLocation.Corridor]: '#ec4899'
    };
    return colors[location] || '#6b7280';
  }

  onFilterChange(filters: FilterOptions) {
    this.filters = filters;
    this.loadChartData();
  }

  onRefresh() {
    this.loadChartData();
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
          this.loadChartData();
        });
    }
  }

  private buildWhereFilter(type: SensorType) {
    const where: any = { type: { eq: type } };
    
    if (this.filters.location) {
      where.name = { eq: this.filters.location };
    }
    
    // If no date filters are provided, default to last 30 minutes
    if (!this.filters.startDate && !this.filters.endDate) {
      const now = new Date();
      const thirtyMinutesAgo = new Date(now.getTime() - 30 * 60 * 1000);
      where.timestamp = { gte: thirtyMinutesAgo.toISOString() };
    } else {
      // Apply custom date filters if provided
      if (this.filters.startDate) {
        where.timestamp = { ...where.timestamp, gte: new Date(this.filters.startDate).toISOString() };
      }
      if (this.filters.endDate) {
        where.timestamp = { ...where.timestamp, lte: new Date(this.filters.endDate).toISOString() };
      }
    }
    
    return where;
  }

  private updateChart(index: number) {
    setTimeout(() => {
      const charts = this.chartComponents?.toArray();
      if (charts && charts[index]) {
        const chartData = [this.airQualityChartData, this.energyChartData, this.motionChartData][index];
        if (chartData) {
          charts[index].updateChart(chartData);
        }
      }
    }, 100);
  }
}

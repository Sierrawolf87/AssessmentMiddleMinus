import { Component, OnInit, OnDestroy, ViewChildren, QueryList } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil, concatMap } from 'rxjs';
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
  
  co2ChartData: any;
  pm25ChartData: any;
  humidityChartData: any;
  energyChartData: any;
  motionChartData: any;
  
  filters: any = {};
  realTimeEnabled: boolean = false;

  // Loading states for each chart
  co2Loading: boolean = false;
  pm25Loading: boolean = false;
  humidityLoading: boolean = false;
  energyLoading: boolean = false;
  motionLoading: boolean = false;

  constructor(
    private sensorService: SensorService,
    private signalrService: SignalrService
  ) {}

  ngOnInit() {
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
    this.loadCO2Data();
    this.loadPM25Data();
    this.loadHumidityData();
    this.loadEnergyData();
    this.loadMotionData();
  }

  loadCO2Data() {
    const where = this.buildWhereFilter(SensorType.AirQuality);
    this.co2Loading = true;
    this.loadAllData(where, [{ timestamp: 'ASC' }]).subscribe({
      next: (allData: SensorReading[]) => {
        this.co2ChartData = this.buildChartData(allData, ['co2']);
        this.updateChart(0);
        this.co2Loading = false;
      },
      error: (err: any) => {
        console.error('Error loading CO2 data:', err);
        this.co2Loading = false;
      }
    });
  }

  loadPM25Data() {
    const where = this.buildWhereFilter(SensorType.AirQuality);
    this.pm25Loading = true;
    this.loadAllData(where, [{ timestamp: 'ASC' }]).subscribe({
      next: (allData: SensorReading[]) => {
        this.pm25ChartData = this.buildChartData(allData, ['pm25']);
        this.updateChart(1);
        this.pm25Loading = false;
      },
      error: (err: any) => {
        console.error('Error loading PM2.5 data:', err);
        this.pm25Loading = false;
      }
    });
  }

  loadHumidityData() {
    const where = this.buildWhereFilter(SensorType.AirQuality);
    this.humidityLoading = true;
    this.loadAllData(where, [{ timestamp: 'ASC' }]).subscribe({
      next: (allData: SensorReading[]) => {
        this.humidityChartData = this.buildChartData(allData, ['humidity']);
        this.updateChart(2);
        this.humidityLoading = false;
      },
      error: (err: any) => {
        console.error('Error loading Humidity data:', err);
        this.humidityLoading = false;
      }
    });
  }

  loadEnergyData() {
    const where = this.buildWhereFilter(SensorType.Energy);
    this.energyLoading = true;
    this.loadAllData(where, [{ timestamp: 'ASC' }]).subscribe({
      next: (allData: SensorReading[]) => {
        this.energyChartData = this.buildChartData(allData, ['energy']);
        this.updateChart(3);
        this.energyLoading = false;
      },
      error: (err: any) => {
        console.error('Error loading energy data:', err);
        this.energyLoading = false;
      }
    });
  }

  loadMotionData() {
    const where = this.buildWhereFilter(SensorType.Motion);
    this.motionLoading = true;
    this.loadAllData(where, [{ timestamp: 'ASC' }]).subscribe({
      next: (allData: SensorReading[]) => {
        this.motionChartData = this.buildMotionChartData(allData);
        this.updateChart(4);
        this.motionLoading = false;
      },
      error: (err: any) => {
        console.error('Error loading motion data:', err);
        this.motionLoading = false;
      }
    });
  }

  private loadAllData(where: any, order: any[]): any {
    const allItems: SensorReading[] = [];
    
    const loadPage = (cursor: string | null = null): any => {
      return this.sensorService.getReadings(200, cursor, where, order).pipe(
        takeUntil(this.destroy$),
        concatMap((data) => {
          // Add items from current page
          if (data?.items) {
            allItems.push(...data.items);
          }
          
          // If there's a next page, fetch it recursively
          if (data?.pageInfo?.hasNextPage && data?.pageInfo?.endCursor) {
            return loadPage(data.pageInfo.endCursor);
          } else {
            // No more pages, return all accumulated data
            return [allItems];
          }
        })
      );
    };
    
    return loadPage();
  }

  private buildChartData(data: SensorReading[], valueFields: string[]) {
    if (!data || data.length === 0) return null;

    // Get the time range
    let startTime: Date;
    if (this.filters.startDate) {
      startTime = new Date(this.filters.startDate);
    } else {
      startTime = new Date(Date.now() - 30 * 60 * 1000);
    }
    const endTime = new Date(startTime.getTime() + 30 * 60 * 1000);

    // Generate labels for 30-minute range (every minute) in 24-hour format
    const labels: string[] = [];
    for (let time = new Date(startTime); time <= endTime; time = new Date(time.getTime() + 60 * 1000)) {
      labels.push(time.toLocaleTimeString('en-GB', { 
        hour: '2-digit', 
        minute: '2-digit',
        hour12: false 
      }));
    }

    // Group data by location - simplified approach
    const grouped: { [key: string]: any } = {};
    data.forEach(reading => {
      const location = reading.name || 'Unknown';
      const timeLabel = new Date(reading.timestamp).toLocaleTimeString('en-GB', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
      });
      
      if (!grouped[location]) {
        grouped[location] = {};
      }
      grouped[location][timeLabel] = reading;
    });

    // Create datasets
    const datasets: any[] = [];
    const colors = this.generateColors(Object.keys(grouped).length * valueFields.length);
    let colorIndex = 0;

    Object.entries(grouped).forEach(([location, timeData]: [string, any]) => {
      valueFields.forEach(field => {
        datasets.push({
          label: `${location} - ${field.toUpperCase()}`,
          data: labels.map(label => {
            const reading = timeData[label];
            return reading?.[field] ?? null;
          }),
          borderColor: colors[colorIndex],
          backgroundColor: colors[colorIndex] + '20',
          borderWidth: 2,
tension: 0.4,
          spanGaps: true
        });
        colorIndex++;
      });
    });

    return { labels, datasets };
  }

  private buildMotionChartData(data: SensorReading[]) {
    if (!data || data.length === 0) return null;

    // Get the time range
    let startTime: Date;
    if (this.filters.startDate) {
      startTime = new Date(this.filters.startDate);
    } else {
      startTime = new Date(Date.now() - 30 * 60 * 1000);
    }
    const endTime = new Date(startTime.getTime() + 30 * 60 * 1000);

    // Generate labels for 30-minute range in 24-hour format
    const labels: string[] = [];
    for (let time = new Date(startTime); time <= endTime; time = new Date(time.getTime() + 60 * 1000)) {
      labels.push(time.toLocaleTimeString('en-GB', { 
        hour: '2-digit', 
        minute: '2-digit',
        hour12: false 
      }));
    }

    // Group data by location
    const grouped: { [key: string]: any } = {};
    data.forEach(reading => {
      const location = reading.name || 'Unknown';
      const timeLabel = new Date(reading.timestamp).toLocaleTimeString('en-GB', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
      });
      
      if (!grouped[location]) {
        grouped[location] = {};
      }
      grouped[location][timeLabel] = reading;
    });

    // Create datasets
    const datasets: any[] = [];
    const colors = this.generateColors(Object.keys(grouped).length);
    let colorIndex = 0;

    Object.entries(grouped).forEach(([location, timeData]: [string, any]) => {
      datasets.push({
        label: location,
        data: labels.map(label => {
          const reading = timeData[label];
          return reading?.motionDetected ? 1 : 0;
        }),
        borderColor: colors[colorIndex],
        backgroundColor: colors[colorIndex] + '33',
        tension: 0.1,
        stepped: true
      });
      colorIndex++;
    });

    return { labels, datasets };
  }

  private generateColors(count: number): string[] {
    const baseColors = [
      '#ef4444', '#f59e0b', '#10b981', '#3b82f6', '#8b5cf6', '#ec4899',
      '#14b8a6', '#f97316', '#06b6d4', '#a855f7', '#84cc16', '#f43f5e'
    ];
    const colors: string[] = [];
    for (let i = 0; i < count; i++) {
      colors.push(baseColors[i % baseColors.length]);
    }
    return colors;
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
    
    // Time logic: 30 minutes from Start Date or current time
    let startTime: Date;
    if (this.filters.startDate) {
      startTime = new Date(this.filters.startDate);
    } else {
      // Default: current time minus 30 minutes
      startTime = new Date(Date.now() - 30 * 60 * 1000);
    }
    
    // End time is always 30 minutes after start time
    const endTime = new Date(startTime.getTime() + 30 * 60 * 1000);
    
    where.timestamp = {
      gte: startTime.toISOString(),
      lte: endTime.toISOString()
    };
    
    console.log('Chart filter - 30 minute window:', {
      from: startTime.toISOString(),
      to: endTime.toISOString(),
      type: type
    });
    
    return where;
  }

  private updateChart(index: number) {
    setTimeout(() => {
      const charts = this.chartComponents?.toArray();
      if (charts && charts[index]) {
        const chartData = [
          this.co2ChartData, 
          this.pm25ChartData, 
          this.humidityChartData,
          this.energyChartData, 
          this.motionChartData
        ][index];
        if (chartData) {
          charts[index].updateChart(chartData);
        }
      }
    }, 100);
  }
}

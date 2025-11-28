import { Component, OnInit, OnDestroy, ViewChildren, QueryList } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { GlassCardComponent } from '../../shared/components/glass-card/glass-card.component';
import { LineChartComponent } from '../../shared/components/line-chart/line-chart.component';
import { FilterPanelComponent, FilterOptions } from '../../shared/components/filter-panel/filter-panel.component';
import { GraphqlService } from '../../core/services/graphql.service';
import { SignalrService } from '../../core/services/signalr.service';
import { AggregatedSensorReading } from '../../core/models/sensor-reading.model';
import { SensorType, SensorLocation } from '../../core/models/enums';
import { TimeRange, getIntervalForTimeRange, calculateStartDate, getDurationMinutes } from '../../core/utils/time-range.utils';

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
  
  filters: FilterOptions = {};
  realTimeEnabled: boolean = false;

  // Loading states for each chart
  co2Loading: boolean = false;
  pm25Loading: boolean = false;
  humidityLoading: boolean = false;
  energyLoading: boolean = false;
  motionLoading: boolean = false;

  constructor(
    private graphqlService: GraphqlService,
    private signalrService: SignalrService
  ) {}

  ngOnInit() {
    // Set default time range if not specified
    if (!this.filters.timeRange) {
      this.filters.timeRange = TimeRange.THIRTY_MIN;
    }
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
  }

  loadCO2Data() {
    this.co2Loading = true;
    const { startDate, endDate, intervalMinutes } = this.getTimeParams();
    
    this.graphqlService.getAggregatedSensorReadings(
      SensorType.AirQuality,
      this.filters.location,
      startDate,
      endDate,
      intervalMinutes
    ).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data: AggregatedSensorReading[]) => {
        this.co2ChartData = this.buildChartDataFromAggregated(data, 'averageCo2', 'CO2');
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
    this.pm25Loading = true;
    const { startDate, endDate, intervalMinutes } = this.getTimeParams();
    
    this.graphqlService.getAggregatedSensorReadings(
      SensorType.AirQuality,
      this.filters.location,
      startDate,
      endDate,
      intervalMinutes
    ).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data: AggregatedSensorReading[]) => {
        this.pm25ChartData = this.buildChartDataFromAggregated(data, 'averagePm25', 'PM2.5');
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
    this.humidityLoading = true;
    const { startDate, endDate, intervalMinutes } = this.getTimeParams();
    
    this.graphqlService.getAggregatedSensorReadings(
      SensorType.AirQuality,
      this.filters.location,
      startDate,
      endDate,
      intervalMinutes
    ).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data: AggregatedSensorReading[]) => {
        this.humidityChartData = this.buildChartDataFromAggregated(data, 'averageHumidity', 'Humidity');
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
    this.energyLoading = true;
    const { startDate, endDate, intervalMinutes } = this.getTimeParams();
    
    this.graphqlService.getAggregatedSensorReadings(
      SensorType.Energy,
      this.filters.location,
      startDate,
      endDate,
      intervalMinutes
    ).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data: AggregatedSensorReading[]) => {
        this.energyChartData = this.buildChartDataFromAggregated(data, 'averageEnergy', 'Energy');
        this.updateChart(3);
        this.energyLoading = false;
      },
      error: (err: any) => {
        console.error('Error loading energy data:', err);
        this.energyLoading = false;
      }
    });
  }

  private getTimeParams(): { startDate: Date, endDate: Date, intervalMinutes: number } {
    const timeRange = this.filters.timeRange || TimeRange.THIRTY_MIN;
    const intervalMinutes = getIntervalForTimeRange(timeRange);
    
    let startDate: Date;
    if (this.filters.startDate) {
      startDate = new Date(this.filters.startDate);
    } else {
      startDate = calculateStartDate(timeRange);
    }
    
    const duration = getDurationMinutes(timeRange);
    const endDate = new Date(startDate.getTime() + duration * 60 * 1000);
    
    return { startDate, endDate, intervalMinutes };
  }

  private buildChartDataFromAggregated(data: AggregatedSensorReading[], valueField: keyof AggregatedSensorReading, label: string) {
    if (!data || data.length === 0) return null;

    const labels: string[] = [];
    const values: (number | null)[] = [];

    data.forEach(reading => {
      const timestamp = new Date(reading.timestamp);
      const timeLabel = this.formatTimeLabel(timestamp);
      labels.push(timeLabel);
      values.push((reading[valueField] as number) ?? null);
    });

    return {
      labels,
      datasets: [{
        label: label,
        data: values,
        borderColor: '#3b82f6',
        backgroundColor: '#3b82f620',
        borderWidth: 2,
        tension: 0.4,
        spanGaps: true
      }]
    };
  }

  private formatTimeLabel(date: Date): string {
    const intervalMinutes = getIntervalForTimeRange(this.filters.timeRange || TimeRange.THIRTY_MIN);
    
    if (intervalMinutes >= 1440) {
      // 1 day or more: show date
      return date.toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
    } else if (intervalMinutes >= 60) {
      // 1 hour or more: show date and hour
      return date.toLocaleString('en-GB', { 
        day: '2-digit', 
        month: 'short', 
        hour: '2-digit',
        hour12: false
      });
    } else {
      // Less than 1 hour: show time
      return date.toLocaleTimeString('en-GB', { 
        hour: '2-digit', 
        minute: '2-digit',
        hour12: false 
      });
    }
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
      this.realTimeSubscription = this.signalrService.getSensorDataUpdates()
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.loadChartData();
        });
    }
  }

  private updateChart(index: number) {
    setTimeout(() => {
      const charts = this.chartComponents?.toArray();
      if (charts && charts[index]) {
        const chartData = [
          this.co2ChartData, 
          this.pm25ChartData, 
          this.humidityChartData,
          this.energyChartData
        ][index];
        if (chartData) {
          charts[index].updateChart(chartData);
        }
      }
    }, 100);
  }
}

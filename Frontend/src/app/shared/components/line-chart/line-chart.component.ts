import { Component, Input, OnInit, OnDestroy, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, ChartConfiguration, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-line-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './line-chart.component.html',
  styleUrls: ['./line-chart.component.css']
})
export class LineChartComponent implements AfterViewInit, OnDestroy {
  @ViewChild('chartCanvas', { static: false }) chartCanvas!: ElementRef<HTMLCanvasElement>;
  @Input() chartData: any;
  @Input() chartTitle: string = 'Chart';

  private chart?: Chart;

  ngAfterViewInit() {
    if (this.chartData) {
      this.createChart();
    }
  }

  ngOnDestroy() {
    if (this.chart) {
      this.chart.destroy();
    }
  }

  private createChart() {
    if (this.chart) {
      this.chart.destroy();
    }

    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    const config: ChartConfiguration = {
      type: 'line',
      data: this.chartData,
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: false, // Disable animations to prevent flickering on updates
        plugins: {
          legend: {
            display: true  // Disable legend to prevent overflow
          },
          title: {
            display: true,
            text: this.chartTitle,
            color: '#e0e7ff',
            font: {
              size: 16
            }
          }
        },
        scales: {
          x: {
            ticks: {
              color: '#cbd5e1',
              maxRotation: 45,
              minRotation: 0
            },
            grid: {
              color: 'rgba(255, 255, 255, 0.1)'
            }
          },
          y: {
            ticks: {
              color: '#cbd5e1'
            },
            grid: {
              color: 'rgba(255, 255, 255, 0.1)'
            }
          }
        }
      }
    };

    this.chart = new Chart(ctx, config);
  }

  updateChart(newData: any) {
    this.chartData = newData;
    if (this.chart) {
      this.createChart();
    }
  }
}

import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { RoomsComponent } from './features/rooms/rooms.component';
import { AnalyticsComponent } from './features/analytics/analytics.component';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'rooms', component: RoomsComponent },
  { path: 'analytics', component: AnalyticsComponent }
];

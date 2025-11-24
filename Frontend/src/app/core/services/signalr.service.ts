import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { SensorMessageDto } from '../models/sensor-reading.model';
import { ConfigService } from './config.service';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection: signalR.HubConnection;
  private sensorDataSubject = new BehaviorSubject<SensorMessageDto | null>(null);

  constructor(private configService: ConfigService) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.configService.signalRUrl)
      .withAutomaticReconnect()
      .build();

    this.startConnection();
    this.addListeners();
  }

  private startConnection() {
    this.hubConnection
      .start()
      .then(() => console.log('SignalR Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err));
  }

  private addListeners() {
    this.hubConnection.on('ReceiveSensorData', (data: SensorMessageDto) => {
      console.log('Received sensor data:', data);
      this.sensorDataSubject.next(data);
    });
  }

  public getSensorDataUpdates(): Observable<SensorMessageDto | null> {
    return this.sensorDataSubject.asObservable();
  }
}

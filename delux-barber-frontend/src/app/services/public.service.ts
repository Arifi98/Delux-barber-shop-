import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { AvailabilityResponse, BookingResult, BookingStatus } from '../models';

@Injectable({ providedIn: 'root' })
export class PublicService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/public`;

  getServices() { return this.http.get<any[]>(`${this.base}/services`); }
  getBarbers() { return this.http.get<any[]>(`${this.base}/barbers`); }

  getAvailability(date: string, barberId?: number, serviceId?: number) {
    let p = new HttpParams().set('date', date);
    if (barberId) p = p.set('barberId', barberId);
    if (serviceId) p = p.set('serviceId', serviceId);
    return this.http.get<AvailabilityResponse>(`${this.base}/availability`, { params: p });
  }

  book(dto: any) { return this.http.post<BookingResult>(`${this.base}/book`, dto); }
  getStatus(token: string) { return this.http.get<BookingStatus>(`${this.base}/status/${token}`); }
  cancel(token: string) { return this.http.post(`${this.base}/cancel/${token}`, {}); }
}

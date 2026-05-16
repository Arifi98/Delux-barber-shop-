import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Appointment, Barber, Client, DashboardStats, Service } from '../models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  // Appointments
  getAppointments(params?: any) {
    let p = new HttpParams();
    if (params) Object.entries(params).forEach(([k, v]) => { if (v !== null && v !== undefined && v !== '') p = p.set(k, String(v)); });
    return this.http.get<Appointment[]>(`${this.base}/appointments`, { params: p });
  }
  createAppointment(dto: any) { return this.http.post<Appointment>(`${this.base}/appointments`, dto); }
  updateAppointment(id: number, dto: any) { return this.http.put<Appointment>(`${this.base}/appointments/${id}`, dto); }
  patchStatus(id: number, status: string) { return this.http.patch(`${this.base}/appointments/${id}/status`, { status }); }
  deleteAppointment(id: number) { return this.http.delete(`${this.base}/appointments/${id}`); }

  // Barbers
  getBarbers() { return this.http.get<Barber[]>(`${this.base}/barbers`); }
  createBarber(dto: any) { return this.http.post<Barber>(`${this.base}/barbers`, dto); }
  updateBarber(id: number, dto: any) { return this.http.put<Barber>(`${this.base}/barbers/${id}`, dto); }
  deleteBarber(id: number) { return this.http.delete(`${this.base}/barbers/${id}`); }

  // Services
  getServices() { return this.http.get<Service[]>(`${this.base}/services`); }
  createService(dto: any) { return this.http.post<Service>(`${this.base}/services`, dto); }
  updateService(id: number, dto: any) { return this.http.put<Service>(`${this.base}/services/${id}`, dto); }
  deleteService(id: number) { return this.http.delete(`${this.base}/services/${id}`); }

  // Clients
  getClients() { return this.http.get<Client[]>(`${this.base}/clients`); }

  // Dashboard
  getDashboardStats() { return this.http.get<DashboardStats>(`${this.base}/dashboard/stats`); }
}

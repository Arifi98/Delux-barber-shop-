import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Appointment, Barber, Service } from '../../models';

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './appointments.component.html',
  styleUrls: ['./appointments.component.scss']
})
export class AppointmentsComponent implements OnInit {
  private api = inject(ApiService);

  appointments: Appointment[] = [];
  barbers: Barber[] = [];
  services: Service[] = [];
  loading = false;
  saving = false;
  error = '';

  // Filters
  filterDate = '';
  filterStatus = '';
  filterBarberId = '';

  // Modal
  showModal = false;
  editingId: number | null = null;

  statusOptions = ['Pending', 'Confirmed', 'Completed', 'Cancelled', 'NoShow'];

  // Form model
  form: any = {
    firstName: '', lastName: '', phone: '', email: '',
    serviceId: '', barberId: '', appointmentDate: '',
    appointmentTime: '', notes: ''
  };

  ngOnInit() {
    this.load();
    this.api.getBarbers().subscribe(b => this.barbers = b);
    this.api.getServices().subscribe(s => this.services = s);
  }

  load() {
    this.loading = true;
    const params: any = {};
    if (this.filterDate) params['date'] = this.filterDate;
    if (this.filterStatus) params['status'] = this.filterStatus;
    if (this.filterBarberId) params['barberId'] = this.filterBarberId;
    this.api.getAppointments(params).subscribe({
      next: a => { this.appointments = a; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  openCreate() {
    this.editingId = null;
    this.form = {
      firstName: '', lastName: '', phone: '', email: '',
      serviceId: '', barberId: '', appointmentDate: '',
      appointmentTime: '', notes: ''
    };
    this.error = '';
    this.showModal = true;
  }

  openEdit(a: Appointment) {
    this.editingId = a.id;
    this.form = {
      firstName: a.firstName,
      lastName: a.lastName,
      phone: a.phone,
      email: a.email || '',
      serviceId: a.serviceId,
      barberId: a.barberId || '',
      appointmentDate: a.appointmentDate,
      appointmentTime: a.appointmentTime,
      notes: a.notes || ''
    };
    this.error = '';
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
    this.error = '';
  }

  save() {
    this.saving = true;
    this.error = '';
    const dto: any = {
      firstName: this.form.firstName,
      lastName: this.form.lastName,
      phone: this.form.phone,
      serviceId: Number(this.form.serviceId),
      appointmentDate: this.form.appointmentDate,
      appointmentTime: this.form.appointmentTime,
    };
    if (this.form.email) dto.email = this.form.email;
    if (this.form.barberId) dto.barberId = Number(this.form.barberId);
    if (this.form.notes) dto.notes = this.form.notes;

    const obs = this.editingId
      ? this.api.updateAppointment(this.editingId, dto)
      : this.api.createAppointment(dto);

    obs.subscribe({
      next: () => { this.saving = false; this.closeModal(); this.load(); },
      error: (err) => { this.error = err.error?.message || 'An error occurred.'; this.saving = false; }
    });
  }

  patchStatus(a: Appointment, status: string) {
    this.api.patchStatus(a.id, status).subscribe({
      next: () => this.load(),
      error: () => alert('Failed to update status.')
    });
  }

  delete(a: Appointment) {
    if (!confirm(`Delete appointment for ${a.firstName} ${a.lastName}?`)) return;
    this.api.deleteAppointment(a.id).subscribe({
      next: () => this.load(),
      error: () => alert('Failed to delete appointment.')
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      Pending: 'badge-pending', Confirmed: 'badge-confirmed',
      Completed: 'badge-completed', Cancelled: 'badge-cancelled', NoShow: 'badge-noshow'
    };
    return map[status] ?? 'badge-pending';
  }

  clearFilters() {
    this.filterDate = '';
    this.filterStatus = '';
    this.filterBarberId = '';
    this.load();
  }
}

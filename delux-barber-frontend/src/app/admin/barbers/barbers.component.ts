import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Barber } from '../../models';

@Component({
  selector: 'app-barbers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './barbers.component.html',
  styleUrls: ['./barbers.component.scss']
})
export class BarbersComponent implements OnInit {
  private api = inject(ApiService);

  barbers: Barber[] = [];
  loading = false;
  saving = false;
  error = '';

  showModal = false;
  editingId: number | null = null;

  form: any = {
    firstName: '', lastName: '', email: '', phone: '',
    bio: '', specialty: '', imageUrl: '', isActive: true
  };

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.api.getBarbers().subscribe({
      next: b => { this.barbers = b; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  openCreate() {
    this.editingId = null;
    this.form = { firstName: '', lastName: '', email: '', phone: '', bio: '', specialty: '', imageUrl: '', isActive: true };
    this.error = '';
    this.showModal = true;
  }

  openEdit(b: Barber) {
    this.editingId = b.id;
    this.form = {
      firstName: b.firstName,
      lastName: b.lastName,
      email: b.email || '',
      phone: b.phone || '',
      bio: b.bio || '',
      specialty: b.specialty || '',
      imageUrl: b.imageUrl || '',
      isActive: b.isActive
    };
    this.error = '';
    this.showModal = true;
  }

  closeModal() { this.showModal = false; this.error = ''; }

  save() {
    this.saving = true; this.error = '';
    const dto: any = {
      firstName: this.form.firstName,
      lastName: this.form.lastName,
      isActive: this.form.isActive
    };
    if (this.form.email) dto.email = this.form.email;
    if (this.form.phone) dto.phone = this.form.phone;
    if (this.form.bio) dto.bio = this.form.bio;
    if (this.form.specialty) dto.specialty = this.form.specialty;
    if (this.form.imageUrl) dto.imageUrl = this.form.imageUrl;

    const obs = this.editingId
      ? this.api.updateBarber(this.editingId, dto)
      : this.api.createBarber(dto);

    obs.subscribe({
      next: () => { this.saving = false; this.closeModal(); this.load(); },
      error: (err) => { this.error = err.error?.message || 'An error occurred.'; this.saving = false; }
    });
  }

  delete(b: Barber) {
    if (!confirm(`Delete barber ${b.firstName} ${b.lastName}?`)) return;
    this.api.deleteBarber(b.id).subscribe({
      next: () => this.load(),
      error: () => alert('Failed to delete barber.')
    });
  }
}

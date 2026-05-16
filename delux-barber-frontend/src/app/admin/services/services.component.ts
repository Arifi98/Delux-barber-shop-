import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Service } from '../../models';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './services.component.html',
  styleUrls: ['./services.component.scss']
})
export class ServicesComponent implements OnInit {
  private api = inject(ApiService);

  services: Service[] = [];
  loading = false;
  saving = false;
  error = '';

  showModal = false;
  editingId: number | null = null;

  form: any = {
    name: '', description: '', price: 0, durationMinutes: 30, isActive: true
  };

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.api.getServices().subscribe({
      next: s => { this.services = s; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  openCreate() {
    this.editingId = null;
    this.form = { name: '', description: '', price: 0, durationMinutes: 30, isActive: true };
    this.error = '';
    this.showModal = true;
  }

  openEdit(s: Service) {
    this.editingId = s.id;
    this.form = {
      name: s.name,
      description: s.description || '',
      price: s.price,
      durationMinutes: s.durationMinutes,
      isActive: s.isActive
    };
    this.error = '';
    this.showModal = true;
  }

  closeModal() { this.showModal = false; this.error = ''; }

  save() {
    this.saving = true; this.error = '';
    const dto: any = {
      name: this.form.name,
      price: Number(this.form.price),
      durationMinutes: Number(this.form.durationMinutes),
      isActive: this.form.isActive
    };
    if (this.form.description) dto.description = this.form.description;

    const obs = this.editingId
      ? this.api.updateService(this.editingId, dto)
      : this.api.createService(dto);

    obs.subscribe({
      next: () => { this.saving = false; this.closeModal(); this.load(); },
      error: (err) => { this.error = err.error?.message || 'An error occurred.'; this.saving = false; }
    });
  }

  delete(s: Service) {
    if (!confirm(`Delete service "${s.name}"?`)) return;
    this.api.deleteService(s.id).subscribe({
      next: () => this.load(),
      error: () => alert('Failed to delete service.')
    });
  }
}

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { DashboardStats } from '../../models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  private api = inject(ApiService);
  stats: DashboardStats | null = null;
  loading = true;

  ngOnInit() {
    this.api.getDashboardStats().subscribe({
      next: s => { this.stats = s; this.loading = false; },
      error: () => this.loading = false
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      Pending: 'badge-pending',
      Confirmed: 'badge-confirmed',
      Completed: 'badge-completed',
      Cancelled: 'badge-cancelled',
      NoShow: 'badge-noshow'
    };
    return map[status] ?? 'badge-pending';
  }
}

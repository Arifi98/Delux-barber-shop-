import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PublicService } from '../../services/public.service';
import { BookingStatus } from '../../models';

@Component({
  selector: 'app-status',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './status.component.html',
  styleUrls: ['./status.component.scss']
})
export class StatusComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private publicSvc = inject(PublicService);
  status: BookingStatus | null = null;
  loading = true;
  error = '';
  cancelled = false;
  cancelling = false;

  ngOnInit() {
    const token = this.route.snapshot.paramMap.get('token')!;
    this.publicSvc.getStatus(token).subscribe({
      next: s => { this.status = s; this.loading = false; },
      error: () => { this.error = 'Rezervimi nuk u gjet.'; this.loading = false; }
    });
  }

  cancel() {
    if (!this.status || !confirm('Jeni i sigurt që doni të anuloni rezervimin?')) return;
    this.cancelling = true;
    this.publicSvc.cancel(this.status.token).subscribe({
      next: () => { this.cancelled = true; this.cancelling = false; if (this.status) this.status.status = 'Cancelled'; },
      error: (err) => { alert(err.error || 'Gabim gjatë anulimit.'); this.cancelling = false; }
    });
  }

  getStatusClass(s: string): string {
    const map: Record<string, string> = {
      Pending: 'badge-pending',
      Confirmed: 'badge-confirmed',
      Completed: 'badge-completed',
      Cancelled: 'badge-cancelled',
      NoShow: 'badge-noshow'
    };
    return map[s] ?? 'badge-pending';
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return d.toLocaleDateString('sq-AL', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
  }

  translateStatus(s: string): string {
    const map: Record<string, string> = {
      Pending: 'Në Pritje',
      Confirmed: 'Konfirmuar',
      Completed: 'Përfunduar',
      Cancelled: 'Anuluar',
      NoShow: 'Nuk u Paraqit'
    };
    return map[s] ?? s;
  }
}

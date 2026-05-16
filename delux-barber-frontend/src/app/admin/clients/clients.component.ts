import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Client } from '../../models';

@Component({
  selector: 'app-clients',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './clients.component.html',
  styleUrls: ['./clients.component.scss']
})
export class ClientsComponent implements OnInit {
  private api = inject(ApiService);

  clients: Client[] = [];
  filtered: Client[] = [];
  loading = false;
  search = '';

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.api.getClients().subscribe({
      next: c => { this.clients = c; this.applySearch(); this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  applySearch() {
    const q = this.search.toLowerCase().trim();
    if (!q) { this.filtered = this.clients; return; }
    this.filtered = this.clients.filter(c =>
      c.firstName.toLowerCase().includes(q) ||
      c.lastName.toLowerCase().includes(q) ||
      c.phone.includes(q) ||
      (c.email || '').toLowerCase().includes(q)
    );
  }
}

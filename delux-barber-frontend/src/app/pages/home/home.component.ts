import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PublicService } from '../../services/public.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  private publicSvc = inject(PublicService);
  services: any[] = [];
  barbers: any[] = [];

  ngOnInit() {
    this.publicSvc.getServices().subscribe(s => this.services = s);
    this.publicSvc.getBarbers().subscribe(b => this.barbers = b);
  }
}

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PublicService } from '../../services/public.service';
import { BookingResult, BookingSlot } from '../../models';

@Component({
  selector: 'app-book',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './book.component.html',
  styleUrls: ['./book.component.scss']
})
export class BookComponent implements OnInit {
  private publicSvc = inject(PublicService);

  // Steps
  currentStep = 1;
  totalSteps = 4;

  // Data
  services: any[] = [];
  barbers: any[] = [];
  slots: BookingSlot[] = [];

  // Selections
  selectedService: any = null;
  selectedBarber: any = null;
  selectedDate = '';
  selectedTime = '';

  // Personal info
  firstName = '';
  lastName = '';
  phone = '';
  email = '';
  notes = '';

  // State
  loadingSlots = false;
  slotsError = '';
  booking = false;
  error = '';
  result: BookingResult | null = null;

  // Min date (today)
  minDate = new Date().toISOString().split('T')[0];

  ngOnInit() {
    this.publicSvc.getServices().subscribe(s => this.services = s);
    this.publicSvc.getBarbers().subscribe(b => this.barbers = b);
  }

  selectService(service: any) {
    this.selectedService = service;
    this.selectedTime = '';
    this.slots = [];
    if (this.selectedDate) {
      this.loadSlots();
    }
  }

  selectBarber(barber: any | null) {
    this.selectedBarber = barber;
    this.selectedTime = '';
    this.slots = [];
    if (this.selectedDate && this.selectedService) {
      this.loadSlots();
    }
  }

  onDateChange() {
    this.selectedTime = '';
    this.slots = [];
    if (this.selectedDate && this.selectedService) {
      this.loadSlots();
    }
  }

  loadSlots() {
    if (!this.selectedDate || !this.selectedService) return;
    this.loadingSlots = true;
    this.slotsError = '';
    this.publicSvc.getAvailability(
      this.selectedDate,
      this.selectedBarber?.id,
      this.selectedService?.id
    ).subscribe({
      next: res => {
        this.loadingSlots = false;
        if (res.closed) {
          this.slotsError = 'The shop is closed on this day.';
          this.slots = [];
        } else {
          this.slots = res.slots;
        }
      },
      error: () => {
        this.loadingSlots = false;
        this.slotsError = 'Could not load time slots. Please try again.';
        this.slots = [];
      }
    });
  }

  isSunday(dateStr: string): boolean {
    if (!dateStr) return false;
    return new Date(dateStr).getDay() === 0;
  }

  canGoNext(): boolean {
    switch (this.currentStep) {
      case 1: return !!this.selectedService;
      case 2: return !!this.selectedDate && !!this.selectedTime && !this.isSunday(this.selectedDate);
      case 3: return !!this.firstName && !!this.lastName && !!this.phone;
      default: return false;
    }
  }

  next() {
    if (this.canGoNext() && this.currentStep < this.totalSteps) {
      this.currentStep++;
    }
  }

  back() {
    if (this.currentStep > 1) {
      this.currentStep--;
      this.error = '';
    }
  }

  book() {
    if (!this.selectedService || !this.selectedDate || !this.selectedTime) return;
    this.booking = true;
    this.error = '';

    const dto: any = {
      firstName: this.firstName,
      lastName: this.lastName,
      phone: this.phone,
      serviceId: this.selectedService.id,
      appointmentDate: this.selectedDate,
      appointmentTime: this.selectedTime,
    };
    if (this.email) dto.email = this.email;
    if (this.selectedBarber) dto.barberId = this.selectedBarber.id;
    if (this.notes) dto.notes = this.notes;

    this.publicSvc.book(dto).subscribe({
      next: res => {
        this.result = res;
        this.booking = false;
        this.currentStep = 4;
      },
      error: (err) => {
        this.error = err.error?.message || err.error || 'Booking failed. Please try again.';
        this.booking = false;
      }
    });
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-GB', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
  }
}

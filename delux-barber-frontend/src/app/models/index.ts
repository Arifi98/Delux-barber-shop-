export interface Service {
  id: number; name: string; description?: string; price: number; durationMinutes: number; isActive: boolean;
}
export interface Barber {
  id: number; firstName: string; lastName: string; email?: string; phone?: string; bio?: string; specialty?: string; imageUrl?: string; isActive: boolean; createdAt?: string;
  fullName?: string; // public endpoint returns fullName
}
export interface Client {
  id: number; firstName: string; lastName: string; phone: string; email?: string; totalAppointments: number; lastAppointmentDate?: string; createdAt: string;
}
export interface Appointment {
  id: number; clientId?: number; firstName: string; lastName: string; phone: string; email?: string;
  serviceId: number; serviceName: string; servicePrice: number;
  barberId?: number; barberName?: string;
  appointmentDate: string; appointmentTime: string; status: string;
  notes?: string; reminderSent: boolean; bookingToken?: string; isPublicBooking: boolean;
  createdAt: string; updatedAt: string;
}
export interface DashboardStats {
  todayAppointments: number; pendingAppointments: number; confirmedAppointments: number;
  totalClients: number; activeBarbers: number; estimatedRevenue: number;
  recentAppointments: RecentAppointment[];
}
export interface RecentAppointment {
  id: number; clientName: string; serviceName: string; barberName?: string;
  appointmentDate: string; appointmentTime: string; status: string;
}
export interface BookingSlot { time: string; available: boolean; }
export interface AvailabilityResponse { closed: boolean; slots: BookingSlot[]; }
export interface BookingResult { bookingToken: string; firstName: string; serviceName: string; appointmentDate: string; appointmentTime: string; barberName?: string; }
export interface BookingStatus {
  token: string; firstName: string; lastName: string; status: string;
  appointmentDate: string; appointmentTime: string; serviceName?: string; barberName?: string;
  notes?: string; createdAt: string; canCancel: boolean;
}

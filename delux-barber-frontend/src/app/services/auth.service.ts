import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private readonly TOKEN_KEY = 'delux_token';
  isLoggedIn$ = signal(this.isLoggedIn());

  login(email: string, password: string) {
    return this.http.post<{ token: string; email: string; role: string; expiresAt: string }>(
      `${environment.apiUrl}/auth/login`, { email, password }
    ).pipe(tap(res => {
      localStorage.setItem(this.TOKEN_KEY, res.token);
      this.isLoggedIn$.set(true);
    }));
  }

  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    this.isLoggedIn$.set(false);
    this.router.navigate(['/admin/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    const token = localStorage.getItem(this.TOKEN_KEY);
    if (!token) return false;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp * 1000 > Date.now();
    } catch { return false; }
  }
}

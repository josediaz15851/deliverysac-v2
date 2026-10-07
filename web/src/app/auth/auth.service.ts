import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { map, tap } from 'rxjs';
import { ConfigService } from '../config/config.service';
import { LoginResponse, SessionUser } from './auth.models';

const STORAGE_KEY = 'deliverysac.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  private readonly _user = signal<SessionUser | null>(readStored());

  readonly user = computed(() => this._user());
  readonly rol = computed(() => this._user()?.rol ?? null);
  readonly isLoggedIn = computed(() => this._user() !== null);

  login(usuario: string, password: string) {
    return this.http
      .post<LoginResponse>(`${this.config.apiUrl}/api/auth/login`, { usuario, password })
      .pipe(
        tap((res) => {
          const session: SessionUser = { usuario, rol: res.rol };
          localStorage.setItem(STORAGE_KEY, JSON.stringify({ ...session, token: res.token }));
          this._user.set(session);
        }),
        map((res) => res.rol)
      );
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this._user.set(null);
  }

  get token(): string | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw).token as string) : null;
    } catch {
      return null;
    }
  }

  homeForRol(rol: string): string {
    switch (rol) {
      case 'REPARTIDOR':
        return '/repartidor';
      case 'SUPERVISOR':
        return '/supervisor';
      default:
        return '/admin';
    }
  }
}

function readStored(): SessionUser | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw);
    return { usuario: parsed.usuario, rol: parsed.rol };
  } catch {
    return null;
  }
}

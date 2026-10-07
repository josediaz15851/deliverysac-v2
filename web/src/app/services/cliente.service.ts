import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from '../config/config.service';

export interface Cliente {
  id: number;
  nombre: string;
  direccion: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  size: number;
  total: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class ClienteService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  get baseUrl(): string {
    return `${this.config.apiUrl}/api/clientes`;
  }

  crear(nombre: string, direccion: string): Observable<Cliente> {
    return this.http.post<Cliente>(this.baseUrl, { nombre, direccion });
  }

  listar(page: number, size = 10): Observable<PagedResult<Cliente>> {
    const params = new HttpParams().set('page', page).set('size', size);
    return this.http.get<PagedResult<Cliente>>(this.baseUrl, { params });
  }
}

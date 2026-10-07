import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from '../config/config.service';
import { PagedResult } from './cliente.service';
import { Producto } from './pedido.service';

@Injectable({ providedIn: 'root' })
export class ProductosService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  get baseUrl(): string {
    return `${this.config.apiUrl}/api/productos`;
  }

  listar(page = 1, size = 100): Observable<PagedResult<Producto>> {
    const params = new HttpParams().set('page', page).set('size', size);
    return this.http.get<PagedResult<Producto>>(this.baseUrl, { params });
  }
}

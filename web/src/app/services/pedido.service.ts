import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ConfigService } from '../config/config.service';
import { PagedResult } from './cliente.service';

export interface LineaPedido {
  id: number;
  productoId: number;
  producto: string;
  cantidad: number;
  precioUnitario: number;
  descripcion: string;
}

export interface Pedido {
  id: number;
  clienteId: number;
  fechaReparto: string;
  comentario: string | null;
  total: number;
  lineas: LineaPedido[];
}

@Injectable({ providedIn: 'root' })
export class PedidoService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  get baseUrl(): string {
    return `${this.config.apiUrl}/api/pedidos`;
  }

  crear(payload: {
    clienteId: number;
    fechaReparto: string;
    comentario?: string;
    lineas: { productoId: number; cantidad: number; precioUnitario: number; descripcion: string }[];
  }): Observable<Pedido> {
    return this.http.post<Pedido>(this.baseUrl, payload);
  }

  obtener(id: number): Observable<Pedido> {
    return this.http.get<Pedido>(`${this.baseUrl}/${id}`);
  }
}

export interface Producto {
  id: number;
  nombre: string;
  precioUnitario: number;
}

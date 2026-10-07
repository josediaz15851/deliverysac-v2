import { Component, OnInit, inject, signal } from '@angular/core';
import { Pedido, PedidoService } from '../../services/pedido.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-pedido-detail',
  standalone: true,
  imports: [],
  template: `
    @if (pedido(); as p) {
      <article class="detalle">
        <h1>Pedido #{{ p.id }}</h1>
        <p><strong>Cliente:</strong> #{{ p.clienteId }}</p>
        <p><strong>Fecha de reparto:</strong> {{ p.fechaReparto }}</p>
        @if (p.comentario) {
          <p><strong>Comentario:</strong> {{ p.comentario }}</p>
        }
        <h2>Líneas</h2>
        <ul>
          @for (l of p.lineas; track l.id) {
            <li>{{ l.cantidad }} × {{ l.precioUnitario }} — {{ l.descripcion }}</li>
          }
        </ul>
        <p class="total"><strong>Total:</strong> {{ p.total }}</p>
      </article>
    } @else {
      <p>Cargando pedido…</p>
    }
  `
})
export class PedidoDetailComponent implements OnInit {
  private readonly pedidos = inject(PedidoService);
  private readonly route = inject(ActivatedRoute);
  readonly pedido = signal<Pedido | null>(null);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.pedidos.obtener(id).subscribe((p) => this.pedido.set(p));
  }
}

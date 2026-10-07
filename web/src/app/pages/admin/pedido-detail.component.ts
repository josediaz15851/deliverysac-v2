import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Pedido, PedidoService } from '../../services/pedido.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-pedido-detail',
  standalone: true,
  imports: [DatePipe],
  template: `
    <section class="page">
      @if (pedido(); as p) {
        <article class="card detalle">
          <header class="detalle-header">
            <h1>Pedido #{{ p.id }}</h1>
            <span class="badge">Total: {{ p.total.toFixed(2) }}</span>
          </header>

          <dl class="detalle-grid">
            <div>
              <dt>Cliente</dt>
              <dd>#{{ p.clienteId }}</dd>
            </div>
            <div>
              <dt>Fecha de reparto</dt>
              <dd>{{ p.fechaReparto | date: 'mediumDate' }}</dd>
            </div>
            @if (p.comentario) {
              <div class="full-width">
                <dt>Comentario</dt>
                <dd>{{ p.comentario }}</dd>
              </div>
            }
          </dl>

          <h2>Líneas del pedido</h2>
          <ul class="lineas">
            @for (l of p.lineas; track l.id) {
              <li>
                <span class="linea-desc">{{ l.descripcion }}</span>
                <span class="linea-cantidad">{{ l.cantidad }} × {{ l.precioUnitario.toFixed(2) }}</span>
                <span class="linea-subtotal font-semibold">{{ (l.cantidad * l.precioUnitario).toFixed(2) }}</span>
              </li>
            }
          </ul>
        </article>
      } @else {
        <p class="text-secondary">Cargando pedido…</p>
      }
    </section>
  `,
  styles: [`
    .page {
      padding: var(--space-4);
      max-width: var(--max-width);
      margin-inline: auto;
    }

    .detalle {
      display: flex;
      flex-direction: column;
      gap: var(--space-5);
    }

    .detalle-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-3);
      flex-wrap: wrap;
    }

    .detalle-header h1 {
      font-size: var(--font-size-2xl);
      font-weight: 800;
    }

    .badge {
      display: inline-flex;
      align-items: center;
      padding: var(--space-2) var(--space-3);
      font-size: var(--font-size-sm);
      font-weight: 700;
      color: var(--color-primary);
      background-color: var(--color-primary-light);
      border-radius: var(--radius-md);
    }

    .detalle h2 {
      font-size: var(--font-size-lg);
      font-weight: 700;
    }

    .detalle-grid {
      display: grid;
      grid-template-columns: 1fr;
      gap: var(--space-4);
    }

    @media (min-width: 640px) {
      .detalle-grid {
        grid-template-columns: repeat(2, 1fr);
      }
    }

    .full-width {
      grid-column: 1 / -1;
    }

    dt {
      font-size: var(--font-size-xs);
      font-weight: 600;
      color: var(--color-text-muted);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    dd {
      margin: var(--space-1) 0 0;
      color: var(--color-text);
    }

    .lineas {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }

    .lineas li {
      display: grid;
      grid-template-columns: 1fr auto auto;
      gap: var(--space-3);
      align-items: center;
      padding: var(--space-3);
      background-color: var(--color-bg);
      border-radius: var(--radius-md);
    }

    .linea-desc {
      font-weight: 500;
    }

    .linea-cantidad {
      font-size: var(--font-size-sm);
      color: var(--color-text-secondary);
      white-space: nowrap;
    }

    .linea-subtotal {
      color: var(--color-primary);
      white-space: nowrap;
    }
  `]
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

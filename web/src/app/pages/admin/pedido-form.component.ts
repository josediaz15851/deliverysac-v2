import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Cliente, ClienteService } from '../../services/cliente.service';
import { Pedido, PedidoService, Producto } from '../../services/pedido.service';
import { ProductosService } from '../../services/productos.service';

interface LineaForm {
  productoId: number | null;
  cantidad: number;
  precioUnitario: number;
  descripcion: string;
}

@Component({
  selector: 'app-pedido-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './pedido-form.component.html',
  styleUrl: './pedido-form.component.css'
})
export class PedidoFormComponent implements OnInit {
  private readonly pedidos = inject(PedidoService);
  private readonly productosService = inject(ProductosService);
  private readonly clientesService = inject(ClienteService);
  private readonly router = inject(Router);

  readonly productos = signal<Producto[]>([]);
  readonly clientes = signal<Cliente[]>([]);
  readonly clienteId = signal<number | null>(null);
  readonly fechaReparto = signal(new Date().toISOString().slice(0, 10));
  readonly comentario = signal('');
  readonly lineas = signal<LineaForm[]>([this.lineaVacia()]);
  readonly errores = signal<string[]>([]);
  readonly guardando = signal(false);
  readonly pedidoCreado = signal<Pedido | null>(null);

  ngOnInit(): void {
    this.productosService.listar().subscribe({
      next: (res) => this.productos.set(res.items)
    });
    this.clientesService.listar(1, 100).subscribe({
      next: (res) => this.clientes.set(res.items)
    });
  }

  private lineaVacia(): LineaForm {
    return { productoId: null, cantidad: 1, precioUnitario: 0, descripcion: '' };
  }

  agregarLinea(): void {
    this.lineas.update((ls) => [...ls, this.lineaVacia()]);
  }

  quitarLinea(index: number): void {
    this.lineas.update((ls) => ls.filter((_, i) => i !== index));
  }

  actualizarLinea(index: number, patch: Partial<LineaForm>): void {
    this.lineas.update((ls) => ls.map((l, i) => (i === index ? { ...l, ...patch } : l)));
    if (patch.productoId) {
      const prod = this.productos().find((p) => p.id === patch.productoId);
      if (prod) this.lineas.update((ls) => ls.map((l, i) => (i === index ? { ...l, precioUnitario: prod.precioUnitario } : l)));
    }
  }

  get totalPreview(): number {
    return this.lineas().reduce((acc, l) => acc + l.cantidad * l.precioUnitario, 0);
  }

  validaciones(): string[] {
    const errs: string[] = [];
    if (!this.clienteId()) errs.push('Debe seleccionar un cliente');
    if (this.lineas().length === 0) errs.push('El pedido debe tener al menos una linea');
    this.lineas().forEach((l, i) => {
      if (!l.productoId) errs.push(`Línea ${i + 1}: seleccione un producto`);
      if (l.cantidad <= 0) errs.push(`Línea ${i + 1}: cantidad debe ser mayor a 0`);
      if (l.precioUnitario < 0) errs.push(`Línea ${i + 1}: precio unitario inválido`);
      if (!l.descripcion.trim()) errs.push(`Línea ${i + 1}: descripción obligatoria`);
    });
    return errs;
  }

  guardar(): void {
    this.errores.set([]);
    const locales = this.validaciones();
    if (locales.length) {
      this.errores.set(locales);
      return;
    }

    this.guardando.set(true);
    this.pedidos
      .crear({
        clienteId: this.clienteId()!,
        fechaReparto: new Date(this.fechaReparto()).toISOString(),
        comentario: this.comentario() || undefined,
        lineas: this.lineas().map((l) => ({
          productoId: l.productoId!,
          cantidad: l.cantidad,
          precioUnitario: l.precioUnitario,
          descripcion: l.descripcion.trim()
        }))
      })
      .subscribe({
        next: (pedido) => {
          this.guardando.set(false);
          this.pedidoCreado.set(pedido);
          this.lineas.set([this.lineaVacia()]);
          this.comentario.set('');
        },
        error: (err) => {
          this.guardando.set(false);
          this.errores.set(
            err.status === 400
              ? Array.isArray(err.error)
                ? err.error
                : ['Datos inválidos en el pedido']
              : ['No se pudo conectar con el servidor']
          );
        }
      });
  }

  verPedido(): void {
    const id = this.pedidoCreado()?.id;
    if (id) this.router.navigate(['/admin/pedidos', id]);
  }
}

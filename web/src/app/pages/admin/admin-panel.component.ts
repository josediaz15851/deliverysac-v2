import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Cliente, ClienteService, PagedResult } from '../../services/cliente.service';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css'
})
export class AdminPanelComponent implements OnInit {
  private readonly clientes = inject(ClienteService);

  readonly nombre = signal('');
  readonly direccion = signal('');
  readonly error = signal('');
  readonly guardando = signal(false);
  readonly pagina = signal(1);
  readonly resultado = signal<PagedResult<Cliente> | null>(null);

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.clientes.listar(this.pagina()).subscribe({
      next: (res) => this.resultado.set(res),
      error: () => this.error.set('No se pudo cargar el listado de clientes')
    });
  }

  irA(pagina: number): void {
    const total = this.resultado()?.totalPages ?? 1;
    if (pagina < 1 || pagina > total) return;
    this.pagina.set(pagina);
    this.cargar();
  }

  guardar(): void {
    this.error.set('');
    this.guardando.set(true);
    this.clientes.crear(this.nombre(), this.direccion()).subscribe({
      next: () => {
        this.guardando.set(false);
        this.nombre.set('');
        this.direccion.set('');
        this.cargar();
      },
      error: (err) => {
        this.guardando.set(false);
        this.error.set(
          err.status === 400 ? 'Nombre y dirección son obligatorios' : 'No se pudo guardar el cliente'
        );
      }
    });
  }
}

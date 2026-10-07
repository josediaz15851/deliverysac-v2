import { Component } from '@angular/core';

@Component({
  selector: 'app-supervisor',
  standalone: true,
  imports: [],
  template: `
    <section class="page">
      <div class="card placeholder">
        <h1>Supervisión del día</h1>
        <p class="text-secondary">Panel de solo lectura para consultar el estado de los pedidos y repartos del día.</p>
        <span class="pronto">Próximamente</span>
      </div>
    </section>
  `,
  styles: [`
    .page {
      padding: var(--space-4);
      max-width: var(--max-width);
      margin-inline: auto;
    }

    .placeholder {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      gap: var(--space-3);
    }

    .placeholder h1 {
      font-size: var(--font-size-2xl);
      font-weight: 800;
    }

    .pronto {
      display: inline-block;
      margin-top: var(--space-2);
      padding: var(--space-1) var(--space-3);
      font-size: var(--font-size-xs);
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--color-warning);
      background-color: var(--color-warning-bg);
      border-radius: var(--radius-md);
    }
  `]
})
export class SupervisorPanelComponent {}

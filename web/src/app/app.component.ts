import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLoggedIn = this.auth.isLoggedIn;
  readonly user = this.auth.user;
  readonly rol = this.auth.rol;

  readonly navItems = computed(() => {
    const rol = this.rol();
    switch (rol) {
      case 'ADMIN':
        return [{ label: 'Panel', path: '/admin' }];
      case 'REPARTIDOR':
        return [{ label: 'Mi ruta', path: '/repartidor' }];
      case 'SUPERVISOR':
        return [{ label: 'Supervisión', path: '/supervisor' }];
      default:
        return [];
    }
  });

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}

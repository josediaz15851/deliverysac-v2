import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  usuario = '';
  password = '';
  readonly error = signal('');
  readonly loading = signal(false);

  submit(): void {
    this.error.set('');
    this.loading.set(true);
    this.auth.login(this.usuario, this.password).subscribe({
      next: (rol) => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        this.router.navigateByUrl(returnUrl ?? this.auth.homeForRol(rol));
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.status === 401 ? 'Usuario o contraseña inválidos' : 'No se pudo conectar con el servidor');
      }
    });
  }
}

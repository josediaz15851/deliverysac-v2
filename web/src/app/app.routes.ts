import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { LoginComponent } from './pages/login/login.component';
import { AdminPanelComponent } from './pages/admin/admin-panel.component';
import { RepartidorRutaComponent } from './pages/repartidor/repartidor-ruta.component';
import { SupervisorPanelComponent } from './pages/supervisor/supervisor-panel.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: 'admin',
    component: AdminPanelComponent,
    canActivate: [authGuard],
    data: { rol: 'ADMIN' }
  },
  {
    path: 'repartidor',
    component: RepartidorRutaComponent,
    canActivate: [authGuard],
    data: { rol: 'REPARTIDOR' }
  },
  {
    path: 'supervisor',
    component: SupervisorPanelComponent,
    canActivate: [authGuard],
    data: { rol: 'SUPERVISOR' }
  },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' }
];

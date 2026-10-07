import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }

  const expected = route.data?.['rol'] as string | undefined;
  if (expected && auth.rol() !== expected) {
    return router.createUrlTree([auth.homeForRol(auth.rol() ?? '')]);
  }

  return true;
};

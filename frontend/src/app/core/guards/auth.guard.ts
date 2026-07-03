import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

/**
 * Blocks routes that require a signed-in user, redirecting anonymous visitors to
 * the login page and preserving where they were headed.
 */
@Injectable({ providedIn: 'root' })
export class AuthGuard {
  constructor(private readonly auth: AuthService, private readonly router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean | UrlTree> {
    return this.auth.isAuthenticated$.pipe(
      take(1),
      map((isAuthenticated) =>
        isAuthenticated
          ? true
          : this.router.createUrlTree(['/auth/login'], {
              queryParams: { returnUrl: route.pathFromRoot.map((r) => r.url).join('/') || '/' },
            })
      )
    );
  }
}

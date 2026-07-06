import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { AuthService } from '@core';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class LoginComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  submitting = false;
  errorMessage: string | null = null;

  private returnUrl = '/';
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
      rememberMe: [true],
    });

    this.returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/';
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSubmit(): void {
    if (this.form.invalid || this.submitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.errorMessage = null;

    this.auth
      .login(this.form.getRawValue())
      .pipe(
        finalize(() => (this.submitting = false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => this.router.navigateByUrl(this.returnUrl),
        error: (err: unknown) => {
          const httpError = err as { status?: number; error?: { title?: string } };

          // 403 = right password, unconfirmed email - finish verification.
          if (httpError.status === 403) {
            this.router.navigate(['/auth/confirm-email'], {
              queryParams: { email: this.form.getRawValue().email, resend: '1' },
            });
            return;
          }

          this.errorMessage = httpError.error?.title ?? 'Invalid email or password.';
        },
      });
  }
}

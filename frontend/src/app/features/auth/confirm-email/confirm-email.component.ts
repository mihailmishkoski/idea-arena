import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, interval } from 'rxjs';
import { finalize, takeUntil, takeWhile } from 'rxjs/operators';
import { AuthService } from '@core';

@Component({
    selector: 'app-confirm-email',
    templateUrl: './confirm-email.component.html',
    styleUrls: ['./confirm-email.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class ConfirmEmailComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  email = '';
  submitting = false;
  errorMessage: string | null = null;
  infoMessage: string | null = null;
  resendCooldown = 0;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';
    if (!this.email) {
      this.router.navigate(['/auth/register']);
      return;
    }

    this.form = this.fb.group({
      code: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
    });

    // Arriving from login means no fresh code exists yet - send one.
    if (this.route.snapshot.queryParamMap.get('resend') === '1') {
      this.onResend();
    } else {
      this.startCooldown();
    }
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
      .confirmEmail({ email: this.email, code: this.form.getRawValue().code })
      .pipe(
        finalize(() => (this.submitting = false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => this.router.navigateByUrl('/'),
        error: (err: unknown) => {
          const problem = err as { error?: { title?: string } };
          this.errorMessage = problem.error?.title ?? 'Invalid or expired code.';
        },
      });
  }

  onResend(): void {
    if (this.resendCooldown > 0) {
      return;
    }

    this.errorMessage = null;
    this.auth
      .resendCode({ email: this.email })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.infoMessage = `A new code is on its way to ${this.email}.`;
          this.startCooldown();
        },
        error: () => (this.errorMessage = 'Could not send the code. Please try again.'),
      });
  }

  private startCooldown(): void {
    this.resendCooldown = 60;
    interval(1000)
      .pipe(
        takeWhile(() => this.resendCooldown > 0),
        takeUntil(this.destroy$)
      )
      .subscribe(() => (this.resendCooldown -= 1));
  }
}

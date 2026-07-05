import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { AVATARS, AuthService, AvatarViewModel, randomAvatarId } from '@core';

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class RegisterComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  submitting = false;
  errorMessage: string | null = null;

  readonly avatars: AvatarViewModel[] = AVATARS;
  selectedAvatarId: string = randomAvatarId();

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      displayName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
    });
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
      .register({ ...this.form.getRawValue(), avatarId: this.selectedAvatarId })
      .pipe(
        finalize(() => (this.submitting = false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => this.router.navigateByUrl('/'),
        error: (err) =>
          (this.errorMessage =
            this.extractError(err) ?? 'Could not create your account. Please try again.'),
      });
  }

  private extractError(err: unknown): string | null {
    const problem = (err as { error?: { errors?: Record<string, string[]> } })?.error;
    if (problem?.errors) {
      const first = Object.values(problem.errors)[0];
      return Array.isArray(first) ? first[0] : null;
    }
    return null;
  }
}

import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { CreateIdeaRequest, IdeasApiService, BusinessIdeaCategory } from '@core';

@Component({
    selector: 'app-idea-create',
    templateUrl: './idea-create.component.html',
    styleUrls: ['./idea-create.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class IdeaCreateComponent implements OnInit, OnDestroy {
  form!: FormGroup;
  submitting = false;
  errorMessage: string | null = null;

  readonly maxCategories = 3;
  readonly allCategories = Object.values(BusinessIdeaCategory).filter(
    (v) => typeof v === 'number'
  ) as BusinessIdeaCategory[];
  selectedCategories: BusinessIdeaCategory[] = [];
  categoriesTouched = false;

  private readonly urlPattern = /^https?:\/\/.+/i;
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly ideasService: IdeasApiService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(150)]],
      uniqueValueProposition: ['', [Validators.required, Validators.maxLength(1000)]],
      problem: ['', [Validators.required, Validators.maxLength(2000)]],
      solution: ['', [Validators.required, Validators.maxLength(2000)]],
      competition: ['', [Validators.maxLength(2000)]],
      incomeStrategy: ['', [Validators.maxLength(2000)]],
      exitStrategy: ['', [Validators.maxLength(2000)]],
      videoPitchUrl: ['', [Validators.pattern(this.urlPattern), Validators.maxLength(2048)]],
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  categoryLabel(category: BusinessIdeaCategory): string {
    return BusinessIdeaCategory[category];
  }

  isCategorySelected(category: BusinessIdeaCategory): boolean {
    return this.selectedCategories.includes(category);
  }

  isCategoryDisabled(category: BusinessIdeaCategory): boolean {
    return (
      !this.isCategorySelected(category) &&
      this.selectedCategories.length >= this.maxCategories
    );
  }

  toggleCategory(category: BusinessIdeaCategory): void {
    this.categoriesTouched = true;
    if (this.isCategorySelected(category)) {
      this.selectedCategories = this.selectedCategories.filter((c) => c !== category);
    } else if (this.selectedCategories.length < this.maxCategories) {
      this.selectedCategories = [...this.selectedCategories, category];
    }
  }

  onSubmit(): void {
    this.categoriesTouched = true;

    if (this.form.invalid || this.selectedCategories.length === 0 || this.submitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.errorMessage = null;

    this.ideasService
      .createIdea(this.toRequest())
      .pipe(
        finalize(() => (this.submitting = false)),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (id) => this.router.navigate(['/ideas', id]),
        error: () => (this.errorMessage = 'Could not publish your idea. Please try again.'),
      });
  }

  private toRequest(): CreateIdeaRequest {
    const v = this.form.getRawValue();
    const trimOrNull = (s: string): string | null => (s && s.trim() ? s.trim() : null);

    return {
      name: v.name.trim(),
      uniqueValueProposition: v.uniqueValueProposition.trim(),
      problem: v.problem.trim(),
      solution: v.solution.trim(),
      competition: trimOrNull(v.competition),
      incomeStrategy: trimOrNull(v.incomeStrategy),
      exitStrategy: trimOrNull(v.exitStrategy),
      videoPitchUrl: trimOrNull(v.videoPitchUrl),
      categories: this.selectedCategories,
    };
  }
}

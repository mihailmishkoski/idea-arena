import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { CreateIdeaRequest, IdeasApiService } from '@core';
import { BusinessIdeaDetailViewModel } from '@core';
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
  ideaId: string | null = null;
  idea: BusinessIdeaDetailViewModel | null = null;
  isEditMode = false;
  private readonly urlPattern = /^https?:\/\/.+/i;
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly ideasService: IdeasApiService,
    private readonly router: Router,
    private readonly route: ActivatedRoute

  ) {
    
  }
 ngOnInit(): void {

  this.form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    uniqueValueProposition: ['', [Validators.required, Validators.maxLength(1000)]],
    problem: ['', [Validators.required, Validators.maxLength(2000)]],
    solution: ['', [Validators.required, Validators.maxLength(2000)]],
    competition: ['', [Validators.maxLength(2000)]],
    incomeStrategy: ['', [Validators.maxLength(2000)]],
    exitStrategy: ['', [Validators.maxLength(2000)]],
    videoPitchUrl: ['', [
       Validators.pattern(this.urlPattern),
       Validators.maxLength(2048)
    ]],
  });


  this.ideaId = this.route.snapshot.paramMap.get('id');


  if(this.ideaId){
    this.isEditMode = true;

    this.idea = history.state.idea;

    if(this.idea){
       this.form.patchValue({
          name: this.idea.name,
          uniqueValueProposition: this.idea.uniqueValueProposition,
          problem: this.idea.problem,
          solution: this.idea.solution,
          competition: this.idea.competition,
          incomeStrategy: this.idea.incomeStrategy,
          exitStrategy: this.idea.exitStrategy,
          videoPitchUrl: this.idea.videoPitchUrl
       });
    }
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
if (this.isEditMode && this.ideaId) {

  this.ideasService.updateIdea(
    this.ideaId,
    this.toRequest()
  )
  .pipe(
    finalize(() => this.submitting = false),
    takeUntil(this.destroy$)
  )
  .subscribe({
    next: () => this.router.navigate(['/ideas', this.ideaId]),
    error: () => this.errorMessage = 'Could not update idea.'
  });

}
else {

  this.ideasService.createIdea(this.toRequest())
  .pipe(
    finalize(() => this.submitting = false),
    takeUntil(this.destroy$)
  )
  .subscribe({
    next: (id) => this.router.navigate(['/ideas', id]),
    error: () => this.errorMessage = 'Could not create idea.'
  });

}
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
    };
  }
}

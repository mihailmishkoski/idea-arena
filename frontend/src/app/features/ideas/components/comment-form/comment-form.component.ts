import { Component, EventEmitter, Input, OnInit, Output, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CreateCommentRequest, IdeaMetric, METRIC_LIST, MetricInfoViewModel } from '@core';

/**
 * Composer for a top-level comment. The user picks the topic by selecting one of
 * the coloured badges (instead of a dropdown) and the comment is anchored to it.
 */
@Component({
    selector: 'app-comment-form',
    templateUrl: './comment-form.component.html',
    styleUrls: ['./comment-form.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class CommentFormComponent implements OnInit {
  @Input() submitting = false;
  @Output() submitComment = new EventEmitter<CreateCommentRequest>();

  form!: FormGroup;
  selectedMetric: IdeaMetric = IdeaMetric.General;

  readonly metrics: MetricInfoViewModel[] = METRIC_LIST;

  constructor(private readonly fb: FormBuilder) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      content: ['', [Validators.required, Validators.maxLength(5000)]],
    });
  }

  selectMetric(metric: IdeaMetric): void {
    this.selectedMetric = metric;
  }

  onSubmit(): void {
    if (this.form.invalid || this.submitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitComment.emit({
      content: this.form.getRawValue().content,
      targetMetric: this.selectedMetric,
    });

    this.form.reset({ content: '' });
    this.selectedMetric = IdeaMetric.General;
  }
}

import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { METRIC_INFO } from '../../../core/metric-info';
import { IdeaMetric } from '../../../core/models/enums';

/** A coloured pill showing which topic a comment is anchored to. */
@Component({
    selector: 'app-metric-badge',
    templateUrl: './metric-badge.component.html',
    styleUrls: ['./metric-badge.component.scss'],
    changeDetection: ChangeDetectionStrategy.Eager,
    standalone: false
})
export class MetricBadgeComponent {
  @Input() metric: IdeaMetric = IdeaMetric.General;

  get info() {
    return METRIC_INFO[this.metric] ?? METRIC_INFO[IdeaMetric.General];
  }
}

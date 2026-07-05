import { Pipe, PipeTransform } from '@angular/core';
import { IdeaMetric } from '@core';

const LABELS: Record<IdeaMetric, string> = {
  [IdeaMetric.General]: 'General',
  [IdeaMetric.UniqueValueProposition]: 'Unique Value Proposition',
  [IdeaMetric.Problem]: 'Problem',
  [IdeaMetric.Solution]: 'Solution',
  [IdeaMetric.Competition]: 'Competition',
  [IdeaMetric.IncomeStrategy]: 'Income Strategy',
  [IdeaMetric.ExitStrategy]: 'Exit Strategy',
  [IdeaMetric.VideoPitch]: 'Video Pitch',
};

/** Turns an IdeaMetric enum value into a readable label. */
@Pipe({
    name: 'metricLabel',
    standalone: false
})
export class MetricLabelPipe implements PipeTransform {
  transform(value: IdeaMetric): string {
    return LABELS[value] ?? 'General';
  }
}

import { IdeaMetric } from '../enums';

export interface MetricInfoViewModel {
  metric: IdeaMetric;
  label: string;
  color: string;
  background: string;
}

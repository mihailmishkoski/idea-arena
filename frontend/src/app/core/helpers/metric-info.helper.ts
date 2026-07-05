import { IdeaMetric } from '../models/enums';
import { MetricInfoViewModel } from '../models/view-models';

export const METRIC_INFO: Record<IdeaMetric, MetricInfoViewModel> = {
  [IdeaMetric.General]: {
    metric: IdeaMetric.General,
    label: 'General',
    color: '#4a4a4b',
    background: '#e9ebee',
  },
  [IdeaMetric.UniqueValueProposition]: {
    metric: IdeaMetric.UniqueValueProposition,
    label: 'Unique Value',
    color: '#7a3ea1',
    background: '#f2e8fb',
  },
  [IdeaMetric.Problem]: {
    metric: IdeaMetric.Problem,
    label: 'Problem',
    color: '#c02434',
    background: '#fde7e9',
  },
  [IdeaMetric.Solution]: {
    metric: IdeaMetric.Solution,
    label: 'Solution',
    color: '#1a7f4b',
    background: '#e3f6ec',
  },
  [IdeaMetric.Competition]: {
    metric: IdeaMetric.Competition,
    label: 'Competition',
    color: '#b45309',
    background: '#fdf0dc',
  },
  [IdeaMetric.IncomeStrategy]: {
    metric: IdeaMetric.IncomeStrategy,
    label: 'Income',
    color: '#0f766e',
    background: '#dbf4f1',
  },
  [IdeaMetric.ExitStrategy]: {
    metric: IdeaMetric.ExitStrategy,
    label: 'Exit',
    color: '#1d4ed8',
    background: '#e4ecfd',
  },
  [IdeaMetric.VideoPitch]: {
    metric: IdeaMetric.VideoPitch,
    label: 'Video',
    color: '#be185d',
    background: '#fce7f0',
  },
};

export const METRIC_LIST: MetricInfoViewModel[] = [
  METRIC_INFO[IdeaMetric.General],
  METRIC_INFO[IdeaMetric.UniqueValueProposition],
  METRIC_INFO[IdeaMetric.Problem],
  METRIC_INFO[IdeaMetric.Solution],
  METRIC_INFO[IdeaMetric.Competition],
  METRIC_INFO[IdeaMetric.IncomeStrategy],
  METRIC_INFO[IdeaMetric.ExitStrategy],
  METRIC_INFO[IdeaMetric.VideoPitch],
];

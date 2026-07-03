/**
 * Mirrors the backend enums. Numeric values must match the C# definitions so
 * they serialize correctly over the wire.
 */

export enum VoteDirection {
  Down = -1,
  Up = 1,
}

export enum IdeaMetric {
  General = 0,
  UniqueValueProposition = 1,
  Problem = 2,
  Solution = 3,
  Competition = 4,
  IncomeStrategy = 5,
  ExitStrategy = 6,
  VideoPitch = 7,
}

export enum IdeaSortOrder {
  Top = 0,
  New = 1,
  Best = 2,
  Controversial = 3,
  Old = 4,
  Winners = 5,
}

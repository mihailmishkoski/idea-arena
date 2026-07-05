export const IDEA_LIFETIME_DAYS = 14;

export const HOT_SCORE_THRESHOLD = 10;

export function ideaExpiresAt(createdAtUtc: string): Date {
  const created = new Date(createdAtUtc);
  return new Date(created.getTime() + IDEA_LIFETIME_DAYS * 24 * 60 * 60 * 1000);
}

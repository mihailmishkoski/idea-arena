/**
 * Client-side mirror of the backend idea "competition" rules. An idea is active
 * for this many days after it is posted; afterwards it leaves the feed and
 * competes for the Winners board. Expiry is derived from the post's created
 * date so we don't need an extra field over the wire.
 */
export const IDEA_LIFETIME_DAYS = 14;

/** Score at or above which an active idea is considered "hot" and styled specially. */
export const HOT_SCORE_THRESHOLD = 10;

export function ideaExpiresAt(createdAtUtc: string): Date {
  const created = new Date(createdAtUtc);
  return new Date(created.getTime() + IDEA_LIFETIME_DAYS * 24 * 60 * 60 * 1000);
}

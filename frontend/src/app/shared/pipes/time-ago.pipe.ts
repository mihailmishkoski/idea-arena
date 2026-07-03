import { Pipe, PipeTransform } from '@angular/core';

/** Formats an ISO timestamp as a compact Reddit-style relative time ("5h ago"). */
@Pipe({ name: 'timeAgo' })
export class TimeAgoPipe implements PipeTransform {
  transform(value: string | Date | null | undefined): string {
    if (!value) {
      return '';
    }

    const date = typeof value === 'string' ? new Date(value) : value;
    const seconds = Math.floor((Date.now() - date.getTime()) / 1000);

    if (seconds < 60) {
      return 'just now';
    }

    const units: [number, string][] = [
      [60, 'm'],
      [3600, 'h'],
      [86400, 'd'],
      [2592000, 'mo'],
      [31536000, 'y'],
    ];

    let previousLimit = 1;
    for (const [limit, label] of units) {
      if (seconds < limit) {
        return `${Math.floor(seconds / previousLimit)}${label} ago`;
      }
      previousLimit = limit;
    }

    return `${Math.floor(seconds / 31536000)}y ago`;
  }
}

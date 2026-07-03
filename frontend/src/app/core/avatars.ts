/**
 * The app's built-in avatar collection. Users pick one at registration (or later
 * from the avatar menu); only the id is stored server-side. Ids must stay in
 * sync with the backend seeder's list.
 */
export interface AvatarInfo {
  id: string;
  emoji: string;
  background: string;
}

export const AVATARS: AvatarInfo[] = [
  { id: 'rocket', emoji: '🚀', background: '#e0e7ff' },
  { id: 'bulb', emoji: '💡', background: '#fef3c7' },
  { id: 'fox', emoji: '🦊', background: '#ffedd5' },
  { id: 'panda', emoji: '🐼', background: '#e5e7eb' },
  { id: 'robot', emoji: '🤖', background: '#dbeafe' },
  { id: 'alien', emoji: '👽', background: '#dcfce7' },
  { id: 'cat', emoji: '🐱', background: '#fce7f3' },
  { id: 'owl', emoji: '🦉', background: '#ede9fe' },
  { id: 'tiger', emoji: '🐯', background: '#fef9c3' },
  { id: 'koala', emoji: '🐨', background: '#f3f4f6' },
  { id: 'dragon', emoji: '🐲', background: '#d1fae5' },
  { id: 'unicorn', emoji: '🦄', background: '#fae8ff' },
  { id: 'bear', emoji: '🐻', background: '#fde68a' },
  { id: 'penguin', emoji: '🐧', background: '#e0f2fe' },
  { id: 'frog', emoji: '🐸', background: '#d9f99d' },
  { id: 'octopus', emoji: '🐙', background: '#ffe4e6' },
];

export const FALLBACK_AVATAR: AvatarInfo = { id: 'default', emoji: '👤', background: '#e5e7eb' };

export function avatarById(id: string | null | undefined): AvatarInfo {
  return AVATARS.find((a) => a.id === id) ?? FALLBACK_AVATAR;
}

export function randomAvatarId(): string {
  return AVATARS[Math.floor(Math.random() * AVATARS.length)].id;
}

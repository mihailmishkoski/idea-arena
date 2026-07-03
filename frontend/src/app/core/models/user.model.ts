export interface CurrentUser {
  id: string;
  email: string;
  displayName: string | null;
  avatarId: string | null;
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface RegisterRequest {
  displayName: string;
  email: string;
  password: string;
  avatarId: string;
}

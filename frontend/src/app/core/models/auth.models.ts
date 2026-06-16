export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  access_token: string;
  expires_at: number;
  token_type: string;
}

export interface SetupRequest {
  email: string;
  password: string;
  confirmPassword: string;
}

export interface RefreshResult {
  token: string;
  expiresAt: Date;
}

export interface TokenPayload {
  sub: string;
  email: string;
  role: string;
  exp: number;
}

export interface UserInfo {
  id: string;
  email: string;
  roles: string[];
}

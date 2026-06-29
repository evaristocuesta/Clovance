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
  firstName: string;
  lastName: string;
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
  firstName: string;
  lastName: string;
  email: string;
  roles: string[];
}

export interface CreateInvitationCommand {
  email: string;
  isAdmin: boolean;
}

export interface CreateInvitationResult {
  id: string;
  email: string;
  expiresAt: Date;
  token: string;
}

export interface RegisterWithInvitationRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  token: string;
}

export interface RegisterWithInvitationResult {
  id: string;
  email: string;
}

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
}

export interface UpdateUserResult {
  token: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

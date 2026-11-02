export type UserRole = 'Admin' | 'Manager' | 'Employee';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  message: string;
  token: string;
  userId: number;
  username: string;
  role: UserRole;
}

export interface CreateUserRequest {
  username: string;
  password: string;
  role: UserRole;
  employeeId?: number | null;
}

export interface CreateUserResponse {
  message: string;
  userId: number;
  username: string;
  role: UserRole;
  employeeId: number | null;
}

export interface UserListItem {
  userId: number;
  username: string;
  role: UserRole;
  isActive: boolean;
  employeeId: number | null;
}
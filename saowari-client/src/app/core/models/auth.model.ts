export interface UserModel {
  userId: number;
  fullName: string;
  email: string;
  phone: string;
  picture?: string;
  roleId: number;
  roleName?: string;
  driverInformtionId?: number;
  supervisorId?: number;
  companyId?: number;
  isActive: boolean;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: UserModel;
}

export interface LoginDto {
  email: string;
  password?: string;
  referrer?: string;
}

export interface RegisterDto {
  fullName: string;
  email: string;
  phone: string;
  password?: string;
  confirmPassword?: string;
  roleId: number;
  companyId?: number;
}

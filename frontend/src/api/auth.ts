import { apiClient } from './client';

export interface LoginRequest {
  tenDangNhap: string;
  matKhau: string;
}

export interface UserInfo {
  id?: number | string;
  tenDangNhap: string;
  hoTen: string;
  vaiTro: string;
}

export interface LoginResponse {
  token: string;
  user?: UserInfo;
  hoTen?: string;
  vaiTro?: string;
  tenDangNhap?: string;
  message?: string;
  isLocked?: boolean;
  remainingAttempts?: number;
}

export const authApi = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
    return response.data;
  },
  logout: async (): Promise<void> => {
    try {
      await apiClient.post('/auth/logout');
    } catch {
      // Ignore network errors on logout
    }
  },
};

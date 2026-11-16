import React, { useState, useEffect } from 'react';
import { authApi } from '../api/auth';
import type { LoginRequest, LoginResponse, UserInfo } from '../api/auth';
import { AuthContext } from './authContextValue';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('token'));
  const [user, setUser] = useState<UserInfo | null>(() => {
    const savedUser = localStorage.getItem('user');
    if (savedUser) {
      try {
        return JSON.parse(savedUser) as UserInfo;
      } catch {
        return null;
      }
    }
    return null;
  });

  useEffect(() => {
    if (token) {
      localStorage.setItem('token', token);
    } else {
      localStorage.removeItem('token');
    }
  }, [token]);

  useEffect(() => {
    if (user) {
      localStorage.setItem('user', JSON.stringify(user));
    } else {
      localStorage.removeItem('user');
    }
  }, [user]);

  const login = async (credentials: LoginRequest): Promise<LoginResponse> => {
    const data = await authApi.login(credentials);
    const authToken = data.token;

    // Resolve user data structure whether nested or flat
    const userObj = data.user;
    const resolvedUser: UserInfo = userObj
      ? userObj
      : {
          id: 1,
          tenDangNhap: data.tenDangNhap || credentials.tenDangNhap,
          hoTen: data.hoTen || credentials.tenDangNhap,
          vaiTro: data.vaiTro || 'Nhân viên',
        };

    setToken(authToken);
    setUser(resolvedUser);
    return data;
  };

  const logout = () => {
    authApi.logout();
    setToken(null);
    setUser(null);
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isAuthenticated: !!token && !!user,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

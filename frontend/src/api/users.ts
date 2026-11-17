import { apiClient } from './client';
import { isAxiosError } from 'axios';

export interface User {
  id: number;
  tenDangNhap: string;
  hoTen: string;
  soDienThoai: string;
  vaiTroId: number;
  tenVaiTro: string;
  trangThai: 'HoatDong' | 'Khoa' | string;
  soLanDangNhapSai?: number;
}

export interface Role {
  id: number;
  tenVaiTro: string;
  moTa?: string;
}

export interface CreateUserRequest {
  tenDangNhap: string;
  matKhau: string;
  hoTen: string;
  soDienThoai: string;
  vaiTroId: number;
}

export interface UpdateUserRequest {
  hoTen: string;
  soDienThoai: string;
  vaiTroId: number;
}

export interface GetUsersParams {
  keyword?: string;
  vaiTroId?: number;
  trangThai?: string;
}

// Fallback seed data if backend is offline
export const SEED_USERS_FALLBACK: User[] = [
  {
    id: 1,
    tenDangNhap: 'admin',
    hoTen: 'Quản Lý Cửa Hàng',
    soDienThoai: '0909888999',
    vaiTroId: 1,
    tenVaiTro: 'QuanLy',
    trangThai: 'HoatDong',
    soLanDangNhapSai: 0,
  },
  {
    id: 2,
    tenDangNhap: 'tranthuha',
    hoTen: 'Trần Thu Hà',
    soDienThoai: '0901234567',
    vaiTroId: 2,
    tenVaiTro: 'NhanVien',
    trangThai: 'HoatDong',
    soLanDangNhapSai: 0,
  },
  {
    id: 3,
    tenDangNhap: 'nguyenvanb',
    hoTen: 'Nguyễn Văn Bình',
    soDienThoai: '0987654321',
    vaiTroId: 2,
    tenVaiTro: 'NhanVien',
    trangThai: 'HoatDong',
    soLanDangNhapSai: 0,
  },
  {
    id: 4,
    tenDangNhap: 'lethithao',
    hoTen: 'Lê Thị Thảo',
    soDienThoai: '0912345678',
    vaiTroId: 2,
    tenVaiTro: 'NhanVien',
    trangThai: 'Khoa',
    soLanDangNhapSai: 5,
  },
];

export const SEED_ROLES_FALLBACK: Role[] = [
  { id: 1, tenVaiTro: 'QuanLy', moTa: 'Toàn quyền quản lý hệ thống, nhân sự, kho và báo cáo' },
  { id: 2, tenVaiTro: 'NhanVien', moTa: 'Bán hàng, quản lý đơn hàng và thu chi theo ca' },
];

let localUsersStore = [...SEED_USERS_FALLBACK];

const normalizeText = (text: string) => {
  return text
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .trim();
};

export const getUsers = async (params?: GetUsersParams): Promise<User[]> => {
  try {
    const queryParams: Record<string, string | number> = {};
    if (params?.keyword && params.keyword.trim()) {
      queryParams.keyword = params.keyword.trim();
    }
    if (params?.vaiTroId && params.vaiTroId > 0) {
      queryParams.vaiTroId = params.vaiTroId;
    }
    if (params?.trangThai && params.trangThai !== 'TatCa' && params.trangThai !== 'All') {
      queryParams.trangThai = params.trangThai;
    }

    const response = await apiClient.get<User[]>('/users', { params: queryParams });
    if (response.data && Array.isArray(response.data)) {
      return response.data;
    }
  } catch (error: unknown) {
    if (isAxiosError(error) && (error.response?.status === 401 || error.response?.status === 403)) {
      throw error;
    }
  }

  // Fallback to local store for offline/demo
  let result = [...localUsersStore];

  if (params?.keyword && params.keyword.trim()) {
    const q = normalizeText(params.keyword);
    result = result.filter(
      (u) =>
        normalizeText(u.hoTen).includes(q) ||
        normalizeText(u.tenDangNhap).includes(q) ||
        u.soDienThoai.includes(params.keyword!.trim())
    );
  }

  if (params?.vaiTroId && params.vaiTroId > 0) {
    result = result.filter((u) => u.vaiTroId === params.vaiTroId);
  }

  if (params?.trangThai && params.trangThai !== 'TatCa' && params.trangThai !== 'All') {
    result = result.filter((u) => u.trangThai === params.trangThai);
  }

  return result;
};

export const getRoles = async (): Promise<Role[]> => {
  try {
    const response = await apiClient.get<Role[]>('/users/roles');
    if (response.data && Array.isArray(response.data) && response.data.length > 0) {
      return response.data;
    }
  } catch (error: unknown) {
    if (isAxiosError(error) && (error.response?.status === 401 || error.response?.status === 403)) {
      throw error;
    }
  }

  return SEED_ROLES_FALLBACK;
};

export const createUser = async (data: CreateUserRequest): Promise<User> => {
  try {
    const response = await apiClient.post<User>('/users', data);
    return response.data;
  } catch (error: unknown) {
    if (isAxiosError<{ message?: string; title?: string }>(error)) {
      if (error.response?.data?.message) {
        throw new Error(error.response.data.message, { cause: error });
      }
      if (error.response?.status === 409) {
        throw new Error(`Tên đăng nhập '${data.tenDangNhap}' đã tồn tại trong hệ thống.`, { cause: error });
      }
      if (error.response?.status === 400) {
        throw new Error(error.response.data?.title || 'Dữ liệu không hợp lệ.', { cause: error });
      }
    }

    // Fallback if backend offline
    if (error && typeof error === 'object' && !('response' in error)) {
      const isDuplicate = localUsersStore.some(
        (u) => u.tenDangNhap.toLowerCase() === data.tenDangNhap.trim().toLowerCase()
      );
      if (isDuplicate) {
        throw new Error(`Tên đăng nhập '${data.tenDangNhap}' đã tồn tại trong hệ thống.`, { cause: error });
      }
      const roleName = data.vaiTroId === 1 ? 'QuanLy' : 'NhanVien';
      const newUser: User = {
        id: Date.now(),
        tenDangNhap: data.tenDangNhap.trim(),
        hoTen: data.hoTen.trim(),
        soDienThoai: data.soDienThoai.trim(),
        vaiTroId: data.vaiTroId,
        tenVaiTro: roleName,
        trangThai: 'HoatDong',
        soLanDangNhapSai: 0,
      };
      localUsersStore = [newUser, ...localUsersStore];
      return newUser;
    }
    throw new Error('Không thể tạo người dùng mới. Vui lòng thử lại sau.', { cause: error });
  }
};

export const updateUser = async (id: number, data: UpdateUserRequest): Promise<User> => {
  try {
    const response = await apiClient.put<User>(`/users/${id}`, data);
    return response.data;
  } catch (error: unknown) {
    if (isAxiosError<{ message?: string }>(error) && error.response?.data?.message) {
      throw new Error(error.response.data.message, { cause: error });
    }

    // Fallback if backend offline
    if (error && typeof error === 'object' && !('response' in error)) {
      const idx = localUsersStore.findIndex((u) => u.id === id);
      if (idx === -1) throw new Error('Không tìm thấy người dùng.', { cause: error });

      const roleName = data.vaiTroId === 1 ? 'QuanLy' : 'NhanVien';
      const updatedUser: User = {
        ...localUsersStore[idx],
        hoTen: data.hoTen.trim(),
        soDienThoai: data.soDienThoai.trim(),
        vaiTroId: data.vaiTroId,
        tenVaiTro: roleName,
      };
      localUsersStore[idx] = updatedUser;
      return updatedUser;
    }
    throw new Error('Không thể cập nhật người dùng. Vui lòng thử lại sau.', { cause: error });
  }
};

export const toggleUserStatus = async (id: number, targetStatus?: string): Promise<User> => {
  try {
    const response = await apiClient.patch<User>(
      `/users/${id}/status`,
      targetStatus ? { trangThai: targetStatus } : {}
    );
    return response.data;
  } catch (error: unknown) {
    if (isAxiosError<{ message?: string }>(error) && error.response?.data?.message) {
      throw new Error(error.response.data.message, { cause: error });
    }

    // Fallback if backend offline
    if (error && typeof error === 'object' && !('response' in error)) {
      const idx = localUsersStore.findIndex((u) => u.id === id);
      if (idx === -1) throw new Error('Không tìm thấy người dùng.', { cause: error });

      const currentUser = localUsersStore[idx];
      const nextStatus = targetStatus || (currentUser.trangThai === 'HoatDong' ? 'Khoa' : 'HoatDong');

      // Rule: cannot lock last manager
      if (currentUser.tenVaiTro === 'QuanLy' && nextStatus === 'Khoa') {
        const activeManagers = localUsersStore.filter(
          (u) => u.id !== id && u.tenVaiTro === 'QuanLy' && u.trangThai === 'HoatDong'
        );
        if (activeManagers.length === 0) {
          throw new Error('Không thể khóa tài khoản Quản lý đang hoạt động cuối cùng của hệ thống.', { cause: error });
        }
      }

      const updatedUser: User = {
        ...currentUser,
        trangThai: nextStatus,
        soLanDangNhapSai: nextStatus === 'HoatDong' ? 0 : currentUser.soLanDangNhapSai,
      };
      localUsersStore[idx] = updatedUser;
      return updatedUser;
    }
    throw new Error('Không thể thay đổi trạng thái tài khoản. Vui lòng thử lại sau.', { cause: error });
  }
};

export const deleteUser = async (id: number): Promise<{ message: string }> => {
  try {
    const response = await apiClient.delete<{ message: string }>(`/users/${id}`);
    return response.data;
  } catch (error: unknown) {
    if (isAxiosError<{ message?: string }>(error) && error.response?.data?.message) {
      throw new Error(error.response.data.message, { cause: error });
    }

    // Fallback if backend offline
    if (error && typeof error === 'object' && !('response' in error)) {
      const idx = localUsersStore.findIndex((u) => u.id === id);
      if (idx === -1) throw new Error('Không tìm thấy người dùng.', { cause: error });

      const currentUser = localUsersStore[idx];
      if (currentUser.tenVaiTro === 'QuanLy') {
        const activeManagers = localUsersStore.filter(
          (u) => u.id !== id && u.tenVaiTro === 'QuanLy' && u.trangThai === 'HoatDong'
        );
        if (activeManagers.length === 0) {
          throw new Error('Không thể xóa tài khoản Quản lý đang hoạt động cuối cùng của hệ thống.', { cause: error });
        }
      }

      localUsersStore = localUsersStore.filter((u) => u.id !== id);
      return { message: 'Xóa tài khoản người dùng thành công.' };
    }
    throw new Error('Không thể xóa người dùng. Vui lòng thử lại sau.', { cause: error });
  }
};

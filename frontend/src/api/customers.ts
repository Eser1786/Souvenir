import { apiClient } from './client';

export interface Customer {
  id: number;
  soDienThoai: string;
  tenKhachHang: string;
  diemTichLuy: number;
  hangThanhVien?: string;
}

export interface GetCustomersParams {
  search?: string;
  phone?: string;
}

export interface CreateCustomerData {
  soDienThoai: string;
  tenKhachHang: string;
}

export interface UpdateCustomerData {
  soDienThoai: string;
  tenKhachHang: string;
  diemTichLuy: number;
}

export interface CustomerOrderItem {
  sanPhamId: number;
  maSP: string;
  tenSP: string;
  soLuong: number;
  donGia: number;
  thanhTien: number;
}

export interface CustomerOrder {
  id: number;
  maDonHang: string;
  ngayTao: string;
  tongTienHang: number;
  soTienGiam: number;
  tongTienThanhToan: number;
  trangThai: string;
  tenHinhThucThanhToan: string;
  maGiamGia?: string | null;
  tenNhanVien: string;
  soLuongSanPham: number;
  items: CustomerOrderItem[];
}

// Fallback customers in case backend is loading or offline
export const FALLBACK_CUSTOMERS: Customer[] = [
  {
    id: 1,
    soDienThoai: '0901234567',
    tenKhachHang: 'Nguyễn Ngọc Lan',
    diemTichLuy: 250,
    hangThanhVien: 'Vàng',
  },
  {
    id: 2,
    soDienThoai: '0987654321',
    tenKhachHang: 'Trần Văn An',
    diemTichLuy: 120,
    hangThanhVien: 'Bạc',
  },
  {
    id: 3,
    soDienThoai: '0912345678',
    tenKhachHang: 'Lê Thị Mai',
    diemTichLuy: 50,
    hangThanhVien: 'Bạc',
  },
  {
    id: 4,
    soDienThoai: '0933456789',
    tenKhachHang: 'Phạm Hoàng Nam',
    diemTichLuy: 520,
    hangThanhVien: 'Kim Cương',
  },
  {
    id: 5,
    soDienThoai: '0977889900',
    tenKhachHang: 'Đỗ Phương Thảo',
    diemTichLuy: 15,
    hangThanhVien: 'Đồng',
  },
];

const normalize = (text: string) =>
  text.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().trim();

export const getCustomers = async (params?: GetCustomersParams): Promise<Customer[]> => {
  try {
    const queryParams: Record<string, string> = {};
    if (params?.search && params.search.trim()) {
      queryParams.search = params.search.trim();
    }
    if (params?.phone && params.phone.trim()) {
      queryParams.phone = params.phone.trim();
    }

    const response = await apiClient.get<Customer[]>('/customers', { params: queryParams });
    if (response.data && Array.isArray(response.data)) {
      return response.data;
    }
  } catch {
    // If backend endpoint is not reachable, use client fallback
  }

  // Client-side fallback
  let list = [...FALLBACK_CUSTOMERS];
  if (params?.phone) {
    const cleanPhone = params.phone.replace(/\D/g, '');
    list = list.filter((c) => c.soDienThoai.includes(cleanPhone));
  }
  if (params?.search) {
    const query = normalize(params.search);
    const cleanDigits = params.search.replace(/\D/g, '');
    list = list.filter(
      (c) =>
        normalize(c.tenKhachHang).includes(query) ||
        (cleanDigits && c.soDienThoai.includes(cleanDigits))
    );
  }
  return list;
};

export const findCustomerByPhone = async (phone: string): Promise<Customer | null> => {
  const cleanPhone = phone.replace(/\D/g, '');
  if (!cleanPhone) return null;

  try {
    const customers = await getCustomers({ phone: cleanPhone });
    const match = customers.find((c) => c.soDienThoai === cleanPhone);
    return match || (customers.length > 0 ? customers[0] : null);
  } catch {
    const fallback = FALLBACK_CUSTOMERS.find((c) => c.soDienThoai === cleanPhone);
    return fallback || null;
  }
};

export const getCustomerById = async (id: number): Promise<Customer> => {
  try {
    const res = await apiClient.get<Customer>(`/customers/${id}`);
    return res.data;
  } catch {
    const fallback = FALLBACK_CUSTOMERS.find((c) => c.id === id);
    if (fallback) return fallback;
    throw new Error(`Không tìm thấy khách hàng với ID #${id}`);
  }
};

export const createCustomer = async (data: CreateCustomerData): Promise<Customer> => {
  const res = await apiClient.post<Customer>('/customers', data);
  return res.data;
};

export const updateCustomer = async (id: number, data: UpdateCustomerData): Promise<Customer> => {
  const res = await apiClient.put<Customer>(`/customers/${id}`, data);
  return res.data;
};

export const deleteCustomer = async (id: number): Promise<void> => {
  await apiClient.delete(`/customers/${id}`);
};

export const getCustomerOrders = async (customerId: number): Promise<CustomerOrder[]> => {
  const res = await apiClient.get<CustomerOrder[]>(`/customers/${customerId}/orders`);
  return Array.isArray(res.data) ? res.data : [];
};

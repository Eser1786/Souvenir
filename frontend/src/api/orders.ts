import { apiClient } from './client';

export type PhuongThucThanhToan = 'TienMat' | 'ChuyenKhoan';

export interface CreateOrderItem {
  sanPhamId: number;
  soLuong: number;
}

export interface CreateOrderRequest {
  items: CreateOrderItem[];
  phuongThuc: PhuongThucThanhToan;
  tienKhachDua?: number;
  caLamViecId?: number;
  khachHangId?: number;
  maGiamGia?: string;
}

export interface OrderItemDto {
  sanPhamId: number;
  maSP: string;
  tenSP: string;
  soLuong: number;
  donGia: number;
  thanhTien: number;
  soLuongTonConLai: number;
}

export interface OrderDto {
  id: number;
  maDonHang: string;
  ngayTao: string;
  trangThai: string;
  nguoiDungId: number;
  caLamViecId?: number | null;
  khachHangId?: number | null;
  hinhThucThanhToanId: number;
  tenHinhThucThanhToan: string;
  maGiamGia?: string | null;
  tongTienHang: number;
  soTienGiam: number;
  tongTienThanhToan: number;
  tienKhachDua?: number | null;
  tienThoi?: number | null;
  items: OrderItemDto[];
}

/**
 * UC07 - POST /api/orders
 * Backend lưu DonHang -> ChiTietDonHang -> trừ tồn kho trong một transaction.
 * Ném Error với thông báo tiếng Việt từ server nếu thất bại.
 */
export const createOrder = async (request: CreateOrderRequest): Promise<OrderDto> => {
  try {
    const response = await apiClient.post<OrderDto>('/orders', request);
    return response.data;
  } catch (error: unknown) {
    const err = error as { response?: { status?: number; data?: { message?: string } } };
    const message =
      err.response?.data?.message ??
      (err.response?.status === 401
        ? 'Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.'
        : 'Không thể kết nối đến máy chủ để tạo đơn hàng.');
    throw new Error(message, { cause: error });
  }
};

export const getOrderById = async (id: number): Promise<OrderDto> => {
  const response = await apiClient.get<OrderDto>(`/orders/${id}`);
  return response.data;
};

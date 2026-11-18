import { apiClient } from './client';

export interface CreateDraftOrderItem {
  sanPhamId: number;
  soLuong: number;
}

export interface CreateDraftOrderRequest {
  items: CreateDraftOrderItem[];
  khachHangId?: number | null;
  maGiamGia?: string | null;
  diemSuDung?: number;
  caLamViecId?: number | null;
  ghiChu?: string;
}

export interface DraftOrderItemDto {
  sanPhamId: number;
  maSP: string;
  tenSP: string;
  imageUrl?: string | null;
  soLuong: number;
  donGia: number;
  thanhTien: number;
  soLuongTon: number;
}

export interface DraftOrderDto {
  id: number;
  maDonHang: string;
  ngayTao: string;
  trangThai: string;
  nguoiDungId: number;
  tenNhanVien?: string | null;
  khachHangId?: number | null;
  tenKhachHang?: string | null;
  soDienThoaiKhach?: string | null;
  maGiamGia?: string | null;
  diemSuDung: number;
  tongTienHang: number;
  soTienGiam: number;
  tongTienThanhToan: number;
  tongSoLuongMon: number;
  items: DraftOrderItemDto[];
}

export const getDraftOrders = async (): Promise<DraftOrderDto[]> => {
  const response = await apiClient.get<DraftOrderDto[]>('/draft-orders');
  return response.data;
};

export const getDraftOrderById = async (id: number): Promise<DraftOrderDto> => {
  const response = await apiClient.get<DraftOrderDto>(`/draft-orders/${id}`);
  return response.data;
};

export const createDraftOrder = async (request: CreateDraftOrderRequest): Promise<DraftOrderDto> => {
  try {
    const response = await apiClient.post<DraftOrderDto>('/draft-orders', request);
    return response.data;
  } catch (error: unknown) {
    const err = error as { response?: { status?: number; data?: { message?: string } } };
    const message =
      err.response?.data?.message ??
      (err.response?.status === 401
        ? 'Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.'
        : 'Không thể kết nối đến máy chủ để lưu đơn nháp.');
    throw new Error(message, { cause: error });
  }
};

export const deleteDraftOrder = async (id: number): Promise<void> => {
  try {
    await apiClient.delete(`/draft-orders/${id}`);
  } catch (error: unknown) {
    const err = error as { response?: { status?: number; data?: { message?: string } } };
    const message =
      err.response?.data?.message ?? 'Không thể xóa đơn nháp khỏi hệ thống.';
    throw new Error(message, { cause: error });
  }
};

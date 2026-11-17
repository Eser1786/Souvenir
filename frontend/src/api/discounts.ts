import { apiClient } from './client';

export interface CheckDiscountRequest {
  maCode: string;
  tongTienHang: number;
}

export interface CheckDiscountResponse {
  hopLe: boolean;
  thongBao: string;
  maCode?: string;
  tenVoucher?: string;
  loaiGiamGia?: string; // "PhanTram" hoặc "SoTien"
  giaTriGiam: number;
  soTienGiam: number;
  thanhTienSauGiam: number;
  ngayKetThuc?: string;
}

/**
 * Gọi API kiểm tra tính hợp lệ của mã Voucher và tính toán số tiền chiết khấu
 */
export const checkDiscount = async (
  request: CheckDiscountRequest
): Promise<CheckDiscountResponse> => {
  try {
    const response = await apiClient.post<CheckDiscountResponse>('/discounts/check', request);
    return response.data;
  } catch (error: any) {
    if (error.response?.data) {
      return error.response.data as CheckDiscountResponse;
    }
    return {
      hopLe: false,
      thongBao: 'Không thể kết nối đến máy chủ để kiểm tra mã giảm giá.',
      giaTriGiam: 0,
      soTienGiam: 0,
      thanhTienSauGiam: request.tongTienHang,
    };
  }
};

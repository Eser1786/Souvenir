import { apiClient } from './client';

export type TinhTrangHang = 'NguyenVen' | 'LoiNhaSanXuat' | 'HuHongDoKhach';

export interface ReturnableItem {
  sanPhamId: number;
  maSP: string;
  tenSP: string;
  hinhAnh?: string | null;
  soLuongMua: number;
  soLuongDaTra: number;
  soLuongConTra: number;
  donGia: number;
  donGiaHoan: number;
}

export interface ReturnableOrder {
  id: number;
  maDonHang: string;
  ngayTao: string;
  trangThai: string;
  khachHangId?: number | null;
  tenKhachHang?: string | null;
  soDienThoaiKhach?: string | null;
  tenHinhThucThanhToan: string;
  maGiamGia?: string | null;
  tongTienHang: number;
  tongTienThanhToan: number;
  tongDaHoan: number;
  soNgayChoPhepDoiTra: number;
  hanDoiTra: string;
  conHanDoiTra: boolean;
  coTheTra: boolean;
  lyDoKhongTheTra?: string | null;
  items: ReturnableItem[];
}

export interface CreateReturnRequest {
  donHangId: number;
  lyDoTra: string;
  items: { sanPhamId: number; soLuongTra: number; tinhTrangHang: TinhTrangHang }[];
}

export interface ReturnItem {
  sanPhamId: number;
  maSP: string;
  tenSP: string;
  soLuongTra: number;
  donGiaHoan: number;
  thanhTien: number;
  tinhTrangHang: TinhTrangHang;
  soLuongTonMoi?: number | null;
}

export interface ReturnSlip {
  id: number;
  maPhieu: string;
  donHangGocId: number;
  maDonHangGoc: string;
  ngayTra: string;
  nguoiDungId: number;
  tenNguoiLap?: string | null;
  lyDoTra: string;
  tongTienHoan: number;
  hinhThucHoan: string;
  trangThaiDonGoc: string;
  diemTichLuyBiTru: number;
  items: ReturnItem[];
}

const toError = (error: unknown, fallback: string) => {
  const err = error as { response?: { status?: number; data?: { message?: string } } };
  if (err.response?.status === 403) return new Error('Chỉ Quản lý mới được thực hiện trả hàng / hoàn tiền.', { cause: error });
  if (err.response?.status === 401) return new Error('Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.', { cause: error });
  return new Error(err.response?.data?.message ?? fallback, { cause: error });
};

/** GET /api/returns/orders – tra cứu hóa đơn gốc theo mã đơn / SĐT / khoảng ngày. */
export const searchReturnableOrders = async (params: {
  keyword?: string;
  tuNgay?: string;
  denNgay?: string;
}): Promise<ReturnableOrder[]> => {
  try {
    const query: Record<string, string> = {};
    if (params.keyword?.trim()) query.keyword = params.keyword.trim();
    if (params.tuNgay) query.tuNgay = params.tuNgay;
    if (params.denNgay) query.denNgay = params.denNgay;
    const res = await apiClient.get<ReturnableOrder[]>('/returns/orders', { params: query });
    return res.data;
  } catch (error) {
    throw toError(error, 'Không thể tra cứu hóa đơn.');
  }
};

export const getReturnableOrder = async (id: number): Promise<ReturnableOrder> => {
  try {
    const res = await apiClient.get<ReturnableOrder>(`/returns/orders/${id}`);
    return res.data;
  } catch (error) {
    throw toError(error, 'Không tải được hóa đơn.');
  }
};

/** POST /api/returns – lập phiếu trả, cộng tồn kho, đơn chuyển DaTraHang. */
export const createReturn = async (request: CreateReturnRequest): Promise<ReturnSlip> => {
  try {
    const res = await apiClient.post<ReturnSlip>('/returns', request);
    return res.data;
  } catch (error) {
    throw toError(error, 'Không thể lập phiếu trả hàng.');
  }
};

export const getReturnsByOrder = async (donHangId: number): Promise<ReturnSlip[]> => {
  try {
    const res = await apiClient.get<ReturnSlip[]>('/returns', { params: { donHangId } });
    return res.data;
  } catch (error) {
    throw toError(error, 'Không tải được lịch sử trả hàng.');
  }
};

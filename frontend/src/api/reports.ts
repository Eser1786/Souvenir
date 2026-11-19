import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export interface RevenueTimelinePoint {
  date: string;
  label: string;
  doanhThu: number;
  soDonHang: number;
  soLuongBan: number;
}

export interface RevenueSummary {
  tongDoanhThu: number;
  tongDonHang: number;
  tongSanPhamDaBan: number;
  tongTienGiam: number;
  tongLoiNhuanUocTinh: number;
  tongTonKho: number;
  tongGiaTriTonKho: number;
  tongKhachHang: number;
  doanhThuTrungBinhMoiDon: number;
  kyBaoCao: string;
  tuNgay: string;
  denNgay: string;
  timeline: RevenueTimelinePoint[];
}

export interface TopProductItem {
  sanPhamId: number;
  maSP: string;
  tenSP: string;
  danhMuc: string;
  hinhAnh?: string;
  giaBan: number;
  giaNhap: number;
  soLuongDaBan: number;
  tongDoanhThu: number;
  loiNhuanUocTinh: number;
  phanTramDoanhThu: number;
}

export interface TopProductsReport {
  items: TopProductItem[];
  tongDoanhThuTop: number;
  tongSoLuongTop: number;
  tongDoanhThuToanPhan: number;
  tuNgay: string;
  denNgay: string;
}

export interface ShiftReportItem {
  caLamViecId: number;
  nhanVienId: number;
  tenNhanVien: string;
  thoiGianMoCa: string;
  thoiGianChotCa?: string;
  tienMatBanGiao: number;
  tienMatThucTe?: number;
  tongDoanhThuTienMat: number;
  tongDoanhThuChuyenKhoan: number;
  tongDoanhThuCa: number;
  chenhLech: number;
  trangThai: string;
  lyDoChenhLech?: string;
  soDonHang: number;
}

export interface ShiftsReport {
  items: ShiftReportItem[];
  tongDoanhThuCacCa: number;
  tongTienMatCacCa: number;
  tongChuyenKhoanCacCa: number;
  tongSoCa: number;
  tuNgay: string;
  denNgay: string;
}

export interface ReportFilterParams {
  period?: 'today' | 'yesterday' | '7days' | '30days' | 'thisMonth' | 'lastMonth' | 'custom';
  fromDate?: string;
  toDate?: string;
}

/**
 * Lấy báo cáo doanh thu & biểu đồ thời gian (UC14)
 */
export const getRevenueReport = async (params?: ReportFilterParams): Promise<RevenueSummary> => {
  const response = await api.get<RevenueSummary>('/reports/revenue', { params });
  return response.data;
};

/**
 * Lấy Top 5 (hoặc Top N) sản phẩm bán chạy nhất (UC14)
 */
export const getTopProductsReport = async (
  limit = 5,
  params?: ReportFilterParams
): Promise<TopProductsReport> => {
  const response = await api.get<TopProductsReport>('/reports/top-products', {
    params: { limit, ...params },
  });
  return response.data;
};

/**
 * Lấy thống kê doanh thu theo các ca làm việc (UC14)
 */
export const getShiftsReport = async (params?: ReportFilterParams): Promise<ShiftsReport> => {
  const response = await api.get<ShiftsReport>('/reports/shifts', { params });
  return response.data;
};

/**
 * Tải file CSV báo cáo (UC14)
 */
export const downloadReportCsv = async (
  type: 'revenue' | 'top-products' | 'shifts',
  params?: ReportFilterParams
): Promise<void> => {
  const response = await api.get('/reports/export', {
    params: { type, ...params },
    responseType: 'blob',
  });

  const blob = new Blob([response.data], { type: 'text/csv;charset=utf-8;' });
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  const fileName = `BaoCao_${type}_${new Date().toISOString().slice(0, 10)}.csv`;
  link.setAttribute('download', fileName);
  document.body.appendChild(link);
  link.click();
  link.parentNode?.removeChild(link);
  window.URL.revokeObjectURL(url);
};

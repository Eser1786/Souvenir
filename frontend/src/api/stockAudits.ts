import { apiClient } from './client';

export interface StockAuditItemRequest {
  sanPhamId: number;
  soTonThucTe: number;
  lyDoChenhLech?: string | null;
}

export interface SaveStockAuditRequest {
  id?: number;
  items: StockAuditItemRequest[];
}

export interface StockAuditDetail {
  sanPhamId: number;
  maSP: string;
  tenSP: string;
  tenDanhMuc: string;
  tenDonVi?: string | null;
  soTonSoSach: number;
  soTonThucTe: number;
  soLuongChenhLech: number;
  lyDoChenhLech?: string | null;
}

export interface StockAudit {
  id: number;
  maPhieu: string;
  nguoiDungId: number;
  tenNguoiKiemKe: string;
  ngayKiemKe: string;
  trangThai: 'DangKiemKe' | 'DaCanBang' | string;
  tongSanPham: number;
  soSanPhamChenhLech: number;
  tongChenhLechThua: number;
  tongChenhLechThieu: number;
  items: StockAuditDetail[];
}

export const getStockAudits = async (trangThai?: string): Promise<StockAudit[]> => {
  const params = trangThai ? { trangThai } : undefined;
  const response = await apiClient.get<StockAudit[]>('/stock-audits', { params });
  return response.data;
};

export const getStockAuditById = async (id: number): Promise<StockAudit> => {
  const response = await apiClient.get<StockAudit>(`/stock-audits/${id}`);
  return response.data;
};

export const saveDraftStockAudit = async (payload: SaveStockAuditRequest): Promise<StockAudit> => {
  const response = await apiClient.post<StockAudit>('/stock-audits/draft', payload);
  return response.data;
};

export const balanceStockAudit = async (payload: SaveStockAuditRequest): Promise<StockAudit> => {
  const response = await apiClient.post<StockAudit>('/stock-audits/balance', payload);
  return response.data;
};

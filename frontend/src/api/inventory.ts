import { apiClient } from './client';

export interface Supplier {
  id: number;
  maNCC: string;
  tenNCC: string;
  soDienThoai: string;
  diaChi?: string;
}

export interface GoodsReceiptDetail {
  sanPhamId: number;
  maSP: string;
  tenSP: string;
  tenDonVi?: string;
  soLuongNhap: number;
  donGiaNhap: number;
  thanhTien: number;
  soLuongTonMoi: number;
}

export interface GoodsReceipt {
  id: number;
  maPhieu: string;
  nhaCungCapId: number;
  tenNCC: string;
  nguoiDungId: number;
  tenNguoiNhap: string;
  tongTienNhap: number;
  ngayNhap: string;
  chiTiet: GoodsReceiptDetail[];
}

export interface GoodsReceiptItemInput {
  sanPhamId?: number;
  maSP?: string;
  tenSP?: string;
  danhMucId?: number;
  tenDanhMuc?: string;
  donViTinhId?: number;
  tenDonVi?: string;
  soLuongNhap: number;
  donGiaNhap: number;
}

export interface CreateGoodsReceiptInput {
  nhaCungCapId: number;
  chiTiet: GoodsReceiptItemInput[];
}

export const FALLBACK_SUPPLIERS: Supplier[] = [
  { id: 1, maNCC: 'NCC001', tenNCC: 'Cơ sở quà tặng Việt An', soDienThoai: '0900000001', diaChi: 'Quận 5, TP. Hồ Chí Minh' },
  { id: 2, maNCC: 'NCC002', tenNCC: 'Cơ sở thủ công Mỹ Nghệ Thành Đạt', soDienThoai: '0900000002', diaChi: 'Quận 6, TP. Hồ Chí Minh' },
  { id: 3, maNCC: 'NCC003', tenNCC: 'Nhà phân phối Văn Phòng Phẩm Minh Châu', soDienThoai: '0900000003', diaChi: 'Quận 10, TP. Hồ Chí Minh' },
  { id: 4, maNCC: 'NCC004', tenNCC: 'Xưởng may Quà Tặng Sao Việt', soDienThoai: '0900000004', diaChi: 'Quận Tân Bình, TP. Hồ Chí Minh' },
];

export const getSuppliers = async (): Promise<Supplier[]> => {
  try {
    const response = await apiClient.get<Supplier[]>('/inventory/suppliers');
    if (response.data && response.data.length > 0) {
      return response.data;
    }
  } catch {
    // Fallback
  }
  return FALLBACK_SUPPLIERS;
};

export const getGoodsReceipts = async (): Promise<GoodsReceipt[]> => {
  try {
    const response = await apiClient.get<GoodsReceipt[]>('/inventory/receipts');
    return response.data;
  } catch {
    return [];
  }
};

export const createGoodsReceipt = async (data: CreateGoodsReceiptInput): Promise<GoodsReceipt> => {
  const response = await apiClient.post<GoodsReceipt>('/inventory/receipts', data);
  return response.data;
};

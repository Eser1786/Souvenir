import { apiClient } from './client';

export interface Product {
  id: number;
  maSP: string;
  tenSP: string;
  danhMucId?: number;
  tenDanhMuc: string;
  donViTinhId?: number;
  tenDonVi?: string;
  giaNhap?: number;
  giaBan: number;
  soLuongTon: number;
  trangThai?: string;
  hinhAnh?: string;
  imageUrl?: string;
}

export interface GetProductsParams {
  search?: string;
  category?: string;
  includeAllStatus?: boolean;
}

export interface CreateProductInput {
  maSP?: string;
  tenSP: string;
  danhMucId?: number;
  tenDanhMuc?: string;
  donViTinhId?: number;
  tenDonVi?: string;
  giaNhap: number;
  giaBan: number;
  soLuongTon: number;
  imageUrl?: string;
}

export interface UpdateProductInput {
  tenSP: string;
  danhMucId?: number;
  tenDanhMuc?: string;
  donViTinhId?: number;
  tenDonVi?: string;
  giaNhap: number;
  giaBan: number;
  soLuongTon: number;
  trangThai?: string;
  imageUrl?: string;
}

// Fallback matching exact seed data in database (database/init/02_seed_data.sql)
export const SEED_PRODUCTS_FALLBACK: Product[] = [
  {
    id: 1,
    maSP: 'SP001',
    tenSP: 'Nam châm tủ lạnh Việt Nam',
    tenDanhMuc: 'Đồ lưu niệm',
    giaNhap: 15000,
    giaBan: 25000,
    soLuongTon: 45,
    hinhAnh: '/gift-showcase.png',
    imageUrl: '/gift-showcase.png',
    tenDonVi: 'Cái',
    trangThai: 'KinhDoanh',
  },
  {
    id: 2,
    maSP: 'SP002',
    tenSP: 'Mô hình xe xích lô mini',
    tenDanhMuc: 'Đồ lưu niệm',
    giaNhap: 50000,
    giaBan: 85000,
    soLuongTon: 18,
    tenDonVi: 'Cái',
    trangThai: 'KinhDoanh',
  },
  {
    id: 3,
    maSP: 'SP003',
    tenSP: 'Móc khóa nón lá',
    tenDanhMuc: 'Móc khóa',
    giaNhap: 10000,
    giaBan: 20000,
    soLuongTon: 80,
    tenDonVi: 'Cái',
    trangThai: 'KinhDoanh',
  },
  {
    id: 4,
    maSP: 'SP004',
    tenSP: 'Móc khóa landmark',
    tenDanhMuc: 'Móc khóa',
    giaNhap: 12000,
    giaBan: 25000,
    soLuongTon: 65,
    tenDonVi: 'Cái',
    trangThai: 'KinhDoanh',
  },
  {
    id: 5,
    maSP: 'SP005',
    tenSP: 'Quạt giấy thủ công',
    tenDanhMuc: 'Đồ thủ công mỹ nghệ',
    giaNhap: 30000,
    giaBan: 50000,
    soLuongTon: 30,
    tenDonVi: 'Cái',
    trangThai: 'KinhDoanh',
  },
  {
    id: 6,
    maSP: 'SP006',
    tenSP: 'Bộ vòng tay handmade',
    tenDanhMuc: 'Trang sức và phụ kiện',
    giaNhap: 35000,
    giaBan: 65000,
    soLuongTon: 24,
    tenDonVi: 'Bộ',
    trangThai: 'KinhDoanh',
  },
  {
    id: 7,
    maSP: 'SP007',
    tenSP: 'Sổ tay họa tiết Việt Nam',
    tenDanhMuc: 'Văn phòng phẩm',
    giaNhap: 22000,
    giaBan: 40000,
    soLuongTon: 35,
    tenDonVi: 'Quyển',
    trangThai: 'KinhDoanh',
  },
  {
    id: 8,
    maSP: 'SP008',
    tenSP: 'Tranh trang trí mini',
    tenDanhMuc: 'Đồ trang trí',
    giaNhap: 45000,
    giaBan: 75000,
    soLuongTon: 15,
    tenDonVi: 'Tấm',
    trangThai: 'KinhDoanh',
  },
  {
    id: 9,
    maSP: 'SP009',
    tenSP: 'Túi vải in họa tiết',
    tenDanhMuc: 'Áo thun và túi vải',
    giaNhap: 35000,
    giaBan: 60000,
    soLuongTon: 40,
    tenDonVi: 'Túi',
    trangThai: 'KinhDoanh',
  },
  {
    id: 10,
    maSP: 'SP010',
    tenSP: 'Hộp quà lưu niệm',
    tenDanhMuc: 'Quà tặng',
    giaNhap: 55000,
    giaBan: 90000,
    soLuongTon: 20,
    hinhAnh: '/gift-showcase.png',
    imageUrl: '/gift-showcase.png',
    tenDonVi: 'Hộp',
    trangThai: 'KinhDoanh',
  },
];

// Helper to remove accents for flexible search/category comparison
const normalizeText = (text: string): string => {
  return text
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .trim();
};

export const getProducts = async (params?: GetProductsParams): Promise<Product[]> => {
  try {
    const queryParams: Record<string, string | boolean> = {};
    if (params?.search && params.search.trim()) {
      queryParams.search = params.search.trim();
    }
    if (params?.category && params.category !== 'Tất cả' && params.category !== 'All') {
      queryParams.category = params.category;
    }
    if (params?.includeAllStatus) {
      queryParams.includeAllStatus = true;
    }

    const response = await apiClient.get<Product[]>('/products', { params: queryParams });
    if (response.data && Array.isArray(response.data) && response.data.length > 0) {
      return response.data.map(p => ({
        ...p,
        imageUrl: p.imageUrl || p.hinhAnh,
        hinhAnh: p.imageUrl || p.hinhAnh,
      }));
    }
  } catch {
    // If backend endpoint is offline or DB empty, proceed to fallback
  }

  // Fallback to seed data products
  let result = [...SEED_PRODUCTS_FALLBACK];

  if (params?.category && params.category !== 'Tất cả') {
    const normalizedCat = normalizeText(params.category);
    result = result.filter((p) => {
      const pCat = normalizeText(p.tenDanhMuc);
      return pCat === normalizedCat || pCat.includes(normalizedCat) || normalizedCat.includes(pCat);
    });
  }

  if (params?.search && params.search.trim()) {
    const normalizedQuery = normalizeText(params.search);
    result = result.filter(
      (p) =>
        normalizeText(p.tenSP).includes(normalizedQuery) ||
        normalizeText(p.maSP).includes(normalizedQuery)
    );
  }

  return result;
};

export const getProductById = async (id: number): Promise<Product> => {
  const response = await apiClient.get<Product>(`/products/${id}`);
  return {
    ...response.data,
    imageUrl: response.data.imageUrl || response.data.hinhAnh,
    hinhAnh: response.data.imageUrl || response.data.hinhAnh,
  };
};

export const createProduct = async (data: CreateProductInput): Promise<Product> => {
  const response = await apiClient.post<Product>('/products', data);
  return {
    ...response.data,
    imageUrl: response.data.imageUrl || response.data.hinhAnh,
    hinhAnh: response.data.imageUrl || response.data.hinhAnh,
  };
};

export const updateProduct = async (id: number, data: UpdateProductInput): Promise<Product> => {
  const response = await apiClient.put<Product>(`/products/${id}`, data);
  return {
    ...response.data,
    imageUrl: response.data.imageUrl || response.data.hinhAnh,
    hinhAnh: response.data.imageUrl || response.data.hinhAnh,
  };
};

export const deleteProduct = async (id: number): Promise<{ message: string; action: string }> => {
  const response = await apiClient.delete<{ message: string; action: string }>(`/products/${id}`);
  return response.data;
};

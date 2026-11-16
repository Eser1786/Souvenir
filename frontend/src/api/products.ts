import { apiClient } from './client';

export interface Product {
  id: number;
  maSP: string;
  tenSP: string;
  tenDanhMuc: string;
  giaBan: number;
  soLuongTon: number;
  hinhAnh?: string;
  tenDonVi?: string;
}

export interface GetProductsParams {
  search?: string;
  category?: string;
}

// Fallback matching exact seed data in database (database/init/02_seed_data.sql)
export const SEED_PRODUCTS_FALLBACK: Product[] = [
  {
    id: 1,
    maSP: 'SP001',
    tenSP: 'Nam châm tủ lạnh Việt Nam',
    tenDanhMuc: 'Đồ lưu niệm',
    giaBan: 25000,
    soLuongTon: 45,
    hinhAnh: '/gift-showcase.png',
    tenDonVi: 'Cái',
  },
  {
    id: 2,
    maSP: 'SP002',
    tenSP: 'Mô hình xe xích lô mini',
    tenDanhMuc: 'Đồ lưu niệm',
    giaBan: 85000,
    soLuongTon: 18,
    tenDonVi: 'Cái',
  },
  {
    id: 3,
    maSP: 'SP003',
    tenSP: 'Móc khóa nón lá',
    tenDanhMuc: 'Móc khóa',
    giaBan: 20000,
    soLuongTon: 80,
    tenDonVi: 'Cái',
  },
  {
    id: 4,
    maSP: 'SP004',
    tenSP: 'Móc khóa landmark',
    tenDanhMuc: 'Móc khóa',
    giaBan: 25000,
    soLuongTon: 65,
    tenDonVi: 'Cái',
  },
  {
    id: 5,
    maSP: 'SP005',
    tenSP: 'Quạt giấy thủ công',
    tenDanhMuc: 'Đồ thủ công mỹ nghệ',
    giaBan: 50000,
    soLuongTon: 30,
    tenDonVi: 'Cái',
  },
  {
    id: 6,
    maSP: 'SP006',
    tenSP: 'Bộ vòng tay handmade',
    tenDanhMuc: 'Trang sức và phụ kiện',
    giaBan: 65000,
    soLuongTon: 24,
    tenDonVi: 'Bộ',
  },
  {
    id: 7,
    maSP: 'SP007',
    tenSP: 'Sổ tay họa tiết Việt Nam',
    tenDanhMuc: 'Văn phòng phẩm',
    giaBan: 40000,
    soLuongTon: 35,
    tenDonVi: 'Quyển',
  },
  {
    id: 8,
    maSP: 'SP008',
    tenSP: 'Tranh trang trí mini',
    tenDanhMuc: 'Đồ trang trí',
    giaBan: 75000,
    soLuongTon: 15,
    tenDonVi: 'Tấm',
  },
  {
    id: 9,
    maSP: 'SP009',
    tenSP: 'Túi vải in họa tiết',
    tenDanhMuc: 'Áo thun và túi vải',
    giaBan: 60000,
    soLuongTon: 40,
    tenDonVi: 'Túi',
  },
  {
    id: 10,
    maSP: 'SP010',
    tenSP: 'Hộp quà lưu niệm',
    tenDanhMuc: 'Quà tặng',
    giaBan: 90000,
    soLuongTon: 20,
    hinhAnh: '/gift-showcase.png',
    tenDonVi: 'Hộp',
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
    const queryParams: Record<string, string> = {};
    if (params?.search && params.search.trim()) {
      queryParams.search = params.search.trim();
    }
    if (params?.category && params.category !== 'Tất cả' && params.category !== 'All') {
      queryParams.category = params.category;
    }

    const response = await apiClient.get<Product[]>('/products', { params: queryParams });
    if (response.data && Array.isArray(response.data) && response.data.length > 0) {
      return response.data;
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

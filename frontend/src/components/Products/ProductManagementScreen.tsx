import React, { useState, useEffect, useMemo, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context';
import {
  getProducts,
  createProduct,
  updateProduct,
  deleteProduct,
  getProductMetadata,
  type Product,
  type CreateProductInput,
  type UpdateProductInput,
  type CategoryItem,
  type UnitItem,
} from '../../api/products';
import './ProductManagementScreen.css';

// SVG Icons
const IconSearch = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="11" cy="11" r="8" />
    <line x1="21" y1="21" x2="16.65" y2="16.65" />
  </svg>
);

const IconFilter = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
    <path d="M4 5h16l-6.5 7.5v5l-3 1.5v-6.5L4 5z" />
  </svg>
);

const IconPlus = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="12" y1="5" x2="12" y2="19" />
    <line x1="5" y1="12" x2="19" y2="12" />
  </svg>
);

const IconEdit = () => (
  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
    <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
  </svg>
);

const IconTrash = () => (
  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="3 6 5 6 21 6" />
    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
  </svg>
);

const IconBox = () => (
  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z" />
    <polyline points="3.27 6.96 12 12.01 20.73 6.96" />
    <line x1="12" y1="22.08" x2="12" y2="12" />
  </svg>
);

const IconAlert = () => (
  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="10" />
    <line x1="12" y1="8" x2="12" y2="12" />
    <line x1="12" y1="16" x2="12.01" y2="16" />
  </svg>
);

const IconTags = () => (
  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z" />
    <line x1="7" y1="7" x2="7.01" y2="7" />
  </svg>
);

const IconDollar = () => (
  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="12" y1="1" x2="12" y2="23" />
    <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
  </svg>
);

const IconArrowLeft = () => (
  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="19" y1="12" x2="5" y2="12" />
    <polyline points="12 19 5 12 12 5" />
  </svg>
);

const IconReceipt = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
    <polyline points="14 2 14 8 20 8" />
    <line x1="16" y1="13" x2="8" y2="13" />
    <line x1="16" y1="17" x2="8" y2="17" />
  </svg>
);

export const ProductManagementScreen: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const isManager = useMemo(() => {
    const role = (user?.vaiTro || '').toLowerCase();
    return role.includes('quản lý') || role.includes('quanly') || role.includes('admin');
  }, [user]);

  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [selectedCategoryIds, setSelectedCategoryIds] = useState<number[]>([]);
  const [isCategoryFilterOpen, setIsCategoryFilterOpen] = useState(false);
  const [categorySearchTerm, setCategorySearchTerm] = useState('');
  const categoryFilterRef = useRef<HTMLDivElement>(null);

  // Metadata from backend (Categories, Units, next suggested MaSP)
  const [categoriesList, setCategoriesList] = useState<CategoryItem[]>([]);
  const [unitsList, setUnitsList] = useState<UnitItem[]>([]);
  const [nextSuggestedMa, setNextSuggestedMa] = useState<string>('SP010');
  const [isAddingNewCategory, setIsAddingNewCategory] = useState<boolean>(false);
  const [customCategoryInput, setCustomCategoryInput] = useState<string>('');

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
  const [modalMode, setModalMode] = useState<'create' | 'edit'>('create');
  const [currentId, setCurrentId] = useState<number | null>(null);

  // Form State
  const [formMaSP, setFormMaSP] = useState<string>('');
  const [formTenSP, setFormTenSP] = useState<string>('');
  const [formDanhMuc, setFormDanhMuc] = useState<string>('Đồ lưu niệm');
  const [formDonVi, setFormDonVi] = useState<string>('Cái');
  const [formGiaNhap, setFormGiaNhap] = useState<number>(0);
  const [formGiaBan, setFormGiaBan] = useState<number>(0);
  const [formSoLuongTon, setFormSoLuongTon] = useState<number>(0);
  const [formImageUrl, setFormImageUrl] = useState<string>('');
  const [formTrangThai, setFormTrangThai] = useState<string>('KinhDoanh');

  // Toast
  const [toastMessage, setToastMessage] = useState<string | null>(null);
  const [toastType, setToastType] = useState<'success' | 'error'>('success');

  const showToast = (msg: string, type: 'success' | 'error' = 'success') => {
    setToastMessage(msg);
    setToastType(type);
    setTimeout(() => setToastMessage(null), 3500);
  };

  const fetchProductList = async () => {
    try {
      setLoading(true);
      const [data, meta] = await Promise.all([
        getProducts({ includeAllStatus: true }),
        getProductMetadata(),
      ]);
      setProducts(data);
      if (meta.categories.length > 0) setCategoriesList(meta.categories);
      if (meta.units.length > 0) setUnitsList(meta.units);
      if (meta.nextSuggestedMaSP) setNextSuggestedMa(meta.nextSuggestedMaSP);
    } catch {
      showToast('Không thể tải danh sách sản phẩm', 'error');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProductList();
  }, []);

  useEffect(() => {
    if (!isCategoryFilterOpen) return;

    const handlePointerDown = (event: PointerEvent) => {
      if (event.target instanceof Node && !categoryFilterRef.current?.contains(event.target)) {
        setIsCategoryFilterOpen(false);
      }
    };
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setIsCategoryFilterOpen(false);
    };

    document.addEventListener('pointerdown', handlePointerDown);
    document.addEventListener('keydown', handleKeyDown);
    return () => {
      document.removeEventListener('pointerdown', handlePointerDown);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isCategoryFilterOpen]);

  const categoryFilters = useMemo<Array<{ id: number | null; label: string }>>(() => {
    const list: Array<{ id: number | null; label: string }> = [{ id: null, label: 'Tất cả' }];

    // Ưu tiên danh sách danh mục từ backend metadata
    const addedIds = new Set<number>();
    for (const c of categoriesList) {
      list.push({ id: c.id, label: c.tenDanhMuc });
      addedIds.add(c.id);
    }

    // Bổ sung các danh mục khác nếu có trong sản phẩm
    for (const product of products) {
      if (product.danhMucId != null && !addedIds.has(product.danhMucId) && product.tenDanhMuc?.trim()) {
        list.push({ id: product.danhMucId, label: product.tenDanhMuc });
        addedIds.add(product.danhMucId);
      }
    }

    return list;
  }, [categoriesList, products]);

  const visibleCategoryFilters = categoryFilters
    .filter((category): category is { id: number; label: string } => category.id !== null)
    .filter((category) => category.label.toLocaleLowerCase('vi').includes(categorySearchTerm.trim().toLocaleLowerCase('vi')));

  // Filter products by category & search term
  const filteredProducts = useMemo(() => {
    return products.filter((p) => {
      const matchCat = selectedCategoryIds.length === 0 || selectedCategoryIds.includes(p.danhMucId ?? -1);
      const matchSearch =
        !searchTerm.trim() ||
        p.tenSP.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.maSP.toLowerCase().includes(searchTerm.toLowerCase());
      return matchCat && matchSearch;
    });
  }, [products, selectedCategoryIds, searchTerm]);

  // Statistics
  const stats = useMemo(() => {
    const totalCount = products.length;
    const totalStock = products.reduce((acc, p) => acc + (p.soLuongTon || 0), 0);
    const lowStockCount = products.filter((p) => (p.soLuongTon || 0) <= 5).length;
    const totalInventoryValue = products.reduce((acc, p) => acc + (p.soLuongTon || 0) * (p.giaBan || 0), 0);
    return { totalCount, totalStock, lowStockCount, totalInventoryValue };
  }, [products]);

  const handleOpenCreateModal = () => {
    setModalMode('create');
    setCurrentId(null);
    setFormMaSP(nextSuggestedMa || 'SP010');
    setFormTenSP('');
    setFormDanhMuc(categoriesList[0]?.tenDanhMuc || 'Đồ lưu niệm');
    setFormDonVi(unitsList[0]?.tenDonVi || 'Cái');
    setFormGiaNhap(10000);
    setFormGiaBan(20000);
    setFormSoLuongTon(0);
    setFormImageUrl('');
    setFormTrangThai('KinhDoanh');
    setIsAddingNewCategory(false);
    setCustomCategoryInput('');
    setIsModalOpen(true);
  };

  const handleOpenEditModal = (p: Product) => {
    setModalMode('edit');
    setCurrentId(p.id);
    setFormMaSP(p.maSP);
    setFormTenSP(p.tenSP);
    setFormDanhMuc(p.tenDanhMuc || 'Đồ lưu niệm');
    setFormDonVi(p.tenDonVi || 'Cái');
    setFormGiaNhap(p.giaNhap || 0);
    setFormGiaBan(p.giaBan || 0);
    setFormSoLuongTon(p.soLuongTon || 0);
    setFormImageUrl(p.imageUrl || p.hinhAnh || '');
    setFormTrangThai(p.trangThai || 'KinhDoanh');
    setIsAddingNewCategory(false);
    setCustomCategoryInput('');
    setIsModalOpen(true);
  };

  const handleSaveProduct = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formTenSP.trim()) {
      showToast('Tên sản phẩm không được để trống.', 'error');
      return;
    }
    if (formGiaNhap < 0 || formGiaBan < 0) {
      showToast('Giá nhập và giá bán không được là số âm.', 'error');
      return;
    }
    if (formSoLuongTon < 0) {
      showToast('Số lượng tồn không được là số âm.', 'error');
      return;
    }

    const finalCategory = isAddingNewCategory ? customCategoryInput.trim() : formDanhMuc.trim();
    if (!finalCategory) {
      showToast('Vui lòng chọn hoặc nhập tên danh mục.', 'error');
      return;
    }

    try {
      if (modalMode === 'create') {
        const payload: CreateProductInput = {
          tenSP: formTenSP.trim(),
          tenDanhMuc: finalCategory,
          tenDonVi: formDonVi,
          giaNhap: Number(formGiaNhap),
          giaBan: Number(formGiaBan),
          soLuongTon: Number(formSoLuongTon),
          imageUrl: formImageUrl.trim() || undefined,
        };
        await createProduct(payload);
        showToast('Thêm mới sản phẩm thành công!');
      } else if (currentId) {
        const payload: UpdateProductInput = {
          tenSP: formTenSP.trim(),
          tenDanhMuc: finalCategory,
          tenDonVi: formDonVi,
          giaNhap: Number(formGiaNhap),
          giaBan: Number(formGiaBan),
          soLuongTon: Number(formSoLuongTon),
          trangThai: formTrangThai,
          imageUrl: formImageUrl.trim() || undefined,
        };
        await updateProduct(currentId, payload);
        showToast('Cập nhật sản phẩm thành công!');
      }
      setIsModalOpen(false);
      fetchProductList();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Có lỗi xảy ra khi lưu sản phẩm.';
      showToast(msg, 'error');
    }
  };

  const handleDeleteProduct = async (p: Product) => {
    const isStopped = p.trangThai?.toLowerCase() === 'ngungkinhdoanh';
    const confirmMessage = isStopped
      ? `Sản phẩm "${p.tenSP}" (${p.maSP}) đã ngừng kinh doanh. Bạn có muốn xóa hẳn không? Sản phẩm có lịch sử giao dịch hoặc kiểm kê sẽ không thể xóa.`
      : `Bạn có chắc muốn xóa hoặc ngừng kinh doanh sản phẩm "${p.tenSP}" (${p.maSP})?`;
    const confirm = window.confirm(confirmMessage);
    if (!confirm) return;

    try {
      const res = await deleteProduct(p.id);
      showToast(res.message || 'Thao tác sản phẩm thành công!');
      fetchProductList();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Có lỗi xảy ra khi xóa sản phẩm.';
      showToast(msg, 'error');
    }
  };

  return (
    <div className="product-screen">
      {/* Header */}
      <header className="product-header">
        <div className="product-header-left">
          <div className="brand-badge">
            <h1 className="brand-title">MERA · SOUVENIR</h1>
            <span className="brand-subtitle">Quản lý kho hàng & Sản phẩm</span>
          </div>
          <nav className="header-nav">
            <button type="button" className="nav-pill-btn" onClick={() => navigate('/pos')}>
              <IconArrowLeft /> Bán hàng (POS)
            </button>
            <button type="button" className="nav-pill-btn active">
              Sản phẩm
            </button>
            {isManager && (
              <>
                <button type="button" className="nav-pill-btn" onClick={() => navigate('/inventory/receipts')}>
                  <IconReceipt /> Nhập kho
                </button>
                <button type="button" className="nav-pill-btn" onClick={() => navigate('/inventory/audits')}>
                  Kiểm kê kho
                </button>
              </>
            )}
            <button type="button" className="nav-pill-btn" onClick={() => navigate('/customers')}>
              Khách hàng
            </button>
            {isManager && (
              <button type="button" className="nav-pill-btn" onClick={() => navigate('/reports')}>
                Báo cáo
              </button>
            )}
          </nav>
        </div>

        <div className="product-header-right">
          <div className="user-tag">
            <div className="user-avatar">{user?.hoTen?.charAt(0) || 'Q'}</div>
            <div className="user-meta">
              <span className="user-name">{user?.hoTen || 'Quản lý kho'}</span>
              <span className="user-role">{user?.vaiTro || 'Quản lý'}</span>
            </div>
          </div>
          <button
            type="button"
            className="btn-logout"
            onClick={() => {
              logout();
              navigate('/login');
            }}
          >
            Đăng xuất
          </button>
        </div>
      </header>

      {/* Main Container */}
      <main className="product-content">
        {/* Stats Row */}
        <div className="product-stats-grid">
          <div className="stat-card">
            <div className="stat-icon-wrap"><IconBox /></div>
            <div className="stat-info">
              <span className="stat-label">Tổng mặt hàng</span>
              <span className="stat-value">{stats.totalCount}</span>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon-wrap"><IconTags /></div>
            <div className="stat-info">
              <span className="stat-label">Tổng lượng tồn kho</span>
              <span className="stat-value">{stats.totalStock.toLocaleString('vi-VN')}</span>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon-wrap" style={{ color: '#D32F2F', background: '#FFEBEE' }}><IconAlert /></div>
            <div className="stat-info">
              <span className="stat-label">Cảnh báo sắp hết</span>
              <span className="stat-value" style={{ color: '#D32F2F' }}>{stats.lowStockCount}</span>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon-wrap"><IconDollar /></div>
            <div className="stat-info">
              <span className="stat-label">Ước tính giá trị bán</span>
              <span className="stat-value">{stats.totalInventoryValue.toLocaleString('vi-VN')} đ</span>
            </div>
          </div>
        </div>

        {/* Controls Bar */}
        <div className="product-controls">
          <div className="product-controls-top">
            <div className="search-box">
              <span className="search-icon"><IconSearch /></span>
              <input
                type="text"
                className="search-input"
                placeholder="Tìm kiếm theo mã sản phẩm hoặc tên món..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>

            {isManager && (
              <div className="action-buttons">
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={() => navigate('/inventory/receipts')}
                >
                  <IconReceipt /> Lập phiếu nhập kho
                </button>
                <button
                  type="button"
                  className="btn-primary"
                  onClick={handleOpenCreateModal}
                >
                  <IconPlus /> Thêm sản phẩm mới 
                </button>
              </div>
            )}
          </div>

          {/* Category Filter */}
          <div className="category-filter-bar">
            <div className="category-filter-control" ref={categoryFilterRef}>
              <button
                type="button"
                className={`category-filter-trigger ${selectedCategoryIds.length > 0 ? 'active' : ''}`}
                aria-expanded={isCategoryFilterOpen}
                aria-controls="product-category-filter"
                onClick={() => setIsCategoryFilterOpen((open) => !open)}
              >
                <IconFilter />
                <span>Danh mục</span>
                {selectedCategoryIds.length > 0 && (
                  <span className="category-filter-count">{selectedCategoryIds.length}</span>
                )}
              </button>

              {isCategoryFilterOpen && (
                <div
                  id="product-category-filter"
                  className="category-filter-popover"
                  role="dialog"
                  aria-label="Lọc sản phẩm theo danh mục"
                >
                  <div className="category-filter-popover-header">
                    <strong>Danh mục sản phẩm</strong>
                    <div className="category-filter-actions">
                      <button
                        type="button"
                        onClick={() => {
                          const visibleIds = visibleCategoryFilters.map((category) => category.id);
                          setSelectedCategoryIds((current) =>
                            Array.from(new Set([...current, ...visibleIds])),
                          );
                        }}
                      >
                        Chọn tất cả
                      </button>
                      <button type="button" onClick={() => setSelectedCategoryIds([])}>
                        Xóa lọc
                      </button>
                    </div>
                  </div>

                  <input
                    type="search"
                    className="category-filter-search"
                    placeholder="Tìm danh mục..."
                    aria-label="Tìm danh mục"
                    value={categorySearchTerm}
                    onChange={(event) => setCategorySearchTerm(event.target.value)}
                  />

                  <div className="category-filter-options">
                    {visibleCategoryFilters.length === 0 ? (
                      <p className="category-filter-empty">Không tìm thấy danh mục.</p>
                    ) : (
                      visibleCategoryFilters.map((category) => (
                        <label className="category-filter-option" key={category.id}>
                          <input
                            type="checkbox"
                            checked={selectedCategoryIds.includes(category.id)}
                            onChange={() => {
                              setSelectedCategoryIds((current) =>
                                current.includes(category.id)
                                  ? current.filter((id) => id !== category.id)
                                  : [...current, category.id],
                              );
                            }}
                          />
                          <span>{category.label}</span>
                        </label>
                      ))
                    )}
                  </div>
                  <div className="category-filter-footer">
                    {selectedCategoryIds.length === 0
                      ? 'Đang hiển thị tất cả danh mục'
                      : `Đã chọn ${selectedCategoryIds.length} danh mục`}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Products Table Card */}
        <div className="product-table-card">
          <table className="product-table">
            <thead>
              <tr>
                <th style={{ width: '80px' }}>Hình ảnh</th>
                <th style={{ width: '100px' }}>Mã SP</th>
                <th>Tên sản phẩm lưu niệm</th>
                <th style={{ width: '140px' }}>Danh mục</th>
                <th style={{ width: '80px' }}>ĐVT</th>
                <th style={{ width: '110px', textAlign: 'right' }}>Giá nhập</th>
                <th style={{ width: '110px', textAlign: 'right' }}>Giá bán</th>
                <th style={{ width: '100px', textAlign: 'center' }}>Tồn kho</th>
                <th style={{ width: '130px', textAlign: 'center' }}>Trạng thái</th>
                {isManager && <th style={{ width: '100px', textAlign: 'center' }}>Thao tác</th>}
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan={isManager ? 10 : 9} style={{ textAlign: 'center', padding: '40px' }}>
                    Đang tải danh mục sản phẩm...
                  </td>
                </tr>
              ) : filteredProducts.length === 0 ? (
                <tr>
                  <td colSpan={isManager ? 10 : 9} style={{ textAlign: 'center', padding: '40px', color: '#8C827A' }}>
                    Không tìm thấy sản phẩm nào phù hợp với bộ lọc.
                  </td>
                </tr>
              ) : (
                filteredProducts.map((p) => {
                  const img = p.imageUrl || p.hinhAnh;
                  const isLow = (p.soLuongTon || 0) <= 5;
                  const isStopped = p.trangThai === 'NgungKinhDoanh';

                  return (
                    <tr key={p.id}>
                      <td>
                        {img ? (
                          <img
                            src={img}
                            alt={p.tenSP}
                            className="product-thumb"
                            onError={(e) => {
                              (e.target as HTMLElement).style.display = 'none';
                            }}
                          />
                        ) : (
                          <div className="product-thumb-placeholder">🎁</div>
                        )}
                      </td>
                      <td><strong>{p.maSP}</strong></td>
                      <td>
                        <span style={{ fontWeight: 600 }}>{p.tenSP}</span>
                      </td>
                      <td>{p.tenDanhMuc}</td>
                      <td>{p.tenDonVi || 'Cái'}</td>
                      <td style={{ textAlign: 'right' }}>
                        {(p.giaNhap || 0).toLocaleString('vi-VN')} đ
                      </td>
                      <td style={{ textAlign: 'right', fontWeight: 600 }}>
                        {p.giaBan.toLocaleString('vi-VN')} đ
                      </td>
                      <td style={{ textAlign: 'center' }}>
                        <span className={`badge-stock ${isLow ? 'stock-low' : ''}`}>
                          {p.soLuongTon} {isLow && '⚠️'}
                        </span>
                      </td>
                      <td style={{ textAlign: 'center' }}>
                        <span className={`badge-tag ${isStopped ? 'badge-inactive' : 'badge-active'}`}>
                          {isStopped ? 'Ngừng kinh doanh' : 'Đang kinh doanh'}
                        </span>
                      </td>
                      {isManager && (
                        <td>
                          <div className="action-cell" style={{ justifyContent: 'center' }}>
                            <button
                              type="button"
                              className="action-btn-sm"
                              title="Sửa thông tin sản phẩm"
                              onClick={() => handleOpenEditModal(p)}
                            >
                              <IconEdit />
                            </button>
                            <button
                              type="button"
                              className="action-btn-sm delete"
                              title={isStopped ? 'Xóa hẳn sản phẩm' : 'Xóa / Ngừng kinh doanh'}
                              onClick={() => handleDeleteProduct(p)}
                            >
                              <IconTrash />
                            </button>
                          </div>
                        </td>
                      )}
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </main>

      {/* Modal Thêm / Sửa Sản Phẩm (UC11) */}
      {isModalOpen && (
        <div className="modal-overlay" onClick={() => setIsModalOpen(false)}>
          <div className="product-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2 className="modal-title">
                {modalMode === 'create' ? 'Thêm sản phẩm lưu niệm mới' : 'Cập nhật thông tin sản phẩm'}
              </h2>
              <button
                type="button"
                className="modal-close-btn"
                onClick={() => setIsModalOpen(false)}
              >
                &times;
              </button>
            </div>

            <form onSubmit={handleSaveProduct}>
              <div className="modal-body">
                <div className="form-grid-2">
                  <div className="form-group">
                    <label className="form-label">
                      {modalMode === 'create' ? 'Mã sản phẩm dự kiến' : 'Mã sản phẩm'}
                    </label>
                    <input
                      type="text"
                      className="form-input"
                      value={formMaSP}
                      readOnly={modalMode === 'create'}
                      disabled={modalMode === 'edit'}
                    />
                    {modalMode === 'create' && (
                      <small className="product-code-note">
                        Backend sẽ tự cấp mã theo dạng SP + ít nhất 3 chữ số khi lưu.
                      </small>
                    )}
                  </div>

                  <div className="form-group">
                    <label className="form-label">Tên sản phẩm *</label>
                    <input
                      type="text"
                      className="form-input"
                      placeholder="VD: Mô hình chùa Một Cột mini..."
                      value={formTenSP}
                      onChange={(e) => setFormTenSP(e.target.value)}
                      required
                    />
                  </div>
                </div>

                <div className="form-grid-2">
                  <div className="form-group">
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '6px' }}>
                      <label className="form-label" style={{ margin: 0 }}>Danh mục sản phẩm</label>
                      <button
                        type="button"
                        style={{
                          background: 'none',
                          border: 'none',
                          color: '#C8A27A',
                          fontSize: '12px',
                          fontWeight: 600,
                          cursor: 'pointer',
                          padding: 0,
                        }}
                        onClick={() => {
                          setIsAddingNewCategory(!isAddingNewCategory);
                          setCustomCategoryInput('');
                        }}
                      >
                        {isAddingNewCategory ? '← Chọn danh mục có sẵn' : '+ Thêm danh mục mới'}
                      </button>
                    </div>

                    {isAddingNewCategory ? (
                      <input
                        type="text"
                        className="form-input"
                        placeholder="Nhập tên danh mục mới (VD: Gốm sứ Bát Tràng)..."
                        value={customCategoryInput}
                        onChange={(e) => setCustomCategoryInput(e.target.value)}
                        autoFocus
                        required
                      />
                    ) : (
                      <select
                        className="form-select"
                        value={formDanhMuc}
                        onChange={(e) => setFormDanhMuc(e.target.value)}
                      >
                        {categoriesList.map((c) => (
                          <option key={c.id} value={c.tenDanhMuc}>
                            {c.tenDanhMuc}
                          </option>
                        ))}
                      </select>
                    )}
                  </div>

                  <div className="form-group">
                    <label className="form-label">Đơn vị tính</label>
                    <select
                      className="form-select"
                      value={formDonVi}
                      onChange={(e) => setFormDonVi(e.target.value)}
                    >
                      {unitsList.map((u) => (
                        <option key={u.id} value={u.tenDonVi}>
                          {u.tenDonVi}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>

                <div className="form-grid-2">
                  <div className="form-group">
                    <label className="form-label">Giá nhập dự kiến (VNĐ) *</label>
                    <input
                      type="number"
                      className="form-input"
                      min={0}
                      value={formGiaNhap}
                      onChange={(e) => setFormGiaNhap(Number(e.target.value))}
                      required
                    />
                  </div>

                  <div className="form-group">
                    <label className="form-label">Giá bán niêm yết (VNĐ) *</label>
                    <input
                      type="number"
                      className="form-input"
                      min={0}
                      value={formGiaBan}
                      onChange={(e) => setFormGiaBan(Number(e.target.value))}
                      required
                    />
                  </div>
                </div>

                <div className="form-grid-2">
                  <div className="form-group">
                    <label className="form-label">Số lượng tồn kho ban đầu *</label>
                    <input
                      type="number"
                      className="form-input"
                      min={0}
                      value={formSoLuongTon}
                      onChange={(e) => setFormSoLuongTon(Number(e.target.value))}
                      required
                    />
                  </div>

                  {modalMode === 'edit' && (
                    <div className="form-group">
                      <label className="form-label">Trạng thái kinh doanh</label>
                      <select
                        className="form-select"
                        value={formTrangThai}
                        onChange={(e) => setFormTrangThai(e.target.value)}
                      >
                        <option value="KinhDoanh">Đang kinh doanh</option>
                        <option value="NgungKinhDoanh">Ngừng kinh doanh</option>
                      </select>
                    </div>
                  )}
                </div>

                {/* Paste URL ảnh và preview trực tiếp */}
                <div className="form-group">
                  <label className="form-label">Đường dẫn ảnh sản phẩm (Paste URL ảnh)</label>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="https://images.unsplash.com/... hoặc /gift-showcase.png"
                    value={formImageUrl}
                    onChange={(e) => setFormImageUrl(e.target.value)}
                  />
                </div>

                {formImageUrl && (
                  <div className="img-preview-box">
                    <img
                      src={formImageUrl}
                      alt="Preview sản phẩm"
                      className="img-preview-card"
                      onError={(e) => {
                        (e.target as HTMLImageElement).src = 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" width="70" height="70" viewBox="0 0 24 24"><text x="5" y="15" fill="%23999" font-size="10">Lỗi ảnh</text></svg>';
                      }}
                    />
                    <div style={{ fontSize: '12px', color: '#8C827A' }}>
                      <strong>Preview ảnh sản phẩm trực tiếp</strong>
                      <p style={{ margin: '4px 0 0 0', wordBreak: 'break-all' }}>{formImageUrl}</p>
                    </div>
                  </div>
                )}
              </div>

              <div className="modal-footer">
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={() => setIsModalOpen(false)}
                >
                  Hủy bỏ
                </button>
                <button type="submit" className="btn-primary">
                  {modalMode === 'create' ? 'Lưu sản phẩm mới' : 'Cập nhật thay đổi'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Toast Notice */}
      {toastMessage && (
        <div className={`toast-notice ${toastType}`}>
          <span>{toastType === 'success' ? '✓' : '⚠️'}</span>
          <span>{toastMessage}</span>
        </div>
      )}
    </div>
  );
};

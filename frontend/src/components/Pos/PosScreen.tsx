import React, { useState, useEffect, useMemo, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context';
import { getProducts } from '../../api/products';
import type { Product } from '../../api/products';
import { findCustomerByPhone } from '../../api/customers';
import './PosScreen.css';

interface CartItem {
  product: Product;
  quantity: number;
}

interface Customer {
  hoTen: string;
  soDienThoai: string;
  diemThuong: number;
  isGuest: boolean;
}

const CATEGORIES = [
  'Tất cả',
  'Quà tặng',
  'Thú bông',
  'Trang trí',
  'Văn phòng phẩm',
];

const DEFAULT_CUSTOMER: Customer = {
  hoTen: 'Nguyễn Ngọc Lan',
  soDienThoai: '0901 234 567',
  diemThuong: 250,
  isGuest: false,
};

const GUEST_CUSTOMER: Customer = {
  hoTen: 'Khách vãng lai',
  soDienThoai: '',
  diemThuong: 0,
  isGuest: true,
};

// Clean minimalist SVG icons
const IconPos = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <rect x="2" y="3" width="20" height="14" rx="2" />
    <line x1="8" y1="21" x2="16" y2="21" />
    <line x1="12" y1="17" x2="12" y2="21" />
  </svg>
);

const IconDraft = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
    <polyline points="14 2 14 8 20 8" />
    <line x1="16" y1="13" x2="8" y2="13" />
    <line x1="16" y1="17" x2="8" y2="17" />
  </svg>
);

const IconTime = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="10" />
    <polyline points="12 6 12 12 16 14" />
  </svg>
);

const IconShift = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
    <path d="M7 11V7a5 5 0 0 1 10 0v4" />
  </svg>
);

const IconCustomer = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
    <circle cx="12" cy="7" r="4" />
  </svg>
);

const IconProduct = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z" />
    <polyline points="3.27 6.96 12 12.01 20.73 6.96" />
    <line x1="12" y1="22.08" x2="12" y2="12" />
  </svg>
);

const IconReport = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="18" y1="20" x2="18" y2="10" />
    <line x1="12" y1="20" x2="12" y2="4" />
    <line x1="6" y1="20" x2="6" y2="14" />
  </svg>
);

const IconStaff = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
    <circle cx="9" cy="7" r="4" />
    <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
    <path d="M16 3.13a4 4 0 0 1 0 7.75" />
  </svg>
);

const IconSearch = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#B0A69B" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="11" cy="11" r="8" />
    <line x1="21" y1="21" x2="16.65" y2="16.65" />
  </svg>
);

export const PosScreen: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  // Products & Filtering State
  const [products, setProducts] = useState<Product[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedCategory, setSelectedCategory] = useState('Tất cả');
  const [searchQuery, setSearchQuery] = useState('');

  // Cart State (Initialized with Figma Screen 02 sample products)
  const [cart, setCart] = useState<CartItem[]>([]);
  const [draftCount, setDraftCount] = useState(3);
  const initializedDemoCartRef = useRef(false);

  // Customer State
  const [customer, setCustomer] = useState<Customer>(DEFAULT_CUSTOMER);
  const [isSearchingCustomer, setIsSearchingCustomer] = useState(false);
  const [customerPhoneInput, setCustomerPhoneInput] = useState('0901 234 567');

  // Discount & Promotion State
  const [appliedVoucher] = useState<{ code: string; discountPercent: number } | null>({
    code: 'QUATANG10',
    discountPercent: 10,
  });
  const [useLoyaltyPoints, setUseLoyaltyPoints] = useState(true);

  // Toast Feedback State
  const [toastMessage, setToastMessage] = useState<{ text: string; type: 'success' | 'info' } | null>(null);

  const showToast = (text: string, type: 'success' | 'info' = 'success') => {
    setToastMessage({ text, type });
    setTimeout(() => {
      setToastMessage(null);
    }, 3000);
  };

  // Determine user role
  const isManager = useMemo(() => {
    const role = (user?.vaiTro || '').toLowerCase();
    return role.includes('quản lý') || role.includes('quanly') || role.includes('admin');
  }, [user]);

  // Fetch products dynamically from GET /api/products
  useEffect(() => {
    let ignore = false;

    getProducts({
      category: selectedCategory,
      search: searchQuery,
    })
      .then((data) => {
        if (!ignore) {
          setProducts(data);
          setIsLoading(false);

          // Populate initial Figma Screen 02 demo items on first load
          if (!initializedDemoCartRef.current && data.length > 0) {
            initializedDemoCartRef.current = true;
            const teddy = data.find((p) => p.maSP === 'GB001');
            const lysu = data.find((p) => p.maSP === 'LY002');
            const demoItems: CartItem[] = [];
            if (teddy) demoItems.push({ product: teddy, quantity: 1 });
            if (lysu) demoItems.push({ product: lysu, quantity: 2 });
            if (demoItems.length > 0) {
              setCart(demoItems);
            }
          }
        }
      })
      .catch(() => {
        if (!ignore) {
          setIsLoading(false);
        }
      });

    return () => {
      ignore = true;
    };
  }, [selectedCategory, searchQuery]);

  // Date formatting
  const todayFormatted = useMemo(() => {
    const now = new Date();
    return now.toLocaleDateString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  }, []);

  // Format currency VND
  const formatVND = (amount: number) => {
    return new Intl.NumberFormat('vi-VN').format(amount) + ' đ';
  };

  // Cart operations
  const handleAddToCart = (product: Product) => {
    if (product.soLuongTon <= 0) {
      showToast('Sản phẩm đã hết hàng trong kho!', 'info');
      return;
    }

    setCart((prevCart) => {
      const existing = prevCart.find((item) => item.product.id === product.id);
      if (existing) {
        if (existing.quantity >= product.soLuongTon) {
          showToast(`Đã đạt số lượng tồn kho tối đa (${product.soLuongTon})!`, 'info');
          return prevCart;
        }
        return prevCart.map((item) =>
          item.product.id === product.id
            ? { ...item, quantity: item.quantity + 1 }
            : item
        );
      }
      return [...prevCart, { product, quantity: 1 }];
    });

    showToast(`Đã thêm "${product.tenSP}" vào giỏ`);
  };

  const handleUpdateQuantity = (productId: number, delta: number) => {
    setCart((prevCart) => {
      return prevCart
        .map((item) => {
          if (item.product.id === productId) {
            const newQty = item.quantity + delta;
            if (newQty <= 0) return null;
            if (newQty > item.product.soLuongTon) {
              showToast(`Số lượng tồn chỉ còn ${item.product.soLuongTon}!`, 'info');
              return item;
            }
            return { ...item, quantity: newQty };
          }
          return item;
        })
        .filter((item): item is CartItem => item !== null);
    });
  };

  const handleRemoveItem = (productId: number) => {
    setCart((prevCart) => prevCart.filter((item) => item.product.id !== productId));
  };

  const handleClearCart = () => {
    setCart([]);
    showToast('Đã bắt đầu đơn mới (giỏ hàng trống)', 'info');
  };

  // Customer handlers
  const handleCustomerSearch = async (e: React.FormEvent) => {
    e.preventDefault();
    const cleanDigits = customerPhoneInput.replace(/\D/g, '');
    if (!cleanDigits) return;

    try {
      const found = await findCustomerByPhone(cleanDigits);
      if (found) {
        setCustomer({
          hoTen: found.tenKhachHang,
          soDienThoai: found.soDienThoai,
          diemThuong: found.diemTichLuy,
          isGuest: false,
        });
        setIsSearchingCustomer(false);
        showToast(`Tìm thấy khách hàng: ${found.tenKhachHang} (⭐ ${found.diemTichLuy} điểm)`);
      } else {
        showToast(`Chưa có hồ sơ cho SĐT ${customerPhoneInput}. Bạn có thể tạo mới trong trang Khách hàng.`, 'info');
      }
    } catch {
      showToast('Có lỗi khi tra cứu thông tin khách hàng.', 'info');
    }
  };

  // Financial calculations matching Figma Screen 02
  const totalItemsCount = useMemo(() => {
    return cart.reduce((sum, item) => sum + item.quantity, 0);
  }, [cart]);

  const subtotal = useMemo(() => {
    return cart.reduce((sum, item) => sum + item.product.giaBan * item.quantity, 0);
  }, [cart]);

  const voucherDiscount = useMemo(() => {
    if (!appliedVoucher || cart.length === 0) return 0;
    return Math.round((subtotal * appliedVoucher.discountPercent) / 100);
  }, [subtotal, appliedVoucher, cart]);

  const pointsDiscount = useMemo(() => {
    if (!useLoyaltyPoints || customer.isGuest || cart.length === 0) return 0;
    // 50 points = 5.000 VND as per Figma design spec
    return 5000;
  }, [useLoyaltyPoints, customer, cart]);

  const totalDiscount = useMemo(() => {
    return voucherDiscount + pointsDiscount;
  }, [voucherDiscount, pointsDiscount]);

  const grandTotal = useMemo(() => {
    return Math.max(0, subtotal - totalDiscount);
  }, [subtotal, totalDiscount]);

  // Actions
  const handleSaveDraft = () => {
    if (cart.length === 0) {
      showToast('Giỏ hàng trống, không thể lưu nháp!', 'info');
      return;
    }
    setDraftCount((prev) => prev + 1);
    setCart([]);
    showToast(`Đã lưu đơn nháp #${draftCount + 1} thành công!`);
  };

  const handleCheckout = () => {
    if (cart.length === 0) {
      showToast('Vui lòng chọn sản phẩm trước khi thanh toán!', 'info');
      return;
    }
    showToast(`Thanh toán thành công ${formatVND(grandTotal)} cho đơn hàng!`);
    setCart([]);
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="pos-layout">
      {/* ========================================================
          CỘT 1: SIDEBAR BÊN TRÁI (Clean Minimalist Navigation)
          ======================================================== */}
      <aside className="pos-sidebar">
        <div className="sidebar-top-section">
          {/* Brand & Role Header */}
          <div className="sidebar-header">
            <div className="sidebar-brand-name">MERA · SOUVENIR</div>
            <div className="sidebar-brand-sub">Quản lý cửa hàng</div>
            <div className="sidebar-badge-wrapper">
              <span className={`sidebar-role-badge ${isManager ? 'quanly' : ''}`}>
                {isManager ? 'QUẢN LÝ' : 'NHÂN VIÊN'}
              </span>
            </div>
          </div>

          {/* Navigation Menu */}
          <nav className="sidebar-nav">
            <div className="nav-section-title">KHÔNG GIAN LÀM VIỆC</div>
            <button type="button" className="nav-item active">
              <span className="nav-icon"><IconPos /></span>
              <span>Bán hàng POS</span>
            </button>
            <button
              type="button"
              className="nav-item"
              onClick={() => showToast(`Có ${draftCount} đơn hàng đang lưu nháp.`, 'info')}
            >
              <span className="nav-icon"><IconDraft /></span>
              <span>Đơn nháp ({draftCount})</span>
            </button>
            <button
              type="button"
              className="nav-item"
              onClick={() => showToast('Mở giao diện Chấm công (In/Out)...', 'info')}
            >
              <span className="nav-icon"><IconTime /></span>
              <span>Chấm công</span>
            </button>
            <button
              type="button"
              className="nav-item"
              onClick={() => showToast('Mở giao diện Mở/Chốt ca...', 'info')}
            >
              <span className="nav-icon"><IconShift /></span>
              <span>Mở/Chốt ca</span>
            </button>
            <button
              type="button"
              className="nav-item"
              onClick={() => navigate('/customers')}
            >
              <span className="nav-icon"><IconCustomer /></span>
              <span>Khách hàng</span>
            </button>
            <button
              type="button"
              className="nav-item"
              onClick={() => showToast('Mở quản lý danh mục Sản phẩm...', 'info')}
            >
              <span className="nav-icon"><IconProduct /></span>
              <span>Sản phẩm</span>
            </button>

            {/* RBAC Section: Managers only */}
            {isManager && (
              <>
                <div className="nav-section-title" style={{ marginTop: '16px' }}>QUẢN TRỊ</div>
                <button
                  type="button"
                  className="nav-item"
                  onClick={() => showToast('Mở Báo cáo doanh thu...', 'info')}
                >
                  <span className="nav-icon"><IconReport /></span>
                  <span>Báo cáo doanh thu</span>
                </button>
                <button
                  type="button"
                  className="nav-item"
                  onClick={() => navigate('/users')}
                >
                  <span className="nav-icon"><IconStaff /></span>
                  <span>Quản lý nhân viên</span>
                </button>
              </>
            )}
          </nav>
        </div>

        {/* Sidebar Footer User Box */}
        <div className="sidebar-footer">
          <div className="user-profile-box">
            <div className="user-display-name">
              {user?.hoTen || user?.tenDangNhap || 'Trần Thu Hà'}
            </div>
            <div className="user-shift-text">Nhân viên · Ca sáng</div>
            <button type="button" className="user-logout-link" onClick={handleLogout}>
              Đăng xuất
            </button>
          </div>
        </div>
      </aside>

      {/* ========================================================
          CỘT 2: KHU VỰC TRUNG TÂM (LƯỚI SẢN PHẨM POS)
          ======================================================== */}
      <main className="pos-center-area">
        {/* Top Header */}
        <header className="pos-top-bar">
          <div className="top-bar-left">
            <div className="breadcrumb-row">
              <span className="breadcrumb-path">Cửa hàng Đồ lưu niệm / Bán hàng POS</span>
            </div>
            <h1 className="pos-page-title">Bán hàng POS</h1>
            <div className="pos-page-subtitle">
              Ca sáng #CA1611-01 · Thu ngân: {user?.hoTen || user?.tenDangNhap || 'Trần Thu Hà'}
            </div>
          </div>

          <div className="top-bar-right">
            <div className="live-status-indicator">
              <span className="status-dot" />
              <span>Đang kết nối</span>
              <span>·</span>
              <span>{todayFormatted}</span>
            </div>
            <div className="action-buttons-row">
              <button
                type="button"
                className="btn-draft-top"
                onClick={() => showToast(`Danh sách ${draftCount} đơn nháp sẵn sàng.`, 'info')}
              >
                Đơn nháp ({draftCount})
              </button>
              <button
                type="button"
                className="btn-new-order-top"
                onClick={handleClearCart}
              >
                Đơn mới
              </button>
            </div>
          </div>
        </header>

        {/* Flat Peach Search Bar */}
        <div className="pos-search-container">
          <span className="search-icon-inside"><IconSearch /></span>
          <input
            type="text"
            className="pos-search-input"
            placeholder="Quét mã vạch/tìm sản phẩm"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>

        {/* Horizontal Category Tabs */}
        <div className="pos-category-tabs" role="tablist">
          {CATEGORIES.map((cat) => (
            <button
              key={cat}
              type="button"
              role="tab"
              aria-selected={selectedCategory === cat}
              className={`category-pill ${selectedCategory === cat ? 'active' : ''}`}
              onClick={() => setSelectedCategory(cat)}
            >
              {cat}
            </button>
          ))}
        </div>

        {/* Product Grid Area (3 Columns strictly per Figma) */}
        <div className="pos-products-container">
          {isLoading ? (
            <div className="pos-loading-state">
              <div className="loading-spinner" />
              <span>Đang tải sản phẩm...</span>
            </div>
          ) : products.length === 0 ? (
            <div className="pos-empty-state">
              <span>Không tìm thấy sản phẩm nào phù hợp.</span>
            </div>
          ) : (
            <div className="products-grid-3col">
              {products.map((p) => {
                const isOutOfStock = p.soLuongTon <= 0;
                return (
                  <article
                    key={p.id}
                    className="product-card"
                    onClick={() => handleAddToCart(p)}
                  >
                    <div className="product-image-container">
                      <img
                        src={p.hinhAnh || '/products/teddy.png'}
                        alt={p.tenSP}
                        className="product-image"
                        onError={(e) => {
                          (e.target as HTMLImageElement).src = '/products/teddy.png';
                        }}
                      />
                    </div>

                    <div className="product-info-block">
                      <h3 className="product-title">{p.tenSP}</h3>
                      <div className="product-sku-stock">
                        SKU: {p.maSP} · Tồn kho: {p.soLuongTon}
                      </div>
                      <div className="product-bottom-line">
                        <span className="product-price">{formatVND(p.giaBan)}</span>
                        <button
                          type="button"
                          className="product-add-circle"
                          disabled={isOutOfStock}
                          onClick={(e) => {
                            e.stopPropagation();
                            handleAddToCart(p);
                          }}
                          aria-label={`Thêm ${p.tenSP}`}
                        >
                          +
                        </button>
                      </div>
                    </div>
                  </article>
                );
              })}
            </div>
          )}

          {/* Grid Footer Subtitle */}
          {!isLoading && products.length > 0 && (
            <div className="products-grid-footer">
              Hiển thị {products.length} / 128 sản phẩm · Quét mã để thêm nhanh vào giỏ
            </div>
          )}
        </div>
      </main>

      {/* ========================================================
          CỘT 3: KHUNG GIỎ HÀNG BÊN PHẢI (CART & SUMMARY)
          ======================================================== */}
      <aside className="pos-cart-panel">
        {/* Cart Header */}
        <div className="cart-header">
          <h2 className="cart-title">Giỏ hàng</h2>
          <span className="cart-count-label">{totalItemsCount} sản phẩm</span>
        </div>

        {/* Customer Input & Info */}
        <div className="customer-box">
          {isSearchingCustomer ? (
            <form onSubmit={handleCustomerSearch} className="customer-phone-form">
              <input
                type="text"
                className="customer-phone-input"
                value={customerPhoneInput}
                onChange={(e) => setCustomerPhoneInput(e.target.value)}
                placeholder="SĐT khách hàng: 0901 234 567"
                autoFocus
              />
              <button type="submit" className="customer-btn-save">Lưu</button>
              <button
                type="button"
                className="customer-btn-cancel"
                onClick={() => setIsSearchingCustomer(false)}
              >
                Hủy
              </button>
            </form>
          ) : (
            <>
              <div
                className="customer-phone-display"
                onClick={() => setIsSearchingCustomer(true)}
                title="Bấm để sửa số điện thoại"
              >
                SĐT khách hàng: {customer.soDienThoai || '0901 234 567'}
              </div>

              <div className="customer-meta-row">
                <span className="customer-name">{customer.hoTen}</span>
                <div style={{ display: 'flex', gap: '8px' }}>
                  <button
                    type="button"
                    className="customer-action-link"
                    onClick={() => setIsSearchingCustomer(true)}
                  >
                    Tìm SĐT
                  </button>
                  <button
                    type="button"
                    className="customer-action-link"
                    onClick={() => navigate('/customers')}
                    title="Mở màn hình Quản lý khách hàng"
                  >
                    Hồ sơ
                  </button>
                </div>
              </div>

              <div className="customer-meta-row">
                <span className="customer-points">
                  Điểm tích lũy: {customer.diemThuong} điểm
                </span>
                <button
                  type="button"
                  className="customer-action-link guest"
                  onClick={() => {
                    if (customer.isGuest) {
                      setCustomer(DEFAULT_CUSTOMER);
                    } else {
                      setCustomer(GUEST_CUSTOMER);
                      setUseLoyaltyPoints(false);
                    }
                  }}
                >
                  {customer.isGuest ? 'Chọn lại VIP' : 'Khách vãng lai'}
                </button>
              </div>
            </>
          )}
        </div>

        {/* Cart Items List */}
        <div className="cart-items-scroll">
          {cart.length === 0 ? (
            <div className="cart-empty-state">
              <p>Chưa có sản phẩm trong giỏ.</p>
              <span>Bấm (+) trên sản phẩm để thêm vào đơn.</span>
            </div>
          ) : (
            cart.map((item) => (
              <div key={item.product.id} className="cart-item-row">
                <div className="cart-item-top">
                  <span className="cart-item-name">{item.product.tenSP}</span>
                  <span className="cart-item-price">
                    {formatVND(item.product.giaBan * item.quantity)}
                  </span>
                </div>

                <div className="cart-item-bottom">
                  <div className="cart-stepper">
                    <button
                      type="button"
                      className="stepper-btn"
                      onClick={() => handleUpdateQuantity(item.product.id, -1)}
                    >
                      −
                    </button>
                    <span className="stepper-value">{item.quantity}</span>
                    <button
                      type="button"
                      className="stepper-btn"
                      disabled={item.quantity >= item.product.soLuongTon}
                      onClick={() => handleUpdateQuantity(item.product.id, 1)}
                    >
                      +
                    </button>
                  </div>

                  <button
                    type="button"
                    className="cart-remove-link"
                    onClick={() => handleRemoveItem(item.product.id)}
                  >
                    Xóa
                  </button>
                </div>
              </div>
            ))
          )}
        </div>

        {/* Discount Box (Áp dụng giảm giá) */}
        <div className="cart-discount-box">
          <div className="discount-box-title">Áp dụng giảm giá</div>
          <div className="discount-box-line">
            Voucher: {appliedVoucher ? `${appliedVoucher.code} / Giảm theo %: ${appliedVoucher.discountPercent}%` : 'Không có'}
          </div>
          {voucherDiscount > 0 && (
            <div className="discount-box-line">
              Giảm giá đơn hàng: -{formatVND(voucherDiscount)}
            </div>
          )}
          <div className="discount-box-redeem-row">
            <span
              className={`redeem-toggle-text ${useLoyaltyPoints ? 'active' : ''}`}
              onClick={() => {
                if (customer.isGuest) {
                  showToast('Khách vãng lai không có điểm thưởng!', 'info');
                  return;
                }
                setUseLoyaltyPoints((prev) => !prev);
              }}
            >
              Đổi điểm: 50 điểm = 5.000 đ · {useLoyaltyPoints ? 'Đã áp dụng' : 'Áp dụng'}
            </span>
          </div>
        </div>

        {/* Order Summary */}
        <div className="cart-summary-section">
          <div className="summary-row">
            <span className="summary-label">Tạm tính</span>
            <span className="summary-val">{formatVND(subtotal)}</span>
          </div>

          <div className="summary-row">
            <span className="summary-label">Giảm giá</span>
            <span className="summary-val discount">
              {totalDiscount > 0 ? `-${formatVND(totalDiscount)}` : '0 đ'}
            </span>
          </div>

          <div className="summary-row total-row">
            <span className="total-label">Tổng thanh toán</span>
            <span className="total-amount">{formatVND(grandTotal)}</span>
          </div>

          {/* Action Buttons */}
          <div className="cart-actions-row">
            <button
              type="button"
              className="btn-cart-draft"
              disabled={cart.length === 0}
              onClick={handleSaveDraft}
            >
              Lưu nháp đơn
            </button>
            <button
              type="button"
              className="btn-cart-checkout"
              disabled={cart.length === 0}
              onClick={handleCheckout}
            >
              Thanh toán
            </button>
          </div>

          <div className="cart-loyalty-footer-note">
            Đơn hoàn tất sẽ cộng 32 điểm cho khách.
          </div>
        </div>
      </aside>

      {/* Toast Feedback Notification */}
      {toastMessage && (
        <div className={`pos-toast ${toastMessage.type}`} role="status">
          <span>{toastMessage.type === 'success' ? '✓' : 'ℹ'}</span>
          <span>{toastMessage.text}</span>
        </div>
      )}
    </div>
  );
};

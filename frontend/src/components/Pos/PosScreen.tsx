import React, { useState, useEffect, useMemo, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context';
import { getProducts } from '../../api/products';
import type { Product } from '../../api/products';
import { checkDiscount, type CheckDiscountResponse } from '../../api/discounts';
import { PaymentModal } from '../Payment/PaymentModal';
import { findCustomerByPhone } from '../../api/customers';
import { OpenShiftModal } from '../Shifts/OpenShiftModal';
import { CloseShiftModal } from '../Shifts/CloseShiftModal';
import { AttendanceModal } from '../Shifts/AttendanceModal';
import { DraftOrdersModal } from './DraftOrdersModal';
import {
  getDraftOrders,
  createDraftOrder,
  deleteDraftOrder,
  type DraftOrderDto,
  type CreateDraftOrderRequest,
} from '../../api/draftOrders';
import './PosScreen.css';
import { shiftApi, type ShiftInfo } from '../../api/shiftApi';

interface CartItem {
  product: Product;
  quantity: number;
}

interface Customer {
  id?: number;
  hoTen: string;
  soDienThoai: string;
  diemThuong: number;
  isGuest: boolean;
}

interface Promotion {
  code: string;
  title: string;
  discountBadge: string;
  description: string;
}

const AVAILABLE_PROMOTIONS: Promotion[] = [
  {
    code: 'QUATANG10',
    title: 'Voucher Quà Tặng Anime',
    discountBadge: 'Giảm 10%',
    description: 'Áp dụng cho mọi đơn hàng quà lưu niệm',
  },
  {
    code: 'GIAM10K',
    title: 'Ưu Đãi Trừ Thẳng',
    discountBadge: 'Giảm 10.000đ',
    description: 'Trừ trực tiếp 10.000đ vào tổng hóa đơn',
  },
  {
    code: 'TRIAN50K',
    title: 'Tri Ân Khách Hàng',
    discountBadge: 'Giảm 50.000đ',
    description: 'Ưu đãi tri ân thành viên cửa hàng',
  },
];

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

// SVG icons
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

  // Cart State (UC04)
  const [cart, setCart] = useState<CartItem[]>([]);

  // Draft Orders State (UC06 - Lưu nháp đơn)
  const [draftOrders, setDraftOrders] = useState<DraftOrderDto[]>([]);
  const [isLoadingDrafts, setIsLoadingDrafts] = useState(false);
  const [isDraftModalOpen, setIsDraftModalOpen] = useState(false);
  const [isSavingDraft, setIsSavingDraft] = useState(false);

  const fetchDrafts = async () => {
    try {
      setIsLoadingDrafts(true);
      const data = await getDraftOrders();
      setDraftOrders(data);
    } catch {
      // silent
    } finally {
      setIsLoadingDrafts(false);
    }
  };

  useEffect(() => {
    fetchDrafts();
  }, []);

  // Customer State
  const [defaultCustomer, setDefaultCustomer] = useState<Customer>(DEFAULT_CUSTOMER);
  const [customer, setCustomer] = useState<Customer>(DEFAULT_CUSTOMER);
  const [isSearchingCustomer, setIsSearchingCustomer] = useState(false);
  const [customerPhoneInput, setCustomerPhoneInput] = useState('0901 234 567');

  useEffect(() => {
    findCustomerByPhone('0901234567')
      .then((found) => {
        if (!found) return;
        const resolved: Customer = {
          id: found.id,
          hoTen: found.tenKhachHang,
          soDienThoai: found.soDienThoai,
          diemThuong: found.diemTichLuy,
          isGuest: false,
        };
        setDefaultCustomer(resolved);
        setCustomer((prev) => (!prev.isGuest && !prev.id ? resolved : prev));
      })
      .catch(() => undefined);
  }, []);

  // Voucher & Discount State (UC05)
  const [voucherInput, setVoucherInput] = useState('');
  const [isCheckingVoucher, setIsCheckingVoucher] = useState(false);
  const [appliedVoucher, setAppliedVoucher] = useState<CheckDiscountResponse | null>(null);
  const [useLoyaltyPoints, setUseLoyaltyPoints] = useState(false);
  const [showVoucherDropdown, setShowVoucherDropdown] = useState(false);
  const voucherContainerRef = useRef<HTMLDivElement>(null);

  // Click outside listener for voucher autocomplete dropdown
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        voucherContainerRef.current &&
        !voucherContainerRef.current.contains(event.target as Node)
      ) {
        setShowVoucherDropdown(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  // Filter available promotions based on user input
  const filteredPromotions = useMemo(() => {
    const q = voucherInput.trim().toUpperCase();
    if (!q) return AVAILABLE_PROMOTIONS;
    return AVAILABLE_PROMOTIONS.filter(
      (p) => p.code.includes(q) || p.title.toUpperCase().includes(q)
    );
  }, [voucherInput]);

  // Toast Feedback State
  const [toastMessage, setToastMessage] = useState<{ text: string; type: 'success' | 'info' | 'error' } | null>(null);

  const showToast = (text: string, type: 'success' | 'info' | 'error' = 'success') => {
    setToastMessage({ text, type });
    setTimeout(() => {
      setToastMessage(null);
    }, 3500);
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

  // ========================================================
  // UC04: THAO TÁC GIỎ HÀNG (THÊM, BỚT, XÓA MÓN, ĐƠN MỚI)
  // ========================================================
  const handleAddToCart = (product: Product) => {
    if (product.soLuongTon <= 0) {
      showToast(`Sản phẩm "${product.tenSP}" đã hết hàng trong kho!`, 'error');
      return;
    }

    setCart((prevCart) => {
      const existing = prevCart.find((item) => item.product.id === product.id);
      if (existing) {
        if (existing.quantity >= product.soLuongTon) {
          showToast(`Đã đạt giới hạn tồn kho tối đa (${product.soLuongTon} cái)!`, 'info');
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
            if (newQty <= 0) {
              // Xóa món khi giảm về 0
              showToast(`Đã bỏ "${item.product.tenSP}" khỏi giỏ hàng.`, 'info');
              return null;
            }
            if (newQty > item.product.soLuongTon) {
              showToast(`Số lượng tồn chỉ còn ${item.product.soLuongTon} sản phẩm!`, 'info');
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
    const item = cart.find((i) => i.product.id === productId);
    setCart((prevCart) => prevCart.filter((i) => i.product.id !== productId));
    if (item) {
      showToast(`Đã xóa "${item.product.tenSP}" khỏi giỏ hàng.`);
    }
  };

  const handleClearCart = () => {
    if (cart.length === 0) return;
    if (window.confirm('Bạn có chắc muốn làm trống giỏ hàng để tạo đơn mới?')) {
      setCart([]);
      setAppliedVoucher(null);
      setVoucherInput('');
      showToast('Đã bắt đầu đơn mới (giỏ hàng trống)', 'info');
    }
  };

  // Quét mã vạch khi nhấn Enter ở ô tìm kiếm
  const handleBarcodeSearch = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && searchQuery.trim()) {
      const query = searchQuery.trim().toUpperCase();
      const found = products.find(
        (p) => p.maSP.toUpperCase() === query || p.tenSP.toUpperCase().includes(query)
      );
      if (found) {
        handleAddToCart(found);
        setSearchQuery('');
      } else {
        showToast(`Không tìm thấy sản phẩm với mã: "${query}"`, 'error');
      }
    }
  };

  // ========================================================
  // UC05: ÁP DỤNG MÃ GIẢM GIÁ (VOUCHER)
  // ========================================================
  const subtotal = useMemo(() => {
    return cart.reduce((sum, item) => sum + item.product.giaBan * item.quantity, 0);
  }, [cart]);

  const handleApplyVoucher = async (codeOverride?: string, e?: React.FormEvent) => {
    if (e) e.preventDefault();

    const code = (codeOverride || voucherInput).trim().toUpperCase();
    if (!code) {
      showToast('Vui lòng nhập mã giảm giá (ví dụ: QUATANG10)', 'info');
      return;
    }

    setVoucherInput(code);
    setShowVoucherDropdown(false);

    if (cart.length === 0) {
      showToast('Giỏ hàng trống! Vui lòng chọn sản phẩm trước khi áp voucher.', 'error');
      return;
    }

    setIsCheckingVoucher(true);
    try {
      // Gọi API POST /api/discounts/check
      const res = await checkDiscount({
        maCode: code,
        tongTienHang: subtotal,
      });

      if (res.hopLe) {
        setAppliedVoucher(res);
        showToast(res.thongBao || `Áp dụng thành công mã ${res.maCode}!`, 'success');
      } else {
        showToast(res.thongBao || 'Mã giảm giá không hợp lệ hoặc đã hết hạn.', 'error');
      }
    } catch {
      showToast('Lỗi khi kiểm tra mã giảm giá với máy chủ.', 'error');
    } finally {
      setIsCheckingVoucher(false);
    }
  };

  const handleRemoveVoucher = () => {
    setAppliedVoucher(null);
    setVoucherInput('');
    showToast('Đã hủy áp dụng mã giảm giá.', 'info');
  };

  // Tính số tiền giảm giá thực tế
  const voucherDiscount = useMemo(() => {
    if (!appliedVoucher || !appliedVoucher.hopLe || cart.length === 0) return 0;
    return appliedVoucher.soTienGiam;
  }, [appliedVoucher, cart]);

  const pointsDiscount = useMemo(() => {
    if (!useLoyaltyPoints || customer.isGuest || cart.length === 0) return 0;
    return 5000; // 50 điểm = 5.000đ theo Figma
  }, [useLoyaltyPoints, customer, cart]);

  const totalDiscount = useMemo(() => {
    return voucherDiscount + pointsDiscount;
  }, [voucherDiscount, pointsDiscount]);

  const grandTotal = useMemo(() => {
    return Math.max(0, subtotal - totalDiscount);
  }, [subtotal, totalDiscount]);

  const totalItemsCount = useMemo(() => {
    return cart.reduce((sum, item) => sum + item.quantity, 0);
  }, [cart]);

  // Customer handlers
  const handleCustomerSearch = async (e: React.FormEvent) => {
    e.preventDefault();
    const cleanDigits = customerPhoneInput.replace(/\D/g, '');
    if (!cleanDigits) return;

    try {
      const found = await findCustomerByPhone(cleanDigits);
      if (found) {
        setCustomer({
          id: found.id,
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

  // UC06: Lưu nháp đơn hàng
  const handleSaveDraft = async () => {
    if (cart.length === 0) {
      showToast('Giỏ hàng trống, không thể lưu nháp!', 'info');
      return;
    }
    try {
      setIsSavingDraft(true);
      const req: CreateDraftOrderRequest = {
        items: cart.map((c) => ({ sanPhamId: c.product.id, soLuong: c.quantity })),
        khachHangId: customer.isGuest ? null : customer.id,
        maGiamGia: appliedVoucher ? appliedVoucher.maCode : undefined,
        diemSuDung: useLoyaltyPoints ? 50 : 0,
        caLamViecId: currentShift?.id,
      };
      const created = await createDraftOrder(req);
      setCart([]);
      setAppliedVoucher(null);
      setVoucherInput('');
      setUseLoyaltyPoints(false);
      await fetchDrafts();
      showToast(`Đã lưu đơn nháp #${created.maDonHang || `NHAP-${created.id}`} thành công!`);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Có lỗi khi lưu đơn nháp!';
      showToast(msg, 'error');
    } finally {
      setIsSavingDraft(false);
    }
  };

  // UC06: Khôi phục đơn nháp lên giỏ hàng
  const handleRestoreDraft = async (draft: DraftOrderDto) => {
    if (cart.length > 0) {
      const confirmRestore = window.confirm(
        'Giỏ hàng hiện tại đang có sản phẩm. Bạn có muốn thay thế giỏ hàng bằng đơn nháp này không?'
      );
      if (!confirmRestore) return;
    }

    const restoredCart: CartItem[] = draft.items.map((item) => {
      const p: Product = products.find((x) => x.id === item.sanPhamId) ?? {
        id: item.sanPhamId,
        maSP: item.maSP,
        tenSP: item.tenSP,
        tenDanhMuc: 'Sản phẩm',
        giaBan: item.donGia,
        soLuongTon: item.soLuongTon,
        hinhAnh: item.imageUrl || undefined,
      };
      return { product: p, quantity: item.soLuong };
    });

    setCart(restoredCart);

    if (draft.khachHangId && draft.tenKhachHang) {
      setCustomer({
        id: draft.khachHangId,
        hoTen: draft.tenKhachHang,
        soDienThoai: draft.soDienThoaiKhach || '',
        diemThuong: 0,
        isGuest: false,
      });
    }

    if (draft.maGiamGia) {
      setVoucherInput(draft.maGiamGia);
      try {
        const res = await checkDiscount({ maCode: draft.maGiamGia, tongTienHang: draft.tongTienHang });
        if (res.hopLe) {
          setAppliedVoucher(res);
        }
      } catch {
        // silent
      }
    } else {
      setAppliedVoucher(null);
      setVoucherInput('');
    }

    setUseLoyaltyPoints(draft.diemSuDung > 0);

    try {
      await deleteDraftOrder(draft.id);
      await fetchDrafts();
    } catch {
      // silent
    }

    setIsDraftModalOpen(false);
    showToast(`Đã khôi phục đơn nháp #${draft.maDonHang || draft.id} vào giỏ hàng!`);
  };

  // UC06: Xóa đơn nháp
  const handleDeleteDraft = async (draftId: number) => {
    const confirmDel = window.confirm('Bạn có chắc chắn muốn xóa đơn nháp này?');
    if (!confirmDel) return;

    try {
      await deleteDraftOrder(draftId);
      await fetchDrafts();
      showToast('Đã xóa đơn nháp thành công!');
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Không thể xóa đơn nháp!';
      showToast(msg, 'error');
    }
  };

  // UC07: mở Modal thanh toán (components/Payment/PaymentModal.tsx - Người 3)
  const [isPaymentOpen, setIsPaymentOpen] = useState(false);

  // UC02 & UC03: Ca làm việc & Chấm công
  const [currentShift, setCurrentShift] = useState<ShiftInfo | null>(null);
  const [isShiftLoaded, setIsShiftLoaded] = useState(false);
  const [isOpenShiftModal, setIsOpenShiftModal] = useState(false);
  const [isCloseShiftModal, setIsCloseShiftModal] = useState(false);
  const [isAttendanceModal, setIsAttendanceModal] = useState(false);

  const fetchCurrentShift = async () => {
    try {
      const data = await shiftApi.getCurrentShift();
      setCurrentShift(data);
      if (!data) {
        setIsOpenShiftModal(true); // Tự động bật popup Mở ca nếu chưa có ca
      }
    } catch {
      // Bỏ qua nếu lỗi mạng tạm thời
    } finally {
      setIsShiftLoaded(true);
    }
  };

  useEffect(() => {
    fetchCurrentShift();
  }, []);

  const handleCheckout = () => {
    if (cart.length === 0) {
      showToast('Vui lòng chọn sản phẩm trước khi thanh toán!', 'error');
      return;
    }
    setIsPaymentOpen(true);
  };

  const handlePaymentSuccess = (order: { maDonHang: string; tongTienThanhToan: number }) => {
    setIsPaymentOpen(false);
    showToast(`Đơn ${order.maDonHang}: thanh toán thành công ${formatVND(order.tongTienThanhToan)}!`);
    setCart([]);
    setAppliedVoucher(null);
    setVoucherInput('');
    setUseLoyaltyPoints(false);
    // Tải lại tồn kho sau khi đã trừ
    getProducts({ category: selectedCategory, search: searchQuery }).then(setProducts).catch(() => undefined);
    // Cập nhật lại điểm tích lũy khách hàng sau khi thanh toán
    if (!customer.isGuest && customer.soDienThoai) {
      const cleanDigits = customer.soDienThoai.replace(/\D/g, '');
      if (cleanDigits) {
        findCustomerByPhone(cleanDigits)
          .then((found) => {
            if (found) {
              setCustomer((prev) =>
                !prev.isGuest ? { ...prev, id: found.id, diemThuong: found.diemTichLuy } : prev
              );
            }
          })
          .catch(() => undefined);
      }
    }
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="pos-layout">
      {/* ========================================================
          CỘT 1: SIDEBAR BÊN TRÁI
          ======================================================== */}
      <aside className="pos-sidebar">
        <div className="sidebar-top-section">
          <div className="sidebar-header">
            <div className="sidebar-brand-name">MERA · SOUVENIR</div>
            <div className="sidebar-brand-sub">Quản lý cửa hàng quà tặng</div>
            <div className="sidebar-badge-wrapper">
              <span className={`sidebar-role-badge ${isManager ? 'quanly' : ''}`}>
                {isManager ? 'QUẢN LÝ' : 'NHÂN VIÊN'}
              </span>
            </div>
          </div>

          <nav className="sidebar-nav">
            <div className="nav-section-title">KHÔNG GIAN LÀM VIỆC</div>
            <button type="button" className="nav-item active">
              <span className="nav-icon"><IconPos /></span>
              <span>Bán hàng POS</span>
            </button>
            <button
              type="button"
              className="nav-item"
              onClick={() => setIsDraftModalOpen(true)}
            >
              <span className="nav-icon"><IconDraft /></span>
              <span>Đơn nháp ({draftOrders.length})</span>
            </button>
            <button
              type="button"
              className="nav-item"
              onClick={() => setIsAttendanceModal(true)}
            >
              <span className="nav-icon"><IconTime /></span>
              <span>Chấm công</span>
            </button>
            <button
              type="button"
              className="nav-item"
              onClick={() => {
                if (currentShift) {
                  setIsCloseShiftModal(true);
                } else {
                  setIsOpenShiftModal(true);
                }
              }}
            >
              <span className="nav-icon"><IconShift /></span>
              <span>{currentShift ? `Chốt ca (#${currentShift.id})` : 'Mở ca'}</span>
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
                  onClick={() => navigate('/returns')}
                >
                  <span className="nav-icon"><IconDraft /></span>
                  <span>Trả hàng / Hoàn tiền</span>
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
                onClick={() => setIsDraftModalOpen(true)}
              >
                Đơn nháp ({draftOrders.length})
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

        {/* Ô tìm kiếm & quét mã vạch */}
        <div className="pos-search-container">
          <span className="search-icon-inside"><IconSearch /></span>
          <input
            type="text"
            className="pos-search-input"
            placeholder="Quét mã vạch hoặc gõ tên sản phẩm (Nhấn Enter để thêm nhanh)..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            onKeyDown={handleBarcodeSearch}
          />
        </div>

        {/* Danh mục */}
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

        {/* Lưới sản phẩm */}
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

          {!isLoading && products.length > 0 && (
            <div className="products-grid-footer">
              Hiển thị {products.length} sản phẩm · Nhấp chọn hoặc quét mã để thêm vào giỏ
            </div>
          )}
        </div>
      </main>

      {/* ========================================================
          CỘT 3: KHUNG GIỎ HÀNG BÊN PHẢI (CART & VOUCHER UC04, UC05)
          ======================================================== */}
      <aside className="pos-cart-panel">
        <div className="cart-header">
          <h2 className="cart-title">Giỏ hàng</h2>
          <div className="cart-header-actions">
            <span className="cart-count-label">{totalItemsCount} món</span>
            {cart.length > 0 && (
              <button
                type="button"
                className="btn-clear-cart-link"
                onClick={handleClearCart}
                title="Xóa toàn bộ món trong giỏ"
              >
                Xóa tất cả
              </button>
            )}
          </div>
        </div>

        {/* Khách hàng */}
        <div className="customer-box">
          {isSearchingCustomer ? (
            <form onSubmit={handleCustomerSearch} className="customer-phone-form">
              <input
                type="text"
                className="customer-phone-input"
                value={customerPhoneInput}
                onChange={(e) => setCustomerPhoneInput(e.target.value)}
                placeholder="SĐT: 0901 234 567"
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
                      setCustomer(defaultCustomer);
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

        {/* UC04: Danh sách sản phẩm trong giỏ hàng */}
        <div className="cart-items-scroll">
          {cart.length === 0 ? (
            <div className="cart-empty-state">
              <div style={{ fontSize: '28px', marginBottom: '4px' }}>🛍️</div>
              <p style={{ fontWeight: 600, color: '#4B453E', margin: 0 }}>Giỏ hàng đang trống</p>
              <span style={{ fontSize: '11px', color: '#8C827A' }}>Nhấp chọn các sản phẩm bên cạnh để thêm vào đơn bán.</span>
            </div>
          ) : (
            cart.map((item) => (
              <div key={item.product.id} className="cart-item-card">
                <img
                  src={item.product.hinhAnh || '/products/teddy.png'}
                  alt={item.product.tenSP}
                  className="cart-item-thumb"
                  onError={(e) => {
                    (e.target as HTMLImageElement).src = '/products/teddy.png';
                  }}
                />
                <div className="cart-item-content">
                  <div className="cart-item-top">
                    <span className="cart-item-name">{item.product.tenSP}</span>
                    <span className="cart-item-price">
                      {formatVND(item.product.giaBan * item.quantity)}
                    </span>
                  </div>
                  <div className="cart-item-subline">
                    Đơn giá: {formatVND(item.product.giaBan)} · Tồn: {item.product.soLuongTon}
                  </div>
                  <div className="cart-item-bottom">
                    <div className="cart-stepper">
                      <button
                        type="button"
                        className="stepper-btn"
                        onClick={() => handleUpdateQuantity(item.product.id, -1)}
                        title="Giảm số lượng"
                      >
                        −
                      </button>
                      <span className="stepper-value">{item.quantity}</span>
                      <button
                        type="button"
                        className="stepper-btn"
                        disabled={item.quantity >= item.product.soLuongTon}
                        onClick={() => handleUpdateQuantity(item.product.id, 1)}
                        title={item.quantity >= item.product.soLuongTon ? 'Đã hết hàng tồn' : 'Tăng số lượng'}
                      >
                        +
                      </button>
                    </div>
                    <button
                      type="button"
                      className="cart-remove-link"
                      onClick={() => handleRemoveItem(item.product.id)}
                      title="Xóa món này khỏi giỏ"
                    >
                      Xóa món
                    </button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>

        {/* ========================================================
            UC05: Ô NHẬP VOUCHER & TÍNH TIỀN GIẢM GIÁ
            ======================================================== */}
        <div className="cart-discount-box">
          <div className="discount-box-title-row">
            <span className="discount-box-title">Khuyến mãi & Voucher</span>
          </div>

          {appliedVoucher && appliedVoucher.hopLe ? (
            /* Khi voucher đã được áp dụng hợp lệ */
            <div className="voucher-applied-card">
              <div className="voucher-applied-info">
                <span className="voucher-applied-code-badge">
                  ✓ {appliedVoucher.maCode}
                </span>
                <span className="voucher-applied-desc">
                  {appliedVoucher.tenVoucher} (-{formatVND(appliedVoucher.soTienGiam)})
                </span>
              </div>
              <button
                type="button"
                className="voucher-btn-remove"
                onClick={handleRemoveVoucher}
                title="Hủy áp dụng mã này"
              >
                ✕ Bỏ mã
              </button>
            </div>
          ) : (
            /* Ô nhập mã Voucher khi chưa áp dụng (chuẩn Figma 100% kèm Autocomplete Dropdown) */
            <div className="voucher-input-wrapper" ref={voucherContainerRef}>
              <form onSubmit={(e) => handleApplyVoucher(undefined, e)} className="voucher-input-group">
                <input
                  type="text"
                  className="voucher-input"
                  placeholder="Nhập mã voucher (vd: QUATANG10)"
                  value={voucherInput}
                  onChange={(e) => {
                    setVoucherInput(e.target.value.toUpperCase());
                    setShowVoucherDropdown(true);
                  }}
                  onFocus={() => setShowVoucherDropdown(true)}
                  disabled={isCheckingVoucher}
                  autoComplete="off"
                />
                <button
                  type="submit"
                  className="voucher-btn-apply"
                  disabled={isCheckingVoucher || !voucherInput.trim()}
                >
                  {isCheckingVoucher ? '...' : 'Áp dụng'}
                </button>
              </form>

              {/* Autocomplete Dropdown - Xổ xuống khi nhấp vào ô nhập */}
              {showVoucherDropdown && (
                <div className="voucher-dropdown-menu">
                  <div className="voucher-dropdown-header">
                    <span>🏷️ Mã ưu đãi hiện có</span>
                    <span className="voucher-dropdown-hint">Bấm để dùng nhanh</span>
                  </div>
                  <div className="voucher-dropdown-list">
                    {filteredPromotions.map((promo) => (
                      <div
                        key={promo.code}
                        className="voucher-dropdown-item"
                        onClick={() => handleApplyVoucher(promo.code)}
                      >
                        <div className="voucher-dropdown-item-left">
                          <span className="voucher-dropdown-code">{promo.code}</span>
                          <span className="voucher-dropdown-title">{promo.title}</span>
                          <span className="voucher-dropdown-desc">{promo.description}</span>
                        </div>
                        <div className="voucher-dropdown-item-right">
                          <span className="voucher-dropdown-badge">{promo.discountBadge}</span>
                          <span className="voucher-dropdown-use-btn">Dùng</span>
                        </div>
                      </div>
                    ))}
                    {filteredPromotions.length === 0 && (
                      <div className="voucher-dropdown-empty">
                        Không tìm thấy mã nào phù hợp với "{voucherInput}"
                      </div>
                    )}
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Đổi điểm thưởng tích lũy */}
          <div className="discount-box-redeem-row">
            <span
              className={`redeem-toggle-text ${useLoyaltyPoints ? 'active' : ''}`}
              onClick={() => {
                if (customer.isGuest) {
                  showToast('Khách vãng lai không có điểm thưởng tích lũy!', 'info');
                  return;
                }
                if (!useLoyaltyPoints && customer.diemThuong < 50) {
                  showToast('Khách hàng chưa đủ 50 điểm tích lũy để đổi!', 'info');
                  return;
                }
                setUseLoyaltyPoints((prev) => !prev);
              }}
            >
              Đổi điểm: 50 điểm = 5.000 đ · {useLoyaltyPoints ? '✓ Đã trừ 5.000 đ' : 'Nhấp để áp dụng'}
            </span>
          </div>
        </div>

        {/* Khối tính tiền chi tiết */}
        <div className="cart-summary-section">
          <div className="summary-row">
            <span className="summary-label">Tạm tính ({totalItemsCount} món)</span>
            <span className="summary-val">{formatVND(subtotal)}</span>
          </div>

          {appliedVoucher && voucherDiscount > 0 && (
            <div className="summary-row">
              <span className="summary-label">
                Mã {appliedVoucher.maCode} ({appliedVoucher.loaiGiamGia === 'PhanTram' ? `${appliedVoucher.giaTriGiam}%` : 'Cố định'})
              </span>
              <span className="summary-val discount">
                -{formatVND(voucherDiscount)}
              </span>
            </div>
          )}

          {useLoyaltyPoints && pointsDiscount > 0 && (
            <div className="summary-row">
              <span className="summary-label">Đổi điểm tích lũy</span>
              <span className="summary-val discount">
                -{formatVND(pointsDiscount)}
              </span>
            </div>
          )}

          <div className="summary-row">
            <span className="summary-label">Tổng giảm giá</span>
            <span className="summary-val discount">
              {totalDiscount > 0 ? `-${formatVND(totalDiscount)}` : '0 đ'}
            </span>
          </div>

          <div className="summary-row total-row">
            <span className="total-label">Tổng thanh toán</span>
            <span className="total-amount">{formatVND(grandTotal)}</span>
          </div>

          <div className="cart-actions-row">
            <button
              type="button"
              className="btn-cart-draft"
              disabled={cart.length === 0 || isSavingDraft}
              onClick={handleSaveDraft}
            >
              {isSavingDraft ? 'Đang lưu...' : 'Lưu nháp đơn'}
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
            Đơn hoàn tất sẽ tích lũy thêm điểm thưởng cho khách hàng.
          </div>
        </div>
      </aside>

      {/* UC07: Modal thanh toán */}
      <PaymentModal
        isOpen={isPaymentOpen}
        onClose={() => setIsPaymentOpen(false)}
        onSuccess={handlePaymentSuccess}
        items={cart.map((item) => ({
          sanPhamId: item.product.id,
          tenSP: item.product.tenSP,
          soLuong: item.quantity,
          donGia: item.product.giaBan,
        }))}
        tongTienHang={subtotal}
        soTienGiam={totalDiscount}
        tongThanhToan={grandTotal}
        maGiamGia={appliedVoucher?.hopLe ? appliedVoucher.maCode : undefined}
        khachHangId={customer.isGuest ? undefined : customer.id}
        diemSuDung={useLoyaltyPoints && !customer.isGuest ? 50 : 0}
        caLamViecId={currentShift?.id}
      />

      {/* UC02 & UC03: Modals Ca làm việc & Chấm công */}
      {isShiftLoaded && (
        <OpenShiftModal
          isOpen={isOpenShiftModal}
          onShiftOpened={(newShift) => {
            setCurrentShift(newShift);
            setIsOpenShiftModal(false);
            showToast(`Đã mở ca #${newShift.id} và tự động Check-in!`);
          }}
        />
      )}

      {currentShift && (
        <CloseShiftModal
          shift={currentShift}
          isOpen={isCloseShiftModal}
          onClose={() => setIsCloseShiftModal(false)}
          onShiftClosed={() => {
            setIsCloseShiftModal(false);
            setCurrentShift(null);
            showToast('Đã chốt ca làm việc thành công!');
          }}
        />
      )}

      <AttendanceModal
        isOpen={isAttendanceModal}
        shift={currentShift}
        onClose={() => setIsAttendanceModal(false)}
        onUpdated={() => {
          fetchCurrentShift();
          showToast('Đã cập nhật trạng thái chấm công!');
        }}
      />

      <DraftOrdersModal
        isOpen={isDraftModalOpen}
        onClose={() => setIsDraftModalOpen(false)}
        drafts={draftOrders}
        isLoading={isLoadingDrafts}
        onRestore={handleRestoreDraft}
        onDelete={handleDeleteDraft}
      />

      {/* Toast thông báo phản hồi */}
      {toastMessage && (
        <div className={`pos-toast ${toastMessage.type}`} role="status">
          <span>{toastMessage.type === 'success' ? '✓' : toastMessage.type === 'error' ? '⚠' : 'ℹ'}</span>
          <span>{toastMessage.text}</span>
        </div>
      )}
    </div>
  );
};

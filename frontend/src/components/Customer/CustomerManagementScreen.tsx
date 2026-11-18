import React, { useState, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context';
import {
  getCustomers,
  createCustomer,
  updateCustomer,
  deleteCustomer,
} from '../../api/customers';
import type { Customer } from '../../api/customers';
import { CustomerOrderHistoryModal } from './CustomerOrderHistoryModal';
import './CustomerManagementScreen.css';

// SVG Icons
const IconHistory = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
    <polyline points="14 2 14 8 20 8" />
    <line x1="16" y1="13" x2="8" y2="13" />
    <line x1="16" y1="17" x2="8" y2="17" />
  </svg>
);
const IconSearch = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="11" cy="11" r="8" />
    <line x1="21" y1="21" x2="16.65" y2="16.65" />
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

const IconUsers = () => (
  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
    <circle cx="9" cy="7" r="4" />
    <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
    <path d="M16 3.13a4 4 0 0 1 0 7.75" />
  </svg>
);

const IconAward = () => (
  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="8" r="7" />
    <polyline points="8.21 13.89 7 23 12 20 17 23 15.79 13.88" />
  </svg>
);

const IconStar = () => (
  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2" />
  </svg>
);

const IconTrending = () => (
  <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="23 6 13.5 15.5 8.5 10.5 1 18" />
    <polyline points="17 6 23 6 23 12" />
  </svg>
);

const IconArrowLeft = () => (
  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="19" y1="12" x2="5" y2="12" />
    <polyline points="12 19 5 12 12 5" />
  </svg>
);

// Format Vietnamese Phone Number nicely: 0901 234 567
const formatPhone = (phone: string) => {
  const digits = phone.replace(/\D/g, '');
  if (digits.length === 10) {
    return `${digits.slice(0, 4)} ${digits.slice(4, 7)} ${digits.slice(7)}`;
  }
  return phone;
};

// Calculate Tier based on Points
const getTier = (points: number): { label: string; className: string } => {
  if (points >= 500) return { label: 'Kim Cương', className: 'diamond' };
  if (points >= 200) return { label: 'Vàng', className: 'gold' };
  if (points >= 50) return { label: 'Bạc', className: 'silver' };
  return { label: 'Đồng', className: 'bronze' };
};

export const CustomerManagementScreen: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  // State
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [selectedTier, setSelectedTier] = useState<string>('all');

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);
  const [editingCustomer, setEditingCustomer] = useState<Customer | null>(null);
  const [formData, setFormData] = useState({
    tenKhachHang: '',
    soDienThoai: '',
    diemTichLuy: 0,
  });
  const [formError, setFormError] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  // Delete Confirmation State
  const [deleteTarget, setDeleteTarget] = useState<Customer | null>(null);

  // Order History Modal State (UC09 - Đợt 2)
  const [historyCustomer, setHistoryCustomer] = useState<Customer | null>(null);

  // Toast State
  const [toast, setToast] = useState<{ message: string; type: 'success' | 'error' } | null>(null);

  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  // Load Customers
  useEffect(() => {
    let ignore = false;

    getCustomers()
      .then((data) => {
        if (!ignore) {
          setCustomers(data);
          setIsLoading(false);
        }
      })
      .catch(() => {
        if (!ignore) {
          setToast({ message: 'Không thể tải danh sách khách hàng từ máy chủ', type: 'error' });
          setIsLoading(false);
        }
      });

    return () => {
      ignore = true;
    };
  }, []);

  // Filtered List
  const filteredCustomers = useMemo(() => {
    return customers.filter((c) => {
      const query = searchQuery.trim().toLowerCase();
      const cleanPhone = searchQuery.replace(/\D/g, '');
      const matchSearch =
        !query ||
        c.tenKhachHang.toLowerCase().includes(query) ||
        (cleanPhone && c.soDienThoai.includes(cleanPhone));

      if (!matchSearch) return false;

      if (selectedTier === 'all') return true;
      const tier = getTier(c.diemTichLuy).label.toLowerCase();
      return tier === selectedTier.toLowerCase();
    });
  }, [customers, searchQuery, selectedTier]);

  // Statistics
  const stats = useMemo(() => {
    const total = customers.length;
    const vip = customers.filter((c) => c.diemTichLuy >= 200).length;
    const totalPoints = customers.reduce((sum, c) => sum + (c.diemTichLuy || 0), 0);
    return { total, vip, totalPoints, newThisMonth: Math.min(total, 4) };
  }, [customers]);

  // Open Modal for Add
  const handleOpenAdd = () => {
    setEditingCustomer(null);
    setFormData({ tenKhachHang: '', soDienThoai: '', diemTichLuy: 0 });
    setFormError('');
    setIsModalOpen(true);
  };

  // Open Modal for Edit
  const handleOpenEdit = (customer: Customer) => {
    setEditingCustomer(customer);
    setFormData({
      tenKhachHang: customer.tenKhachHang,
      soDienThoai: customer.soDienThoai,
      diemTichLuy: customer.diemTichLuy,
    });
    setFormError('');
    setIsModalOpen(true);
  };

  // Validate & Submit Form
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError('');

    const cleanPhone = formData.soDienThoai.replace(/\D/g, '');
    if (!cleanPhone || !/^0\d{9}$/.test(cleanPhone)) {
      setFormError('Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0.');
      return;
    }

    if (!formData.tenKhachHang.trim()) {
      setFormError('Họ và tên khách hàng không được để trống.');
      return;
    }

    setIsSubmitting(true);
    try {
      if (editingCustomer) {
        // Update
        const updated = await updateCustomer(editingCustomer.id, {
          soDienThoai: cleanPhone,
          tenKhachHang: formData.tenKhachHang.trim(),
          diemTichLuy: Number(formData.diemTichLuy) || 0,
        });
        setCustomers((prev) =>
          prev.map((c) => (c.id === editingCustomer.id ? { ...c, ...updated } : c))
        );
        showToast(`Đã cập nhật hồ sơ khách hàng ${updated.tenKhachHang}`);
      } else {
        // Create
        const created = await createCustomer({
          soDienThoai: cleanPhone,
          tenKhachHang: formData.tenKhachHang.trim(),
        });
        setCustomers((prev) => [created, ...prev]);
        showToast(`Thêm mới khách hàng ${created.tenKhachHang} thành công!`);
      }
      setIsModalOpen(false);
    } catch (err: unknown) {
      const errorMsg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Có lỗi xảy ra khi lưu thông tin khách hàng.';
      setFormError(errorMsg);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Confirm Delete
  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await deleteCustomer(deleteTarget.id);
      setCustomers((prev) => prev.filter((c) => c.id !== deleteTarget.id));
      showToast(`Đã xóa hồ sơ khách hàng ${deleteTarget.tenKhachHang}`);
      setDeleteTarget(null);
    } catch (err: unknown) {
      const msg =
        (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
        'Không thể xóa khách hàng. Có thể khách đã có đơn hàng.';
      showToast(msg, 'error');
      setDeleteTarget(null);
    }
  };

  return (
    <div className="customer-screen">
      {/* Top Navigation Header */}
      <header className="customer-header">
        <div className="customer-header-left">
          <div className="brand-badge">
            <h1 className="brand-title">MERA · SOUVENIR</h1>
            <span className="brand-subtitle">Quản Lý Khách Hàng (UC09)</span>
          </div>

          <nav className="header-nav">
            <button className="nav-pill-btn" onClick={() => navigate('/pos')}>
              <IconArrowLeft /> Quay lại Bán hàng (POS)
            </button>
            <span className="nav-pill-btn active">Hồ sơ khách hàng</span>
          </nav>
        </div>

        <div className="customer-header-right">
          <div className="user-tag">
            <div className="user-avatar">
              {(user?.hoTen || 'U').charAt(0).toUpperCase()}
            </div>
            <div>
              <div className="user-name">{user?.hoTen || 'Nhân viên'}</div>
              <div className="user-role">{user?.vaiTro || 'Thu ngân'}</div>
            </div>
          </div>

          <button className="btn-logout" onClick={logout}>
            Đăng xuất
          </button>
        </div>
      </header>

      {/* Main Content */}
      <main className="customer-main">
        {/* Title Banner */}
        <div className="page-title-banner">
          <h2 className="page-main-title">Quản Lý Hồ Sơ Khách Hàng</h2>
          <p className="page-description">
            Tra cứu thông tin, tạo hồ sơ khách hàng thân thiết và theo dõi điểm thưởng tích lũy (Chuẩn thiết kế Mera Souvenir).
          </p>
        </div>

        {/* KPI Cards */}
        <div className="kpi-grid">
          <div className="kpi-card">
            <div>
              <div className="kpi-label">Tổng khách hàng</div>
              <div className="kpi-value">{stats.total}</div>
            </div>
            <div className="kpi-icon-wrap tan">
              <IconUsers />
            </div>
          </div>

          <div className="kpi-card">
            <div>
              <div className="kpi-label">Khách thân thiết (VIP)</div>
              <div className="kpi-value">{stats.vip}</div>
            </div>
            <div className="kpi-icon-wrap sage">
              <IconAward />
            </div>
          </div>

          <div className="kpi-card">
            <div>
              <div className="kpi-label">Tổng điểm tích lũy</div>
              <div className="kpi-value">{stats.totalPoints.toLocaleString('vi-VN')}</div>
            </div>
            <div className="kpi-icon-wrap gold">
              <IconStar />
            </div>
          </div>

          <div className="kpi-card">
            <div>
              <div className="kpi-label">Khách mới gần đây</div>
              <div className="kpi-value">+{stats.newThisMonth}</div>
            </div>
            <div className="kpi-icon-wrap peach">
              <IconTrending />
            </div>
          </div>
        </div>

        {/* Action & Filter Bar */}
        <div className="action-bar">
          <div className="search-filter-group">
            <div className="search-input-wrap">
              <span className="search-icon">
                <IconSearch />
              </span>
              <input
                type="text"
                className="search-input"
                placeholder="Tìm kiếm theo Số điện thoại hoặc Họ tên khách..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>

            <select
              className="tier-filter"
              value={selectedTier}
              onChange={(e) => setSelectedTier(e.target.value)}
            >
              <option value="all">Tất cả hạng thành viên</option>
              <option value="kim cương">Hạng Kim Cương (&gt;= 500đ)</option>
              <option value="vàng">Hạng Vàng (&gt;= 200đ)</option>
              <option value="bạc">Hạng Bạc (&gt;= 50đ)</option>
              <option value="đồng">Hạng Đồng (&lt; 50đ)</option>
            </select>
          </div>

          <button className="btn-primary-action" onClick={handleOpenAdd}>
            <IconPlus /> Thêm khách hàng mới
          </button>
        </div>

        {/* Customer Table */}
        <div className="table-card">
          <table className="customers-table">
            <thead>
              <tr>
                <th style={{ width: '80px' }}>Mã KH</th>
                <th>Khách hàng</th>
                <th>Số điện thoại</th>
                <th>Điểm tích lũy</th>
                <th>Hạng thành viên</th>
                <th style={{ textAlign: 'right', width: '220px' }}>Hành động</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={6} style={{ textAlign: 'center', padding: '40px' }}>
                    Đang tải danh sách khách hàng...
                  </td>
                </tr>
              ) : filteredCustomers.length === 0 ? (
                <tr>
                  <td colSpan={6}>
                    <div className="empty-table-state">
                      <div className="empty-icon">👥</div>
                      <div className="empty-title">Không tìm thấy khách hàng nào</div>
                      <p>Vui lòng thử tìm kiếm bằng từ khóa khác hoặc nhấn "Thêm khách hàng mới".</p>
                    </div>
                  </td>
                </tr>
              ) : (
                filteredCustomers.map((cust) => {
                  const tier = getTier(cust.diemTichLuy);
                  return (
                    <tr key={cust.id}>
                      <td style={{ fontWeight: 600, color: '#8C827A' }}>
                        #{String(cust.id).padStart(3, '0')}
                      </td>
                      <td>
                        <div className="customer-name-cell">
                          <div className="avatar-circle">
                            {cust.tenKhachHang.charAt(0).toUpperCase()}
                          </div>
                          <div>
                            <div style={{ fontWeight: 600, color: '#2B2523' }}>
                              {cust.tenKhachHang}
                            </div>
                            <div style={{ fontSize: '12px', color: '#78716C' }}>
                              Đã xác thực hồ sơ
                            </div>
                          </div>
                        </div>
                      </td>
                      <td>
                        <span className="customer-phone">{formatPhone(cust.soDienThoai)}</span>
                      </td>
                      <td>
                        <span className="points-pill">
                          ⭐ {cust.diemTichLuy.toLocaleString('vi-VN')} điểm
                        </span>
                      </td>
                      <td>
                        <span className={`tier-badge ${tier.className}`}>
                          {tier.label}
                        </span>
                      </td>
                      <td>
                        <div className="action-btn-group" style={{ justifyContent: 'flex-end' }}>
                          <button
                            className="btn-history-action"
                            title="Xem lịch sử mua hàng"
                            onClick={() => setHistoryCustomer(cust)}
                          >
                            <IconHistory /> Lịch sử
                          </button>
                          <button
                            className="btn-icon-action"
                            title="Chỉnh sửa thông tin"
                            onClick={() => handleOpenEdit(cust)}
                          >
                            <IconEdit />
                          </button>
                          <button
                            className="btn-icon-action delete"
                            title="Xóa hồ sơ"
                            onClick={() => setDeleteTarget(cust)}
                          >
                            <IconTrash />
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </main>

      {/* Add / Edit Modal */}
      {isModalOpen && (
        <div className="modal-overlay" onClick={() => !isSubmitting && setIsModalOpen(false)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">
                {editingCustomer ? 'Chỉnh Sửa Hồ Sơ Khách Hàng' : 'Thêm Khách Hàng Mới'}
              </h3>
              <button
                className="btn-close-modal"
                onClick={() => !isSubmitting && setIsModalOpen(false)}
              >
                ✕
              </button>
            </div>

            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label className="form-label">Số điện thoại (10 chữ số) *</label>
                <input
                  type="tel"
                  className="form-input"
                  placeholder="Ví dụ: 0901234567"
                  maxLength={11}
                  value={formData.soDienThoai}
                  onChange={(e) =>
                    setFormData({ ...formData, soDienThoai: e.target.value.replace(/[^\d\s]/g, '') })
                  }
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Họ và tên khách hàng *</label>
                <input
                  type="text"
                  className="form-input"
                  placeholder="Ví dụ: Nguyễn Ngọc Lan"
                  value={formData.tenKhachHang}
                  onChange={(e) => setFormData({ ...formData, tenKhachHang: e.target.value })}
                  required
                />
              </div>

              {editingCustomer && (
                <div className="form-group">
                  <label className="form-label">Điểm tích lũy</label>
                  <input
                    type="number"
                    min="0"
                    className="form-input"
                    value={formData.diemTichLuy}
                    onChange={(e) =>
                      setFormData({ ...formData, diemTichLuy: Math.max(0, parseInt(e.target.value, 10) || 0) })
                    }
                  />
                </div>
              )}

              {formError && <div className="form-error">⚠️ {formError}</div>}

              <div className="modal-actions">
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={() => setIsModalOpen(false)}
                  disabled={isSubmitting}
                >
                  Hủy
                </button>
                <button type="submit" className="btn-submit" disabled={isSubmitting}>
                  {isSubmitting ? 'Đang lưu...' : editingCustomer ? 'Cập nhật' : 'Lưu hồ sơ'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Delete Confirmation Modal */}
      {deleteTarget && (
        <div className="modal-overlay" onClick={() => setDeleteTarget(null)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">Xác Nhận Xóa Hồ Sơ</h3>
              <button className="btn-close-modal" onClick={() => setDeleteTarget(null)}>
                ✕
              </button>
            </div>
            <p style={{ fontSize: '14px', color: '#555', lineHeight: '1.6' }}>
              Bạn có chắc chắn muốn xóa hồ sơ khách hàng{' '}
              <strong>{deleteTarget.tenKhachHang}</strong> (SĐT:{' '}
              <code>{formatPhone(deleteTarget.soDienThoai)}</code>)? Hành động này không thể hoàn tác.
            </p>
            <div className="modal-actions">
              <button
                type="button"
                className="btn-secondary"
                onClick={() => setDeleteTarget(null)}
              >
                Hủy bỏ
              </button>
              <button
                type="button"
                className="btn-submit"
                style={{ background: '#D32F2F' }}
                onClick={handleDelete}
              >
                Xóa vĩnh viễn
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Order History Modal (UC09 - Đợt 2) */}
      {historyCustomer && (
        <CustomerOrderHistoryModal
          customer={historyCustomer}
          onClose={() => setHistoryCustomer(null)}
        />
      )}

      {/* Toast Notification */}
      {toast && (
        <div className={`customer-toast ${toast.type}`}>
          {toast.type === 'success' ? '✓' : '⚠️'} {toast.message}
        </div>
      )}
    </div>
  );
};

export default CustomerManagementScreen;

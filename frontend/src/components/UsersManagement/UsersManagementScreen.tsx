import React, { useState, useEffect, useMemo, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { isAxiosError } from 'axios';
import { useAuth } from '../../context';
import type { User, Role } from '../../api/users';
import {
  getUsers,
  getRoles,
  toggleUserStatus,
  deleteUser,
} from '../../api/users';
import { UserModal } from './UserModal';
import { ConfirmModal } from './ConfirmModal';
import type { ConfirmType } from './ConfirmModal';
import './UsersManagementScreen.css';

interface ToastMessage {
  id: number;
  text: string;
  type: 'success' | 'error' | 'info';
}

export const UsersManagementScreen: React.FC = () => {
  const { user: currentUser, logout } = useAuth();
  const navigate = useNavigate();

  // Data states
  const [users, setUsers] = useState<User[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorBanner, setErrorBanner] = useState<string | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  // Filters & search
  const [keyword, setKeyword] = useState('');
  const [selectedRole, setSelectedRole] = useState<number>(0); // 0 = all
  const [selectedStatus, setSelectedStatus] = useState<string>('TatCa'); // TatCa | HoatDong | Khoa

  // Modal states
  const [isUserModalOpen, setIsUserModalOpen] = useState(false);
  const [userToEdit, setUserToEdit] = useState<User | null>(null);

  // Confirmation modal state
  const [confirmModalData, setConfirmModalData] = useState<{
    isOpen: boolean;
    type: ConfirmType;
    user: User | null;
    isLoading: boolean;
  }>({
    isOpen: false,
    type: 'lock',
    user: null,
    isLoading: false,
  });

  // Toast notifications
  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  const showToast = useCallback((text: string, type: 'success' | 'error' | 'info' = 'success') => {
    const id = Date.now() + Math.random();
    setToasts((prev) => [...prev, { id, text, type }]);
    setTimeout(() => {
      setToasts((prev) => prev.filter((t) => t.id !== id));
    }, 4000);
  }, []);

  const removeToast = (id: number) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  };

  // Check RBAC permission for UC15 (Alternative Flow 4a)
  const isManager = useMemo(() => {
    if (!currentUser) return false;
    const roleLower = (currentUser.vaiTro || '').toLowerCase();
    return (
      roleLower.includes('quan') ||
      roleLower.includes('quản') ||
      roleLower.includes('admin') ||
      currentUser.vaiTro === 'QuanLy'
    );
  }, [currentUser]);

  // Load initial roles & users (asynchronous, no synchronous setState in effect body)
  useEffect(() => {
    let ignore = false;

    const fetchData = async () => {
      try {
        const [fetchedRoles, fetchedUsers] = await Promise.all([
          getRoles(),
          getUsers({
            keyword: keyword.trim() || undefined,
            vaiTroId: selectedRole > 0 ? selectedRole : undefined,
            trangThai: selectedStatus !== 'TatCa' ? selectedStatus : undefined,
          }),
        ]);

        if (!ignore) {
          setRoles(fetchedRoles);
          setUsers(fetchedUsers);
          setErrorBanner(null);
          setIsLoading(false);
        }
      } catch (err: unknown) {
        if (!ignore) {
          if (isAxiosError(err) && (err.response?.status === 403 || err.response?.status === 401)) {
            setErrorBanner('Bạn không có quyền quản lý nhân sự. Vui lòng đăng nhập với tài khoản Quản lý.');
          } else if (err instanceof Error) {
            setErrorBanner(err.message);
          } else {
            setErrorBanner('Không thể tải danh sách nhân sự từ máy chủ.');
          }
          setIsLoading(false);
        }
      }
    };

    fetchData();

    return () => {
      ignore = true;
    };
  }, [keyword, selectedRole, selectedStatus, refreshKey]);

  // Quick statistics calculation
  const stats = useMemo(() => {
    const total = users.length;
    const active = users.filter((u) => u.trangThai === 'HoatDong').length;
    const locked = users.filter((u) => u.trangThai === 'Khoa' || u.trangThai === 'BiKhoa').length;
    const managers = users.filter(
      (u) =>
        u.vaiTroId === 1 ||
        u.tenVaiTro === 'QuanLy' ||
        u.tenVaiTro?.toLowerCase().includes('quản lý')
    ).length;
    return { total, active, locked, managers };
  }, [users]);

  // Handler: Open Add Modal
  const handleOpenAddModal = () => {
    setUserToEdit(null);
    setIsUserModalOpen(true);
  };

  // Handler: Open Edit Modal
  const handleOpenEditModal = (user: User) => {
    setUserToEdit(user);
    setIsUserModalOpen(true);
  };

  // Handler: Open Lock/Unlock Confirm Modal
  const handlePromptToggleStatus = (user: User) => {
    const isCurrentlyActive = user.trangThai === 'HoatDong';
    setConfirmModalData({
      isOpen: true,
      type: isCurrentlyActive ? 'lock' : 'unlock',
      user,
      isLoading: false,
    });
  };

  // Handler: Open Delete Confirm Modal
  const handlePromptDelete = (user: User) => {
    setConfirmModalData({
      isOpen: true,
      type: 'delete',
      user,
      isLoading: false,
    });
  };

  // Handler: Execute Confirmation Action
  const handleExecuteConfirm = async () => {
    const targetUser = confirmModalData.user;
    if (!targetUser) return;

    setConfirmModalData((prev) => ({ ...prev, isLoading: true }));

    try {
      if (confirmModalData.type === 'delete') {
        await deleteUser(targetUser.id);
        showToast(`Đã xóa vĩnh viễn tài khoản "${targetUser.hoTen}" khỏi hệ thống.`, 'success');
      } else {
        const nextStatus = confirmModalData.type === 'lock' ? 'Khoa' : 'HoatDong';
        await toggleUserStatus(targetUser.id, nextStatus);
        const actionLabel = confirmModalData.type === 'lock' ? 'Khóa' : 'Mở khóa';
        showToast(`${actionLabel} tài khoản "${targetUser.hoTen}" thành công!`, 'success');
      }

      setConfirmModalData({ isOpen: false, type: 'lock', user: null, isLoading: false });
      setRefreshKey((k) => k + 1);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Thao tác không thành công. Vui lòng thử lại.';
      showToast(msg, 'error');
      setConfirmModalData((prev) => ({ ...prev, isLoading: false }));
    }
  };

  // Reset all filters
  const handleResetFilters = () => {
    setKeyword('');
    setSelectedRole(0);
    setSelectedStatus('TatCa');
  };

  // Avatar initials helper
  const getInitials = (name: string): string => {
    if (!name) return 'U';
    const parts = name.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  };

  // Helper for role badge display
  const isManagerRole = (u: User) => {
    return (
      u.vaiTroId === 1 ||
      u.tenVaiTro === 'QuanLy' ||
      u.tenVaiTro?.toLowerCase().includes('quản lý')
    );
  };

  // Helper for status badge display
  const isActiveStatus = (u: User) => {
    return u.trangThai === 'HoatDong';
  };

  // ========================================================
  // RENDER: RBAC Access Denied View (UC15 Flow 4a)
  // ========================================================
  if (!isManager) {
    return (
      <div className="users-page-container">
        <header className="users-top-bar">
          <div className="users-top-left">
            <button
              type="button"
              className="btn-back-pos"
              onClick={() => navigate('/pos')}
            >
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <line x1="19" y1="12" x2="5" y2="12" />
                <polyline points="12 19 5 12 12 5" />
              </svg>
              <span>Quay lại Bán hàng POS</span>
            </button>
            <div className="brand-breadcrumb">
              <span className="brand-title">MERA SOUVENIR</span>
              <span className="breadcrumb-separator">/</span>
              <span className="breadcrumb-page">Quản lý nhân sự</span>
            </div>
          </div>
        </header>

        <main className="users-main-body">
          <div className="access-denied-card">
            <div className="access-denied-icon">
              <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="#DC2626" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <circle cx="12" cy="12" r="10" />
                <line x1="12" y1="8" x2="12" y2="12" />
                <line x1="12" y1="16" x2="12.01" y2="16" />
              </svg>
            </div>
            <h2 className="access-denied-title">Truy cập bị từ chối</h2>
            <p className="access-denied-desc">
              Tài khoản hiện tại của bạn không có vai trò <strong>Quản lý</strong>. Phân hệ Quản lý nhân sự &amp; Phân quyền (UC15) chỉ dành cho người dùng được cấp thẩm quyền quản trị cửa hàng.
            </p>
            <button
              type="button"
              className="btn-primary-action"
              onClick={() => navigate('/pos')}
            >
              Quay lại Bán hàng (POS)
            </button>
          </div>
        </main>
      </div>
    );
  }

  // ========================================================
  // RENDER: Full Manager View
  // ========================================================
  return (
    <div className="users-page-container">
      {/* Toast notifications */}
      <div className="toast-portal">
        {toasts.map((toast) => (
          <div key={toast.id} className={`toast-item toast-${toast.type}`}>
            <span className="toast-icon">
              {toast.type === 'success' && (
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                  <polyline points="20 6 9 17 4 12" />
                </svg>
              )}
              {toast.type === 'error' && (
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                  <circle cx="12" cy="12" r="10" />
                  <line x1="15" y1="9" x2="9" y2="15" />
                  <line x1="9" y1="9" x2="15" y2="15" />
                </svg>
              )}
              {toast.type === 'info' && (
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                  <circle cx="12" cy="12" r="10" />
                  <line x1="12" y1="16" x2="12" y2="12" />
                  <line x1="12" y1="8" x2="12.01" y2="8" />
                </svg>
              )}
            </span>
            <span className="toast-text">{toast.text}</span>
            <button
              type="button"
              className="toast-close-btn"
              onClick={() => removeToast(toast.id)}
            >
              &times;
            </button>
          </div>
        ))}
      </div>

      {/* Top Header Navigation */}
      <header className="users-top-bar">
        <div className="users-top-left">
          <button
            type="button"
            className="btn-back-pos"
            onClick={() => navigate('/pos')}
            title="Quay lại giao diện bán hàng"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <line x1="19" y1="12" x2="5" y2="12" />
              <polyline points="12 19 5 12 12 5" />
            </svg>
            <span>Về POS</span>
          </button>
          <div className="brand-breadcrumb">
            <span className="brand-title">MERA SOUVENIR</span>
            <span className="breadcrumb-separator">/</span>
            <span className="breadcrumb-page">Quản trị nhân sự &amp; Phân quyền</span>
          </div>
        </div>

        <div className="users-top-right">
          <div className="user-profile-badge">
            <div className="profile-avatar">
              {getInitials(currentUser?.hoTen || currentUser?.tenDangNhap || 'Admin')}
            </div>
            <div className="profile-details">
              <span className="profile-name">
                {currentUser?.hoTen || currentUser?.tenDangNhap}
              </span>
              <span className="profile-role">👑 Quản lý hệ thống</span>
            </div>
          </div>
          <button
            type="button"
            className="btn-logout"
            onClick={() => {
              logout();
              navigate('/login');
            }}
            title="Đăng xuất khỏi hệ thống"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
              <polyline points="16 17 21 12 16 7" />
              <line x1="21" y1="12" x2="9" y2="12" />
            </svg>
            <span>Đăng xuất</span>
          </button>
        </div>
      </header>

      {/* Main Container */}
      <main className="users-main-body">
        {/* Page Title & Intro */}
        <section className="page-header-section">
          <div className="page-header-text">
            <h1 className="page-heading">Quản lý nhân sự &amp; Phân quyền</h1>
            <p className="page-subheading">
              Theo dõi danh sách tài khoản, thiết lập quyền truy cập 2 cấp (Quản lý / Nhân viên) và kiểm soát trạng thái hoạt động theo chuẩn Mera Souvenir.
            </p>
          </div>
          <button
            type="button"
            className="btn-add-employee"
            onClick={handleOpenAddModal}
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <line x1="12" y1="5" x2="12" y2="19" />
              <line x1="5" y1="12" x2="19" y2="12" />
            </svg>
            <span>+ Thêm nhân viên</span>
          </button>
        </section>

        {/* Quick KPI / Stat Cards */}
        <section className="stats-cards-grid">
          <div className="stat-card">
            <div className="stat-icon-wrapper stat-total">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#967858" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
                <circle cx="9" cy="7" r="4" />
                <path d="M23 21v-2a4 4 0 0 0-3-3.87" />
                <path d="M16 3.13a4 4 0 0 1 0 7.75" />
              </svg>
            </div>
            <div className="stat-content">
              <span className="stat-label">Tổng nhân sự</span>
              <span className="stat-number">{stats.total}</span>
            </div>
          </div>

          <div className="stat-card">
            <div className="stat-icon-wrapper stat-active">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#059669" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
                <polyline points="22 4 12 14.01 9 11.01" />
              </svg>
            </div>
            <div className="stat-content">
              <span className="stat-label">Đang hoạt động</span>
              <span className="stat-number active-number">{stats.active}</span>
            </div>
          </div>

          <div className="stat-card">
            <div className="stat-icon-wrapper stat-locked">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#DC2626" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
                <path d="M7 11V7a5 5 0 0 1 10 0v4" />
              </svg>
            </div>
            <div className="stat-content">
              <span className="stat-label">Đang bị khóa</span>
              <span className="stat-number locked-number">{stats.locked}</span>
            </div>
          </div>

          <div className="stat-card">
            <div className="stat-icon-wrapper stat-manager">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#D97706" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2" />
              </svg>
            </div>
            <div className="stat-content">
              <span className="stat-label">Cấp Quản lý</span>
              <span className="stat-number manager-number">{stats.managers}</span>
            </div>
          </div>
        </section>

        {/* Global Error Banner if any */}
        {errorBanner && (
          <div className="users-error-banner">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <circle cx="12" cy="12" r="10" />
              <line x1="12" y1="8" x2="12" y2="12" />
              <line x1="12" y1="16" x2="12.01" y2="16" />
            </svg>
            <span>{errorBanner}</span>
            <button
              type="button"
              className="btn-retry"
              onClick={() => setRefreshKey((k) => k + 1)}
            >
              Thử lại
            </button>
          </div>
        )}

        {/* Table Card Surface */}
        <div className="users-table-card">
          {/* Toolbar */}
          <div className="users-toolbar">
            <div className="toolbar-search-wrapper">
              <svg className="search-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <circle cx="11" cy="11" r="8" />
                <line x1="21" y1="21" x2="16.65" y2="16.65" />
              </svg>
              <input
                type="text"
                placeholder="Tìm theo họ tên, tên đăng nhập, số điện thoại..."
                value={keyword}
                onChange={(e) => setKeyword(e.target.value)}
                className="toolbar-search-input"
              />
              {keyword && (
                <button
                  type="button"
                  className="search-clear-btn"
                  onClick={() => setKeyword('')}
                  title="Xóa tìm kiếm"
                >
                  &times;
                </button>
              )}
            </div>

            <div className="toolbar-filters">
              {/* Role Filter */}
              <div className="filter-item">
                <label htmlFor="filterRole" className="filter-label">Vai trò:</label>
                <select
                  id="filterRole"
                  value={selectedRole}
                  onChange={(e) => setSelectedRole(Number(e.target.value))}
                  className="filter-select"
                >
                  <option value={0}>Tất cả vai trò</option>
                  <option value={1}>👑 Quản lý</option>
                  <option value={2}>💼 Nhân viên</option>
                </select>
              </div>

              {/* Status Filter */}
              <div className="filter-item">
                <label htmlFor="filterStatus" className="filter-label">Trạng thái:</label>
                <select
                  id="filterStatus"
                  value={selectedStatus}
                  onChange={(e) => setSelectedStatus(e.target.value)}
                  className="filter-select"
                >
                  <option value="TatCa">Tất cả trạng thái</option>
                  <option value="HoatDong">🟢 Đang hoạt động</option>
                  <option value="Khoa">🔴 Đang bị khóa</option>
                </select>
              </div>

              {/* Reset filter button */}
              {(keyword || selectedRole !== 0 || selectedStatus !== 'TatCa') && (
                <button
                  type="button"
                  className="btn-reset-filters"
                  onClick={handleResetFilters}
                  title="Đặt lại bộ lọc"
                >
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <polyline points="1 4 1 10 7 10" />
                    <path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10" />
                  </svg>
                  <span>Đặt lại</span>
                </button>
              )}
            </div>
          </div>

          {/* Table Container */}
          <div className="table-responsive-container">
            <table className="users-table">
              <thead>
                <tr>
                  <th style={{ width: '60px', textAlign: 'center' }}>STT</th>
                  <th>Họ và tên</th>
                  <th>Tên đăng nhập</th>
                  <th>Số điện thoại</th>
                  <th>Vai trò</th>
                  <th>Trạng thái</th>
                  <th style={{ width: '150px', textAlign: 'center' }}>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                {isLoading ? (
                  /* Loading Skeletons */
                  Array.from({ length: 4 }).map((_, idx) => (
                    <tr key={`skeleton-${idx}`} className="skeleton-row">
                      <td style={{ textAlign: 'center' }}><div className="skeleton-box sm" /></td>
                      <td><div className="skeleton-box md" /></td>
                      <td><div className="skeleton-box sm" /></td>
                      <td><div className="skeleton-box sm" /></td>
                      <td><div className="skeleton-box badge" /></td>
                      <td><div className="skeleton-box badge" /></td>
                      <td><div className="skeleton-box actions" /></td>
                    </tr>
                  ))
                ) : users.length === 0 ? (
                  /* Empty State */
                  <tr>
                    <td colSpan={7} className="empty-table-cell">
                      <div className="empty-state-content">
                        <div className="empty-state-icon">
                          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="#967858" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
                            <circle cx="11" cy="11" r="8" />
                            <line x1="21" y1="21" x2="16.65" y2="16.65" />
                            <line x1="8" y1="11" x2="14" y2="11" />
                          </svg>
                        </div>
                        <h4 className="empty-state-title">Không tìm thấy nhân viên nào</h4>
                        <p className="empty-state-desc">
                          Không có dữ liệu nhân viên nào khớp với bộ lọc hoặc từ khóa tìm kiếm hiện tại.
                        </p>
                        <button
                          type="button"
                          className="btn-clear-empty-filter"
                          onClick={handleResetFilters}
                        >
                          Đặt lại bộ lọc
                        </button>
                      </div>
                    </td>
                  </tr>
                ) : (
                  /* User List Rows */
                  users.map((item, index) => {
                    const manager = isManagerRole(item);
                    const active = isActiveStatus(item);

                    return (
                      <tr key={item.id} className="user-row">
                        <td className="row-index">{index + 1}</td>
                        <td className="row-name">
                          <div className="user-avatar-group">
                            <div className={`avatar-pill ${manager ? 'avatar-mgr' : 'avatar-staff'}`}>
                              {getInitials(item.hoTen)}
                            </div>
                            <div className="user-name-col">
                              <span className="user-full-name">{item.hoTen}</span>
                              {currentUser?.tenDangNhap === item.tenDangNhap && (
                                <span className="current-user-tag">Bạn</span>
                              )}
                            </div>
                          </div>
                        </td>
                        <td className="row-username">
                          <code className="username-pill">@{item.tenDangNhap}</code>
                        </td>
                        <td className="row-phone">
                          {item.soDienThoai ? (
                            <span className="phone-number">{item.soDienThoai}</span>
                          ) : (
                            <span className="no-phone">Chưa có</span>
                          )}
                        </td>
                        <td className="row-role">
                          <span className={`badge-role ${manager ? 'badge-manager' : 'badge-staff'}`}>
                            {manager ? '👑 Quản lý' : '💼 Nhân viên'}
                          </span>
                        </td>
                        <td className="row-status">
                          <span className={`pill-status ${active ? 'status-active' : 'status-locked'}`}>
                            <span className={`status-dot ${active ? 'dot-active' : 'dot-locked'}`} />
                            <span>{active ? 'Hoạt động' : 'Bị khóa'}</span>
                          </span>
                          {!active && item.soLanDangNhapSai && item.soLanDangNhapSai > 0 ? (
                            <span className="attempts-warning" title={`Đã đăng nhập sai ${item.soLanDangNhapSai} lần`}>
                              (Sai {item.soLanDangNhapSai} lần)
                            </span>
                          ) : null}
                        </td>
                        <td className="row-actions">
                          <div className="action-buttons-group">
                            {/* Nút Sửa */}
                            <button
                              type="button"
                              className="btn-action btn-edit"
                              onClick={() => handleOpenEditModal(item)}
                              title="Chỉnh sửa thông tin nhân viên"
                            >
                              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                                <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
                              </svg>
                            </button>

                            {/* Nút Khóa / Mở khóa */}
                            <button
                              type="button"
                              className={`btn-action ${active ? 'btn-lock' : 'btn-unlock'}`}
                              onClick={() => handlePromptToggleStatus(item)}
                              title={active ? 'Khóa tài khoản' : 'Mở khóa tài khoản'}
                            >
                              {active ? (
                                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                  <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
                                  <path d="M7 11V7a5 5 0 0 1 10 0v4" />
                                </svg>
                              ) : (
                                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                  <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
                                  <path d="M7 11V7a5 5 0 0 1 9.9-1" />
                                </svg>
                              )}
                            </button>

                            {/* Nút Xóa */}
                            <button
                              type="button"
                              className="btn-action btn-delete"
                              onClick={() => handlePromptDelete(item)}
                              title="Xóa tài khoản nhân viên"
                            >
                              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                <polyline points="3 6 5 6 21 6" />
                                <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                                <line x1="10" y1="11" x2="10" y2="17" />
                                <line x1="14" y1="11" x2="14" y2="17" />
                              </svg>
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

          {/* Table Footer Summary */}
          <div className="table-footer-summary">
            <span>
              Đang hiển thị <strong>{users.length}</strong> nhân sự trong hệ thống Mera Souvenir
            </span>
            <span className="system-guard-note">
              🛡️ Quy tắc bảo vệ: Luôn duy trì ít nhất 01 Quản lý đang hoạt động.
            </span>
          </div>
        </div>
      </main>

      {/* Add / Edit User Modal */}
      <UserModal
        isOpen={isUserModalOpen}
        onClose={() => setIsUserModalOpen(false)}
        onSuccess={(msg) => {
          showToast(msg, 'success');
          setRefreshKey((k) => k + 1);
        }}
        userToEdit={userToEdit}
        roles={roles}
      />

      {/* Confirm Action Modal */}
      <ConfirmModal
        isOpen={confirmModalData.isOpen}
        type={confirmModalData.type}
        userName={confirmModalData.user?.hoTen || ''}
        userLogin={confirmModalData.user?.tenDangNhap || ''}
        onConfirm={handleExecuteConfirm}
        onClose={() =>
          setConfirmModalData((prev) => ({ ...prev, isOpen: false, user: null }))
        }
        isLoading={confirmModalData.isLoading}
      />
    </div>
  );
};

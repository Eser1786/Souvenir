import React, { useState } from 'react';
import type { User, Role, CreateUserRequest, UpdateUserRequest } from '../../api/users';
import { createUser, updateUser } from '../../api/users';

interface UserModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (message: string) => void;
  userToEdit: User | null;
  roles: Role[];
}

interface UserModalContentProps {
  onClose: () => void;
  onSuccess: (message: string) => void;
  userToEdit: User | null;
  roles: Role[];
}

const UserModalContent: React.FC<UserModalContentProps> = ({
  onClose,
  onSuccess,
  userToEdit,
  roles,
}) => {
  const isEditing = !!userToEdit;

  const [hoTen, setHoTen] = useState(userToEdit?.hoTen || '');
  const [tenDangNhap, setTenDangNhap] = useState(userToEdit?.tenDangNhap || '');
  const [matKhau, setMatKhau] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [soDienThoai, setSoDienThoai] = useState(userToEdit?.soDienThoai || '');
  const [vaiTroId, setVaiTroId] = useState<number>(userToEdit?.vaiTroId || roles[1]?.id || 2);

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [apiError, setApiError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!hoTen.trim()) {
      newErrors.hoTen = 'Họ và tên không được để trống.';
    } else if (hoTen.trim().length < 2) {
      newErrors.hoTen = 'Họ và tên phải có ít nhất 2 ký tự.';
    }

    if (!isEditing) {
      if (!tenDangNhap.trim()) {
        newErrors.tenDangNhap = 'Tên đăng nhập không được để trống.';
      } else if (tenDangNhap.trim().length < 3) {
        newErrors.tenDangNhap = 'Tên đăng nhập phải có ít nhất 3 ký tự.';
      } else if (!/^[a-zA-Z0-9_.-]+$/.test(tenDangNhap.trim())) {
        newErrors.tenDangNhap = 'Tên đăng nhập chỉ chứa chữ cái, số, dấu gạch dưới hoặc gạch ngang.';
      }

      if (!matKhau) {
        newErrors.matKhau = 'Mật khẩu khởi tạo không được để trống.';
      } else if (matKhau.length < 6) {
        newErrors.matKhau = 'Mật khẩu phải có tối thiểu 6 ký tự.';
      }
    }

    const cleanPhone = soDienThoai.trim().replace(/\s+/g, '');
    if (!cleanPhone) {
      newErrors.soDienThoai = 'Số điện thoại không được để trống.';
    } else if (!/^0\d{9}$/.test(cleanPhone)) {
      newErrors.soDienThoai = 'Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0.';
    }

    if (!vaiTroId) {
      newErrors.vaiTroId = 'Vui lòng chọn vai trò phân quyền.';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setApiError(null);

    if (!validate()) {
      return;
    }

    setIsSubmitting(true);
    const cleanPhone = soDienThoai.trim().replace(/\s+/g, '');

    try {
      if (isEditing && userToEdit) {
        const updateData: UpdateUserRequest = {
          hoTen: hoTen.trim(),
          soDienThoai: cleanPhone,
          vaiTroId: Number(vaiTroId),
        };
        await updateUser(userToEdit.id, updateData);
        onSuccess(`Cập nhật thông tin nhân viên "${hoTen.trim()}" thành công!`);
        onClose();
      } else {
        const createData: CreateUserRequest = {
          tenDangNhap: tenDangNhap.trim(),
          matKhau,
          hoTen: hoTen.trim(),
          soDienThoai: cleanPhone,
          vaiTroId: Number(vaiTroId),
        };
        await createUser(createData);
        onSuccess(`Thêm nhân viên mới "${hoTen.trim()}" thành công!`);
        onClose();
      }
    } catch (err: unknown) {
      if (err instanceof Error) {
        setApiError(err.message);
      } else {
        setApiError('Có lỗi xảy ra trong quá trình lưu thông tin.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="user-modal-container" onClick={(e) => e.stopPropagation()}>
      {/* Modal Header */}
      <div className="user-modal-header">
        <div className="user-modal-title-group">
          <div className="user-modal-icon">
            {isEditing ? (
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
              </svg>
            ) : (
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
                <circle cx="8.5" cy="7" r="4" />
                <line x1="20" y1="8" x2="20" y2="14" />
                <line x1="23" y1="11" x2="17" y2="11" />
              </svg>
            )}
          </div>
          <div>
            <h2 className="user-modal-title">
              {isEditing ? 'Chỉnh sửa nhân viên' : 'Thêm nhân viên mới'}
            </h2>
            <p className="user-modal-subtitle">
              {isEditing
                ? `Cập nhật thông tin và phân quyền cho tài khoản @${userToEdit.tenDangNhap}`
                : 'Khởi tạo tài khoản và thiết lập phân quyền truy cập hệ thống Mera'}
            </p>
          </div>
        </div>
        <button
          type="button"
          className="user-modal-close-btn"
          onClick={onClose}
          aria-label="Đóng modal"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>
      </div>

      {/* API Error Alert */}
      {apiError && (
        <div className="user-modal-alert">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <circle cx="12" cy="12" r="10" />
            <line x1="12" y1="8" x2="12" y2="12" />
            <line x1="12" y1="16" x2="12.01" y2="16" />
          </svg>
          <span>{apiError}</span>
        </div>
      )}

      {/* Modal Form */}
      <form onSubmit={handleSubmit} className="user-modal-form" noValidate>
        {/* Họ và tên */}
        <div className="form-group">
          <label htmlFor="hoTen" className="form-label">
            Họ và tên <span className="required-mark">*</span>
          </label>
          <input
            id="hoTen"
            type="text"
            value={hoTen}
            onChange={(e) => {
              setHoTen(e.target.value);
              if (errors.hoTen) setErrors({ ...errors, hoTen: '' });
            }}
            placeholder="Ví dụ: Nguyễn Văn An"
            className={`form-input ${errors.hoTen ? 'has-error' : ''}`}
            disabled={isSubmitting}
          />
          {errors.hoTen && <span className="field-error-text">{errors.hoTen}</span>}
        </div>

        {/* Tên đăng nhập */}
        <div className="form-group">
          <label htmlFor="tenDangNhap" className="form-label">
            Tên đăng nhập <span className="required-mark">*</span>
            {isEditing && <span className="readonly-tag">Chỉ đọc</span>}
          </label>
          <input
            id="tenDangNhap"
            type="text"
            value={tenDangNhap}
            onChange={(e) => {
              setTenDangNhap(e.target.value);
              if (errors.tenDangNhap) setErrors({ ...errors, tenDangNhap: '' });
            }}
            placeholder="Ví dụ: nguyenvana"
            className={`form-input ${errors.tenDangNhap ? 'has-error' : ''} ${isEditing ? 'disabled-input' : ''}`}
            disabled={isEditing || isSubmitting}
            autoComplete="off"
          />
          {errors.tenDangNhap && <span className="field-error-text">{errors.tenDangNhap}</span>}
          {isEditing && (
            <span className="field-hint-text">
              Tên đăng nhập dùng để xác thực hệ thống và không được phép thay đổi.
            </span>
          )}
        </div>

        {/* Mật khẩu (chỉ hiển thị khi thêm mới) */}
        {!isEditing && (
          <div className="form-group">
            <label htmlFor="matKhau" className="form-label">
              Mật khẩu khởi tạo <span className="required-mark">*</span>
            </label>
            <div className="password-input-wrapper">
              <input
                id="matKhau"
                type={showPassword ? 'text' : 'password'}
                value={matKhau}
                onChange={(e) => {
                  setMatKhau(e.target.value);
                  if (errors.matKhau) setErrors({ ...errors, matKhau: '' });
                }}
                placeholder="Nhập tối thiểu 6 ký tự..."
                className={`form-input ${errors.matKhau ? 'has-error' : ''}`}
                disabled={isSubmitting}
              />
              <button
                type="button"
                className="password-toggle-btn"
                onClick={() => setShowPassword(!showPassword)}
                tabIndex={-1}
                aria-label={showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
              >
                {showPassword ? (
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" />
                    <line x1="1" y1="1" x2="23" y2="23" />
                  </svg>
                ) : (
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
                    <circle cx="12" cy="12" r="3" />
                  </svg>
                )}
              </button>
            </div>
            {errors.matKhau && <span className="field-error-text">{errors.matKhau}</span>}
          </div>
        )}

        {/* Số điện thoại */}
        <div className="form-group">
          <label htmlFor="soDienThoai" className="form-label">
            Số điện thoại <span className="required-mark">*</span>
          </label>
          <input
            id="soDienThoai"
            type="tel"
            value={soDienThoai}
            onChange={(e) => {
              setSoDienThoai(e.target.value);
              if (errors.soDienThoai) setErrors({ ...errors, soDienThoai: '' });
            }}
            placeholder="Ví dụ: 0901234567"
            className={`form-input ${errors.soDienThoai ? 'has-error' : ''}`}
            disabled={isSubmitting}
          />
          {errors.soDienThoai && <span className="field-error-text">{errors.soDienThoai}</span>}
        </div>

        {/* Vai trò / Phân quyền */}
        <div className="form-group">
          <label htmlFor="vaiTroId" className="form-label">
            Vai trò phân quyền <span className="required-mark">*</span>
          </label>
          <select
            id="vaiTroId"
            value={vaiTroId}
            onChange={(e) => {
              setVaiTroId(Number(e.target.value));
              if (errors.vaiTroId) setErrors({ ...errors, vaiTroId: '' });
            }}
            className="form-select"
            disabled={isSubmitting}
          >
            {roles.length > 0 ? (
              roles.map((r) => (
                <option key={r.id} value={r.id}>
                  {r.tenVaiTro === 'QuanLy' || r.tenVaiTro === 'Quản lý'
                    ? '👑 Quản lý (Toàn quyền hệ thống)'
                    : '💼 Nhân viên (Bán hàng & ca trực)'}
                </option>
              ))
            ) : (
              <>
                <option value={2}>💼 Nhân viên (Bán hàng & ca trực)</option>
                <option value={1}>👑 Quản lý (Toàn quyền hệ thống)</option>
              </>
            )}
          </select>
          {errors.vaiTroId && <span className="field-error-text">{errors.vaiTroId}</span>}
          <span className="field-hint-text">
            {vaiTroId === 1
              ? 'Quản lý: Có quyền quản lý nhân sự, kho hàng, danh mục sản phẩm và xem báo cáo tài chính.'
              : 'Nhân viên: Giới hạn thao tác tại màn hình POS, tạo đơn bán và quản lý ca làm việc cá nhân.'}
          </span>
        </div>

        {/* Form Actions */}
        <div className="user-modal-actions">
          <button
            type="button"
            className="btn-modal-cancel"
            onClick={onClose}
            disabled={isSubmitting}
          >
            Hủy
          </button>
          <button
            type="submit"
            className="btn-modal-submit"
            disabled={isSubmitting}
          >
            {isSubmitting ? (
              <>
                <span className="modal-spinner" />
                <span>Đang xử lý...</span>
              </>
            ) : (
              <>
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <polyline points="20 6 9 17 4 12" />
                </svg>
                <span>{isEditing ? 'Lưu thay đổi' : 'Tạo nhân viên'}</span>
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
};

export const UserModal: React.FC<UserModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  userToEdit,
  roles,
}) => {
  if (!isOpen) return null;

  return (
    <div className="modal-backdrop" onClick={onClose} role="dialog" aria-modal="true">
      <UserModalContent
        key={userToEdit ? userToEdit.id : 'create'}
        onClose={onClose}
        onSuccess={onSuccess}
        userToEdit={userToEdit}
        roles={roles}
      />
    </div>
  );
};

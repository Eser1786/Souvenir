import React from 'react';

export type ConfirmType = 'lock' | 'unlock' | 'delete';

interface ConfirmModalProps {
  isOpen: boolean;
  type: ConfirmType;
  userName: string;
  userLogin: string;
  onConfirm: () => void;
  onClose: () => void;
  isLoading?: boolean;
}

export const ConfirmModal: React.FC<ConfirmModalProps> = ({
  isOpen,
  type,
  userName,
  userLogin,
  onConfirm,
  onClose,
  isLoading = false,
}) => {
  if (!isOpen) return null;

  const config = {
    lock: {
      title: 'Khóa tài khoản nhân viên',
      message: `Bạn có chắc chắn muốn khóa tài khoản của "${userName}" (@${userLogin}) không? Nhân viên này sẽ không thể đăng nhập vào hệ thống bán hàng cho đến khi được mở khóa lại.`,
      confirmText: 'Khóa tài khoản',
      confirmClass: 'btn-confirm-warning',
      icon: (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#D97706" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
          <path d="M7 11V7a5 5 0 0 1 10 0v4" />
        </svg>
      ),
      iconBg: '#FEF3C7',
    },
    unlock: {
      title: 'Mở khóa tài khoản',
      message: `Bạn muốn kích hoạt lại tài khoản của "${userName}" (@${userLogin})? Nhân viên sẽ có thể tiếp tục đăng nhập và làm việc bình thường. Số lần đăng nhập sai sẽ được đặt lại về 0.`,
      confirmText: 'Mở khóa',
      confirmClass: 'btn-confirm-success',
      icon: (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#059669" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <rect x="3" y="11" width="18" height="11" rx="2" ry="2" />
          <path d="M7 11V7a5 5 0 0 1 9.9-1" />
        </svg>
      ),
      iconBg: '#D1FAE5',
    },
    delete: {
      title: 'Xóa vĩnh viễn nhân viên',
      message: `Hành động này KHÔNG THỂ HOÀN TÁC! Bạn có chắc chắn muốn xóa tài khoản của "${userName}" (@${userLogin}) khỏi hệ thống cửa hàng?`,
      confirmText: 'Xác nhận xóa',
      confirmClass: 'btn-confirm-danger',
      icon: (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#DC2626" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <polyline points="3 6 5 6 21 6" />
          <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
          <line x1="10" y1="11" x2="10" y2="17" />
          <line x1="14" y1="11" x2="14" y2="17" />
        </svg>
      ),
      iconBg: '#FEE2E2',
    },
  }[type];

  return (
    <div className="modal-backdrop" onClick={onClose} role="dialog" aria-modal="true">
      <div className="confirm-modal-container" onClick={(e) => e.stopPropagation()}>
        <div className="confirm-modal-body">
          <div className="confirm-icon-badge" style={{ backgroundColor: config.iconBg }}>
            {config.icon}
          </div>
          <div className="confirm-text-content">
            <h3 className="confirm-modal-title">{config.title}</h3>
            <p className="confirm-modal-desc">{config.message}</p>
          </div>
        </div>

        <div className="confirm-modal-actions">
          <button
            type="button"
            className="btn-modal-cancel"
            onClick={onClose}
            disabled={isLoading}
          >
            Đóng
          </button>
          <button
            type="button"
            className={`btn-confirm-action ${config.confirmClass}`}
            onClick={onConfirm}
            disabled={isLoading}
          >
            {isLoading ? (
              <>
                <span className="modal-spinner" />
                <span>Đang xử lý...</span>
              </>
            ) : (
              config.confirmText
            )}
          </button>
        </div>
      </div>
    </div>
  );
};

import React from 'react';
import type { DraftOrderDto } from '../../api/draftOrders';
import './DraftOrdersModal.css';

export interface DraftOrdersModalProps {
  isOpen: boolean;
  onClose: () => void;
  drafts: DraftOrderDto[];
  isLoading: boolean;
  onRestore: (draft: DraftOrderDto) => void;
  onDelete: (draftId: number) => void;
}

export const DraftOrdersModal: React.FC<DraftOrdersModalProps> = ({
  isOpen,
  onClose,
  drafts,
  isLoading,
  onRestore,
  onDelete,
}) => {
  if (!isOpen) return null;

  const formatVnd = (amount: number) =>
    new Intl.NumberFormat('vi-VN').format(amount) + ' đ';

  const formatTime = (isoString: string) => {
    try {
      const d = new Date(isoString);
      return `${d.getHours().toString().padStart(2, '0')}:${d
        .getMinutes()
        .toString()
        .padStart(2, '0')} · ${d.toLocaleDateString('vi-VN')}`;
    } catch {
      return isoString;
    }
  };

  return (
    <div className="draft-overlay" onClick={onClose}>
      <div className="draft-modal" onClick={(e) => e.stopPropagation()}>
        {/* Header */}
        <div className="draft-header">
          <div className="draft-header-info">
            <div className="draft-header-icon">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
                <polyline points="14 2 14 8 20 8" />
                <line x1="16" y1="13" x2="8" y2="13" />
                <line x1="16" y1="17" x2="8" y2="17" />
                <polyline points="10 9 9 9 8 9" />
              </svg>
            </div>
            <div>
              <h3 className="draft-title">Danh sách đơn nháp đang lưu ({drafts.length})</h3>
              <p className="draft-subtitle">Các đơn hàng tạm hoãn để giải phóng quầy thanh toán</p>
            </div>
          </div>
          <button type="button" className="draft-close-btn" onClick={onClose} aria-label="Đóng">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>

        {/* Body */}
        <div className="draft-body">
          {isLoading ? (
            <div className="draft-empty">
              <p className="draft-empty-desc">Đang tải danh sách đơn nháp...</p>
            </div>
          ) : drafts.length === 0 ? (
            <div className="draft-empty">
              <div className="draft-empty-icon">
                <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
                  <polyline points="14 2 14 8 20 8" />
                </svg>
              </div>
              <div className="draft-empty-title">Không có đơn nháp nào</div>
              <div className="draft-empty-desc">
                Khi khách hàng muốn tạm dừng để chọn thêm đồ, bấm nút <strong>"Lưu nháp đơn"</strong> ở góc dưới giỏ hàng để lưu tạm tại đây.
              </div>
            </div>
          ) : (
            drafts.map((draft) => (
              <div key={draft.id} className="draft-card">
                {/* Meta info */}
                <div className="draft-card-top">
                  <div className="draft-card-meta">
                    <span className="draft-badge-code">#{draft.maDonHang || `NHAP-${draft.id}`}</span>
                    <span className="draft-time">{formatTime(draft.ngayTao)}</span>
                    {draft.tenKhachHang && (
                      <span className="draft-customer-tag">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                          <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
                          <circle cx="12" cy="7" r="4" />
                        </svg>
                        {draft.tenKhachHang} {draft.soDienThoaiKhach ? `(${draft.soDienThoaiKhach})` : ''}
                      </span>
                    )}
                    {draft.maGiamGia && (
                      <span className="draft-customer-tag" style={{ backgroundColor: '#fef3c7', color: '#92400e' }}>
                        🎫 {draft.maGiamGia}
                      </span>
                    )}
                  </div>
                  <span style={{ fontSize: '12px', color: '#8c827a' }}>
                    {draft.items.length} mặt hàng ({draft.tongSoLuongMon} món)
                  </span>
                </div>

                {/* Items preview list */}
                <div className="draft-items-preview">
                  {draft.items.map((item, idx) => (
                    <div key={`${draft.id}-${item.sanPhamId}-${idx}`} className="draft-item-row">
                      <div className="draft-item-left">
                        {item.imageUrl ? (
                          <img src={item.imageUrl} alt={item.tenSP} className="draft-item-img" />
                        ) : (
                          <div className="draft-item-img" style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '10px' }}>📦</div>
                        )}
                        <span className="draft-item-name">{item.tenSP}</span>
                        <span className="draft-item-qty">x{item.soLuong}</span>
                      </div>
                      <span className="draft-item-price">{formatVnd(item.thanhTien)}</span>
                    </div>
                  ))}
                </div>

                {/* Bottom Total & Actions */}
                <div className="draft-card-bottom">
                  <div className="draft-total-group">
                    <span className="draft-total-label">Tổng tiền tạm tính</span>
                    <span className="draft-total-val">{formatVnd(draft.tongTienThanhToan)}</span>
                  </div>

                  <div className="draft-card-actions">
                    <button
                      type="button"
                      className="btn-delete-draft"
                      onClick={() => onDelete(draft.id)}
                      title="Xóa đơn nháp"
                    >
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <polyline points="3 6 5 6 21 6" />
                        <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
                      </svg>
                      Xóa
                    </button>
                    <button
                      type="button"
                      className="btn-restore-draft"
                      onClick={() => onRestore(draft)}
                      title="Nạp lại vào giỏ hàng"
                    >
                      <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                        <polyline points="1 4 1 10 7 10" />
                        <path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10" />
                      </svg>
                      Khôi phục
                    </button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
};

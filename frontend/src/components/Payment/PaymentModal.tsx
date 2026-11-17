import React, { useEffect, useMemo, useRef, useState } from 'react';
import { createOrder, type OrderDto, type PhuongThucThanhToan } from '../../api/orders';
import { buildVietQrUrl, suggestCashAmounts, VIETQR_CONFIG } from './vietQr';
import './PaymentModal.css';

/** Một dòng trong giỏ hàng, truyền từ màn hình POS. */
export interface PaymentCartItem {
  sanPhamId: number;
  tenSP: string;
  soLuong: number;
  donGia: number;
}

export interface PaymentModalProps {
  isOpen: boolean;
  onClose: () => void;
  /** Gọi khi tạo đơn thành công và nhân viên bấm "Đơn mới". */
  onSuccess: (order: OrderDto) => void;
  items: PaymentCartItem[];
  /** Tổng tiền hàng trước giảm giá (để hiển thị). */
  tongTienHang: number;
  /** Số tiền giảm (voucher, điểm...) để hiển thị. */
  soTienGiam?: number;
  /** Số tiền khách cần trả. */
  tongThanhToan: number;
  /** Mã voucher đã áp dụng (UC05), server sẽ kiểm tra lại. */
  maGiamGia?: string;
  /** Khách hàng thành viên (UC09). Bỏ trống = khách vãng lai. */
  khachHangId?: number;
  /** Ca làm việc (UC03). Bỏ trống = server tự lấy ca đang mở. */
  caLamViecId?: number;
}

const formatVND = (amount: number) => new Intl.NumberFormat('vi-VN').format(Math.round(amount)) + ' đ';
const parseMoney = (raw: string) => Number(raw.replace(/\D/g, '')) || 0;

const IconCash = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
    <rect x="2" y="6" width="20" height="12" rx="2" />
    <circle cx="12" cy="12" r="2.5" />
    <path d="M6 12h.01M18 12h.01" />
  </svg>
);

const IconQr = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="3" width="7" height="7" rx="1" />
    <rect x="14" y="3" width="7" height="7" rx="1" />
    <rect x="3" y="14" width="7" height="7" rx="1" />
    <path d="M14 14h3v3h-3zM20 14v.01M14 20h.01M17 20h4v-3" />
  </svg>
);

const IconClose = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
    <path d="M18 6 6 18M6 6l12 12" />
  </svg>
);

const IconCheck = () => (
  <svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round">
    <path d="M20 6 9 17l-5-5" />
  </svg>
);

/**
 * UC07 - Modal thanh toán tại quầy POS.
 * Hai hình thức: Tiền mặt (nhập tiền khách đưa → tự tính tiền thối) và Quét mã VietQR.
 * Khi xác nhận sẽ gọi POST /api/orders (lưu đơn + chi tiết + trừ tồn kho).
 */
export const PaymentModal: React.FC<PaymentModalProps> = (props) => {
  // Chỉ mount phần thân khi mở → mọi state (tab, tiền khách đưa, lỗi...) tự reset mỗi lần mở
  if (!props.isOpen) return null;
  return <PaymentModalBody {...props} />;
};

const PaymentModalBody: React.FC<PaymentModalProps> = ({
  onClose,
  onSuccess,
  items,
  tongTienHang,
  soTienGiam = 0,
  tongThanhToan,
  maGiamGia,
  khachHangId,
  caLamViecId,
}) => {
  const [method, setMethod] = useState<PhuongThucThanhToan>('TienMat');
  const [cashInput, setCashInput] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [completedOrder, setCompletedOrder] = useState<OrderDto | null>(null);
  const [qrFailed, setQrFailed] = useState(false);
  const cashInputRef = useRef<HTMLInputElement>(null);

  // Mã tham chiếu chuyển khoản (nội dung CK). Body chỉ mount khi modal mở nên mỗi lần mở là một mã mới.
  const [transferRef] = useState(() => `MERA${Date.now().toString().slice(-8)}`);

  useEffect(() => {
    if (method === 'TienMat' && !completedOrder) {
      cashInputRef.current?.focus();
    }
  }, [method, completedOrder]);

  const tienKhachDua = parseMoney(cashInput);
  const tienThoi = tienKhachDua - tongThanhToan;
  const cashSuggestions = useMemo(() => suggestCashAmounts(tongThanhToan), [tongThanhToan]);
  const qrUrl = useMemo(() => buildVietQrUrl(tongThanhToan, transferRef), [tongThanhToan, transferRef]);
  const totalQuantity = items.reduce((sum, i) => sum + i.soLuong, 0);

  const canConfirm =
    !isSubmitting && items.length > 0 && (method === 'ChuyenKhoan' || tienKhachDua >= tongThanhToan);

  const handleClose = () => {
    if (isSubmitting) return;
    if (completedOrder) {
      onSuccess(completedOrder);
      return;
    }
    onClose();
  };

  // Phím Esc để đóng
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') handleClose();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  });

  const handleConfirm = async (e?: React.FormEvent) => {
    e?.preventDefault();
    if (!canConfirm) return;

    setIsSubmitting(true);
    setError(null);
    try {
      const order = await createOrder({
        items: items.map((i) => ({ sanPhamId: i.sanPhamId, soLuong: i.soLuong })),
        phuongThuc: method,
        tienKhachDua: method === 'TienMat' ? tienKhachDua : undefined,
        maGiamGia: maGiamGia || undefined,
        khachHangId,
        caLamViecId,
      });
      setCompletedOrder(order);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Thanh toán thất bại, vui lòng thử lại.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="pay-overlay" onMouseDown={(e) => e.target === e.currentTarget && handleClose()}>
      <div className="pay-modal" role="dialog" aria-modal="true" aria-labelledby="pay-modal-title">
        {completedOrder ? (
          /* ===================== THANH TOÁN THÀNH CÔNG ===================== */
          <div className="pay-success">
            <div className="pay-success-icon"><IconCheck /></div>
            <h2 className="pay-success-title">Thanh toán thành công</h2>
            <p className="pay-success-sub">
              Đơn <strong>{completedOrder.maDonHang}</strong> · {completedOrder.tenHinhThucThanhToan}
            </p>

            <div className="pay-receipt">
              <div className="pay-row">
                <span>Tổng tiền hàng</span>
                <span>{formatVND(completedOrder.tongTienHang)}</span>
              </div>
              {completedOrder.soTienGiam > 0 && (
                <div className="pay-row discount">
                  <span>Giảm giá{completedOrder.maGiamGia ? ` (${completedOrder.maGiamGia})` : ''}</span>
                  <span>-{formatVND(completedOrder.soTienGiam)}</span>
                </div>
              )}
              <div className="pay-row strong">
                <span>Khách thanh toán</span>
                <span>{formatVND(completedOrder.tongTienThanhToan)}</span>
              </div>
              {completedOrder.tienKhachDua != null && (
                <>
                  <div className="pay-row">
                    <span>Tiền khách đưa</span>
                    <span>{formatVND(completedOrder.tienKhachDua)}</span>
                  </div>
                  <div className="pay-row change">
                    <span>Tiền thối</span>
                    <span>{formatVND(completedOrder.tienThoi ?? 0)}</span>
                  </div>
                </>
              )}
            </div>

            <button type="button" className="pay-btn-primary" onClick={() => onSuccess(completedOrder)} autoFocus>
              Đơn mới
            </button>
          </div>
        ) : (
          /* ===================== CHỌN HÌNH THỨC THANH TOÁN ===================== */
          <form onSubmit={handleConfirm}>
            <div className="pay-header">
              <div>
                <h2 id="pay-modal-title" className="pay-title">Thanh toán đơn hàng</h2>
                <p className="pay-subtitle">{totalQuantity} sản phẩm · {items.length} mặt hàng</p>
              </div>
              <button type="button" className="pay-close" onClick={handleClose} aria-label="Đóng">
                <IconClose />
              </button>
            </div>

            <div className="pay-amount-card">
              <span className="pay-amount-label">Khách cần trả</span>
              <span className="pay-amount-value">{formatVND(tongThanhToan)}</span>
              {soTienGiam > 0 && (
                <span className="pay-amount-note">
                  Tạm tính {formatVND(tongTienHang)} · Đã giảm {formatVND(soTienGiam)}
                </span>
              )}
            </div>

            <div className="pay-tabs" role="tablist">
              <button
                type="button"
                role="tab"
                aria-selected={method === 'TienMat'}
                className={`pay-tab ${method === 'TienMat' ? 'active' : ''}`}
                onClick={() => { setMethod('TienMat'); setError(null); }}
              >
                <IconCash /> Tiền mặt
              </button>
              <button
                type="button"
                role="tab"
                aria-selected={method === 'ChuyenKhoan'}
                className={`pay-tab ${method === 'ChuyenKhoan' ? 'active' : ''}`}
                onClick={() => { setMethod('ChuyenKhoan'); setError(null); }}
              >
                <IconQr /> Quét mã QR
              </button>
            </div>

            {method === 'TienMat' ? (
              <div className="pay-panel">
                <label className="pay-label" htmlFor="pay-cash-input">Tiền khách đưa</label>
                <div className="pay-input-wrap">
                  <input
                    id="pay-cash-input"
                    ref={cashInputRef}
                    className="pay-input"
                    inputMode="numeric"
                    autoComplete="off"
                    placeholder="0"
                    value={cashInput ? new Intl.NumberFormat('vi-VN').format(tienKhachDua) : ''}
                    onChange={(e) => setCashInput(e.target.value)}
                  />
                  <span className="pay-input-suffix">đ</span>
                </div>

                <div className="pay-chips">
                  {cashSuggestions.map((amount) => (
                    <button
                      key={amount}
                      type="button"
                      className={`pay-chip ${tienKhachDua === amount ? 'active' : ''}`}
                      onClick={() => setCashInput(String(amount))}
                    >
                      {amount === tongThanhToan ? 'Đủ tiền' : formatVND(amount)}
                    </button>
                  ))}
                </div>

                <div className={`pay-change ${cashInput && tienThoi < 0 ? 'short' : ''}`}>
                  <span>{cashInput && tienThoi < 0 ? 'Còn thiếu' : 'Tiền thối lại khách'}</span>
                  <span className="pay-change-value">
                    {cashInput ? formatVND(Math.abs(tienThoi)) : '0 đ'}
                  </span>
                </div>
              </div>
            ) : (
              <div className="pay-panel pay-qr-panel">
                <div className="pay-qr-frame">
                  {qrFailed ? (
                    <div className="pay-qr-fallback">
                      Không tải được mã QR. Kiểm tra kết nối Internet rồi thử lại.
                      <button type="button" className="pay-link" onClick={() => setQrFailed(false)}>Tải lại</button>
                    </div>
                  ) : (
                    <img
                      src={qrUrl}
                      alt={`Mã VietQR thanh toán ${formatVND(tongThanhToan)}`}
                      className="pay-qr-img"
                      onError={() => setQrFailed(true)}
                    />
                  )}
                </div>
                <div className="pay-qr-info">
                  <div className="pay-row"><span>Ngân hàng</span><span>{VIETQR_CONFIG.bankId}</span></div>
                  <div className="pay-row"><span>Số tài khoản</span><span>{VIETQR_CONFIG.accountNo}</span></div>
                  <div className="pay-row"><span>Chủ tài khoản</span><span>{VIETQR_CONFIG.accountName}</span></div>
                  <div className="pay-row"><span>Nội dung</span><span className="mono">{transferRef}</span></div>
                  <div className="pay-row strong"><span>Số tiền</span><span>{formatVND(tongThanhToan)}</span></div>
                </div>
                <p className="pay-hint">Khách quét mã bằng app ngân hàng. Bấm xác nhận sau khi đã thấy tiền về tài khoản.</p>
              </div>
            )}

            {error && <div className="pay-error" role="alert">{error}</div>}

            <div className="pay-footer">
              <button type="button" className="pay-btn-secondary" onClick={handleClose} disabled={isSubmitting}>
                Quay lại giỏ hàng
              </button>
              <button type="submit" className="pay-btn-primary" disabled={!canConfirm}>
                {isSubmitting
                  ? 'Đang xử lý...'
                  : method === 'TienMat'
                    ? 'Xác nhận đã thu tiền'
                    : 'Đã nhận chuyển khoản'}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};

export default PaymentModal;

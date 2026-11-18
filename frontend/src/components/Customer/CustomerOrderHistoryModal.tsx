import React, { useEffect, useMemo, useState } from 'react';
import { getCustomerOrders } from '../../api/customers';
import type { Customer, CustomerOrder } from '../../api/customers';

interface CustomerOrderHistoryModalProps {
  customer: Customer;
  onClose: () => void;
}

const formatPhone = (phone: string) => {
  const digits = phone.replace(/\D/g, '');
  if (digits.length === 10) {
    return `${digits.slice(0, 4)} ${digits.slice(4, 7)} ${digits.slice(7)}`;
  }
  return phone;
};

const formatCurrency = (amount: number) =>
  `${Math.round(amount).toLocaleString('vi-VN')} đ`;

const formatDateTime = (isoDate: string) => {
  const d = new Date(isoDate);
  if (Number.isNaN(d.getTime())) return isoDate;
  return d.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

const getTier = (points: number): { label: string; className: string } => {
  if (points >= 500) return { label: 'Kim Cương', className: 'diamond' };
  if (points >= 200) return { label: 'Vàng', className: 'gold' };
  if (points >= 50) return { label: 'Bạc', className: 'silver' };
  return { label: 'Đồng', className: 'bronze' };
};

const getOrderStatusMeta = (status: string): { label: string; className: string } => {
  switch (status) {
    case 'DaThanhToan':
      return { label: 'Đã thanh toán', className: 'paid' };
    case 'DaTraHang':
      return { label: 'Đã trả hàng', className: 'returned' };
    case 'DaHuy':
      return { label: 'Đã hủy', className: 'cancelled' };
    case 'LuuNhap':
      return { label: 'Lưu nháp', className: 'draft' };
    case 'ChoThanhToan':
      return { label: 'Chờ thanh toán', className: 'pending' };
    case 'DangLapDon':
      return { label: 'Đang lập đơn', className: 'draft' };
    default:
      return { label: status || 'Không xác định', className: 'draft' };
  }
};

export const CustomerOrderHistoryModal: React.FC<CustomerOrderHistoryModalProps> = ({
  customer,
  onClose,
}) => {
  const [orders, setOrders] = useState<CustomerOrder[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('ALL');
  const [expandedOrderId, setExpandedOrderId] = useState<number | null>(null);
  const [reloadTick, setReloadTick] = useState<number>(0);

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [onClose]);

  useEffect(() => {
    let ignore = false;

    getCustomerOrders(customer.id)
      .then((data) => {
        if (!ignore) {
          setOrders(data);
          setIsLoading(false);
        }
      })
      .catch((err: unknown) => {
        if (!ignore) {
          const message =
            (err as { response?: { data?: { message?: string } } })?.response?.data?.message ||
            'Không thể tải lịch sử mua hàng từ máy chủ.';
          setError(message);
          setIsLoading(false);
        }
      });

    return () => {
      ignore = true;
    };
  }, [customer.id, reloadTick]);

  const handleRetry = () => {
    setIsLoading(true);
    setError('');
    setReloadTick((t) => t + 1);
  };

  const summary = useMemo(() => {
    const totalOrders = orders.length;
    const paidOrders = orders.filter((o) => o.trangThai === 'DaThanhToan');
    const totalSpent = paidOrders.reduce((sum, o) => sum + o.tongTienThanhToan, 0);
    const totalItems = paidOrders.reduce((sum, o) => sum + o.soLuongSanPham, 0);
    return {
      totalOrders,
      paidCount: paidOrders.length,
      totalSpent,
      totalItems,
    };
  }, [orders]);

  const filteredOrders = useMemo(() => {
    if (statusFilter === 'ALL') return orders;
    if (statusFilter === 'OTHER') {
      return orders.filter((o) => o.trangThai !== 'DaThanhToan' && o.trangThai !== 'DaTraHang');
    }
    return orders.filter((o) => o.trangThai === statusFilter);
  }, [orders, statusFilter]);

  const tier = getTier(customer.diemTichLuy);

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-card history-modal-card" onClick={(e) => e.stopPropagation()}>
        {/* Header */}
        <div className="modal-header history-modal-header">
          <div className="history-customer-banner">
            <div className="avatar-circle history-avatar">
              {customer.tenKhachHang.charAt(0).toUpperCase()}
            </div>
            <div>
              <div className="history-title-row">
                <h3 className="modal-title">Lịch Sử Mua Hàng</h3>
                <span className={`tier-badge ${tier.className}`}>{tier.label}</span>
              </div>
              <div className="history-customer-sub">
                <strong>{customer.tenKhachHang}</strong> ·{' '}
                <span className="customer-phone">{formatPhone(customer.soDienThoai)}</span> ·{' '}
                <span className="points-pill">
                  ⭐ {customer.diemTichLuy.toLocaleString('vi-VN')} điểm
                </span>
              </div>
            </div>
          </div>
          <button
            type="button"
            className="btn-close-modal"
            onClick={onClose}
            aria-label="Đóng cửa sổ"
          >
            ✕
          </button>
        </div>

        {/* Summary KPI Strip */}
        <div className="history-summary-grid">
          <div className="history-summary-item">
            <span className="history-summary-label">Tổng số đơn</span>
            <span className="history-summary-value">{summary.totalOrders} đơn</span>
          </div>
          <div className="history-summary-item">
            <span className="history-summary-label">Tổng chi tiêu (đã thanh toán)</span>
            <span className="history-summary-value highlight">
              {formatCurrency(summary.totalSpent)}
            </span>
          </div>
          <div className="history-summary-item">
            <span className="history-summary-label">Sản phẩm đã mua</span>
            <span className="history-summary-value">{summary.totalItems} món</span>
          </div>
        </div>

        {/* Status Filter Pills */}
        <div className="history-filter-row">
          <button
            type="button"
            className={`history-filter-pill ${statusFilter === 'ALL' ? 'active' : ''}`}
            onClick={() => setStatusFilter('ALL')}
          >
            Tất cả ({orders.length})
          </button>
          <button
            type="button"
            className={`history-filter-pill ${statusFilter === 'DaThanhToan' ? 'active' : ''}`}
            onClick={() => setStatusFilter('DaThanhToan')}
          >
            Đã thanh toán ({summary.paidCount})
          </button>
          <button
            type="button"
            className={`history-filter-pill ${statusFilter === 'DaTraHang' ? 'active' : ''}`}
            onClick={() => setStatusFilter('DaTraHang')}
          >
            Đã trả hàng ({orders.filter((o) => o.trangThai === 'DaTraHang').length})
          </button>
          <button
            type="button"
            className={`history-filter-pill ${statusFilter === 'OTHER' ? 'active' : ''}`}
            onClick={() => setStatusFilter('OTHER')}
          >
            Khác ({orders.filter((o) => o.trangThai !== 'DaThanhToan' && o.trangThai !== 'DaTraHang').length})
          </button>
        </div>

        {/* Body Content */}
        <div className="history-table-container">
          {isLoading ? (
            <div className="history-state-box">
              <div className="history-spinner" />
              <p>Đang tải lịch sử mua hàng...</p>
            </div>
          ) : error ? (
            <div className="history-state-box error">
              <p>⚠️ {error}</p>
              <button
                type="button"
                className="btn-secondary"
                onClick={handleRetry}
              >
                Thử lại
              </button>
            </div>
          ) : filteredOrders.length === 0 ? (
            <div className="history-state-box">
              <div className="empty-icon">🧾</div>
              <div className="empty-title">
                {orders.length === 0
                  ? 'Khách hàng chưa có đơn hàng nào'
                  : 'Không có đơn hàng thuộc trạng thái này'}
              </div>
              <p className="history-empty-desc">
                {orders.length === 0
                  ? 'Các hóa đơn phát sinh khi mua hàng tại quầy POS sẽ tự động lưu tại đây.'
                  : 'Hãy chọn bộ lọc "Tất cả" để xem toàn bộ lịch sử giao dịch.'}
              </p>
            </div>
          ) : (
            <table className="history-orders-table">
              <thead>
                <tr>
                  <th>Mã đơn</th>
                  <th>Ngày mua</th>
                  <th>Thanh toán</th>
                  <th style={{ textAlign: 'right' }}>Tổng tiền</th>
                  <th style={{ textAlign: 'center' }}>Trạng thái</th>
                  <th style={{ width: '44px' }} />
                </tr>
              </thead>
              <tbody>
                {filteredOrders.map((order) => {
                  const statusMeta = getOrderStatusMeta(order.trangThai);
                  const isExpanded = expandedOrderId === order.id;

                  return (
                    <React.Fragment key={order.id}>
                      <tr
                        className={`history-order-row ${isExpanded ? 'expanded' : ''}`}
                        onClick={() => setExpandedOrderId(isExpanded ? null : order.id)}
                        title="Bấm để xem chi tiết sản phẩm trong đơn"
                      >
                        <td>
                          <div className="order-code-text">{order.maDonHang}</div>
                          <div className="order-sub-meta">
                            {order.soLuongSanPham} sản phẩm
                            {order.maGiamGia ? ` · Mã: ${order.maGiamGia}` : ''}
                          </div>
                        </td>
                        <td>
                          <div className="order-date-text">{formatDateTime(order.ngayTao)}</div>
                          {order.tenNhanVien && (
                            <div className="order-sub-meta">TN: {order.tenNhanVien}</div>
                          )}
                        </td>
                        <td>
                          <span className="payment-method-chip">
                            {order.tenHinhThucThanhToan || '—'}
                          </span>
                        </td>
                        <td style={{ textAlign: 'right' }}>
                          <div className="order-total-amount">
                            {formatCurrency(order.tongTienThanhToan)}
                          </div>
                          {order.soTienGiam > 0 && (
                            <div className="order-discount-note">
                              Giảm {formatCurrency(order.soTienGiam)}
                            </div>
                          )}
                        </td>
                        <td style={{ textAlign: 'center' }}>
                          <span className={`order-status-badge ${statusMeta.className}`}>
                            {statusMeta.label}
                          </span>
                        </td>
                        <td style={{ textAlign: 'center' }}>
                          <span className={`expand-chevron ${isExpanded ? 'open' : ''}`}>▾</span>
                        </td>
                      </tr>

                      {isExpanded && (
                        <tr className="history-detail-row">
                          <td colSpan={6}>
                            <div className="order-detail-panel">
                              <div className="order-detail-heading">
                                Chi tiết sản phẩm ({order.items.length} mặt hàng)
                              </div>
                              {order.items.length === 0 ? (
                                <div className="order-sub-meta">Không có dữ liệu chi tiết sản phẩm.</div>
                              ) : (
                                <table className="order-items-subtable">
                                  <thead>
                                    <tr>
                                      <th>Mã SP</th>
                                      <th>Tên sản phẩm</th>
                                      <th style={{ textAlign: 'center' }}>SL</th>
                                      <th style={{ textAlign: 'right' }}>Đơn giá</th>
                                      <th style={{ textAlign: 'right' }}>Thành tiền</th>
                                    </tr>
                                  </thead>
                                  <tbody>
                                    {order.items.map((item) => (
                                      <tr key={`${order.id}-${item.sanPhamId}`}>
                                        <td className="item-sku">{item.maSP}</td>
                                        <td className="item-name">{item.tenSP}</td>
                                        <td style={{ textAlign: 'center' }}>{item.soLuong}</td>
                                        <td style={{ textAlign: 'right' }}>
                                          {formatCurrency(item.donGia)}
                                        </td>
                                        <td style={{ textAlign: 'right', fontWeight: 600 }}>
                                          {formatCurrency(item.thanhTien)}
                                        </td>
                                      </tr>
                                    ))}
                                  </tbody>
                                </table>
                              )}

                              <div className="order-detail-footer">
                                <span>
                                  Tạm tính: <strong>{formatCurrency(order.tongTienHang)}</strong>
                                </span>
                                {order.soTienGiam > 0 && (
                                  <span>
                                    Giảm giá:{' '}
                                    <strong style={{ color: '#4A6B44' }}>
                                      -{formatCurrency(order.soTienGiam)}
                                    </strong>
                                  </span>
                                )}
                                <span>
                                  Thanh toán:{' '}
                                  <strong style={{ color: '#A67C52' }}>
                                    {formatCurrency(order.tongTienThanhToan)}
                                  </strong>
                                </span>
                              </div>
                            </div>
                          </td>
                        </tr>
                      )}
                    </React.Fragment>
                  );
                })}
              </tbody>
            </table>
          )}
        </div>

        {/* Footer */}
        <div className="modal-actions history-modal-footer">
          <span className="history-footer-hint">
            Bấm vào từng dòng đơn hàng để xem chi tiết sản phẩm đã mua.
          </span>
          <button type="button" className="btn-secondary" onClick={onClose}>
            Đóng
          </button>
        </div>
      </div>
    </div>
  );
};

export default CustomerOrderHistoryModal;

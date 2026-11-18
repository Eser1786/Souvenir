import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context';
import {
  createReturn,
  getReturnableOrder,
  getReturnsByOrder,
  searchReturnableOrders,
  type ReturnableOrder,
  type ReturnSlip,
  type TinhTrangHang,
} from '../../api/returns';
import './ReturnsScreen.css';

const formatVND = (amount: number) => new Intl.NumberFormat('vi-VN').format(Math.round(amount)) + ' đ';
const formatDateTime = (iso: string) =>
  new Date(iso).toLocaleString('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit', year: 'numeric' });
const formatDate = (iso: string) => new Date(iso).toLocaleDateString('vi-VN');

const TINH_TRANG_OPTIONS: { value: TinhTrangHang; label: string }[] = [
  { value: 'NguyenVen', label: 'Nguyên vẹn' },
  { value: 'LoiNhaSanXuat', label: 'Lỗi nhà sản xuất' },
  { value: 'HuHongDoKhach', label: 'Hư hỏng do khách' },
];
const tinhTrangLabel = (v: string) => TINH_TRANG_OPTIONS.find((o) => o.value === v)?.label ?? v;

const LY_DO_GOI_Y = ['Khách đổi ý', 'Sản phẩm lỗi', 'Giao nhầm mẫu', 'Không đúng mô tả'];

const TRANG_THAI_LABEL: Record<string, string> = {
  DaThanhToan: 'Đã thanh toán',
  DaTraHang: 'Đã trả hàng',
};

interface LineState {
  soLuong: number;
  tinhTrang: TinhTrangHang;
}

const IconArrowLeft = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M19 12H5M12 19l-7-7 7-7" />
  </svg>
);
const IconSearch = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round">
    <circle cx="11" cy="11" r="7" />
    <path d="m20 20-3.5-3.5" />
  </svg>
);
const IconCheck = () => (
  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round">
    <path d="M20 6 9 17l-5-5" />
  </svg>
);

/** UC08 - Trả hàng / Hoàn tiền: tra cứu hóa đơn gốc → chọn món trả → xác nhận hoàn tiền. */
export const ReturnsScreen: React.FC = () => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  // Tra cứu
  const [keyword, setKeyword] = useState('');
  const [tuNgay, setTuNgay] = useState('');
  const [denNgay, setDenNgay] = useState('');
  const [results, setResults] = useState<ReturnableOrder[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [searchError, setSearchError] = useState<string | null>(null);
  const [hasSearched, setHasSearched] = useState(false);

  // Hóa đơn đang chọn
  const [order, setOrder] = useState<ReturnableOrder | null>(null);
  const [lines, setLines] = useState<Record<number, LineState>>({});
  const [lyDo, setLyDo] = useState('');
  const [history, setHistory] = useState<ReturnSlip[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [slip, setSlip] = useState<ReturnSlip | null>(null);

  const isManager = useMemo(() => {
    const role = (user?.vaiTro || '').toLowerCase();
    return role.includes('quản lý') || role.includes('quanly') || role.includes('admin');
  }, [user]);

  const runSearch = async (e?: React.FormEvent) => {
    e?.preventDefault();
    if (!keyword.trim() && !tuNgay && !denNgay) {
      setSearchError('Nhập mã đơn, số điện thoại khách hoặc chọn ngày mua để tra cứu.');
      return;
    }
    setIsSearching(true);
    setSearchError(null);
    try {
      const data = await searchReturnableOrders({ keyword, tuNgay, denNgay });
      setResults(data);
      setHasSearched(true);
      if (data.length === 1) selectOrder(data[0]);
    } catch (err) {
      setSearchError(err instanceof Error ? err.message : 'Không thể tra cứu hóa đơn.');
    } finally {
      setIsSearching(false);
    }
  };

  const selectOrder = (o: ReturnableOrder) => {
    setOrder(o);
    setLines(
      Object.fromEntries(o.items.map((i) => [i.sanPhamId, { soLuong: 0, tinhTrang: 'NguyenVen' as TinhTrangHang }])),
    );
    setLyDo('');
    setSubmitError(null);
  };

  // Lịch sử phiếu trả của hóa đơn đang chọn
  useEffect(() => {
    if (!order) return;
    let ignore = false;
    getReturnsByOrder(order.id)
      .then((data) => !ignore && setHistory(data))
      .catch(() => !ignore && setHistory([]));
    return () => {
      ignore = true;
    };
  }, [order]);

  const setLine = (sanPhamId: number, patch: Partial<LineState>) =>
    setLines((prev) => ({ ...prev, [sanPhamId]: { ...prev[sanPhamId], ...patch } }));

  const selectedItems = useMemo(
    () => (order ? order.items.filter((i) => (lines[i.sanPhamId]?.soLuong ?? 0) > 0) : []),
    [order, lines],
  );
  const hasDamagedByCustomer = selectedItems.some((i) => lines[i.sanPhamId]?.tinhTrang === 'HuHongDoKhach');
  const tongHoanDuKien = useMemo(() => {
    if (!order) return 0;
    const sum = selectedItems.reduce((s, i) => s + i.donGiaHoan * lines[i.sanPhamId].soLuong, 0);
    return Math.min(sum, Math.max(0, order.tongTienThanhToan - order.tongDaHoan));
  }, [order, selectedItems, lines]);
  const tongSoLuongTra = selectedItems.reduce((s, i) => s + lines[i.sanPhamId].soLuong, 0);

  const canSubmit =
    !!order && order.coTheTra && selectedItems.length > 0 && !hasDamagedByCustomer && lyDo.trim().length > 0 && !isSubmitting;

  const handleSubmit = async () => {
    if (!order || !canSubmit) return;
    setIsSubmitting(true);
    setSubmitError(null);
    try {
      const result = await createReturn({
        donHangId: order.id,
        lyDoTra: lyDo.trim(),
        items: selectedItems.map((i) => ({
          sanPhamId: i.sanPhamId,
          soLuongTra: lines[i.sanPhamId].soLuong,
          tinhTrangHang: lines[i.sanPhamId].tinhTrang,
        })),
      });
      setSlip(result);
      // Làm mới hóa đơn & kết quả tra cứu (số lượng còn trả, trạng thái)
      const refreshed = await getReturnableOrder(order.id);
      selectOrder(refreshed);
      setResults((prev) => prev.map((o) => (o.id === refreshed.id ? refreshed : o)));
    } catch (err) {
      setSubmitError(err instanceof Error ? err.message : 'Không thể lập phiếu trả hàng.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="rt-screen">
      <header className="rt-header">
        <div className="rt-header-left">
          <div>
            <h1 className="rt-brand">MERA · SOUVENIR</h1>
            <span className="rt-brand-sub">Trả hàng / Hoàn tiền (UC08)</span>
          </div>
          <button type="button" className="rt-pill" onClick={() => navigate('/pos')}>
            <IconArrowLeft /> Quay lại Bán hàng (POS)
          </button>
        </div>
        <div className="rt-header-right">
          <span className="rt-user">{user?.hoTen || user?.tenDangNhap} · {isManager ? 'Quản lý' : 'Nhân viên'}</span>
          <button type="button" className="rt-logout" onClick={() => { logout(); navigate('/login'); }}>
            Đăng xuất
          </button>
        </div>
      </header>

      {!isManager && (
        <div className="rt-banner">Chức năng Trả hàng / Hoàn tiền chỉ dành cho Quản lý. Bạn có thể tra cứu nhưng không lập được phiếu.</div>
      )}

      <main className="rt-main">
        {/* ============ CỘT TRÁI: TRA CỨU HÓA ĐƠN GỐC ============ */}
        <section className="rt-card rt-search-col">
          <h2 className="rt-card-title">Tra cứu hóa đơn gốc</h2>
          <form className="rt-search-form" onSubmit={runSearch}>
            <label className="rt-label" htmlFor="rt-keyword">Mã đơn hoặc SĐT khách</label>
            <div className="rt-input-icon">
              <IconSearch />
              <input
                id="rt-keyword"
                className="rt-input"
                placeholder="VD: DH261118-00001 hoặc 0901234567"
                value={keyword}
                onChange={(e) => setKeyword(e.target.value)}
              />
            </div>
            <div className="rt-date-row">
              <div>
                <label className="rt-label" htmlFor="rt-from">Từ ngày</label>
                <input id="rt-from" type="date" className="rt-input" value={tuNgay} onChange={(e) => setTuNgay(e.target.value)} />
              </div>
              <div>
                <label className="rt-label" htmlFor="rt-to">Đến ngày</label>
                <input id="rt-to" type="date" className="rt-input" value={denNgay} onChange={(e) => setDenNgay(e.target.value)} />
              </div>
            </div>
            <button type="submit" className="rt-btn-primary" disabled={isSearching}>
              {isSearching ? 'Đang tra cứu...' : 'Tra cứu'}
            </button>
          </form>

          {searchError && <div className="rt-error">{searchError}</div>}

          <div className="rt-results">
            {hasSearched && results.length === 0 && !searchError && (
              <div className="rt-empty">Không tìm thấy hóa đơn nào. Thử tra theo số điện thoại hoặc ngày mua.</div>
            )}
            {results.map((o) => (
              <button
                type="button"
                key={o.id}
                className={`rt-result ${order?.id === o.id ? 'active' : ''}`}
                onClick={() => selectOrder(o)}
              >
                <div className="rt-result-top">
                  <span className="rt-result-code">{o.maDonHang}</span>
                  <span className="rt-result-total">{formatVND(o.tongTienThanhToan)}</span>
                </div>
                <div className="rt-result-meta">
                  {formatDateTime(o.ngayTao)} · {o.tenKhachHang || 'Khách vãng lai'}
                </div>
                <span className={`rt-badge ${o.coTheTra ? 'ok' : 'no'}`}>
                  {o.coTheTra ? `Còn hạn đến ${formatDate(o.hanDoiTra)}` : o.conHanDoiTra ? TRANG_THAI_LABEL[o.trangThai] ?? o.trangThai : 'Quá hạn đổi trả'}
                </span>
              </button>
            ))}
          </div>
        </section>

        {/* ============ CỘT PHẢI: LẬP PHIẾU TRẢ ============ */}
        <section className="rt-card rt-detail-col">
          {!order ? (
            <div className="rt-placeholder">
              <div className="rt-placeholder-title">Chưa chọn hóa đơn</div>
              <p>Tra cứu theo mã đơn in trên hóa đơn, số điện thoại khách hoặc ngày mua, rồi chọn hóa đơn để lập phiếu trả.</p>
            </div>
          ) : (
            <>
              <div className="rt-order-head">
                <div>
                  <h2 className="rt-card-title">Hóa đơn {order.maDonHang}</h2>
                  <div className="rt-order-meta">
                    {formatDateTime(order.ngayTao)} · {order.tenHinhThucThanhToan}
                    {order.tenKhachHang ? ` · ${order.tenKhachHang} (${order.soDienThoaiKhach})` : ' · Khách vãng lai'}
                  </div>
                </div>
                <span className={`rt-badge ${order.trangThai === 'DaTraHang' ? 'warn' : 'ok'}`}>
                  {TRANG_THAI_LABEL[order.trangThai] ?? order.trangThai}
                </span>
              </div>

              <div className="rt-policy">
                Chính sách: đổi trả trong {order.soNgayChoPhepDoiTra} ngày, hạn cuối <strong>{formatDate(order.hanDoiTra)}</strong>.
                Khách đã trả {formatVND(order.tongTienThanhToan)}
                {order.maGiamGia ? ` (có voucher ${order.maGiamGia})` : ''}; đã hoàn {formatVND(order.tongDaHoan)}.
              </div>

              {!order.coTheTra && <div className="rt-error">{order.lyDoKhongTheTra}</div>}

              <div className="rt-table-wrap">
                <table className="rt-table">
                  <thead>
                    <tr>
                      <th>Sản phẩm</th>
                      <th className="num">Đã mua</th>
                      <th className="num">Đã trả</th>
                      <th className="num">Giá hoàn</th>
                      <th className="center">SL trả</th>
                      <th>Tình trạng</th>
                    </tr>
                  </thead>
                  <tbody>
                    {order.items.map((i) => {
                      const line = lines[i.sanPhamId];
                      const disabled = !order.coTheTra || i.soLuongConTra === 0;
                      return (
                        <tr key={i.sanPhamId} className={line?.soLuong > 0 ? 'selected' : ''}>
                          <td>
                            <div className="rt-product">
                              {i.hinhAnh && <img src={i.hinhAnh} alt="" className="rt-thumb" />}
                              <div>
                                <div className="rt-product-name">{i.tenSP}</div>
                                <div className="rt-product-sku">{i.maSP} · Giá bán {formatVND(i.donGia)}</div>
                              </div>
                            </div>
                          </td>
                          <td className="num">{i.soLuongMua}</td>
                          <td className="num">{i.soLuongDaTra}</td>
                          <td className="num">{formatVND(i.donGiaHoan)}</td>
                          <td className="center">
                            <div className="rt-stepper">
                              <button
                                type="button"
                                disabled={disabled || !line || line.soLuong <= 0}
                                onClick={() => setLine(i.sanPhamId, { soLuong: line.soLuong - 1 })}
                                aria-label="Giảm"
                              >−</button>
                              <span>{line?.soLuong ?? 0}</span>
                              <button
                                type="button"
                                disabled={disabled || !line || line.soLuong >= i.soLuongConTra}
                                onClick={() => setLine(i.sanPhamId, { soLuong: line.soLuong + 1 })}
                                aria-label="Tăng"
                              >+</button>
                            </div>
                            <div className="rt-hint">tối đa {i.soLuongConTra}</div>
                          </td>
                          <td>
                            <select
                              className={`rt-select ${line?.tinhTrang === 'HuHongDoKhach' ? 'bad' : ''}`}
                              value={line?.tinhTrang ?? 'NguyenVen'}
                              disabled={disabled}
                              onChange={(e) => setLine(i.sanPhamId, { tinhTrang: e.target.value as TinhTrangHang })}
                            >
                              {TINH_TRANG_OPTIONS.map((opt) => (
                                <option key={opt.value} value={opt.value}>{opt.label}</option>
                              ))}
                            </select>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>

              <label className="rt-label" htmlFor="rt-reason">Lý do trả hàng</label>
              <div className="rt-chips">
                {LY_DO_GOI_Y.map((g) => (
                  <button type="button" key={g} className={`rt-chip ${lyDo === g ? 'active' : ''}`} onClick={() => setLyDo(g)}>
                    {g}
                  </button>
                ))}
              </div>
              <textarea
                id="rt-reason"
                className="rt-textarea"
                maxLength={255}
                rows={2}
                placeholder="Mô tả lý do, tình trạng hàng khách mang trả..."
                value={lyDo}
                onChange={(e) => setLyDo(e.target.value)}
                disabled={!order.coTheTra}
              />

              {hasDamagedByCustomer && (
                <div className="rt-error">Hàng hư hỏng do khách không đủ điều kiện đổi trả theo chính sách. Bỏ chọn món đó để tiếp tục.</div>
              )}
              {submitError && <div className="rt-error">{submitError}</div>}

              <div className="rt-summary">
                <div>
                  <div className="rt-summary-label">Tiền hoàn cho khách ({tongSoLuongTra} sản phẩm)</div>
                  <div className="rt-summary-value">{formatVND(tongHoanDuKien)}</div>
                  <div className="rt-summary-note">Hoàn bằng: {order.tenHinhThucThanhToan}</div>
                </div>
                <button type="button" className="rt-btn-primary rt-btn-lg" disabled={!canSubmit || !isManager} onClick={handleSubmit}>
                  {isSubmitting ? 'Đang xử lý...' : 'Xác nhận hoàn tiền'}
                </button>
              </div>

              {history.length > 0 && (
                <div className="rt-history">
                  <div className="rt-history-title">Phiếu trả đã lập cho hóa đơn này</div>
                  {history.map((h) => (
                    <div key={h.id} className="rt-history-row">
                      <span className="mono">{h.maPhieu}</span>
                      <span>{formatDateTime(h.ngayTra)}</span>
                      <span>{h.items.map((it) => `${it.tenSP} ×${it.soLuongTra}`).join(', ')}</span>
                      <strong>{formatVND(h.tongTienHoan)}</strong>
                    </div>
                  ))}
                </div>
              )}
            </>
          )}
        </section>
      </main>

      {/* ============ PHIẾU TRẢ VỪA LẬP ============ */}
      {slip && (
        <div className="rt-overlay" onMouseDown={(e) => e.target === e.currentTarget && setSlip(null)}>
          <div className="rt-modal" role="dialog" aria-modal="true">
            <div className="rt-success-icon"><IconCheck /></div>
            <h2 className="rt-modal-title">Đã lập phiếu trả {slip.maPhieu}</h2>
            <p className="rt-modal-sub">Đơn gốc {slip.maDonHangGoc} chuyển sang "Đã trả hàng" · tồn kho đã được cộng lại</p>
            <div className="rt-receipt">
              {slip.items.map((it) => (
                <div key={it.sanPhamId} className="rt-receipt-row">
                  <span>{it.tenSP} × {it.soLuongTra} <em>({tinhTrangLabel(it.tinhTrangHang)})</em></span>
                  <span>{formatVND(it.thanhTien)}</span>
                </div>
              ))}
              <div className="rt-receipt-row muted"><span>Lý do</span><span>{slip.lyDoTra}</span></div>
              {slip.diemTichLuyBiTru > 0 && (
                <div className="rt-receipt-row muted"><span>Điểm tích lũy bị trừ lại</span><span>−{slip.diemTichLuyBiTru} điểm</span></div>
              )}
              <div className="rt-receipt-row total">
                <span>Hoàn cho khách ({slip.hinhThucHoan})</span>
                <span>{formatVND(slip.tongTienHoan)}</span>
              </div>
            </div>
            <button type="button" className="rt-btn-primary rt-btn-block" onClick={() => setSlip(null)} autoFocus>
              Xong
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default ReturnsScreen;

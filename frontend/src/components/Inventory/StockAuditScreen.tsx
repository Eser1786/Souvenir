import React, { useState, useEffect, useMemo, useCallback, useRef } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context';
import { getProducts, type Product } from '../../api/products';
import {
  getStockAudits,
  saveDraftStockAudit,
  balanceStockAudit,
  type StockAudit,
  type StockAuditItemRequest,
} from '../../api/stockAudits';
import './StockAuditScreen.css';

interface AuditRowInput {
  selected: boolean;
  actualQty: string;
  reason: string;
}

const IconArrowLeft = () => (
  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="19" y1="12" x2="5" y2="12" />
    <polyline points="12 19 5 12 12 5" />
  </svg>
);

const IconSearch = () => (
  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="11" cy="11" r="8" />
    <line x1="21" y1="21" x2="16.65" y2="16.65" />
  </svg>
);

const IconClipboard = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2" />
    <rect x="8" y="2" width="8" height="4" rx="1" ry="1" />
  </svg>
);

const IconCheckCircle = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
    <polyline points="22 4 12 14.01 9 11.01" />
  </svg>
);

const IconAlert = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="10" />
    <line x1="12" y1="8" x2="12" y2="12" />
    <line x1="12" y1="16" x2="12.01" y2="16" />
  </svg>
);

const IconSave = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z" />
    <polyline points="17 21 17 13 7 13 7 21" />
    <polyline points="7 3 7 8 15 8" />
  </svg>
);

const IconCheck = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="20 6 9 17 4 12" />
  </svg>
);

const formatDateTime = (iso: string) => {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

const extractErrorMessage = (err: unknown, fallback: string): string => {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { message?: string } | undefined;
    if (data?.message) return data.message;
  }
  return err instanceof Error ? err.message : fallback;
};

const buildFreshInputs = (productList: Product[]): Record<number, AuditRowInput> => {
  const map: Record<number, AuditRowInput> = {};
  for (const p of productList) {
    const isActive = (p.trangThai || 'KinhDoanh') === 'KinhDoanh';
    map[p.id] = {
      selected: isActive,
      actualQty: String(p.soLuongTon),
      reason: '',
    };
  }
  return map;
};

const buildDraftInputs = (productList: Product[], audit: StockAudit): Record<number, AuditRowInput> => {
  const itemMap = new Map(audit.items.map((it) => [it.sanPhamId, it]));
  const map: Record<number, AuditRowInput> = {};
  for (const p of productList) {
    const saved = itemMap.get(p.id);
    if (saved) {
      map[p.id] = {
        selected: true,
        actualQty: String(saved.soTonThucTe),
        reason: saved.lyDoChenhLech ?? '',
      };
    } else {
      map[p.id] = {
        selected: false,
        actualQty: String(p.soLuongTon),
        reason: '',
      };
    }
  }
  return map;
};

export const StockAuditScreen: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const toastTimerRef = useRef<number | null>(null);

  const [products, setProducts] = useState<Product[]>([]);
  const [audits, setAudits] = useState<StockAudit[]>([]);
  const [rowInputs, setRowInputs] = useState<Record<number, AuditRowInput>>({});
  const [activeDraft, setActiveDraft] = useState<StockAudit | null>(null);

  const [isLoading, setIsLoading] = useState(true);
  const [isSavingDraft, setIsSavingDraft] = useState(false);
  const [isBalancing, setIsBalancing] = useState(false);

  const [searchTerm, setSearchTerm] = useState('');
  const [rowFilter, setRowFilter] = useState<'all' | 'diff' | 'missingReason'>('all');
  const [historyFilter, setHistoryFilter] = useState<'all' | 'DangKiemKe' | 'DaCanBang'>('all');
  const [showValidationErrors, setShowValidationErrors] = useState(false);
  const [viewingAudit, setViewingAudit] = useState<StockAudit | null>(null);

  const [toast, setToast] = useState<{ text: string; type: 'success' | 'error' } | null>(null);

  const showToast = useCallback((text: string, type: 'success' | 'error' = 'success') => {
    setToast({ text, type });
    if (toastTimerRef.current !== null) {
      window.clearTimeout(toastTimerRef.current);
    }
    toastTimerRef.current = window.setTimeout(() => setToast(null), 4000);
  }, []);

  const refreshData = useCallback(async (keepDraft?: StockAudit | null) => {
    const [productList, auditList] = await Promise.all([
      getProducts({ includeAllStatus: true }),
      getStockAudits(),
    ]);
    setProducts(productList);
    setAudits(auditList);
    if (keepDraft) {
      setRowInputs(buildDraftInputs(productList, keepDraft));
    } else {
      setRowInputs(buildFreshInputs(productList));
    }
  }, []);

  useEffect(() => {
    let ignore = false;
    Promise.all([getProducts({ includeAllStatus: true }), getStockAudits()])
      .then(([productList, auditList]) => {
        if (ignore) return;
        setProducts(productList);
        setAudits(auditList);
        setRowInputs(buildFreshInputs(productList));
      })
      .catch((err) => {
        if (!ignore) {
          showToast(extractErrorMessage(err, 'Không thể tải dữ liệu kiểm kê kho.'), 'error');
        }
      })
      .finally(() => {
        if (!ignore) setIsLoading(false);
      });

    return () => {
      ignore = true;
      if (toastTimerRef.current !== null) {
        window.clearTimeout(toastTimerRef.current);
      }
    };
  }, [showToast]);

  const parsedRows = useMemo(() => {
    return products.map((p) => {
      const input = rowInputs[p.id] ?? {
        selected: false,
        actualQty: String(p.soLuongTon),
        reason: '',
      };
      const raw = input.actualQty.trim();
      const parsed = raw === '' ? NaN : Number(raw);
      const isValidQty = Number.isInteger(parsed) && parsed >= 0;
      const actual = isValidQty ? parsed : p.soLuongTon;
      const diff = isValidQty ? actual - p.soLuongTon : 0;
      const missingReason = input.selected && diff !== 0 && input.reason.trim().length === 0;

      return {
        product: p,
        input,
        isValidQty,
        actual,
        diff,
        missingReason,
      };
    });
  }, [products, rowInputs]);

  const stats = useMemo(() => {
    const selected = parsedRows.filter((r) => r.input.selected);
    const matchedCount = selected.filter((r) => r.isValidQty && r.diff === 0).length;
    const mismatched = selected.filter((r) => r.isValidQty && r.diff !== 0);
    const surplusTotal = mismatched.filter((r) => r.diff > 0).reduce((s, r) => s + r.diff, 0);
    const shortageTotal = mismatched.filter((r) => r.diff < 0).reduce((s, r) => s + Math.abs(r.diff), 0);
    const missingReasonCount = selected.filter((r) => r.missingReason).length;
    const draftCount = audits.filter((a) => a.trangThai === 'DangKiemKe').length;

    return {
      selectedCount: selected.length,
      matchedCount,
      mismatchedCount: mismatched.length,
      surplusTotal,
      shortageTotal,
      missingReasonCount,
      draftCount,
    };
  }, [parsedRows, audits]);

  const visibleRows = useMemo(() => {
    const kw = searchTerm.trim().toLowerCase();
    return parsedRows.filter((row) => {
      const matchSearch =
        !kw ||
        row.product.maSP.toLowerCase().includes(kw) ||
        row.product.tenSP.toLowerCase().includes(kw) ||
        row.product.tenDanhMuc.toLowerCase().includes(kw);
      if (!matchSearch) return false;
      if (rowFilter === 'diff') return row.input.selected && row.diff !== 0;
      if (rowFilter === 'missingReason') return row.missingReason;
      return true;
    });
  }, [parsedRows, searchTerm, rowFilter]);

  const allVisibleSelected =
    visibleRows.length > 0 && visibleRows.every((r) => r.input.selected);

  const handleToggleSelectAllVisible = () => {
    const targetState = !allVisibleSelected;
    setRowInputs((prev) => {
      const next = { ...prev };
      for (const row of visibleRows) {
        const existing = next[row.product.id] ?? {
          selected: false,
          actualQty: String(row.product.soLuongTon),
          reason: '',
        };
        next[row.product.id] = { ...existing, selected: targetState };
      }
      return next;
    });
  };

  const updateRowInput = (productId: number, patch: Partial<AuditRowInput>, fallbackQty: number) => {
    setRowInputs((prev) => {
      const current = prev[productId] ?? {
        selected: false,
        actualQty: String(fallbackQty),
        reason: '',
      };
      return {
        ...prev,
        [productId]: { ...current, ...patch },
      };
    });
  };

  const buildRequestItems = (): StockAuditItemRequest[] | null => {
    const selected = parsedRows.filter((r) => r.input.selected);
    if (selected.length === 0) {
      showToast('Vui lòng chọn ít nhất một sản phẩm để kiểm kê.', 'error');
      return null;
    }

    const invalidRow = selected.find((r) => !r.isValidQty);
    if (invalidRow) {
      showToast(
        `Số lượng thực tế của "${invalidRow.product.tenSP}" phải là số nguyên >= 0.`,
        'error',
      );
      return null;
    }

    return selected.map((r) => ({
      sanPhamId: r.product.id,
      soTonThucTe: Number(r.input.actualQty.trim()),
      lyDoChenhLech: r.diff !== 0 || r.input.reason.trim() ? r.input.reason.trim() : null,
    }));
  };

  const handleSaveDraft = async () => {
    const items = buildRequestItems();
    if (!items) return;

    try {
      setIsSavingDraft(true);
      const saved = await saveDraftStockAudit({
        id: activeDraft?.id,
        items,
      });
      setActiveDraft(saved);
      await refreshData(saved);
      setShowValidationErrors(false);
      showToast(`Đã lưu tạm phiếu ${saved.maPhieu} (tồn kho sổ sách giữ nguyên).`, 'success');
    } catch (err) {
      showToast(extractErrorMessage(err, 'Không thể lưu tạm phiếu kiểm kê.'), 'error');
    } finally {
      setIsSavingDraft(false);
    }
  };

  const handleBalanceStock = async () => {
    const items = buildRequestItems();
    if (!items) return;

    if (stats.missingReasonCount > 0) {
      setShowValidationErrors(true);
      setRowFilter('missingReason');
      showToast(
        `Còn ${stats.missingReasonCount} sản phẩm chênh lệch chưa nhập lý do giải trình.`,
        'error',
      );
      return;
    }

    const confirmText =
      stats.mismatchedCount > 0
        ? `Xác nhận cân bằng tồn kho? Có ${stats.mismatchedCount} sản phẩm sẽ được điều chỉnh số lượng tồn theo thực tế và khóa phiếu.`
        : 'Tất cả sản phẩm đều khớp sổ sách. Xác nhận hoàn tất và khóa phiếu kiểm kê?';

    if (!window.confirm(confirmText)) return;

    try {
      setIsBalancing(true);
      const balanced = await balanceStockAudit({
        id: activeDraft?.id,
        items,
      });
      setActiveDraft(null);
      setShowValidationErrors(false);
      setRowFilter('all');
      await refreshData(null);
      showToast(`Cân bằng tồn kho thành công (${balanced.maPhieu}).`, 'success');
    } catch (err) {
      showToast(extractErrorMessage(err, 'Không thể cân bằng tồn kho.'), 'error');
    } finally {
      setIsBalancing(false);
    }
  };

  const handleResumeDraft = (audit: StockAudit) => {
    setActiveDraft(audit);
    setRowInputs(buildDraftInputs(products, audit));
    setRowFilter('all');
    setShowValidationErrors(false);
    window.scrollTo({ top: 0, behavior: 'smooth' });
    showToast(`Đã nạp phiếu tạm ${audit.maPhieu} để tiếp tục kiểm kê.`, 'success');
  };

  const handleResetNewAudit = () => {
    setActiveDraft(null);
    setRowInputs(buildFreshInputs(products));
    setRowFilter('all');
    setShowValidationErrors(false);
  };

  const filteredHistory = useMemo(() => {
    if (historyFilter === 'all') return audits;
    return audits.filter((a) => a.trangThai === historyFilter);
  }, [audits, historyFilter]);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="audit-screen">
      {/* Header */}
      <header className="audit-header">
        <div className="audit-header-left">
          <div className="audit-brand">
            <h1 className="audit-brand-title">MERA · SOUVENIR</h1>
            <span className="audit-brand-sub">Kiểm kê &amp; Đối soát tồn kho</span>
          </div>

          <nav className="audit-nav">
            <button type="button" className="audit-nav-pill" onClick={() => navigate('/pos')}>
              <IconArrowLeft /> Bán hàng (POS)
            </button>
            <button type="button" className="audit-nav-pill" onClick={() => navigate('/products')}>
              Sản phẩm
            </button>
            <button type="button" className="audit-nav-pill" onClick={() => navigate('/inventory/receipts')}>
              Nhập kho
            </button>
            <button type="button" className="audit-nav-pill active">
              Kiểm kê kho
            </button>
            <button type="button" className="audit-nav-pill" onClick={() => navigate('/reports')}>
              Báo cáo
            </button>
          </nav>
        </div>

        <div className="audit-header-right">
          <div className="audit-user-tag">
            <div className="audit-user-avatar">{user?.hoTen?.charAt(0) || 'Q'}</div>
            <div className="audit-user-meta">
              <span className="audit-user-name">{user?.hoTen || 'Quản lý'}</span>
              <span className="audit-user-role">{user?.vaiTro || 'QuanLy'}</span>
            </div>
          </div>
          <button type="button" className="audit-nav-pill" onClick={handleLogout}>
            Đăng xuất
          </button>
        </div>
      </header>

      <main className="audit-content">
        {/* KPI Cards */}
        <section className="audit-stats-grid">
          <div className="audit-stat-card">
            <div className="audit-stat-icon"><IconClipboard /></div>
            <div className="audit-stat-info">
              <span className="audit-stat-label">Đang chọn kiểm kê</span>
              <span className="audit-stat-value">{stats.selectedCount}</span>
              <span className="audit-stat-sub">Trên tổng {products.length} mặt hàng</span>
            </div>
          </div>

          <div className="audit-stat-card">
            <div className="audit-stat-icon ok"><IconCheckCircle /></div>
            <div className="audit-stat-info">
              <span className="audit-stat-label">Số mặt hàng khớp</span>
              <span className="audit-stat-value">{stats.matchedCount}</span>
              <span className="audit-stat-sub">Thực tế bằng sổ sách</span>
            </div>
          </div>

          <div className="audit-stat-card">
            <div className="audit-stat-icon warn"><IconAlert /></div>
            <div className="audit-stat-info">
              <span className="audit-stat-label">Mặt hàng chênh lệch</span>
              <span className="audit-stat-value" style={{ color: stats.mismatchedCount > 0 ? '#B91C1C' : '#1C1C1E' }}>
                {stats.mismatchedCount}
              </span>
              <span className="audit-stat-sub">
                Thừa +{stats.surplusTotal} · Thiếu -{stats.shortageTotal}
              </span>
            </div>
          </div>

          <div className="audit-stat-card">
            <div className="audit-stat-icon sand"><IconSave /></div>
            <div className="audit-stat-info">
              <span className="audit-stat-label">Phiếu đang lưu tạm</span>
              <span className="audit-stat-value">{stats.draftCount}</span>
              <span className="audit-stat-sub">Chờ hoàn tất &amp; cân bằng</span>
            </div>
          </div>
        </section>

        {/* Active Draft Banner */}
        {activeDraft && (
          <div className="audit-draft-banner">
            <div className="audit-draft-banner-left">
              <span className="audit-draft-pill">PHIẾU TẠM</span>
              <span>
                Đang tiếp tục kiểm kê phiếu <strong>{activeDraft.maPhieu}</strong> (lưu lúc{' '}
                {formatDateTime(activeDraft.ngayKiemKe)})
              </span>
            </div>
            <button type="button" className="btn-table-action" onClick={handleResetNewAudit}>
              Tạo phiếu kiểm kê mới
            </button>
          </div>
        )}

        {/* Audit Sheet Card */}
        <section className="audit-card">
          <div className="audit-card-header">
            <div>
              <h2 className="audit-card-title">Bảng kiểm đếm tồn kho thực tế (UC13)</h2>
              <p className="audit-card-desc">
                Nhập số lượng đếm được thực tế tại cửa hàng. Mọi mặt hàng có chênh lệch bắt buộc phải nhập lý do giải trình trước khi cân bằng kho.
              </p>
            </div>
          </div>

          <div className="audit-toolbar">
            <div className="audit-search-box">
              <span className="audit-search-icon"><IconSearch /></span>
              <input
                type="text"
                className="audit-search-input"
                placeholder="Tìm theo mã SP, tên sản phẩm hoặc danh mục..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>

            <div className="audit-filter-pills">
              <button
                type="button"
                className={`audit-filter-pill ${rowFilter === 'all' ? 'active' : ''}`}
                onClick={() => setRowFilter('all')}
              >
                Tất cả ({products.length})
              </button>
              <button
                type="button"
                className={`audit-filter-pill ${rowFilter === 'diff' ? 'active' : ''}`}
                onClick={() => setRowFilter('diff')}
              >
                Có chênh lệch ({stats.mismatchedCount})
              </button>
              <button
                type="button"
                className={`audit-filter-pill warn ${rowFilter === 'missingReason' ? 'active' : ''}`}
                onClick={() => setRowFilter('missingReason')}
              >
                Thiếu lý do ({stats.missingReasonCount})
              </button>
            </div>
          </div>

          <div className="audit-table-wrap">
            {isLoading ? (
              <div className="audit-empty">Đang tải danh sách sản phẩm trong kho...</div>
            ) : visibleRows.length === 0 ? (
              <div className="audit-empty">Không có sản phẩm nào khớp bộ lọc hiện tại.</div>
            ) : (
              <table className="audit-table">
                <thead>
                  <tr>
                    <th style={{ width: '44px', textAlign: 'center' }}>
                      <input
                        type="checkbox"
                        checked={allVisibleSelected}
                        onChange={handleToggleSelectAllVisible}
                        aria-label="Chọn tất cả sản phẩm hiển thị"
                      />
                    </th>
                    <th>Sản phẩm</th>
                    <th style={{ width: '110px', textAlign: 'right' }}>Tồn sổ sách</th>
                    <th style={{ width: '130px', textAlign: 'center' }}>Tồn thực tế</th>
                    <th style={{ width: '125px', textAlign: 'center' }}>Chênh lệch</th>
                    <th>Lý do giải trình</th>
                  </tr>
                </thead>
                <tbody>
                  {visibleRows.map(({ product, input, isValidQty, diff, missingReason }) => {
                    const showReasonError = (showValidationErrors || input.reason.length > 0) && missingReason;
                    const rowClass = !input.selected
                      ? 'row-unselected'
                      : showValidationErrors && missingReason
                        ? 'row-missing-reason'
                        : '';

                    return (
                      <tr key={product.id} className={rowClass}>
                        <td style={{ textAlign: 'center' }}>
                          <input
                            type="checkbox"
                            checked={input.selected}
                            onChange={(e) =>
                              updateRowInput(product.id, { selected: e.target.checked }, product.soLuongTon)
                            }
                            aria-label={`Chọn ${product.tenSP}`}
                          />
                        </td>
                        <td>
                          <div className="audit-product-sku">{product.maSP}</div>
                          <div className="audit-product-name">{product.tenSP}</div>
                          <div className="audit-product-meta">
                            {product.tenDanhMuc} · {product.tenDonVi || 'Cái'}
                            {product.trangThai && product.trangThai !== 'KinhDoanh' ? ' · Ngừng KD' : ''}
                          </div>
                        </td>
                        <td style={{ textAlign: 'right', fontWeight: 700 }}>
                          {product.soLuongTon}
                        </td>
                        <td style={{ textAlign: 'center' }}>
                          <input
                            type="number"
                            min={0}
                            step={1}
                            disabled={!input.selected}
                            className="audit-qty-input"
                            value={input.actualQty}
                            onChange={(e) =>
                              updateRowInput(product.id, { actualQty: e.target.value }, product.soLuongTon)
                            }
                          />
                        </td>
                        <td style={{ textAlign: 'center' }}>
                          {!input.selected ? (
                            <span style={{ color: '#9E958C', fontSize: '12px' }}>Bỏ qua</span>
                          ) : !isValidQty ? (
                            <span className="diff-badge shortage">Không hợp lệ</span>
                          ) : diff === 0 ? (
                            <span className="diff-badge match">Khớp (0)</span>
                          ) : diff > 0 ? (
                            <span className="diff-badge surplus">Thừa +{diff}</span>
                          ) : (
                            <span className="diff-badge shortage">Thiếu {diff}</span>
                          )}
                        </td>
                        <td>
                          <input
                            type="text"
                            maxLength={255}
                            disabled={!input.selected || (isValidQty && diff === 0)}
                            placeholder={
                              !input.selected
                                ? 'Không kiểm kê'
                                : diff === 0
                                  ? 'Số lượng khớp, không cần giải trình'
                                  : 'Bắt buộc nhập lý do (hư hỏng, mất mát, nhầm mã...)'
                            }
                            className={`audit-reason-input ${showReasonError ? 'required-error' : ''}`}
                            value={input.reason}
                            onChange={(e) =>
                              updateRowInput(product.id, { reason: e.target.value }, product.soLuongTon)
                            }
                          />
                          {showValidationErrors && missingReason && (
                            <div className="audit-reason-hint">
                              Bắt buộc nhập lý do khi số lượng thực tế khác sổ sách
                            </div>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            )}
          </div>

          <div className="audit-action-bar">
            <div className="audit-action-note">
              * <strong>Lưu tạm</strong> chỉ ghi nhận kết quả đang đếm, chưa thay đổi tồn kho.{' '}
              <strong>Cân bằng tồn kho</strong> sẽ cập nhật tồn kho theo thực tế và khóa phiếu.
            </div>
            <div className="audit-action-buttons">
              <button
                type="button"
                className="btn-audit-secondary"
                disabled={isLoading || isSavingDraft || isBalancing || stats.selectedCount === 0}
                onClick={handleSaveDraft}
              >
                <IconSave />
                {isSavingDraft ? 'Đang lưu tạm...' : 'Lưu tạm kiểm kê'}
              </button>
              <button
                type="button"
                className="btn-audit-primary"
                disabled={isLoading || isSavingDraft || isBalancing || stats.selectedCount === 0}
                onClick={handleBalanceStock}
              >
                <IconCheck />
                {isBalancing ? 'Đang cân bằng...' : 'Cân bằng tồn kho'}
              </button>
            </div>
          </div>
        </section>

        {/* Audit History Card */}
        <section className="audit-card">
          <div className="audit-card-header">
            <div>
              <h2 className="audit-card-title">Lịch sử phiếu kiểm kê</h2>
              <p className="audit-card-desc">
                Có thể mở lại phiếu đang tạm lưu để kiểm đếm tiếp hoặc xem chi tiết phiếu đã cân bằng.
              </p>
            </div>
            <div className="audit-filter-pills">
              <button
                type="button"
                className={`audit-filter-pill ${historyFilter === 'all' ? 'active' : ''}`}
                onClick={() => setHistoryFilter('all')}
              >
                Tất cả ({audits.length})
              </button>
              <button
                type="button"
                className={`audit-filter-pill ${historyFilter === 'DangKiemKe' ? 'active' : ''}`}
                onClick={() => setHistoryFilter('DangKiemKe')}
              >
                Đang kiểm kê ({stats.draftCount})
              </button>
              <button
                type="button"
                className={`audit-filter-pill ${historyFilter === 'DaCanBang' ? 'active' : ''}`}
                onClick={() => setHistoryFilter('DaCanBang')}
              >
                Đã cân bằng ({audits.length - stats.draftCount})
              </button>
            </div>
          </div>

          <div className="audit-table-wrap">
            {filteredHistory.length === 0 ? (
              <div className="audit-empty">Chưa có phiếu kiểm kê kho nào.</div>
            ) : (
              <table className="audit-table">
                <thead>
                  <tr>
                    <th>Mã phiếu</th>
                    <th>Người lập</th>
                    <th>Thời gian</th>
                    <th style={{ textAlign: 'center' }}>Số SP kiểm</th>
                    <th style={{ textAlign: 'center' }}>Chênh lệch</th>
                    <th style={{ textAlign: 'center' }}>Trạng thái</th>
                    <th style={{ textAlign: 'right' }}>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredHistory.map((a) => (
                    <tr key={a.id}>
                      <td>
                        <span className="audit-product-sku">{a.maPhieu}</span>
                      </td>
                      <td><strong>{a.tenNguoiKiemKe || 'Quản lý'}</strong></td>
                      <td>{formatDateTime(a.ngayKiemKe)}</td>
                      <td style={{ textAlign: 'center' }}>
                        {a.tongSanPham} ({a.soSanPhamChenhLech} lệch)
                      </td>
                      <td style={{ textAlign: 'center' }}>
                        {a.soSanPhamChenhLech === 0 ? (
                          <span className="diff-badge match">Khớp 100%</span>
                        ) : (
                          <span>
                            {a.tongChenhLechThua > 0 && (
                              <span className="diff-badge surplus" style={{ marginRight: '4px' }}>
                                +{a.tongChenhLechThua}
                              </span>
                            )}
                            {a.tongChenhLechThieu > 0 && (
                              <span className="diff-badge shortage">-{a.tongChenhLechThieu}</span>
                            )}
                          </span>
                        )}
                      </td>
                      <td style={{ textAlign: 'center' }}>
                        <span className={`status-badge ${a.trangThai === 'DaCanBang' ? 'balanced' : 'draft'}`}>
                          {a.trangThai === 'DaCanBang' ? 'Đã cân bằng' : 'Đang kiểm kê'}
                        </span>
                      </td>
                      <td style={{ textAlign: 'right' }}>
                        {a.trangThai === 'DangKiemKe' ? (
                          <button
                            type="button"
                            className="btn-table-action resume"
                            onClick={() => handleResumeDraft(a)}
                          >
                            Tiếp tục kiểm kê
                          </button>
                        ) : (
                          <button
                            type="button"
                            className="btn-table-action"
                            onClick={() => setViewingAudit(a)}
                          >
                            Xem chi tiết
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </section>
      </main>

      {/* Read-only Detail Modal */}
      {viewingAudit && (
        <div className="audit-modal-overlay" onClick={() => setViewingAudit(null)}>
          <div className="audit-modal" onClick={(e) => e.stopPropagation()}>
            <div className="audit-modal-header">
              <h3 className="audit-modal-title">Chi tiết phiếu kiểm kê {viewingAudit.maPhieu}</h3>
              <button
                type="button"
                className="audit-modal-close"
                onClick={() => setViewingAudit(null)}
                aria-label="Đóng"
              >
                ✕
              </button>
            </div>
            <div className="audit-modal-body">
              <div className="audit-modal-meta">
                <div>
                  <span style={{ color: '#78716C', display: 'block', fontSize: '11px' }}>NGƯỜI KIỂM KÊ</span>
                  <strong>{viewingAudit.tenNguoiKiemKe || 'Quản lý'}</strong>
                </div>
                <div>
                  <span style={{ color: '#78716C', display: 'block', fontSize: '11px' }}>THỜI GIAN CHỐT</span>
                  <strong>{formatDateTime(viewingAudit.ngayKiemKe)}</strong>
                </div>
                <div>
                  <span style={{ color: '#78716C', display: 'block', fontSize: '11px' }}>TRẠNG THÁI</span>
                  <strong>{viewingAudit.trangThai === 'DaCanBang' ? 'Đã cân bằng' : 'Đang kiểm kê'}</strong>
                </div>
              </div>

              <div className="audit-table-wrap">
                <table className="audit-table" style={{ minWidth: '640px' }}>
                  <thead>
                    <tr>
                      <th>Sản phẩm</th>
                      <th style={{ textAlign: 'right' }}>Sổ sách</th>
                      <th style={{ textAlign: 'right' }}>Thực tế</th>
                      <th style={{ textAlign: 'center' }}>Chênh lệch</th>
                      <th>Lý do giải trình</th>
                    </tr>
                  </thead>
                  <tbody>
                    {viewingAudit.items.map((item) => (
                      <tr key={item.sanPhamId}>
                        <td>
                          <div className="audit-product-sku">{item.maSP}</div>
                          <div className="audit-product-name">{item.tenSP}</div>
                        </td>
                        <td style={{ textAlign: 'right' }}>{item.soTonSoSach}</td>
                        <td style={{ textAlign: 'right', fontWeight: 700 }}>{item.soTonThucTe}</td>
                        <td style={{ textAlign: 'center' }}>
                          {item.soLuongChenhLech === 0 ? (
                            <span className="diff-badge match">0</span>
                          ) : item.soLuongChenhLech > 0 ? (
                            <span className="diff-badge surplus">+{item.soLuongChenhLech}</span>
                          ) : (
                            <span className="diff-badge shortage">{item.soLuongChenhLech}</span>
                          )}
                        </td>
                        <td>{item.lyDoChenhLech || <span style={{ color: '#9E958C' }}>—</span>}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Toast */}
      {toast && (
        <div className={`audit-toast ${toast.type}`}>
          <span>{toast.type === 'success' ? '✓' : '⚠️'}</span>
          <span>{toast.text}</span>
        </div>
      )}
    </div>
  );
};

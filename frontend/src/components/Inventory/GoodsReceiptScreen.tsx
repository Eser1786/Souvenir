import React, { useState, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context';
import { getProducts, type Product } from '../../api/products';
import {
  getSuppliers,
  getGoodsReceipts,
  createGoodsReceipt,
  type Supplier,
  type GoodsReceipt,
  type GoodsReceiptItemInput,
} from '../../api/inventory';
import './GoodsReceiptScreen.css';

// SVG Icons
const IconArrowLeft = () => (
  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="19" y1="12" x2="5" y2="12" />
    <polyline points="12 19 5 12 12 5" />
  </svg>
);

const IconPlus = () => (
  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="12" y1="5" x2="12" y2="19" />
    <line x1="5" y1="12" x2="19" y2="12" />
  </svg>
);

const IconTrash = () => (
  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="3 6 5 6 21 6" />
    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" />
  </svg>
);

const IconCheck = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="20 6 9 17 4 12" />
  </svg>
);

interface ReceiptLineItem {
  id: string;
  isNewProduct: boolean;
  sanPhamId?: number;
  maSP: string;
  tenSP: string;
  donViTinh: string;
  soLuongNhap: number;
  donGiaNhap: number;
}

export const GoodsReceiptScreen: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [receiptHistory, setReceiptHistory] = useState<GoodsReceipt[]>([]);
  const [selectedSupplierId, setSelectedSupplierId] = useState<number>(1);
  const [lineItems, setLineItems] = useState<ReceiptLineItem[]>([]);
  const [submitting, setSubmitting] = useState<boolean>(false);

  // Toast
  const [toastMessage, setToastMessage] = useState<string | null>(null);
  const [toastType, setToastType] = useState<'success' | 'error'>('success');

  const showToast = (msg: string, type: 'success' | 'error' = 'success') => {
    setToastMessage(msg);
    setToastType(type);
    setTimeout(() => setToastMessage(null), 3500);
  };

  const loadInitialData = async () => {
    try {
      const [supplierList, productList, receipts] = await Promise.all([
        getSuppliers(),
        getProducts({ includeAllStatus: true }),
        getGoodsReceipts(),
      ]);
      setSuppliers(supplierList);
      if (supplierList.length > 0) {
        setSelectedSupplierId(supplierList[0].id);
      }
      setProducts(productList);
      setReceiptHistory(receipts);

      // Default one row
      if (productList.length > 0) {
        const first = productList[0];
        setLineItems([
          {
            id: 'row-1',
            isNewProduct: false,
            sanPhamId: first.id,
            maSP: first.maSP,
            tenSP: first.tenSP,
            donViTinh: first.tenDonVi || 'Cái',
            soLuongNhap: 10,
            donGiaNhap: first.giaNhap || 15000,
          },
        ]);
      }
    } catch {
      showToast('Lỗi khi tải dữ liệu khởi tạo', 'error');
    }
  };

  useEffect(() => {
    loadInitialData();
  }, []);

  // Tính tổng tiền phiếu nhập theo bước 39, 41 UC12
  const totalAmount = useMemo(() => {
    return lineItems.reduce((acc, item) => acc + item.soLuongNhap * item.donGiaNhap, 0);
  }, [lineItems]);

  const totalQuantity = useMemo(() => {
    return lineItems.reduce((acc, item) => acc + item.soLuongNhap, 0);
  }, [lineItems]);

  const handleAddLineItem = () => {
    const newId = `row-${Date.now()}`;
    const defaultProduct = products[0];
    setLineItems([
      ...lineItems,
      {
        id: newId,
        isNewProduct: false,
        sanPhamId: defaultProduct?.id,
        maSP: defaultProduct?.maSP || '',
        tenSP: defaultProduct?.tenSP || '',
        donViTinh: defaultProduct?.tenDonVi || 'Cái',
        soLuongNhap: 5,
        donGiaNhap: defaultProduct?.giaNhap || 20000,
      },
    ]);
  };

  const handleAddNewCustomProductLine = () => {
    const newId = `row-${Date.now()}`;
    setLineItems([
      ...lineItems,
      {
        id: newId,
        isNewProduct: true,
        maSP: '',
        tenSP: '',
        donViTinh: 'Cái',
        soLuongNhap: 10,
        donGiaNhap: 25000,
      },
    ]);
  };

  const handleRemoveLineItem = (id: string) => {
    if (lineItems.length <= 1) {
      showToast('Phiếu nhập phải có ít nhất 1 mặt hàng', 'error');
      return;
    }
    setLineItems(lineItems.filter((i) => i.id !== id));
  };

  const handleProductSelect = (id: string, productId: number) => {
    const selected = products.find((p) => p.id === productId);
    if (!selected) return;

    setLineItems(
      lineItems.map((item) => {
        if (item.id === id) {
          return {
            ...item,
            isNewProduct: false,
            sanPhamId: selected.id,
            maSP: selected.maSP,
            tenSP: selected.tenSP,
            donViTinh: selected.tenDonVi || 'Cái',
            donGiaNhap: selected.giaNhap || 15000,
          };
        }
        return item;
      })
    );
  };

  const handleUpdateLine = (id: string, field: keyof ReceiptLineItem, value: any) => {
    setLineItems(
      lineItems.map((item) => {
        if (item.id === id) {
          return { ...item, [field]: value };
        }
        return item;
      })
    );
  };

  // Bước 45: Nhấn "Xác nhận nhập kho" (UC12)
  const handleSubmitReceipt = async () => {
    if (!selectedSupplierId) {
      showToast('Vui lòng chọn Nhà cung cấp', 'error');
      return;
    }

    if (lineItems.length === 0) {
      showToast('Vui lòng thêm sản phẩm vào phiếu nhập', 'error');
      return;
    }

    for (const item of lineItems) {
      if (item.isNewProduct && !item.tenSP.trim()) {
        showToast('Vui lòng nhập tên cho mặt hàng mới', 'error');
        return;
      }
      if (item.soLuongNhap <= 0) {
        showToast('Số lượng nhập phải lớn hơn 0', 'error');
        return;
      }
      if (item.donGiaNhap < 0) {
        showToast('Đơn giá nhập không được âm', 'error');
        return;
      }
    }

    const payloadItems: GoodsReceiptItemInput[] = lineItems.map((item) => ({
      sanPhamId: item.isNewProduct ? undefined : item.sanPhamId,
      maSP: item.maSP.trim() || undefined,
      tenSP: item.tenSP.trim(),
      soLuongNhap: Number(item.soLuongNhap),
      donGiaNhap: Number(item.donGiaNhap),
    }));

    try {
      setSubmitting(true);
      const res = await createGoodsReceipt({
        nhaCungCapId: selectedSupplierId,
        chiTiet: payloadItems,
      });

      showToast(`Tạo phiếu nhập ${res.maPhieu} thành công! Số lượng tồn đã được cộng tăng.`, 'success');

      // Refresh data
      const [updatedProducts, updatedReceipts] = await Promise.all([
        getProducts({ includeAllStatus: true }),
        getGoodsReceipts(),
      ]);
      setProducts(updatedProducts);
      setReceiptHistory(updatedReceipts);

      // Reset form
      if (updatedProducts.length > 0) {
        const first = updatedProducts[0];
        setLineItems([
          {
            id: `row-${Date.now()}`,
            isNewProduct: false,
            sanPhamId: first.id,
            maSP: first.maSP,
            tenSP: first.tenSP,
            donViTinh: first.tenDonVi || 'Cái',
            soLuongNhap: 10,
            donGiaNhap: first.giaNhap || 15000,
          },
        ]);
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Lỗi khi lưu phiếu nhập kho';
      showToast(msg, 'error');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="receipt-screen">
      {/* Header */}
      <header className="receipt-header">
        <div className="receipt-header-left">
          <div className="brand-badge">
            <h1 className="brand-title">MERA · SOUVENIR</h1>
            <span className="brand-subtitle">Quản lý nhập kho & Nhà cung cấp</span>
          </div>
          <nav className="header-nav">
            <button type="button" className="nav-pill-btn" onClick={() => navigate('/products')}>
              <IconArrowLeft /> Quản lý sản phẩm
            </button>
            <button type="button" className="nav-pill-btn active">
              Phiếu nhập kho (UC12)
            </button>
            <button type="button" className="nav-pill-btn" onClick={() => navigate('/pos')}>
              Bán hàng (POS)
            </button>
          </nav>
        </div>

        <div className="product-header-right">
          <div className="user-tag">
            <div className="user-avatar">{user?.hoTen?.charAt(0) || 'Q'}</div>
            <div className="user-meta">
              <span className="user-name">{user?.hoTen || 'Quản lý kho'}</span>
              <span className="user-role">Quản lý</span>
            </div>
          </div>
        </div>
      </header>

      {/* Main Container */}
      <main className="receipt-content">
        <div className="receipt-grid-layout">
          {/* Left Form: Select Supplier & Products */}
          <div className="receipt-card">
            <h2 className="receipt-card-title">
              <span>Lập phiếu nhập kho từ Nhà cung cấp</span>
              <span style={{ fontSize: '12px', color: '#8C827A', fontWeight: 500 }}>
                Hệ thống tự động cộng dồn tồn kho
              </span>
            </h2>

            {/* Bước 2 UC12: Chọn Nhà cung cấp */}
            <div className="form-row">
              <label className="form-label">Chọn Nhà cung cấp *</label>
              <select
                className="form-select"
                value={selectedSupplierId}
                onChange={(e) => setSelectedSupplierId(Number(e.target.value))}
              >
                {suppliers.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.maNCC} - {s.tenNCC} ({s.soDienThoai})
                  </option>
                ))}
              </select>
            </div>

            {/* Chi tiết mặt hàng nhập */}
            <div style={{ marginTop: '20px' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span className="form-label" style={{ margin: 0 }}>
                  Danh sách sản phẩm nhập kho ({lineItems.length} món)
                </span>
                <div style={{ display: 'flex', gap: '8px' }}>
                  <button
                    type="button"
                    className="nav-pill-btn"
                    style={{ fontSize: '11px', padding: '4px 12px' }}
                    onClick={handleAddLineItem}
                  >
                    <IconPlus /> Chọn SP có sẵn
                  </button>
                  <button
                    type="button"
                    className="nav-pill-btn"
                    style={{ fontSize: '11px', padding: '4px 12px', borderColor: '#C8A27A', color: '#A67C52' }}
                    onClick={handleAddNewCustomProductLine}
                  >
                    <IconPlus /> Tạo nhanh SP mới (Nhánh 2a)
                  </button>
                </div>
              </div>

              <table className="items-table">
                <thead>
                  <tr>
                    <th style={{ width: '38%' }}>Sản phẩm</th>
                    <th style={{ width: '12%' }}>ĐVT</th>
                    <th style={{ width: '18%', textAlign: 'right' }}>Đơn giá nhập</th>
                    <th style={{ width: '14%', textAlign: 'center' }}>Số lượng</th>
                    <th style={{ width: '14%', textAlign: 'right' }}>Thành tiền</th>
                    <th style={{ width: '4%' }}></th>
                  </tr>
                </thead>
                <tbody>
                  {lineItems.map((item) => {
                    const rowSubtotal = item.soLuongNhap * item.donGiaNhap;
                    return (
                      <tr key={item.id}>
                        <td>
                          {item.isNewProduct ? (
                            <div>
                              <input
                                type="text"
                                className="form-input"
                                style={{ height: '32px', marginBottom: '4px' }}
                                placeholder="Tên món hàng mới..."
                                value={item.tenSP}
                                onChange={(e) => handleUpdateLine(item.id, 'tenSP', e.target.value)}
                              />
                              <input
                                type="text"
                                className="form-input"
                                style={{ height: '28px', fontSize: '11px' }}
                                placeholder="Mã SP (tự sinh nếu trống)"
                                value={item.maSP}
                                onChange={(e) => handleUpdateLine(item.id, 'maSP', e.target.value)}
                              />
                            </div>
                          ) : (
                            <select
                              className="form-select"
                              style={{ height: '36px' }}
                              value={item.sanPhamId}
                              onChange={(e) => handleProductSelect(item.id, Number(e.target.value))}
                            >
                              {products.map((p) => (
                                <option key={p.id} value={p.id}>
                                  [{p.maSP}] {p.tenSP} (Tồn hiện tại: {p.soLuongTon})
                                </option>
                              ))}
                            </select>
                          )}
                        </td>
                        <td>
                          <span style={{ fontSize: '12px', color: '#8C827A' }}>{item.donViTinh}</span>
                        </td>
                        <td style={{ textAlign: 'right' }}>
                          <input
                            type="number"
                            className="input-num"
                            min={0}
                            value={item.donGiaNhap}
                            onChange={(e) => handleUpdateLine(item.id, 'donGiaNhap', Number(e.target.value))}
                          />
                        </td>
                        <td style={{ textAlign: 'center' }}>
                          <input
                            type="number"
                            className="input-num"
                            min={1}
                            style={{ width: '64px', textAlign: 'center' }}
                            value={item.soLuongNhap}
                            onChange={(e) => handleUpdateLine(item.id, 'soLuongNhap', Number(e.target.value))}
                          />
                        </td>
                        <td style={{ textAlign: 'right', fontWeight: 600 }}>
                          {rowSubtotal.toLocaleString('vi-VN')} đ
                        </td>
                        <td>
                          <button
                            type="button"
                            className="action-btn-sm delete"
                            style={{ width: '28px', height: '28px' }}
                            onClick={() => handleRemoveLineItem(item.id)}
                            title="Xóa dòng"
                          >
                            <IconTrash />
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>

          {/* Right Summary & Confirm (Bước 44, 45 UC12) */}
          <div>
            <div className="receipt-card">
              <h2 className="receipt-card-title">Tổng quan phiếu nhập</h2>

              <div className="summary-line">
                <span>Nhà cung cấp:</span>
                <strong style={{ color: '#2B2523' }}>
                  {suppliers.find((s) => s.id === selectedSupplierId)?.tenNCC || 'Chưa chọn'}
                </strong>
              </div>

              <div className="summary-line">
                <span>Người lập phiếu:</span>
                <span>{user?.hoTen || 'Quản lý'}</span>
              </div>

              <div className="summary-line">
                <span>Tổng số lượng mặt hàng:</span>
                <span>{lineItems.length} dòng</span>
              </div>

              <div className="summary-line">
                <span>Tổng số sản phẩm nhập:</span>
                <strong style={{ color: '#2B2523' }}>{totalQuantity} cái/món</strong>
              </div>

              <div className="summary-line total">
                <span>Tổng tiền nhập kho:</span>
                <span className="summary-val-highlight">
                  {totalAmount.toLocaleString('vi-VN')} đ
                </span>
              </div>

              <button
                type="button"
                className="btn-confirm-receipt"
                onClick={handleSubmitReceipt}
                disabled={submitting || lineItems.length === 0}
              >
                <IconCheck />
                {submitting ? 'Đang lưu phiếu...' : 'Xác nhận nhập kho (UC12)'}
              </button>

              <div style={{ marginTop: '14px', fontSize: '11px', color: '#8C827A', textAlign: 'center' }}>
                * Sau khi xác nhận, phiếu nhập được khóa và số lượng tồn kho sẽ được cộng tăng tự động.
              </div>
            </div>
          </div>
        </div>

        {/* Lịch sử phiếu nhập gần đây */}
        <div className="history-card">
          <h3 className="receipt-card-title">Lịch sử phiếu nhập kho đã thực hiện</h3>
          {receiptHistory.length === 0 ? (
            <p style={{ fontSize: '13px', color: '#8C827A', margin: 0 }}>Chưa có phiếu nhập kho nào được lập.</p>
          ) : (
            <table className="product-table">
              <thead>
                <tr>
                  <th>Mã phiếu</th>
                  <th>Nhà cung cấp</th>
                  <th>Người nhập</th>
                  <th>Thời gian nhập</th>
                  <th style={{ textAlign: 'right' }}>Tổng tiền nhập</th>
                  <th style={{ textAlign: 'center' }}>Chi tiết</th>
                </tr>
              </thead>
              <tbody>
                {receiptHistory.map((rc) => (
                  <tr key={rc.id}>
                    <td>
                      <span className="receipt-badge">{rc.maPhieu}</span>
                    </td>
                    <td><strong>{rc.tenNCC}</strong></td>
                    <td>{rc.tenNguoiNhap}</td>
                    <td>{new Date(rc.ngayNhap).toLocaleString('vi-VN')}</td>
                    <td style={{ textAlign: 'right', fontWeight: 600 }}>
                      {rc.tongTienNhap.toLocaleString('vi-VN')} đ
                    </td>
                    <td style={{ textAlign: 'center' }}>
                      <span style={{ fontSize: '12px', color: '#8C827A' }}>
                        {rc.chiTiet?.length || 0} sản phẩm
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </main>

      {/* Toast Notice */}
      {toastMessage && (
        <div className={`toast-notice ${toastType}`}>
          <span>{toastType === 'success' ? '✓' : '⚠️'}</span>
          <span>{toastMessage}</span>
        </div>
      )}
    </div>
  );
};

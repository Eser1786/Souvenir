import React, { useState, useEffect } from 'react';
import { isAxiosError } from 'axios';
import { shiftApi, type ShiftInfo } from '../../api/shiftApi';

interface Props {
  shift: ShiftInfo;
  isOpen: boolean;
  onClose: () => void;
  onShiftClosed: () => void;
}

export const CloseShiftModal: React.FC<Props> = ({ shift, isOpen, onClose, onShiftClosed }) => {
  const [actualCash, setActualCash] = useState<string>('');
  const [reason, setReason] = useState<string>('');
  const [currentShift, setCurrentShift] = useState<ShiftInfo>(shift);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setCurrentShift(shift);
  }, [shift]);

  if (!isOpen) return null;

  const expectedCash = currentShift.tienMatBanGiao + currentShift.tongDoanhThuTienMat;

  const handleCloseShift = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setLoading(true);
      setError(null);
      const updated = await shiftApi.closeShift(currentShift.id, Number(actualCash));
      setCurrentShift(updated);
      if (updated.trangThai === 'DA_CHOT') {
        onShiftClosed();
      }
    } catch (err) {
      setError(isAxiosError(err) ? err.response?.data?.message || 'Lỗi khi chốt ca.' : 'Lỗi hệ thống.');
    } finally {
      setLoading(false);
    }
  };

  const handleExplain = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setLoading(true);
      setError(null);
      await shiftApi.explainShift(currentShift.id, reason);
      onShiftClosed();
    } catch (err) {
      setError(isAxiosError(err) ? err.response?.data?.message || 'Lỗi khi gửi giải trình.' : 'Lỗi hệ thống.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        zIndex: 10000,
        backgroundColor: 'rgba(0, 0, 0, 0.6)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      <div
        style={{
          backgroundColor: '#fff',
          borderRadius: '12px',
          padding: '24px',
          width: '100%',
          maxWidth: '460px',
          boxShadow: '0 20px 25px rgba(0,0,0,0.2)',
        }}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ fontSize: '18px', fontWeight: 700, margin: 0 }}>
            Chốt ca & Bàn giao (#CA-{currentShift.id})
          </h2>
          <button
            type="button"
            onClick={onClose}
            style={{ background: 'none', border: 'none', fontSize: '18px', cursor: 'pointer' }}
          >
            ✕
          </button>
        </div>

        <div style={{ marginTop: '14px', backgroundColor: '#f9fafb', padding: '14px', borderRadius: '8px', fontSize: '14px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '6px' }}>
            <span>Tiền mặt đầu ca:</span>
            <strong>{currentShift.tienMatBanGiao.toLocaleString()} đ</strong>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '6px' }}>
            <span>Doanh thu tiền mặt trong ca:</span>
            <strong style={{ color: '#059669' }}>+{currentShift.tongDoanhThuTienMat.toLocaleString()} đ</strong>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', borderTop: '1px solid #e5e7eb', paddingTop: '8px' }}>
            <span>Tiền mặt lý thuyết trong két:</span>
            <strong style={{ color: '#2563eb', fontSize: '16px' }}>{expectedCash.toLocaleString()} đ</strong>
          </div>
        </div>

        {currentShift.trangThai !== 'CHO_XU_LY_CHENH_LECH' ? (
          <form onSubmit={handleCloseShift} style={{ marginTop: '16px' }}>
            <label style={{ display: 'block', fontSize: '14px', fontWeight: 500 }}>
              Tiền mặt thực tế kiểm đếm (VNĐ)
            </label>
            <input
              type="number"
              min={0}
              value={actualCash}
              onChange={(e) => setActualCash(e.target.value)}
              style={{
                marginTop: '6px',
                width: '100%',
                boxSizing: 'border-box',
                padding: '10px',
                borderRadius: '8px',
                border: '1px solid #d1d5db',
                fontSize: '16px',
                fontWeight: 600,
              }}
              required
            />
            {error && <p style={{ color: '#dc2626', fontSize: '13px', marginTop: '6px' }}>{error}</p>}
            <button
              type="submit"
              disabled={loading}
              style={{
                marginTop: '14px',
                width: '100%',
                padding: '10px',
                borderRadius: '8px',
                border: 'none',
                backgroundColor: '#d97706',
                color: '#fff',
                fontWeight: 600,
                cursor: 'pointer',
              }}
            >
              {loading ? 'Đang xử lý...' : 'Kiểm đếm & Chốt ca'}
            </button>
          </form>
        ) : (
          <form onSubmit={handleExplain} style={{ marginTop: '16px' }}>
            <div style={{ backgroundColor: '#fef2f2', color: '#b91c1c', padding: '10px', borderRadius: '8px', fontSize: '13px' }}>
              Phát hiện chênh lệch: <strong>{currentShift.chenhLech.toLocaleString()} đ</strong>. Vui lòng nhập lý do giải trình để chốt ca.
            </div>
            <label style={{ display: 'block', fontSize: '14px', fontWeight: 500, marginTop: '10px' }}>
              Lý do chênh lệch
            </label>
            <textarea
              rows={3}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="VD: Thối nhầm tiền lẻ cho khách..."
              style={{
                marginTop: '6px',
                width: '100%',
                boxSizing: 'border-box',
                padding: '8px',
                borderRadius: '8px',
                border: '1px solid #d1d5db',
              }}
              required
            />
            {error && <p style={{ color: '#dc2626', fontSize: '13px', marginTop: '6px' }}>{error}</p>}
            <button
              type="submit"
              disabled={loading}
              style={{
                marginTop: '12px',
                width: '100%',
                padding: '10px',
                borderRadius: '8px',
                border: 'none',
                backgroundColor: '#dc2626',
                color: '#fff',
                fontWeight: 600,
                cursor: 'pointer',
              }}
            >
              {loading ? 'Đang gửi...' : 'Xác nhận giải trình & Hoàn tất chốt ca'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
};
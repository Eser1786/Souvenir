import React, { useState } from 'react';
import { isAxiosError } from 'axios';
import { shiftApi, type ShiftInfo } from '../../api/shiftApi';

interface Props {
  isOpen: boolean;
  onShiftOpened: (shift: ShiftInfo) => void;
}

export const OpenShiftModal: React.FC<Props> = ({ isOpen, onShiftOpened }) => {
  const [initialCash, setInitialCash] = useState<string>('500000');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const amount = Number(initialCash);
    if (isNaN(amount) || amount < 0) {
      setError('Số tiền mặt đầu ca không hợp lệ.');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const shift = await shiftApi.openShift(amount);
      onShiftOpened(shift);
    } catch (err) {
      if (isAxiosError(err)) {
        setError(err.response?.data?.message || 'Không thể mở ca làm việc.');
      } else {
        setError('Đã xảy ra lỗi không xác định.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        zIndex: 10000,
        backgroundColor: 'rgba(0, 0, 0, 0.6)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      <div
        style={{
          backgroundColor: '#ffffff',
          borderRadius: '12px',
          padding: '24px',
          width: '100%',
          maxWidth: '420px',
          boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.2)',
        }}
      >
        <h2 style={{ fontSize: '20px', fontWeight: 700, color: '#1f2937', margin: 0 }}>
          Mở ca làm việc mới
        </h2>
        <p style={{ marginTop: '6px', fontSize: '14px', color: '#6b7280' }}>
          Vui lòng kiểm đếm và nhập số tiền mặt bàn giao trong két trước khi bán hàng. Hệ thống sẽ tự động chấm công vào ca.
        </p>

        <form onSubmit={handleSubmit} style={{ marginTop: '20px' }}>
          <div>
            <label style={{ display: 'block', fontSize: '14px', fontWeight: 500, color: '#374151' }}>
              Tiền mặt đầu ca (VNĐ)
            </label>
            <input
              type="number"
              min={0}
              step={1000}
              value={initialCash}
              onChange={(e) => setInitialCash(e.target.value)}
              style={{
                marginTop: '6px',
                width: '100%',
                boxSizing: 'border-box',
                borderRadius: '8px',
                border: '1px solid #d1d5db',
                padding: '10px 12px',
                fontSize: '18px',
                fontWeight: 600,
              }}
              required
            />
          </div>

          {error && (
            <p style={{ marginTop: '8px', fontSize: '13px', color: '#dc2626' }}>{error}</p>
          )}

          <button
            type="submit"
            disabled={loading}
            style={{
              marginTop: '16px',
              width: '100%',
              borderRadius: '8px',
              backgroundColor: '#2563eb',
              color: '#ffffff',
              padding: '10px',
              fontSize: '15px',
              fontWeight: 600,
              border: 'none',
              cursor: loading ? 'not-allowed' : 'pointer',
              opacity: loading ? 0.6 : 1,
            }}
          >
            {loading ? 'Đang mở ca...' : 'Xác nhận Mở ca & Check-in'}
          </button>
        </form>
      </div>
    </div>
  );
};
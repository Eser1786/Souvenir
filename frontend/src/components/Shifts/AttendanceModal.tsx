import React, { useState } from 'react';
import { isAxiosError } from 'axios';
import { shiftApi, type ShiftInfo } from '../../api/shiftApi';

interface Props {
  isOpen: boolean;
  shift: ShiftInfo | null;
  onClose: () => void;
  onUpdated: () => void;
}

export const AttendanceModal: React.FC<Props> = ({ isOpen, shift, onClose, onUpdated }) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen) return null;

  const handleCheckIn = async () => {
    try {
      setLoading(true);
      setError(null);
      await shiftApi.checkIn();
      onUpdated();
    } catch (err) {
      setError(isAxiosError(err) ? err.response?.data?.message || 'Lỗi check-in.' : 'Lỗi hệ thống.');
    } finally {
      setLoading(false);
    }
  };

  const handleCheckOut = async () => {
    try {
      setLoading(true);
      setError(null);
      await shiftApi.checkOut();
      onUpdated();
    } catch (err) {
      setError(isAxiosError(err) ? err.response?.data?.message || 'Lỗi check-out.' : 'Lỗi hệ thống.');
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
          maxWidth: '400px',
          boxShadow: '0 20px 25px rgba(0,0,0,0.2)',
        }}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ fontSize: '18px', fontWeight: 700, margin: 0 }}>Chấm công ca làm việc</h2>
          <button
            type="button"
            onClick={onClose}
            style={{ background: 'none', border: 'none', fontSize: '18px', cursor: 'pointer' }}
          >
            ✕
          </button>
        </div>

        {!shift ? (
          <p style={{ marginTop: '14px', fontSize: '14px', color: '#6b7280' }}>
            Bạn chưa mở ca làm việc nào. Vui lòng mở ca trước để ghi nhận chấm công.
          </p>
        ) : (
          <div style={{ marginTop: '16px', fontSize: '14px' }}>
            <div style={{ backgroundColor: '#f3f4f6', padding: '12px', borderRadius: '8px', lineHeight: '1.8' }}>
              <div><strong>Ca làm việc:</strong> #CA-{shift.id}</div>
              <div>
                <strong>Giờ vào ca (Check-in):</strong>{' '}
                {shift.chamCong
                  ? new Date(shift.chamCong.gioCheckIn).toLocaleString('vi-VN')
                  : 'Chưa ghi nhận'}
              </div>
              <div>
                <strong>Giờ tan ca (Check-out):</strong>{' '}
                {shift.chamCong?.gioCheckOut
                  ? new Date(shift.chamCong.gioCheckOut).toLocaleString('vi-VN')
                  : 'Đang trong ca'}
              </div>
            </div>

            {error && <p style={{ color: '#dc2626', fontSize: '13px', marginTop: '8px' }}>{error}</p>}

            <div style={{ display: 'flex', gap: '10px', marginTop: '16px' }}>
              {!shift.chamCong && (
                <button
                  type="button"
                  disabled={loading}
                  onClick={handleCheckIn}
                  style={{
                    flex: 1,
                    padding: '10px',
                    borderRadius: '8px',
                    border: 'none',
                    backgroundColor: '#2563eb',
                    color: '#fff',
                    fontWeight: 600,
                    cursor: 'pointer',
                  }}
                >
                  Check-in Vào ca
                </button>
              )}

              {shift.chamCong && !shift.chamCong.gioCheckOut && (
                <button
                  type="button"
                  disabled={loading}
                  onClick={handleCheckOut}
                  style={{
                    flex: 1,
                    padding: '10px',
                    borderRadius: '8px',
                    border: 'none',
                    backgroundColor: '#059669',
                    color: '#fff',
                    fontWeight: 600,
                    cursor: 'pointer',
                  }}
                >
                  {loading ? 'Đang xử lý...' : 'Check-out Tan ca'}
                </button>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
import React, { useEffect, useState } from 'react';
import { shiftApi, type ShiftInfo } from '../../api/shiftApi';
import { OpenShiftModal } from './OpenShiftModal';
import { CloseShiftModal } from './CloseShiftModal';

export const ShiftSidebarWidget: React.FC<{ onShiftChange?: (shift: ShiftInfo | null) => void }> = ({
  onShiftChange,
}) => {
  const [shift, setShift] = useState<ShiftInfo | null>(null);
  const [loading, setLoading] = useState(true);
  const [apiError, setApiError] = useState<string | null>(null);
  const [showCloseModal, setShowCloseModal] = useState(false);

  const loadShift = async () => {
    try {
      setLoading(true);
      setApiError(null);
      const data = await shiftApi.getCurrentShift();
      setShift(data);
      onShiftChange?.(data);
    } catch (err: any) {
      console.error('Lỗi tải ca làm việc:', err);
      setApiError(err?.response?.data?.message || err?.message || 'Lỗi kết nối API Ca làm việc');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadShift();
  }, []);

  const handleQuickAttendance = async () => {
    if (!shift) return;
    if (!shift.chamCong?.gioCheckOut) {
      await shiftApi.checkOut();
    } else {
      await shiftApi.checkIn();
    }
    await loadShift();
  };

  // Khung cố định góc dưới trái bằng inline style để luôn hiển thị dù có Tailwind hay không
  const containerStyle: React.CSSProperties = {
    position: 'fixed',
    bottom: '16px',
    left: '16px',
    zIndex: 9999,
    backgroundColor: '#ffffff',
    border: '1px solid #bfdbfe',
    borderRadius: '10px',
    padding: '12px',
    boxShadow: '0 10px 25px rgba(0,0,0,0.15)',
    minWidth: '240px',
    fontSize: '13px',
  };

  if (loading) {
    return <div style={containerStyle}>Đang kiểm tra ca làm việc...</div>;
  }

  if (apiError) {
    return (
      <div style={{ ...containerStyle, borderColor: '#fca5a5', backgroundColor: '#fef2f2', color: '#b91c1c' }}>
        <strong>Lỗi Ca/Chấm công:</strong> {apiError}
        <button
          onClick={loadShift}
          style={{ display: 'block', marginTop: '6px', textDecoration: 'underline', cursor: 'pointer' }}
        >
          Thử lại
        </button>
      </div>
    );
  }

  return (
    <>
      <OpenShiftModal
        isOpen={!shift}
        onShiftOpened={(newShift) => {
          setShift(newShift);
          onShiftChange?.(newShift);
        }}
      />

      {shift && (
        <div style={containerStyle}>
          <div style={{ fontWeight: 700, color: '#1e3a8a', marginBottom: '4px' }}>
            Ca #{shift.id} ({shift.trangThai === 'DANG_MO' ? 'Đang mở' : 'Chờ giải trình'})
          </div>
          <div style={{ color: '#4b5563' }}>
            Check-in: {shift.chamCong ? new Date(shift.chamCong.gioCheckIn).toLocaleTimeString('vi-VN') : '--:--'}
          </div>
          {shift.chamCong?.gioCheckOut && (
            <div style={{ color: '#4b5563' }}>
              Check-out: {new Date(shift.chamCong.gioCheckOut).toLocaleTimeString('vi-VN')}
            </div>
          )}

          <div style={{ display: 'flex', gap: '8px', marginTop: '10px' }}>
            {!shift.chamCong?.gioCheckOut && (
              <button
                onClick={handleQuickAttendance}
                style={{
                  flex: 1,
                  backgroundColor: '#059669',
                  color: '#fff',
                  border: 'none',
                  borderRadius: '6px',
                  padding: '6px 8px',
                  cursor: 'pointer',
                  fontWeight: 600,
                }}
              >
                Tan ca (Out)
              </button>
            )}
            <button
              onClick={() => setShowCloseModal(true)}
              style={{
                flex: 1,
                backgroundColor: '#d97706',
                color: '#fff',
                border: 'none',
                borderRadius: '6px',
                padding: '6px 8px',
                cursor: 'pointer',
                fontWeight: 600,
              }}
            >
              Chốt ca
            </button>
          </div>

          <CloseShiftModal
            shift={shift}
            isOpen={showCloseModal}
            onClose={() => setShowCloseModal(false)}
            onShiftClosed={() => {
              setShowCloseModal(false);
              loadShift();
            }}
          />
        </div>
      )}
    </>
  );
};
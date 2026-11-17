import React, { useEffect, useState } from 'react';
import { shiftApi, type ShiftInfo } from '../../api/shiftApi';
import { OpenShiftModal } from './OpenShiftModal';
import { CloseShiftModal } from './CloseShiftModal';

export const ShiftSidebarWidget: React.FC<{ onShiftChange?: (shift: ShiftInfo | null) => void }> = ({
  onShiftChange,
}) => {
  const [shift, setShift] = useState<ShiftInfo | null>(null);
  const [loading, setLoading] = useState(true);
  const [showCloseModal, setShowCloseModal] = useState(false);

  const loadShift = async () => {
    try {
      setLoading(true);
      const data = await shiftApi.getCurrentShift();
      setShift(data);
      onShiftChange?.(data);
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

  if (loading) return <div className="p-3 text-xs text-gray-400">Đang tải thông tin ca...</div>;

  return (
    <>
      {/* Bắt buộc mở ca nếu chưa có ca hoạt động */}
      <OpenShiftModal
        isOpen={!shift}
        onShiftOpened={(newShift) => {
          setShift(newShift);
          onShiftChange?.(newShift);
        }}
      />

      {shift && (
        <div className="m-2 rounded-lg border border-blue-100 bg-blue-50/70 p-3 text-xs">
          <div className="flex items-center justify-between font-semibold text-blue-900">
            <span>Ca #{shift.id} ({shift.trangThai === 'DANG_MO' ? 'Đang mở' : 'Chờ giải trình'})</span>
          </div>

          <div className="mt-1 text-gray-600">
            Check-in: {shift.chamCong ? new Date(shift.chamCong.gioCheckIn).toLocaleTimeString('vi-VN') : '--:--'}
          </div>
          {shift.chamCong?.gioCheckOut && (
            <div className="text-gray-600">
              Check-out: {new Date(shift.chamCong.gioCheckOut).toLocaleTimeString('vi-VN')}
            </div>
          )}

          <div className="mt-2.5 flex gap-1.5">
            {!shift.chamCong?.gioCheckOut && (
              <button
                onClick={handleQuickAttendance}
                className="flex-1 rounded bg-emerald-600 px-2 py-1.5 font-medium text-white hover:bg-emerald-700"
              >
                Tan ca (Out)
              </button>
            )}
            <button
              onClick={() => setShowCloseModal(true)}
              className="flex-1 rounded bg-amber-600 px-2 py-1.5 font-medium text-white hover:bg-amber-700"
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
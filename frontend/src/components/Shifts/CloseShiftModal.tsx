import React, { useState } from 'react';
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
    } catch (err: any) {
      setError(err?.response?.data?.message || 'Lỗi khi chốt ca.');
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
    } catch (err: any) {
      setError(err?.response?.data?.message || 'Lỗi khi gửi giải trình.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
      <div className="w-full max-w-lg rounded-xl bg-white p-6 shadow-2xl">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-bold text-gray-800">Chốt ca & Bàn giao (#CA-{currentShift.id})</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">✕</button>
        </div>

        <div className="mt-4 rounded-lg bg-gray-50 p-4 text-sm space-y-2">
          <div className="flex justify-between">
            <span className="text-gray-600">Tiền mặt đầu ca:</span>
            <span className="font-semibold">{currentShift.tienMatBanGiao.toLocaleString()} đ</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-600">Doanh thu tiền mặt trong ca:</span>
            <span className="font-semibold text-green-600">+{currentShift.tongDoanhThuTienMat.toLocaleString()} đ</span>
          </div>
          <div className="flex justify-between border-t pt-2 text-base">
            <span className="font-medium text-gray-800">Tiền mặt lý thuyết trong két:</span>
            <span className="font-bold text-blue-600">{expectedCash.toLocaleString()} đ</span>
          </div>
        </div>

        {currentShift.trangThai !== 'CHO_XU_LY_CHENH_LECH' ? (
          <form onSubmit={handleCloseShift} className="mt-4 space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700">
                Tiền mặt thực tế kiểm đếm (VNĐ)
              </label>
              <input
                type="number"
                min={0}
                value={actualCash}
                onChange={(e) => setActualCash(e.target.value)}
                className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2 text-lg font-semibold"
                required
              />
            </div>
            {error && <p className="text-sm text-red-600">{error}</p>}
            <button
              type="submit"
              disabled={loading}
              className="w-full rounded-lg bg-amber-600 py-2.5 font-semibold text-white hover:bg-amber-700"
            >
              {loading ? 'Đang xử lý...' : 'Kiểm đếm & Chốt ca'}
            </button>
          </form>
        ) : (
          <form onSubmit={handleExplain} className="mt-4 space-y-4">
            <div className="rounded-lg bg-red-50 p-3 text-sm text-red-700">
              Phát hiện chênh lệch: <strong>{currentShift.chenhLech.toLocaleString()} đ</strong>. Vui lòng nhập lý do giải trình để hoàn tất chốt ca.
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700">Lý do chênh lệch</label>
              <textarea
                rows={3}
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                placeholder="VD: Thối nhầm tiền lẻ cho khách đơn #12..."
                className="mt-1 w-full rounded-lg border border-gray-300 p-2.5 text-sm"
                required
              />
            </div>
            {error && <p className="text-sm text-red-600">{error}</p>}
            <button
              type="submit"
              disabled={loading}
              className="w-full rounded-lg bg-red-600 py-2.5 font-semibold text-white hover:bg-red-700"
            >
              {loading ? 'Đang gửi...' : 'Xác nhận giải trình & Hoàn tất chốt ca'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
};
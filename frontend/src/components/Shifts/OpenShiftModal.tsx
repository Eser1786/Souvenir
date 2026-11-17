import React, { useState } from 'react';
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
    } catch (err: any) {
      setError(err?.response?.data?.message || 'Không thể mở ca làm việc.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
      <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-2xl">
        <h2 className="text-xl font-bold text-gray-800">Mở ca làm việc mới</h2>
        <p className="mt-1 text-sm text-gray-500">
          Vui lòng kiểm đếm và nhập số tiền mặt bàn giao trong két trước khi bán hàng. Hệ thống sẽ tự động chấm công vào ca.
        </p>

        <form onSubmit={handleSubmit} className="mt-5 space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">
              Tiền mặt đầu ca (VNĐ)
            </label>
            <input
              type="number"
              min={0}
              step={1000}
              value={initialCash}
              onChange={(e) => setInitialCash(e.target.value)}
              className="mt-1 w-full rounded-lg border border-gray-300 px-3 py-2.5 text-lg font-semibold focus:border-blue-600 focus:outline-none"
              required
            />
          </div>

          {error && <p className="text-sm text-red-600">{error}</p>}

          <button
            type="submit"
            disabled={loading}
            className="w-full rounded-lg bg-blue-600 py-2.5 font-semibold text-white hover:bg-blue-700 disabled:opacity-50"
          >
            {loading ? 'Đang mở ca...' : 'Xác nhận Mở ca & Check-in'}
          </button>
        </form>
      </div>
    </div>
  );
};
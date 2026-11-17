/**
 * Cấu hình VietQR (https://www.vietqr.io/danh-sach-api/link-tao-ma-nhanh).
 * Đổi tài khoản nhận tiền qua biến môi trường Vite trong frontend/.env.local:
 *   VITE_VIETQR_BANK_ID=MB            (mã BIN hoặc tên viết tắt ngân hàng)
 *   VITE_VIETQR_ACCOUNT_NO=0123456789
 *   VITE_VIETQR_ACCOUNT_NAME=MERA SOUVENIR
 *   VITE_VIETQR_TEMPLATE=compact2
 * Giá trị mặc định bên dưới chỉ để demo giao diện.
 */
export const VIETQR_CONFIG = {
  bankId: import.meta.env.VITE_VIETQR_BANK_ID ?? 'MB',
  accountNo: import.meta.env.VITE_VIETQR_ACCOUNT_NO ?? '0123456789',
  accountName: import.meta.env.VITE_VIETQR_ACCOUNT_NAME ?? 'MERA SOUVENIR',
  template: import.meta.env.VITE_VIETQR_TEMPLATE ?? 'compact2',
};

export const buildVietQrUrl = (amount: number, addInfo: string): string => {
  const { bankId, accountNo, accountName, template } = VIETQR_CONFIG;
  const params = new URLSearchParams({
    amount: String(Math.round(amount)),
    addInfo,
    accountName,
  });
  return `https://img.vietqr.io/image/${encodeURIComponent(bankId)}-${encodeURIComponent(accountNo)}-${encodeURIComponent(template)}.png?${params.toString()}`;
};

/** Gợi ý mệnh giá tiền khách đưa: đúng số tiền + làm tròn lên các mệnh giá phổ biến. */
export const suggestCashAmounts = (total: number): number[] => {
  if (total <= 0) return [];
  const steps = [10000, 50000, 100000, 200000, 500000];
  const set = new Set<number>([total]);
  for (const step of steps) {
    set.add(Math.ceil(total / step) * step);
  }
  return Array.from(set).sort((a, b) => a - b).slice(0, 5);
};

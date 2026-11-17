import {apiClient} from './client'; // điều chỉnh theo tên file axios có sẵn của nhóm

export interface AttendanceInfo {
  id: number;
  caLamViecId: number;
  gioCheckIn: string;
  gioCheckOut: string | null;
}

export interface ShiftInfo {
  id: number;
  nguoiDungId: number;
  thoiGianMoCa: string;
  thoiGianChotCa: string | null;
  tienMatBanGiao: number;
  tienMatThucTe: number | null;
  tongDoanhThuTienMat: number;
  chenhLech: number;
  trangThai: 'DANG_MO' | 'CHO_XU_LY_CHENH_LECH' | 'DA_CHOT';
  lyDoChenhLech: string | null;
  chamCong: AttendanceInfo | null;
}

export const shiftApi = {
  getCurrentShift: async (): Promise<ShiftInfo | null> => {
    const res = await apiClient.get('/shifts/current');
    return res.data.data;
  },
  openShift: async (tienMatBanGiao: number): Promise<ShiftInfo> => {
    const res = await apiClient.post('/shifts/open', { tienMatBanGiao });
    return res.data.data;
  },
  closeShift: async (shiftId: number, tienMatThucTe: number): Promise<ShiftInfo> => {
    const res = await apiClient.post(`/shifts/${shiftId}/close`, { tienMatThucTe });
    return res.data.data;
  },
  explainShift: async (shiftId: number, lyDoChenhLech: string): Promise<ShiftInfo> => {
    const res = await apiClient.post(`/shifts/${shiftId}/explain`, { lyDoChenhLech });
    return res.data.data;
  },
  checkIn: async (): Promise<AttendanceInfo> => {
    const res = await apiClient.post('/attendance/check-in');
    return res.data.data;
  },
  checkOut: async (): Promise<AttendanceInfo> => {
    const res = await apiClient.post('/attendance/check-out');
    return res.data.data;
  },
};
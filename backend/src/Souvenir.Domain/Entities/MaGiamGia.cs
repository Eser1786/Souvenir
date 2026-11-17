namespace Souvenir.Domain.Entities;

public class MaGiamGia
{
    public int Id { get; set; }
    public string MaCode { get; set; } = string.Empty;
    public string TenVoucher { get; set; } = string.Empty;
    public string LoaiGiamGia { get; set; } = "PhanTram"; // "PhanTram" hoặc "SoTien"
    public decimal GiaTriGiam { get; set; }
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }

    public bool KiemTraHieuLuc(DateTime ngayHienTai)
    {
        return ngayHienTai >= NgayBatDau && ngayHienTai <= NgayKetThuc;
    }

    public decimal TinhTienGiam(decimal tongTienHang)
    {
        if (tongTienHang <= 0 || GiaTriGiam <= 0) return 0;

        if (string.Equals(LoaiGiamGia, "PhanTram", StringComparison.OrdinalIgnoreCase))
        {
            var tienGiam = Math.Round(tongTienHang * (GiaTriGiam / 100m));
            return Math.Min(tienGiam, tongTienHang);
        }

        return Math.Min(GiaTriGiam, tongTienHang);
    }
}

namespace Souvenir.Domain.Entities;

public class ChamCong
{
    public int Id { get; set; }
    public int NguoiDungId { get; set; }
    public int CaLamViecId { get; set; }
    public DateTime GioCheckIn { get; set; } = DateTime.UtcNow;
    public DateTime? GioCheckOut { get; set; }

    public CaLamViec CaLamViec { get; set; } = null!;
}
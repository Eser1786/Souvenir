namespace Souvenir.Domain.Entities;

public class VaiTro
{
    public int Id { get; set; }
    public string TenVaiTro { get; set; } = string.Empty;
    public string? MoTa { get; set; }

    public ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
}

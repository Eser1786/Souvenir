namespace Souvenir.Domain.Entities;

public class DonViTinh
{
    public int Id { get; set; }
    public string TenDonVi { get; set; } = string.Empty;

    public ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}

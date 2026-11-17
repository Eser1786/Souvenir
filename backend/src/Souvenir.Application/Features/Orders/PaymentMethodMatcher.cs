using System.Globalization;
using System.Text;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Orders;

/// <summary>Ánh xạ "TienMat"/"ChuyenKhoan" sang bản ghi hinh_thuc_thanh_toan (tên có/không dấu đều khớp).</summary>
public static class PaymentMethodMatcher
{
    public const string TienMat = "TienMat";
    public const string ChuyenKhoan = "ChuyenKhoan";

    public static string Normalize(string value)
    {
        var decomposed = value.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static bool IsCash(HinhThucThanhToan method) => Normalize(method.TenHinhThuc).Contains("tien mat");

    public static HinhThucThanhToan? Find(IEnumerable<HinhThucThanhToan> methods, string phuongThuc)
    {
        var key = Normalize(phuongThuc).Replace(" ", string.Empty);
        string? keyword = key switch
        {
            "tienmat" or "cash" => "tien mat",
            "chuyenkhoan" or "chuyenkhoanqr" or "qr" or "vietqr" => "chuyen khoan",
            _ => null
        };

        if (keyword == null)
        {
            return methods.FirstOrDefault(m => Normalize(m.TenHinhThuc).Replace(" ", string.Empty) == key);
        }

        return methods.FirstOrDefault(m => Normalize(m.TenHinhThuc).Contains(keyword));
    }
}

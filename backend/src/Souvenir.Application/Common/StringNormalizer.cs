using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Souvenir.Application.Common;

public static class StringNormalizer
{
    /// <summary>
    /// Loại bỏ dấu tiếng Việt, chuyển về chữ thường không dấu và chuẩn hóa khoảng trắng
    /// Ví dụ: "Đồ lưu niệm " -> "do luu niem"
    /// </summary>
    public static string NormalizeKey(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var normalizedString = input.Trim().Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        // Thay thế các ký tự đặc thù tiếng Việt như đ/Đ
        result = Regex.Replace(result, "[đĐ]", "d", RegexOptions.IgnoreCase);
        // Thu gọn nhiều khoảng trắng liên tiếp
        result = Regex.Replace(result, @"\s+", " ").Trim().ToLowerInvariant();

        return result;
    }

    /// <summary>
    /// So sánh 2 chuỗi có cùng nghĩa tiếng Việt không (bỏ qua có dấu/không dấu và hoa thường)
    /// </summary>
    public static bool EqualsNormalized(string? a, string? b)
    {
        return string.Equals(NormalizeKey(a), NormalizeKey(b), StringComparison.Ordinal);
    }
}

using System.Text.RegularExpressions;

namespace Souvenir.Application.Common;

public static class ProductCode
{
    private static readonly Regex FormatRegex = new(
        @"\ASP[0-9]{3,}\z",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public const string FormatErrorMessage =
        "Mã sản phẩm phải có định dạng SP theo sau bởi ít nhất 3 chữ số (ví dụ: SP011).";

    public static string Normalize(string? code) => (code ?? string.Empty).Trim().ToUpperInvariant();

    public static bool HasValidFormat(string code) => FormatRegex.IsMatch(code);
}

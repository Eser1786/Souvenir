using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, int ExpiresIn) GenerateToken(NguoiDung user)
    {
        var secretKey = _configuration["Jwt:Key"]
            ?? "Souvenir_Super_Secret_Key_For_JWT_Authentication_2026_AtLeast32BytesLong!";
        var issuer = _configuration["Jwt:Issuer"] ?? "Souvenir.Api";
        var audience = _configuration["Jwt:Audience"] ?? "Souvenir.Client";
        var expireMinutesStr = _configuration["Jwt:ExpireMinutes"];

        if (!int.TryParse(expireMinutesStr, out var expireMinutes) || expireMinutes <= 0)
        {
            expireMinutes = 480; // 8 hours default
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roleName = user.VaiTro?.TenVaiTro ?? "NhanVien";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.TenDangNhap),
            new(ClaimTypes.Role, roleName),
            new("role", roleName),
            new("fullName", user.HoTen),
            new("phoneNumber", user.SoDienThoai?.Trim() ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expires = DateTime.UtcNow.AddMinutes(expireMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(securityToken);

        int expiresInSeconds = expireMinutes * 60;
        return (tokenString, expiresInSeconds);
    }
}

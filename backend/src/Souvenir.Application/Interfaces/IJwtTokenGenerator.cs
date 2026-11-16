using Souvenir.Domain.Entities;

namespace Souvenir.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, int ExpiresIn) GenerateToken(NguoiDung user);
}

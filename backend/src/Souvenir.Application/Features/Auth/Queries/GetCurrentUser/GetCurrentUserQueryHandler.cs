using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Auth;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCurrentUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.NguoiDungs
            .Include(u => u.VaiTro)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            TenDangNhap = user.TenDangNhap,
            HoTen = user.HoTen,
            SoDienThoai = user.SoDienThoai?.Trim() ?? string.Empty,
            VaiTro = user.VaiTro?.TenVaiTro ?? string.Empty,
            TrangThai = user.TrangThai
        };
    }
}

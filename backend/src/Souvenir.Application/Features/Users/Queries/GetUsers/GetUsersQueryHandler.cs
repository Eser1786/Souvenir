using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Users;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.NguoiDungs
            .Include(u => u.VaiTro)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();
            query = query.Where(u => u.HoTen.ToLower().Contains(keyword)
                                  || u.TenDangNhap.ToLower().Contains(keyword)
                                  || u.SoDienThoai.ToLower().Contains(keyword));
        }

        if (request.VaiTroId.HasValue && request.VaiTroId.Value > 0)
        {
            query = query.Where(u => u.VaiTroId == request.VaiTroId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.TrangThai) &&
            !request.TrangThai.Equals("TatCa", StringComparison.OrdinalIgnoreCase) &&
            !request.TrangThai.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(u => u.TrangThai == request.TrangThai.Trim());
        }

        var users = await query
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);

        return users.Select(UserDto.FromEntity).ToList();
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Users;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IApplicationDbContext _context;

    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.NguoiDungs
            .Include(u => u.VaiTro)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        return user == null ? null : UserDto.FromEntity(user);
    }
}

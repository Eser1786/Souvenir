using MediatR;
using Microsoft.Extensions.Logging;
using Souvenir.Application.DTOs.Auth;
using Souvenir.Application.Interfaces;

namespace Souvenir.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, LogoutResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(IApplicationDbContext context, ILogger<LogoutCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LogoutResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId.HasValue)
        {
            _logger.LogInformation("Người dùng có Id {UserId} đã đăng xuất khỏi hệ thống.", request.UserId.Value);
        }
        else
        {
            _logger.LogInformation("Đã thực hiện yêu cầu đăng xuất.");
        }

        await Task.CompletedTask;
        return LogoutResult.Success("Đăng xuất thành công.");
    }
}

using MediatR;
using Souvenir.Application.DTOs.Returns;

namespace Souvenir.Application.Features.Returns.Commands.CreateReturn;

/// <summary>Lập phiếu trả hàng / hoàn tiền (UC08).</summary>
public record CreateReturnCommand(int NguoiDungId, CreateReturnRequest Request) : IRequest<ReturnOperationResult<ReturnDto>>;

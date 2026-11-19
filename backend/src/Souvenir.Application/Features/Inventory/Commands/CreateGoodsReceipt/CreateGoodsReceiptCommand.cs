using MediatR;
using Souvenir.Application.DTOs.Inventory;

namespace Souvenir.Application.Features.Inventory.Commands.CreateGoodsReceipt;

public record CreateGoodsReceiptCommand(int NguoiDungId, CreateGoodsReceiptRequest Request) : IRequest<GoodsReceiptResult<GoodsReceiptDto>>;

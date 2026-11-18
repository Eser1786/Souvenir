using MediatR;
using Souvenir.Application.DTOs.Returns;

namespace Souvenir.Application.Features.Returns.Queries.GetReturnableOrder;

public record GetReturnableOrderQuery(int DonHangId) : IRequest<ReturnableOrderDto?>;

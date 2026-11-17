using MediatR;
using Souvenir.Application.DTOs.Discounts;

namespace Souvenir.Application.Features.Discounts.Queries.CheckDiscount;

public record CheckDiscountQuery(string MaCode, decimal TongTienHang) : IRequest<CheckDiscountResponse>;

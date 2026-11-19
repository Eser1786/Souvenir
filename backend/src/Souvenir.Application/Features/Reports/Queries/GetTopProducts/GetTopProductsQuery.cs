using System;
using MediatR;
using Souvenir.Application.DTOs.Reports;

namespace Souvenir.Application.Features.Reports.Queries.GetTopProducts;

public record GetTopProductsQuery(
    int Limit = 5,
    string? Period = "thisMonth",
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<TopProductsReportDto>;

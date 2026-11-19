using System;
using MediatR;
using Souvenir.Application.DTOs.Reports;

namespace Souvenir.Application.Features.Reports.Queries.GetRevenueReport;

public record GetRevenueReportQuery(
    string? Period = "thisMonth",
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<RevenueSummaryDto>;

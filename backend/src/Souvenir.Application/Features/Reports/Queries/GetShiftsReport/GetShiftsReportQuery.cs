using System;
using MediatR;
using Souvenir.Application.DTOs.Reports;

namespace Souvenir.Application.Features.Reports.Queries.GetShiftsReport;

public record GetShiftsReportQuery(
    string? Period = "thisMonth",
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<ShiftsReportDto>;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.Common;
using Souvenir.Application.DTOs.Inventory;
using Souvenir.Application.Interfaces;
using Souvenir.Domain.Entities;

namespace Souvenir.Application.Features.Inventory.Commands.CreateGoodsReceipt;

public class CreateGoodsReceiptCommandHandler : IRequestHandler<CreateGoodsReceiptCommand, GoodsReceiptResult<GoodsReceiptDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateGoodsReceiptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GoodsReceiptResult<GoodsReceiptDto>> Handle(CreateGoodsReceiptCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        if (req == null || req.ChiTiet == null || req.ChiTiet.Count == 0)
        {
            return GoodsReceiptResult<GoodsReceiptDto>.Failed("Danh sách sản phẩm nhập kho không được để trống.", 400);
        }

        var supplier = await _context.NhaCungCaps.FirstOrDefaultAsync(n => n.Id == req.NhaCungCapId, cancellationToken);
        if (supplier == null)
        {
            return GoodsReceiptResult<GoodsReceiptDto>.Failed($"Không tìm thấy nhà cung cấp với mã #{req.NhaCungCapId}.", 404);
        }

        foreach (var item in req.ChiTiet)
        {
            if (item.SoLuongNhap <= 0)
            {
                return GoodsReceiptResult<GoodsReceiptDto>.Failed("Số lượng nhập mỗi món phải lớn hơn 0.", 400);
            }
            if (item.DonGiaNhap < 0)
            {
                return GoodsReceiptResult<GoodsReceiptDto>.Failed("Đơn giá nhập không được âm.", 400);
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Tạo mã phiếu nhập: PNK + yyyyMMdd + 4 số ngẫu nhiên / count
            var countToday = await _context.PhieuNhapKhos.CountAsync(cancellationToken);
            var maPhieu = $"PNK{DateTime.UtcNow:yyyyMMdd}{(countToday + 1):D4}";

            var user = await _context.NguoiDungs.FindAsync(new object[] { command.NguoiDungId }, cancellationToken);
            var tenNguoiNhap = user?.HoTen ?? "Quản lý";

            decimal tongTienNhap = 0;
            var detailEntities = new List<ChiTietNhapKho>();
            var detailDtos = new List<GoodsReceiptDetailDto>();

            var defaultDm = await _context.DanhMucSanPhams.FirstOrDefaultAsync(cancellationToken);
            var defaultDvt = await _context.DonViTinhs.FirstOrDefaultAsync(cancellationToken);

            foreach (var item in req.ChiTiet)
            {
                SanPham? sp = null;
                var requestedMaSP = ProductCode.Normalize(item.MaSP);
                if (item.SanPhamId.HasValue && item.SanPhamId.Value > 0)
                {
                    sp = await _context.SanPhams
                        .Include(p => p.DonViTinh)
                        .FirstOrDefaultAsync(p => p.Id == item.SanPhamId.Value, cancellationToken);
                }
                else if (requestedMaSP.Length > 0)
                {
                    sp = await _context.SanPhams
                        .Include(p => p.DonViTinh)
                        .FirstOrDefaultAsync(p => p.MaSP.ToUpper() == requestedMaSP, cancellationToken);
                }

                // Nếu mặt hàng mới hoàn toàn (UC12 nhánh 2a)
                if (sp == null)
                {
                    if (string.IsNullOrWhiteSpace(item.TenSP))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return GoodsReceiptResult<GoodsReceiptDto>.Failed("Tên sản phẩm mới không được để trống.", 400);
                    }

                    string newMaSP = requestedMaSP;
                    if (newMaSP.Length > 0 && !ProductCode.HasValidFormat(newMaSP))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return GoodsReceiptResult<GoodsReceiptDto>.Failed(ProductCode.FormatErrorMessage, 400);
                    }

                    if (newMaSP.Length == 0)
                    {
                        var existingMaList = await _context.SanPhams
                            .AsNoTracking()
                            .Select(p => p.MaSP)
                            .ToListAsync(cancellationToken);

                        int maxNum = 0;
                        foreach (var existing in existingMaList)
                        {
                            if (string.IsNullOrWhiteSpace(existing)) continue;
                            var clean = existing.Trim().ToUpperInvariant();
                            if (clean.StartsWith("SP") && int.TryParse(clean[2..], out var num))
                            {
                                if (num > maxNum) maxNum = num;
                            }
                        }

                        int candidate = maxNum + 1;
                        newMaSP = $"SP{candidate:D3}";
                        while (await _context.SanPhams.AnyAsync(p => p.MaSP == newMaSP, cancellationToken))
                        {
                            candidate++;
                            newMaSP = $"SP{candidate:D3}";
                        }
                    }

                    int chosenDmId = item.DanhMucId ?? 0;
                    if (chosenDmId <= 0)
                    {
                        if (!string.IsNullOrWhiteSpace(item.TenDanhMuc))
                        {
                            var trimmedCat = item.TenDanhMuc.Trim();
                            var allCats = await _context.DanhMucSanPhams.ToListAsync(cancellationToken);
                            var foundDm = allCats.FirstOrDefault(d => 
                                Souvenir.Application.Common.StringNormalizer.EqualsNormalized(d.TenDanhMuc, trimmedCat));
                            if (foundDm != null)
                            {
                                chosenDmId = foundDm.Id;
                            }
                            else
                            {
                                var newDm = new DanhMucSanPham { TenDanhMuc = trimmedCat, TrangThai = "HoatDong" };
                                _context.DanhMucSanPhams.Add(newDm);
                                await _context.SaveChangesAsync(cancellationToken);
                                chosenDmId = newDm.Id;
                            }
                        }
                        else
                        {
                            chosenDmId = defaultDm?.Id ?? 1;
                        }
                    }

                    int chosenDvtId = item.DonViTinhId ?? 0;
                    if (chosenDvtId <= 0)
                    {
                        if (!string.IsNullOrWhiteSpace(item.TenDonVi))
                        {
                            var trimmedDvt = item.TenDonVi.Trim();
                            var allUnits = await _context.DonViTinhs.ToListAsync(cancellationToken);
                            var foundDvt = allUnits.FirstOrDefault(d => 
                                Souvenir.Application.Common.StringNormalizer.EqualsNormalized(d.TenDonVi, trimmedDvt));
                            if (foundDvt != null)
                            {
                                chosenDvtId = foundDvt.Id;
                            }
                            else
                            {
                                var newDvt = new DonViTinh { TenDonVi = trimmedDvt };
                                _context.DonViTinhs.Add(newDvt);
                                await _context.SaveChangesAsync(cancellationToken);
                                chosenDvtId = newDvt.Id;
                            }
                        }
                        else
                        {
                            chosenDvtId = defaultDvt?.Id ?? 1;
                        }
                    }

                    sp = new SanPham
                    {
                        MaSP = newMaSP,
                        TenSP = item.TenSP.Trim(),
                        DanhMucId = chosenDmId,
                        DonViTinhId = chosenDvtId,
                        GiaNhap = item.DonGiaNhap,
                        GiaBan = Math.Round(item.DonGiaNhap * 1.2m, 0), // Default margin nếu chưa đặt
                        SoLuongTon = 0,
                        TrangThai = "KinhDoanh"
                    };
                    _context.SanPhams.Add(sp);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                // Cộng dồn tồn kho: soLuongTon += soLuongNhap (UC12 bước 48, 61)
                sp.SoLuongTon += item.SoLuongNhap;
                sp.GiaNhap = item.DonGiaNhap; // Cập nhật lại giá nhập mới nhất của mặt hàng
                await _context.SaveChangesAsync(cancellationToken);

                var thanhTien = item.SoLuongNhap * item.DonGiaNhap;
                tongTienNhap += thanhTien;

                detailEntities.Add(new ChiTietNhapKho
                {
                    SanPhamId = sp.Id,
                    SoLuongNhap = item.SoLuongNhap,
                    DonGiaNhap = item.DonGiaNhap,
                    ThanhTien = thanhTien
                });

                detailDtos.Add(new GoodsReceiptDetailDto
                {
                    SanPhamId = sp.Id,
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    TenDonVi = sp.DonViTinh?.TenDonVi ?? defaultDvt?.TenDonVi,
                    SoLuongNhap = item.SoLuongNhap,
                    DonGiaNhap = item.DonGiaNhap,
                    ThanhTien = thanhTien,
                    SoLuongTonMoi = sp.SoLuongTon
                });
            }

            var phieuNhap = new PhieuNhapKho
            {
                MaPhieu = maPhieu,
                NhaCungCapId = supplier.Id,
                NguoiDungId = command.NguoiDungId,
                TongTienNhap = tongTienNhap,
                NgayNhap = DateTime.UtcNow
            };

            _context.PhieuNhapKhos.Add(phieuNhap);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var detail in detailEntities)
            {
                detail.PhieuNhapKhoId = phieuNhap.Id;
                _context.ChiTietNhapKhos.Add(detail);
            }
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var resultDto = new GoodsReceiptDto
            {
                Id = phieuNhap.Id,
                MaPhieu = phieuNhap.MaPhieu,
                NhaCungCapId = supplier.Id,
                TenNCC = supplier.TenNCC,
                NguoiDungId = command.NguoiDungId,
                TenNguoiNhap = tenNguoiNhap,
                TongTienNhap = phieuNhap.TongTienNhap,
                NgayNhap = phieuNhap.NgayNhap,
                ChiTiet = detailDtos
            };

            return GoodsReceiptResult<GoodsReceiptDto>.Created(resultDto, "Nhập kho và cập nhật số lượng tồn thành công.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return GoodsReceiptResult<GoodsReceiptDto>.Failed($"Lỗi lưu phiếu nhập kho: {ex.Message}", 500);
        }
    }
}

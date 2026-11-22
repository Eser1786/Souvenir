using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Souvenir.Application.DTOs.Inventory;
using Souvenir.Application.DTOs.Products;
using Souvenir.Application.DTOs.Users;
using Souvenir.Application.Features.Inventory.Commands.CreateGoodsReceipt;
using Souvenir.Application.Features.Users.Commands.CreateUser;
using Souvenir.Application.Features.Users.Commands.DeleteUser;
using Souvenir.Application.Features.Users.Commands.ToggleUserStatus;
using Souvenir.Application.Features.Users.Commands.UpdateUser;
using Souvenir.Application.Features.Discounts.Queries.CheckDiscount;
using Souvenir.Application.DTOs.Orders;
using Souvenir.Application.Features.DraftOrders.Commands.CreateDraftOrder;
using Souvenir.Application.Features.DraftOrders.Commands.DeleteDraftOrder;
using Souvenir.Application.Features.DraftOrders.Queries.GetDraftOrders;
using Souvenir.Application.Interfaces;
using Souvenir.Application.Features.Reports.Queries.GetRevenueReport;
using Souvenir.Application.Features.Reports.Queries.GetTopProducts;
using Souvenir.Application.Features.Reports.Queries.GetShiftsReport;
using Souvenir.Application.Features.Customers.Queries.GetCustomerOrders;
using Souvenir.Application.DTOs.StockAudits;
using Souvenir.Application.Features.StockAudits.Commands.BalanceStockAudit;
using Souvenir.Application.Features.StockAudits.Commands.SaveDraftStockAudit;
using Souvenir.Application.Features.Products.Commands.CreateProduct;
using Souvenir.Application.Features.Products.Commands.DeleteProduct;
using Souvenir.Domain.Constants;
using Souvenir.Domain.Entities;
using Souvenir.Infrastructure.Data;

namespace Souvenir.Verification;

class FastPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => $"hash_{password}";
    public bool VerifyPassword(string password, string passwordHash) => passwordHash == $"hash_{password}";
}

class MockCaLamViecLookup : ICaLamViecLookup
{
    public Task<int?> GetOpenShiftIdAsync(int nguoiDungId, CancellationToken cancellationToken = default) => Task.FromResult<int?>(1);
    public Task<bool> IsShiftOpenAsync(int caLamViecId, CancellationToken cancellationToken = default) => Task.FromResult(true);
}

class Program
{
    private static int _passedTests = 0;
    private static int _failedTests = 0;

    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("===============================================================");
        Console.WriteLine("   MERA SOUVENIR - AUTOMATED VERIFICATION SUITE (UC15)");
        Console.WriteLine("===============================================================\n");

        await Test_CreateUser_DuplicateUsername_Returns409();
        await Test_CreateUser_DuplicateUsername_CaseInsensitive_Returns409();
        await Test_CreateUser_Success_Returns201();
        await Test_LockLastActiveManager_Returns400();
        await Test_DeleteLastActiveManager_Returns400();
        await Test_DemoteLastActiveManager_Returns400();
        await Test_MultipleManagers_AllowsActionOnFirst_BlocksActionOnLast();

        Console.WriteLine("\n--- VERIFICATION SUITE FOR PRODUCT CODE FORMAT ---");
        await Test_CreateProduct_InvalidCode_Returns400();
        await Test_CreateProduct_NormalizesValidCode();
        await Test_CreateProduct_AutoGeneratesNextCode();
        await Test_GoodsReceipt_InvalidNewProductCode_Returns400();

        Console.WriteLine("\n--- VERIFICATION SUITE FOR PRODUCT DELETION ---");
        await Test_DeleteDiscontinuedProduct_WithoutHistory_IsHardDeleted();
        await Test_DeleteDiscontinuedProduct_WithHistory_IsPreserved();

        Console.WriteLine("\n--- VERIFICATION SUITE FOR UC05 (VOUCHERS & DISCOUNT) ---");
        await Test_CheckDiscount_QUATANG10_ValidPercentDiscount();
        await Test_CheckDiscount_GIAM10K_ValidFixedAmountDiscount();
        await Test_CheckDiscount_ExpiredVoucher_ReturnsError();
        await Test_CheckDiscount_NonExistentVoucher_ReturnsError();

        Console.WriteLine("\n--- VERIFICATION SUITE FOR UC06 (DRAFT ORDERS / LƯU NHÁP) ---");
        await Test_CreateDraftOrder_Success_DoesNotDeductStock();
        await Test_GetDraftOrders_ReturnsDraftOrdersOnly();
        await Test_DeleteDraftOrder_RemovesOrderAndDetails();

        Console.WriteLine("\n--- VERIFICATION SUITE FOR UC14 (BÁO CÁO DOANH THU & THỐNG KÊ) ---");
        await Test_GetRevenueReport_CalculatesCorrectTotals();
        await Test_GetTopProducts_ReturnsRankedProducts();
        await Test_GetShiftsReport_CalculatesCashAndTransferRevenues();

        Console.WriteLine("\n--- VERIFICATION SUITE FOR UC09 (LỊCH SỬ ĐƠN HÀNG KHÁCH) ---");
        await Test_GetCustomerOrders_ReturnsCustomerHistory();

        Console.WriteLine("\n--- VERIFICATION SUITE FOR UC13 (KIỂM KÊ KHO) ---");
        await Test_StockAudit_SaveDraft_DoesNotChangeStock();
        await Test_StockAudit_Balance_MissingReason_Returns400AndKeepsStock();
        await Test_StockAudit_Balance_FromDraft_UpdatesStockAndLocksAudit();

        Console.WriteLine("\n===============================================================");
        Console.WriteLine($"   TEST RESULTS: {_passedTests} PASSED, {_failedTests} FAILED");
        Console.WriteLine("===============================================================");

        return _failedTests == 0 ? 0 : 1;
    }

    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        // Seed default roles
        var roleQuanLy = new VaiTro { Id = 1, TenVaiTro = VaiTroConstants.QuanLy, MoTa = "Quản lý hệ thống" };
        var roleNhanVien = new VaiTro { Id = 2, TenVaiTro = VaiTroConstants.NhanVien, MoTa = "Nhân viên bán hàng" };

        context.VaiTros.AddRange(roleQuanLy, roleNhanVien);
        context.SaveChanges();

        return context;
    }

    private static void AssertTest(string testName, bool condition, string failureDetails = "")
    {
        if (condition)
        {
            _passedTests++;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[PASS] ");
            Console.ResetColor();
            Console.WriteLine(testName);
        }
        else
        {
            _failedTests++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[FAIL] ");
            Console.ResetColor();
            Console.WriteLine($"{testName} - {failureDetails}");
        }
    }

    private static async Task Test_CreateProduct_InvalidCode_Returns400()
    {
        using var context = CreateInMemoryDbContext();
        var handler = new CreateProductCommandHandler(context);
        var result = await handler.Handle(
            new CreateProductCommand(new CreateProductRequest
            {
                MaSP = "ABC123",
                TenSP = "Móc khóa thử nghiệm",
                TenDanhMuc = "Móc khóa",
                TenDonVi = "Cái",
                GiaNhap = 10000m,
                GiaBan = 20000m
            }),
            CancellationToken.None);

        AssertTest(
            "Product code: Rejects a non-SP code",
            !result.IsSuccess && result.StatusCode == 400 && !await context.SanPhams.AnyAsync(),
            $"Expected 400 with no product saved, got {result.StatusCode}. Message: {result.Message}"
        );
    }

    private static async Task Test_CreateProduct_NormalizesValidCode()
    {
        using var context = CreateInMemoryDbContext();
        var handler = new CreateProductCommandHandler(context);
        var result = await handler.Handle(
            new CreateProductCommand(new CreateProductRequest
            {
                MaSP = "sp011",
                TenSP = "Móc khóa thử nghiệm",
                TenDanhMuc = "Móc khóa",
                TenDonVi = "Cái",
                GiaNhap = 10000m,
                GiaBan = 20000m
            }),
            CancellationToken.None);

        AssertTest(
            "Product code: Normalizes valid lowercase code",
            result.IsSuccess && result.Data?.MaSP == "SP011",
            $"Expected SP011, got {result.Data?.MaSP ?? result.Message}"
        );
    }

    private static async Task Test_CreateProduct_AutoGeneratesNextCode()
    {
        using var context = CreateInMemoryDbContext();
        var category = new DanhMucSanPham { TenDanhMuc = "Quà tặng" };
        var unit = new DonViTinh { TenDonVi = "Cái" };
        context.DanhMucSanPhams.Add(category);
        context.DonViTinhs.Add(unit);
        await context.SaveChangesAsync();
        context.SanPhams.Add(new SanPham
        {
            MaSP = "SP009",
            TenSP = "Sản phẩm hiện có",
            DanhMucId = category.Id,
            DonViTinhId = unit.Id
        });
        await context.SaveChangesAsync();

        var handler = new CreateProductCommandHandler(context);
        var result = await handler.Handle(
            new CreateProductCommand(new CreateProductRequest
            {
                TenSP = "Móc khóa thử nghiệm",
                TenDanhMuc = category.TenDanhMuc,
                TenDonVi = unit.TenDonVi,
                GiaNhap = 10000m,
                GiaBan = 20000m
            }),
            CancellationToken.None);

        AssertTest(
            "Product code: Generates next code from SP009",
            result.IsSuccess && result.Data?.MaSP == "SP010",
            $"Expected SP010, got {result.Data?.MaSP ?? result.Message}"
        );
    }

    private static async Task Test_GoodsReceipt_InvalidNewProductCode_Returns400()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        using var context = new AppDbContext(options);
        var supplier = new NhaCungCap
        {
            MaNCC = "NCC001",
            TenNCC = "Nhà cung cấp thử nghiệm",
            SoDienThoai = "0900000000"
        };
        context.NhaCungCaps.Add(supplier);
        await context.SaveChangesAsync();

        var handler = new CreateGoodsReceiptCommandHandler(context);
        var result = await handler.Handle(
            new CreateGoodsReceiptCommand(1, new CreateGoodsReceiptRequest
            {
                NhaCungCapId = supplier.Id,
                ChiTiet =
                [
                    new GoodsReceiptItemRequest
                    {
                        MaSP = "ABC123",
                        TenSP = "Mặt hàng thử nghiệm",
                        SoLuongNhap = 1,
                        DonGiaNhap = 10000m
                    }
                ]
            }),
            CancellationToken.None);

        AssertTest(
            "Product code: Goods receipt rejects an invalid new-product code",
            !result.IsSuccess && result.StatusCode == 400 && !await context.SanPhams.AnyAsync(),
            $"Expected 400 with no product saved, got {result.StatusCode}. Message: {result.Message}"
        );
    }

    private static async Task Test_DeleteDiscontinuedProduct_WithoutHistory_IsHardDeleted()
    {
        using var context = CreateInMemoryDbContext();
        var product = await SeedProductForDeleteTestAsync(context, "NgungKinhDoanh");
        var handler = new DeleteProductCommandHandler(context);

        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);
        var stillExists = await context.SanPhams.AnyAsync(p => p.Id == product.Id);

        AssertTest(
            "Product deletion: Permanently deletes a discontinued product without history",
            result.IsSuccess && result.Data == "DaXoa" && !stillExists,
            $"Expected hard delete, got status {result.StatusCode}. Message: {result.Message}"
        );
    }

    private static async Task Test_DeleteDiscontinuedProduct_WithHistory_IsPreserved()
    {
        using var context = CreateInMemoryDbContext();
        var product = await SeedProductForDeleteTestAsync(context, "KinhDoanh");
        var manager = new NguoiDung
        {
            VaiTroId = 1,
            TenDangNhap = "delete-test-manager",
            MatKhau = "hash",
            HoTen = "Quản lý kiểm thử",
            SoDienThoai = "0900000000"
        };
        context.NguoiDungs.Add(manager);
        await context.SaveChangesAsync();

        var audit = new PhieuKiemKe
        {
            MaPhieu = "KK-DELETE-TEST",
            NguoiDungId = manager.Id,
            NgayKiemKe = DateTime.UtcNow
        };
        context.PhieuKiemKes.Add(audit);
        await context.SaveChangesAsync();
        context.ChiTietKiemKes.Add(new ChiTietKiemKe
        {
            PhieuKiemKeId = audit.Id,
            SanPhamId = product.Id
        });
        await context.SaveChangesAsync();

        var handler = new DeleteProductCommandHandler(context);
        var firstResult = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);
        var secondResult = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);
        var retainedProduct = await context.SanPhams.FirstOrDefaultAsync(p => p.Id == product.Id);

        AssertTest(
            "Product deletion: Keeps history and blocks hard delete after discontinuation",
            firstResult.IsSuccess &&
            firstResult.Data == "NgungKinhDoanh" &&
            !secondResult.IsSuccess &&
            secondResult.StatusCode == 409 &&
            retainedProduct?.TrangThai == "NgungKinhDoanh",
            $"Expected soft delete then 409, got first {firstResult.StatusCode}, second {secondResult.StatusCode}."
        );
    }

    private static async Task<SanPham> SeedProductForDeleteTestAsync(AppDbContext context, string status)
    {
        var category = new DanhMucSanPham { TenDanhMuc = "Danh mục kiểm thử" };
        var unit = new DonViTinh { TenDonVi = "Cái" };
        context.DanhMucSanPhams.Add(category);
        context.DonViTinhs.Add(unit);
        await context.SaveChangesAsync();

        var product = new SanPham
        {
            MaSP = "SP900",
            TenSP = "Sản phẩm kiểm thử",
            DanhMucId = category.Id,
            DonViTinhId = unit.Id,
            TrangThai = status
        };
        context.SanPhams.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    private static async Task Test_CreateUser_DuplicateUsername_Returns409()
    {
        using var context = CreateInMemoryDbContext();
        var hasher = new FastPasswordHasher();

        // Seed an existing user
        context.NguoiDungs.Add(new NguoiDung
        {
            Id = 1,
            TenDangNhap = "nhanvien1",
            MatKhau = hasher.HashPassword("123456"),
            HoTen = "Nguyen Van A",
            SoDienThoai = "0901234567",
            VaiTroId = 2,
            TrangThai = TrangThaiTaiKhoan.HoatDong
        });
        await context.SaveChangesAsync();

        var handler = new CreateUserCommandHandler(context, hasher);
        var command = new CreateUserCommand(new CreateUserRequest
        {
            TenDangNhap = "nhanvien1",
            MatKhau = "NewPass@123",
            HoTen = "Tran Thi B",
            SoDienThoai = "0912345678",
            VaiTroId = 2
        });

        var result = await handler.Handle(command, CancellationToken.None);

        AssertTest(
            "Constraint 1: Duplicate TenDangNhap returns 409 Conflict",
            result.StatusCode == 409 && !result.IsSuccess && (result.Message?.Contains("đã tồn tại") ?? false),
            $"Expected status 409, got {result.StatusCode}. Message: {result.Message}"
        );
    }

    private static async Task Test_CreateUser_DuplicateUsername_CaseInsensitive_Returns409()
    {
        using var context = CreateInMemoryDbContext();
        var hasher = new FastPasswordHasher();

        context.NguoiDungs.Add(new NguoiDung
        {
            Id = 1,
            TenDangNhap = "Manager01",
            MatKhau = hasher.HashPassword("123456"),
            HoTen = "Manager A",
            SoDienThoai = "0901234567",
            VaiTroId = 1,
            TrangThai = TrangThaiTaiKhoan.HoatDong
        });
        await context.SaveChangesAsync();

        var handler = new CreateUserCommandHandler(context, hasher);
        var command = new CreateUserCommand(new CreateUserRequest
        {
            TenDangNhap = "manager01", // lowercase check
            MatKhau = "NewPass@123",
            HoTen = "Manager B",
            SoDienThoai = "0912345678",
            VaiTroId = 1
        });

        var result = await handler.Handle(command, CancellationToken.None);

        AssertTest(
            "Constraint 1 (Case-Insensitive): 'manager01' vs 'Manager01' returns 409 Conflict",
            result.StatusCode == 409 && !result.IsSuccess,
            $"Expected status 409, got {result.StatusCode}. Message: {result.Message}"
        );
    }

    private static async Task Test_CreateUser_Success_Returns201()
    {
        using var context = CreateInMemoryDbContext();
        var hasher = new FastPasswordHasher();

        var handler = new CreateUserCommandHandler(context, hasher);
        var command = new CreateUserCommand(new CreateUserRequest
        {
            TenDangNhap = "unique_employee",
            MatKhau = "SecurePassword@123",
            HoTen = "Le Van Unique",
            SoDienThoai = "0988776655",
            VaiTroId = 2
        });

        var result = await handler.Handle(command, CancellationToken.None);

        AssertTest(
            "CreateUser Positive: Unique username successfully created with status 201 Created",
            result.StatusCode == 201 && result.IsSuccess && result.Data?.TenDangNhap == "unique_employee",
            $"Expected status 201, got {result.StatusCode}. Message: {result.Message}"
        );
    }

    private static async Task Test_LockLastActiveManager_Returns400()
    {
        using var context = CreateInMemoryDbContext();
        var roleQuanLy = await context.VaiTros.FirstAsync(r => r.TenVaiTro == VaiTroConstants.QuanLy);

        var soleManager = new NguoiDung
        {
            Id = 10,
            TenDangNhap = "admin_master",
            MatKhau = "hashed",
            HoTen = "Quan Ly Duy Nhat",
            SoDienThoai = "0900000001",
            VaiTroId = roleQuanLy.Id,
            VaiTro = roleQuanLy,
            TrangThai = TrangThaiTaiKhoan.HoatDong
        };
        context.NguoiDungs.Add(soleManager);
        await context.SaveChangesAsync();

        var handler = new ToggleUserStatusCommandHandler(context);
        var command = new ToggleUserStatusCommand(soleManager.Id, TrangThaiTaiKhoan.Khoa);

        var result = await handler.Handle(command, CancellationToken.None);

        AssertTest(
            "Constraint 2 (Lock Manager): Lock sole active QuanLy returns 400 Bad Request",
            result.StatusCode == 400 && !result.IsSuccess && (result.Message?.Contains("Không thể khóa tài khoản Quản lý đang hoạt động cuối cùng") ?? false),
            $"Expected status 400, got {result.StatusCode}. Message: {result.Message}"
        );
    }

    private static async Task Test_DeleteLastActiveManager_Returns400()
    {
        using var context = CreateInMemoryDbContext();
        var roleQuanLy = await context.VaiTros.FirstAsync(r => r.TenVaiTro == VaiTroConstants.QuanLy);

        var soleManager = new NguoiDung
        {
            Id = 11,
            TenDangNhap = "admin_master",
            MatKhau = "hashed",
            HoTen = "Quan Ly Duy Nhat",
            SoDienThoai = "0900000001",
            VaiTroId = roleQuanLy.Id,
            VaiTro = roleQuanLy,
            TrangThai = TrangThaiTaiKhoan.HoatDong
        };
        context.NguoiDungs.Add(soleManager);
        await context.SaveChangesAsync();

        var handler = new DeleteUserCommandHandler(context);
        var command = new DeleteUserCommand(soleManager.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        AssertTest(
            "Constraint 2 (Delete Manager): Delete sole active QuanLy returns 400 Bad Request",
            result.StatusCode == 400 && !result.IsSuccess && (result.Message?.Contains("Không thể xóa tài khoản Quản lý đang hoạt động cuối cùng") ?? false),
            $"Expected status 400, got {result.StatusCode}. Message: {result.Message}"
        );
    }

    private static async Task Test_DemoteLastActiveManager_Returns400()
    {
        using var context = CreateInMemoryDbContext();
        var roleQuanLy = await context.VaiTros.FirstAsync(r => r.TenVaiTro == VaiTroConstants.QuanLy);
        var roleNhanVien = await context.VaiTros.FirstAsync(r => r.TenVaiTro == VaiTroConstants.NhanVien);

        var soleManager = new NguoiDung
        {
            Id = 12,
            TenDangNhap = "admin_master",
            MatKhau = "hashed",
            HoTen = "Quan Ly Duy Nhat",
            SoDienThoai = "0900000001",
            VaiTroId = roleQuanLy.Id,
            VaiTro = roleQuanLy,
            TrangThai = TrangThaiTaiKhoan.HoatDong
        };
        context.NguoiDungs.Add(soleManager);
        await context.SaveChangesAsync();

        var handler = new UpdateUserCommandHandler(context);
        var command = new UpdateUserCommand(soleManager.Id, new UpdateUserRequest
        {
            HoTen = "Quan Ly Bi Ha Quyen",
            SoDienThoai = "0900000001",
            VaiTroId = roleNhanVien.Id // Demote to NhanVien
        });

        var result = await handler.Handle(command, CancellationToken.None);

        AssertTest(
            "Constraint 2 (Demote Manager): Demoting sole active QuanLy returns 400 Bad Request",
            result.StatusCode == 400 && !result.IsSuccess && (result.Message?.Contains("Không thể hạ quyền tài khoản Quản lý đang hoạt động cuối cùng") ?? false),
            $"Expected status 400, got {result.StatusCode}. Message: {result.Message}"
        );
    }

    private static async Task Test_MultipleManagers_AllowsActionOnFirst_BlocksActionOnLast()
    {
        using var context = CreateInMemoryDbContext();
        var roleQuanLy = await context.VaiTros.FirstAsync(r => r.TenVaiTro == VaiTroConstants.QuanLy);

        var manager1 = new NguoiDung
        {
            Id = 20,
            TenDangNhap = "manager_1",
            MatKhau = "hashed",
            HoTen = "Quan Ly 1",
            SoDienThoai = "0900000001",
            VaiTroId = roleQuanLy.Id,
            VaiTro = roleQuanLy,
            TrangThai = TrangThaiTaiKhoan.HoatDong
        };
        var manager2 = new NguoiDung
        {
            Id = 21,
            TenDangNhap = "manager_2",
            MatKhau = "hashed",
            HoTen = "Quan Ly 2",
            SoDienThoai = "0900000002",
            VaiTroId = roleQuanLy.Id,
            VaiTro = roleQuanLy,
            TrangThai = TrangThaiTaiKhoan.HoatDong
        };
        context.NguoiDungs.AddRange(manager1, manager2);
        await context.SaveChangesAsync();

        var toggleHandler = new ToggleUserStatusCommandHandler(context);

        // 1. Locking manager1 should succeed because manager2 is still active
        var result1 = await toggleHandler.Handle(new ToggleUserStatusCommand(manager1.Id, TrangThaiTaiKhoan.Khoa), CancellationToken.None);
        bool firstLockPassed = result1.StatusCode == 200 && result1.IsSuccess;

        // 2. Locking manager2 should now FAIL (400) because manager1 is locked, leaving manager2 as the last active manager
        var result2 = await toggleHandler.Handle(new ToggleUserStatusCommand(manager2.Id, TrangThaiTaiKhoan.Khoa), CancellationToken.None);
        bool secondLockBlocked = result2.StatusCode == 400 && !result2.IsSuccess;

        AssertTest(
            "Multiple Managers: Can lock 1st manager (200 OK), but subsequent lock on last active manager is blocked (400 Bad Request)",
            firstLockPassed && secondLockBlocked,
            $"FirstLock status: {result1.StatusCode}, SecondLock status: {result2.StatusCode}"
        );
    }

    private static async Task Test_CheckDiscount_QUATANG10_ValidPercentDiscount()
    {
        using var context = CreateInMemoryDbContext();
        context.MaGiamGias.Add(new MaGiamGia
        {
            Id = 1,
            MaCode = "QUATANG10",
            TenVoucher = "Voucher quà tặng 10%",
            LoaiGiamGia = "PhanTram",
            GiaTriGiam = 10,
            NgayBatDau = DateTime.UtcNow.AddDays(-10),
            NgayKetThuc = DateTime.UtcNow.AddDays(30)
        });
        await context.SaveChangesAsync();

        var handler = new CheckDiscountQueryHandler(context);
        var res = await handler.Handle(new CheckDiscountQuery("quatang10", 200000m), CancellationToken.None);

        AssertTest(
            "UC05: Voucher QUATANG10 applies 10% discount on 200.000d -> 20.000d discount, 180.000d total",
            res.HopLe && res.SoTienGiam == 20000m && res.ThanhTienSauGiam == 180000m,
            $"HopLe: {res.HopLe}, SoTienGiam: {res.SoTienGiam}, ThanhTienSauGiam: {res.ThanhTienSauGiam}"
        );
    }

    private static async Task Test_CheckDiscount_GIAM10K_ValidFixedAmountDiscount()
    {
        using var context = CreateInMemoryDbContext();
        context.MaGiamGias.Add(new MaGiamGia
        {
            Id = 2,
            MaCode = "GIAM10K",
            TenVoucher = "Giảm 10.000 đ",
            LoaiGiamGia = "SoTien",
            GiaTriGiam = 10000,
            NgayBatDau = DateTime.UtcNow.AddDays(-10),
            NgayKetThuc = DateTime.UtcNow.AddDays(30)
        });
        await context.SaveChangesAsync();

        var handler = new CheckDiscountQueryHandler(context);
        var res = await handler.Handle(new CheckDiscountQuery("GIAM10K", 50000m), CancellationToken.None);

        AssertTest(
            "UC05: Voucher GIAM10K applies 10.000d fixed discount on 50.000d -> 10.000d discount, 40.000d total",
            res.HopLe && res.SoTienGiam == 10000m && res.ThanhTienSauGiam == 40000m,
            $"HopLe: {res.HopLe}, SoTienGiam: {res.SoTienGiam}, ThanhTienSauGiam: {res.ThanhTienSauGiam}"
        );
    }

    private static async Task Test_CheckDiscount_ExpiredVoucher_ReturnsError()
    {
        using var context = CreateInMemoryDbContext();
        context.MaGiamGias.Add(new MaGiamGia
        {
            Id = 3,
            MaCode = "HETHAN",
            TenVoucher = "Mã hết hạn",
            LoaiGiamGia = "PhanTram",
            GiaTriGiam = 20,
            NgayBatDau = DateTime.UtcNow.AddDays(-20),
            NgayKetThuc = DateTime.UtcNow.AddDays(-1)
        });
        await context.SaveChangesAsync();

        var handler = new CheckDiscountQueryHandler(context);
        var res = await handler.Handle(new CheckDiscountQuery("HETHAN", 100000m), CancellationToken.None);

        AssertTest(
            "UC05: Expired voucher returns HopLe = false and preserves order total",
            !res.HopLe && res.SoTienGiam == 0 && res.ThanhTienSauGiam == 100000m,
            $"HopLe: {res.HopLe}, ThongBao: {res.ThongBao}"
        );
    }

    private static async Task Test_CheckDiscount_NonExistentVoucher_ReturnsError()
    {
        using var context = CreateInMemoryDbContext();
        var handler = new CheckDiscountQueryHandler(context);
        var res = await handler.Handle(new CheckDiscountQuery("KHONGCO", 150000m), CancellationToken.None);

        AssertTest(
            "UC05: Non-existent voucher returns HopLe = false",
            !res.HopLe && res.SoTienGiam == 0,
            $"HopLe: {res.HopLe}, ThongBao: {res.ThongBao}"
        );
    }

    private static async Task Test_CreateDraftOrder_Success_DoesNotDeductStock()
    {
        using var context = CreateInMemoryDbContext();
        context.SanPhams.Add(new SanPham { Id = 1, MaSP = "SP01", TenSP = "Gấu bông", GiaBan = 100000m, SoLuongTon = 50, TrangThai = "KinhDoanh" });
        context.HinhThucThanhToans.Add(new HinhThucThanhToan { Id = 1, TenHinhThuc = "Tiền mặt", TrangThai = "HoatDong" });
        context.NguoiDungs.Add(new NguoiDung { Id = 1, TenDangNhap = "cashier1", HoTen = "Thu Ngân 1", MatKhau = "hash", SoDienThoai = "0900000001", TrangThai = "HoatDong", VaiTroId = 1 });
        await context.SaveChangesAsync();

        var handler = new CreateDraftOrderCommandHandler(context, new MockCaLamViecLookup());
        var req = new CreateDraftOrderRequest
        {
            Items = new List<CreateDraftOrderItemRequest> { new() { SanPhamId = 1, SoLuong = 5 } }
        };

        var result = await handler.Handle(new CreateDraftOrderCommand(1, req), CancellationToken.None);
        var product = await context.SanPhams.FindAsync(1);

        AssertTest(
            "UC06: Saving draft order succeeds and DOES NOT deduct stock",
            result.IsSuccess && result.Data != null && result.Data.TrangThai == TrangThaiDonHang.LuuNhap && product?.SoLuongTon == 50,
            $"IsSuccess: {result.IsSuccess}, Stock remaining: {product?.SoLuongTon} (expected 50)"
        );
    }

    private static async Task Test_GetDraftOrders_ReturnsDraftOrdersOnly()
    {
        using var context = CreateInMemoryDbContext();
        context.SanPhams.Add(new SanPham { Id = 1, MaSP = "SP01", TenSP = "Gấu bông", GiaBan = 100000m, SoLuongTon = 50 });
        context.HinhThucThanhToans.Add(new HinhThucThanhToan { Id = 1, TenHinhThuc = "Tiền mặt", TrangThai = "HoatDong" });
        context.NguoiDungs.Add(new NguoiDung { Id = 1, TenDangNhap = "cashier1", HoTen = "Thu Ngân 1", MatKhau = "hash", SoDienThoai = "0900000001", TrangThai = "HoatDong", VaiTroId = 1 });

        context.DonHangs.Add(new DonHang { Id = 101, TrangThai = TrangThaiDonHang.LuuNhap, NguoiDungId = 1, CaLamViecId = 1, HinhThucThanhToanId = 1, NgayTao = DateTime.UtcNow, TongTienThanhToan = 100000m });
        context.DonHangs.Add(new DonHang { Id = 102, TrangThai = TrangThaiDonHang.DaThanhToan, NguoiDungId = 1, CaLamViecId = 1, HinhThucThanhToanId = 1, NgayTao = DateTime.UtcNow, TongTienThanhToan = 200000m });
        await context.SaveChangesAsync();

        var handler = new GetDraftOrdersQueryHandler(context);
        var list = await handler.Handle(new GetDraftOrdersQuery(), CancellationToken.None);

        AssertTest(
            "UC06: GetDraftOrders returns only orders with TrangThai = LuuNhap",
            list.Count == 1 && list[0].Id == 101,
            $"Returned count: {list.Count}, first order ID: {(list.Count > 0 ? list[0].Id : 0)}"
        );
    }

    private static async Task Test_DeleteDraftOrder_RemovesOrderAndDetails()
    {
        using var context = CreateInMemoryDbContext();
        context.DonHangs.Add(new DonHang { Id = 201, TrangThai = TrangThaiDonHang.LuuNhap, NguoiDungId = 1, CaLamViecId = 1, HinhThucThanhToanId = 1, NgayTao = DateTime.UtcNow, TongTienThanhToan = 100000m });
        context.ChiTietDonHangs.Add(new ChiTietDonHang { DonHangId = 201, SanPhamId = 1, SoLuong = 2, DonGia = 50000m, ThanhTien = 100000m });
        await context.SaveChangesAsync();

        var handler = new DeleteDraftOrderCommandHandler(context);
        var res = await handler.Handle(new DeleteDraftOrderCommand(201), CancellationToken.None);

        var orderAfter = await context.DonHangs.FindAsync(201);
        var detailsAfter = await context.ChiTietDonHangs.Where(c => c.DonHangId == 201).ToListAsync();

        AssertTest(
            "UC06: DeleteDraftOrder removes both DonHang and ChiTietDonHang records",
            res.IsSuccess && orderAfter == null && detailsAfter.Count == 0,
            $"IsSuccess: {res.IsSuccess}, Order exists: {orderAfter != null}, Details count: {detailsAfter.Count}"
        );
    }

    private static async Task Test_GetRevenueReport_CalculatesCorrectTotals()
    {
        using var context = CreateInMemoryDbContext();
        var now = DateTime.Now;

        // Seed products
        var p1 = new SanPham { Id = 10, TenSP = "Gấu bông", MaSP = "GB001", GiaNhap = 50000m, GiaBan = 100000m, SoLuongTon = 20 };
        var p2 = new SanPham { Id = 20, TenSP = "Ly sứ", MaSP = "LY002", GiaNhap = 30000m, GiaBan = 60000m, SoLuongTon = 30 };
        context.SanPhams.AddRange(p1, p2);

        // Seed 2 completed orders and 1 draft order
        var o1 = new DonHang { Id = 1, TrangThai = TrangThaiDonHang.DaThanhToan, NgayTao = now, TongTienThanhToan = 200000m, NguoiDungId = 1, HinhThucThanhToanId = 1 };
        o1.ChiTietDonHangs.Add(new ChiTietDonHang { DonHangId = 1, SanPhamId = 10, SoLuong = 2, DonGia = 100000m, ThanhTien = 200000m, SanPham = p1 });

        var o2 = new DonHang { Id = 2, TrangThai = TrangThaiDonHang.DaThanhToan, NgayTao = now, TongTienThanhToan = 120000m, NguoiDungId = 1, HinhThucThanhToanId = 2 };
        o2.ChiTietDonHangs.Add(new ChiTietDonHang { DonHangId = 2, SanPhamId = 20, SoLuong = 2, DonGia = 60000m, ThanhTien = 120000m, SanPham = p2 });

        var oDraft = new DonHang { Id = 3, TrangThai = TrangThaiDonHang.LuuNhap, NgayTao = now, TongTienThanhToan = 500000m, NguoiDungId = 1, HinhThucThanhToanId = 1 };

        context.DonHangs.AddRange(o1, o2, oDraft);
        await context.SaveChangesAsync();

        var handler = new GetRevenueReportQueryHandler(context);
        var report = await handler.Handle(new GetRevenueReportQuery("today"), CancellationToken.None);

        AssertTest(
            "UC14: GetRevenueReport correctly calculates gross revenue, orders count, and inventory",
            report.TongDoanhThu == 320000m && report.TongDonHang == 2 && report.TongTonKho == 50,
            $"TongDoanhThu: {report.TongDoanhThu}, TongDonHang: {report.TongDonHang}, TongTonKho: {report.TongTonKho}"
        );
    }

    private static async Task Test_GetTopProducts_ReturnsRankedProducts()
    {
        using var context = CreateInMemoryDbContext();
        var now = DateTime.Now;

        var p1 = new SanPham { TenSP = "Móc khóa", MaSP = "MK01", GiaBan = 20000m, SoLuongTon = 50 };
        var p2 = new SanPham { TenSP = "Đèn ngủ", MaSP = "DN01", GiaBan = 150000m, SoLuongTon = 10 };
        context.SanPhams.AddRange(p1, p2);
        await context.SaveChangesAsync();

        var o = new DonHang { TrangThai = TrangThaiDonHang.DaThanhToan, NgayTao = now, TongTienThanhToan = 340000m, NguoiDungId = 1, HinhThucThanhToanId = 1 };
        context.DonHangs.Add(o);
        await context.SaveChangesAsync();

        var ct1 = new ChiTietDonHang { DonHangId = o.Id, SanPhamId = p1.Id, SoLuong = 7, DonGia = 20000m, ThanhTien = 140000m, SanPham = p1 };
        var ct2 = new ChiTietDonHang { DonHangId = o.Id, SanPhamId = p2.Id, SoLuong = 2, DonGia = 100000m, ThanhTien = 200000m, SanPham = p2 };
        context.ChiTietDonHangs.AddRange(ct1, ct2);
        o.ChiTietDonHangs.Add(ct1);
        o.ChiTietDonHangs.Add(ct2);
        await context.SaveChangesAsync();

        var handler = new GetTopProductsQueryHandler(context);
        var result = await handler.Handle(new GetTopProductsQuery(Limit: 5, Period: "today"), CancellationToken.None);

        AssertTest(
            "UC14: GetTopProducts correctly ranks product by quantity sold",
            result.Items.Count >= 2 && result.Items[0].SanPhamId == p1.Id && result.Items[0].SoLuongDaBan == 7,
            $"Top 1 item: ID={result.Items.FirstOrDefault()?.SanPhamId}, Qty={result.Items.FirstOrDefault()?.SoLuongDaBan}"
        );
    }

    private static async Task Test_GetShiftsReport_CalculatesCashAndTransferRevenues()
    {
        using var context = CreateInMemoryDbContext();
        var now = DateTime.Now;

        var user = new NguoiDung { TenDangNhap = "staff1", MatKhau = "pass", HoTen = "Trần Thu Hà", VaiTroId = 2 };
        context.NguoiDungs.Add(user);
        await context.SaveChangesAsync();

        var shift = new CaLamViec { NguoiDungId = user.Id, ThoiGianMoCa = now.AddHours(-4), TienMatBanGiao = 500000m, TienMatThucTe = 750000m, TrangThai = "DA_CHOT" };
        context.CaLamViecs.Add(shift);
        await context.SaveChangesAsync();

        // 1 cash order (250.000) + 1 transfer order (100.000)
        var oCash = new DonHang { CaLamViecId = shift.Id, TrangThai = TrangThaiDonHang.DaThanhToan, NgayTao = now.AddHours(-2), HinhThucThanhToanId = 1, TongTienThanhToan = 250000m, NguoiDungId = user.Id };
        var oTransfer = new DonHang { CaLamViecId = shift.Id, TrangThai = TrangThaiDonHang.DaThanhToan, NgayTao = now.AddHours(-1), HinhThucThanhToanId = 2, TongTienThanhToan = 100000m, NguoiDungId = user.Id };
        context.DonHangs.AddRange(oCash, oTransfer);
        await context.SaveChangesAsync();

        var handler = new GetShiftsReportQueryHandler(context);
        var result = await handler.Handle(new GetShiftsReportQuery("today"), CancellationToken.None);

        var shiftItem = result.Items.FirstOrDefault(s => s.CaLamViecId == shift.Id);
        AssertTest(
            "UC14: GetShiftsReport accurately computes cash, transfer revenues and discrepancy",
            shiftItem != null && shiftItem.TongDoanhThuTienMat == 250000m && shiftItem.TongDoanhThuChuyenKhoan == 100000m && shiftItem.ChenhLech == 0m,
            $"ShiftItem: Cash={shiftItem?.TongDoanhThuTienMat}, Transfer={shiftItem?.TongDoanhThuChuyenKhoan}, Diff={shiftItem?.ChenhLech}"
        );
    }

    private static async Task Test_GetCustomerOrders_ReturnsCustomerHistory()
    {
        using var context = CreateInMemoryDbContext();
        var user = new NguoiDung { TenDangNhap = "staff_order", MatKhau = "pass", HoTen = "Nhân viên bán", VaiTroId = 2 };
        context.NguoiDungs.Add(user);
        var paymentMethod = new HinhThucThanhToan { Id = 1, TenHinhThuc = "Tiền mặt" };
        context.HinhThucThanhToans.Add(paymentMethod);
        var cust = new KhachHang { TenKhachHang = "Nguyễn Ngọc Lan", SoDienThoai = "0901234567", DiemTichLuy = 250 };
        context.KhachHangs.Add(cust);
        await context.SaveChangesAsync();

        var o = new DonHang { KhachHangId = cust.Id, TrangThai = TrangThaiDonHang.DaThanhToan, NgayTao = DateTime.Now, TongTienThanhToan = 180000m, NguoiDungId = user.Id, HinhThucThanhToanId = 1 };
        context.DonHangs.Add(o);
        await context.SaveChangesAsync();

        var handler = new GetCustomerOrdersQueryHandler(context);
        var result = await handler.Handle(new GetCustomerOrdersQuery(cust.Id), CancellationToken.None);

        AssertTest(
            "UC09: GetCustomerOrders returns purchase history for customer",
            result.IsSuccess && result.Data != null && result.Data.Count == 1 && result.Data[0].Id == o.Id,
            $"Success: {result.IsSuccess}, Orders count: {result.Data?.Count}"
        );
    }

    private static async Task<(NguoiDung Manager, SanPham Product1, SanPham Product2)> SeedStockAuditFixturesAsync(AppDbContext context)
    {
        var manager = new NguoiDung { TenDangNhap = "manager_audit", MatKhau = "hash", HoTen = "Quản Lý Kho", SoDienThoai = "0909888999", VaiTroId = 1 };
        var dm = new DanhMucSanPham { TenDanhMuc = "Thú bông", TrangThai = "HoatDong" };
        var dvt = new DonViTinh { TenDonVi = "Cái" };
        context.NguoiDungs.Add(manager);
        context.DanhMucSanPhams.Add(dm);
        context.DonViTinhs.Add(dvt);
        await context.SaveChangesAsync();

        var p1 = new SanPham { MaSP = "GB001", TenSP = "Gấu bông Teddy", DanhMucId = dm.Id, DonViTinhId = dvt.Id, GiaNhap = 90000m, GiaBan = 180000m, SoLuongTon = 24 };
        var p2 = new SanPham { MaSP = "LY002", TenSP = "Ly sứ tặng Ba", DanhMucId = dm.Id, DonViTinhId = dvt.Id, GiaNhap = 45000m, GiaBan = 95000m, SoLuongTon = 36 };
        context.SanPhams.AddRange(p1, p2);
        await context.SaveChangesAsync();

        return (manager, p1, p2);
    }

    private static async Task Test_StockAudit_SaveDraft_DoesNotChangeStock()
    {
        using var context = CreateInMemoryDbContext();
        var (manager, p1, p2) = await SeedStockAuditFixturesAsync(context);

        var handler = new SaveDraftStockAuditCommandHandler(context);
        var res = await handler.Handle(new SaveDraftStockAuditCommand(manager.Id, new SaveStockAuditRequest
        {
            Items =
            [
                new StockAuditItemRequest { SanPhamId = p1.Id, SoTonThucTe = 21, LyDoChenhLech = "Thiếu 3 gấu" },
                new StockAuditItemRequest { SanPhamId = p2.Id, SoTonThucTe = 36 }
            ]
        }), CancellationToken.None);

        var p1After = await context.SanPhams.FindAsync(p1.Id);
        var p2After = await context.SanPhams.FindAsync(p2.Id);

        AssertTest(
            "UC13: SaveDraft saves DangKiemKe with accurate diff and does NOT change stock",
            res.IsSuccess &&
            res.StatusCode == 201 &&
            res.Data?.TrangThai == TrangThaiPhieuKiemKe.DangKiemKe &&
            res.Data.Items.First(i => i.SanPhamId == p1.Id).SoLuongChenhLech == -3 &&
            p1After?.SoLuongTon == 24 &&
            p2After?.SoLuongTon == 36,
            $"Success={res.IsSuccess}, Status={res.StatusCode}, State={res.Data?.TrangThai}, P1Stock={p1After?.SoLuongTon}"
        );
    }

    private static async Task Test_StockAudit_Balance_MissingReason_Returns400AndKeepsStock()
    {
        using var context = CreateInMemoryDbContext();
        var (manager, p1, _) = await SeedStockAuditFixturesAsync(context);

        var handler = new BalanceStockAuditCommandHandler(context);
        var res = await handler.Handle(new BalanceStockAuditCommand(manager.Id, new SaveStockAuditRequest
        {
            Items =
            [
                new StockAuditItemRequest { SanPhamId = p1.Id, SoTonThucTe = 20, LyDoChenhLech = "   " }
            ]
        }), CancellationToken.None);

        var p1After = await context.SanPhams.FindAsync(p1.Id);
        var auditCount = await context.PhieuKiemKes.CountAsync();

        AssertTest(
            "UC13: Balance fails with 400 when diff != 0 and reason is missing, keeping stock intact",
            !res.IsSuccess && res.StatusCode == 400 && p1After?.SoLuongTon == 24 && auditCount == 0,
            $"Success={res.IsSuccess}, Status={res.StatusCode}, P1Stock={p1After?.SoLuongTon}, AuditCount={auditCount}"
        );
    }

    private static async Task Test_StockAudit_Balance_FromDraft_UpdatesStockAndLocksAudit()
    {
        using var context = CreateInMemoryDbContext();
        var (manager, p1, p2) = await SeedStockAuditFixturesAsync(context);

        var draftHandler = new SaveDraftStockAuditCommandHandler(context);
        var draftRes = await draftHandler.Handle(new SaveDraftStockAuditCommand(manager.Id, new SaveStockAuditRequest
        {
            Items =
            [
                new StockAuditItemRequest { SanPhamId = p1.Id, SoTonThucTe = 22 },
                new StockAuditItemRequest { SanPhamId = p2.Id, SoTonThucTe = 38 }
            ]
        }), CancellationToken.None);

        var balanceHandler = new BalanceStockAuditCommandHandler(context);
        var balRes = await balanceHandler.Handle(new BalanceStockAuditCommand(manager.Id, new SaveStockAuditRequest
        {
            Id = draftRes.Data!.Id,
            Items =
            [
                new StockAuditItemRequest { SanPhamId = p1.Id, SoTonThucTe = 22, LyDoChenhLech = "Hao hụt trưng bày" },
                new StockAuditItemRequest { SanPhamId = p2.Id, SoTonThucTe = 38, LyDoChenhLech = "Kiểm sót đợt nhập trước" }
            ]
        }), CancellationToken.None);

        var relockRes = await balanceHandler.Handle(new BalanceStockAuditCommand(manager.Id, new SaveStockAuditRequest
        {
            Id = draftRes.Data.Id,
            Items =
            [
                new StockAuditItemRequest { SanPhamId = p1.Id, SoTonThucTe = 24, LyDoChenhLech = "Sửa lại" }
            ]
        }), CancellationToken.None);

        var p1After = await context.SanPhams.FindAsync(p1.Id);
        var p2After = await context.SanPhams.FindAsync(p2.Id);

        AssertTest(
            "UC13: Balance from draft updates stock to actual counts, sets DaCanBang, and locks against edits (409)",
            balRes.IsSuccess &&
            balRes.Data?.TrangThai == TrangThaiPhieuKiemKe.DaCanBang &&
            p1After?.SoLuongTon == 22 &&
            p2After?.SoLuongTon == 38 &&
            !relockRes.IsSuccess &&
            relockRes.StatusCode == 409,
            $"BalSuccess={balRes.IsSuccess}, State={balRes.Data?.TrangThai}, P1={p1After?.SoLuongTon}, P2={p2After?.SoLuongTon}, RelockCode={relockRes.StatusCode}"
        );
    }
}

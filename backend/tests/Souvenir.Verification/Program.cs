using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Souvenir.Application.DTOs.Users;
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

        Console.WriteLine("\n--- VERIFICATION SUITE FOR UC05 (VOUCHERS & DISCOUNT) ---");
        await Test_CheckDiscount_QUATANG10_ValidPercentDiscount();
        await Test_CheckDiscount_GIAM10K_ValidFixedAmountDiscount();
        await Test_CheckDiscount_ExpiredVoucher_ReturnsError();
        await Test_CheckDiscount_NonExistentVoucher_ReturnsError();

        Console.WriteLine("\n--- VERIFICATION SUITE FOR UC06 (DRAFT ORDERS / LƯU NHÁP) ---");
        await Test_CreateDraftOrder_Success_DoesNotDeductStock();
        await Test_GetDraftOrders_ReturnsDraftOrdersOnly();
        await Test_DeleteDraftOrder_RemovesOrderAndDetails();

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
}

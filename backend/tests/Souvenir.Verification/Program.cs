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
}

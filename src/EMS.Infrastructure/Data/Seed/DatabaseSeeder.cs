using EMS.Domain.Entities;
using EMS.Domain.Enums;
using EMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Only seed if tables are empty — safe to call on every startup
        await SeedLeaveTypesAsync(context);
        await SeedSuperAdminAsync(context);
    }

    private static async Task SeedLeaveTypesAsync(ApplicationDbContext context)
    {
        if (await context.LeaveTypes.AnyAsync()) return;

        var leaveTypes = new List<LeaveType>
        {
            new() { Name = "Annual Leave",   MaxDaysAllowed = 21, Description = "Yearly paid leave" },
            new() { Name = "Sick Leave",     MaxDaysAllowed = 10, Description = "Medical/illness leave" },
            new() { Name = "Casual Leave",   MaxDaysAllowed = 7,  Description = "Short personal leave" },
            new() { Name = "Maternity Leave",MaxDaysAllowed = 90, Description = "For new mothers" },
            new() { Name = "Paternity Leave",MaxDaysAllowed = 14, Description = "For new fathers" },
        };

        await context.LeaveTypes.AddRangeAsync(leaveTypes);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Leave types seeded.");
    }

    private static async Task SeedSuperAdminAsync(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin)) return;

        var superAdmin = new User
        {
            FirstName    = "Super",
            LastName     = "Admin",
            Email        = "superadmin@ems.com",
            // NEVER store plain text passwords — BCrypt hashes it
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role         = UserRole.SuperAdmin,
            IsActive     = true,
            PhoneNumber  = "0000000000"
        };

        await context.Users.AddAsync(superAdmin);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ SuperAdmin seeded. Email: superadmin@ems.com | Pass: Admin@123");
    }
}
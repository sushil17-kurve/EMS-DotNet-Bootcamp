using EMS.Application.Interfaces;
using EMS.Application.Interfaces.Repositories;
using EMS.Infrastructure.Data;
using EMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("EMS.Infrastructure")
            )
        );

        // Unit of Work — Scoped means one instance per HTTP request
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Individual repositories (UoW handles these,
        // but register separately for direct injection if ever needed)
        services.AddScoped<IUserRepository,         UserRepository>();
        services.AddScoped<IEmployeeRepository,     EmployeeRepository>();
        services.AddScoped<IDepartmentRepository,   DepartmentRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();

        return services;
    }
}
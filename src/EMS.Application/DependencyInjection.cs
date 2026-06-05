using EMS.Application.Interfaces.Services;
using EMS.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        services.AddScoped<IDashboardService, DashboardService>();  // ← Add

        return services;
    }
}
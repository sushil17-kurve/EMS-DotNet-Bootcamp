using AutoMapper;
using EMS.Application.Interfaces.Services;
using EMS.Application.Mappings;
using EMS.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(DepartmentProfile).Assembly);

        // Services
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();


        return services;
    }
}
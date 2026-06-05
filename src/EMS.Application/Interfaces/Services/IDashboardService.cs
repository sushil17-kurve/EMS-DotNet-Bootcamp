using EMS.Application.DTOs.Common;
using EMS.Application.DTOs.Dashboard;

namespace EMS.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<ApiResponseDto<DashboardStatsDto>> GetStatsAsync();
}
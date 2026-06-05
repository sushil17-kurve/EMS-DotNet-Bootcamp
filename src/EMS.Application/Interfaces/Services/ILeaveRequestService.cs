using EMS.Application.DTOs.Common;
using EMS.Application.DTOs.LeaveRequest;

namespace EMS.Application.Interfaces.Services;

public interface ILeaveRequestService
{
    Task<ApiResponseDto<IEnumerable<LeaveRequestDto>>> GetAllAsync();
    Task<ApiResponseDto<IEnumerable<LeaveRequestDto>>> GetMyLeavesAsync(int employeeId);
    Task<ApiResponseDto<LeaveRequestDto>> GetByIdAsync(int id);
    Task<ApiResponseDto<LeaveRequestDto>> CreateAsync(int employeeId, CreateLeaveRequestDto dto);
    Task<ApiResponseDto<LeaveRequestDto>> ReviewAsync(int id, int reviewerId, ReviewLeaveRequestDto dto);
    Task<ApiResponseDto<bool>> CancelAsync(int id, int employeeId);
    Task<ApiResponseDto<IEnumerable<LeaveTypeDto>>> GetLeaveTypesAsync();
    Task<ApiResponseDto<IEnumerable<LeaveBalanceDto>>> GetLeaveBalanceAsync(int employeeId);
}
using EMS.Application.DTOs.LeaveRequest;

namespace EMS.Application.Interfaces.Services;

public interface ILeaveRequestService
{
    Task<IEnumerable<LeaveRequestDto>> GetAllAsync();
    Task<LeaveRequestDto?> GetByIdAsync(int id);
}
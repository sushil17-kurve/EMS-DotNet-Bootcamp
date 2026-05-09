using EMS.Domain.Entities;
using EMS.Domain.Enums;

namespace EMS.Application.Interfaces.Repositories;

public interface ILeaveRequestRepository : IGenericRepository<LeaveRequest>
{
    Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<LeaveRequest>> GetAllWithDetailsAsync();
    Task<LeaveRequest?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveStatus status);

    Task<bool> HasOverlappingLeaveAsync(
        int employeeId,
        DateTime startDate,
        DateTime endDate,
        int? excludeRequestId = null);

    Task<int> GetUsedDaysThisYearAsync(int employeeId, int leaveTypeId);
}
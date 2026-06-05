using EMS.Domain.Entities;

namespace EMS.Application.Interfaces.Repositories;

public interface ILeaveTypeRepository : IGenericRepository<LeaveType>
{
    Task<IEnumerable<LeaveType>> GetAllActiveAsync();
}
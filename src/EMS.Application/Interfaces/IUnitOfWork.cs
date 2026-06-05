using EMS.Application.Interfaces.Repositories;

namespace EMS.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IEmployeeRepository Employees { get; }
    IDepartmentRepository Departments { get; }
    ILeaveRequestRepository LeaveRequests { get; }
    ILeaveTypeRepository LeaveTypes { get; }  // ← Add this

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
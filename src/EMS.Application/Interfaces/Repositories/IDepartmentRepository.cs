using EMS.Domain.Entities;

namespace EMS.Application.Interfaces.Repositories;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<IEnumerable<Department>> GetAllWithEmployeeCountAsync();
    Task<Department?> GetByIdWithEmployeesAsync(int id);
    Task<bool> HasEmployeesAsync(int departmentId);
}
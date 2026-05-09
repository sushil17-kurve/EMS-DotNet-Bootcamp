using EMS.Domain.Entities;

namespace EMS.Application.Interfaces.Repositories;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
    Task<IEnumerable<Employee>> GetAllWithDetailsAsync();
    Task<Employee?> GetByIdWithDetailsAsync(int id);
    Task<Employee?> GetByUserIdAsync(int userId);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId);
    Task<string> GenerateEmployeeCodeAsync();

    Task<(IEnumerable<Employee> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        int? departmentId = null,
        bool? isActive = null);
}
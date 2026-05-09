using EMS.Application.Interfaces.Repositories;
using EMS.Domain.Entities;
using EMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Employee>> GetAllWithDetailsAsync()
        => await _dbSet
            .Include(e => e.User)
            .Include(e => e.Department)
            .Where(e => e.IsActive)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

    public async Task<Employee?> GetByIdWithDetailsAsync(int id)
        => await _dbSet
            .Include(e => e.User)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Employee?> GetByUserIdAsync(int userId)
        => await _dbSet
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.UserId == userId);

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(int departmentId)
        => await _dbSet
            .Include(e => e.User)
            .Where(e => e.DepartmentId == departmentId && e.IsActive)
            .ToListAsync();

    public async Task<string> GenerateEmployeeCodeAsync()
    {
        // Get the highest existing code number
        var lastEmployee = await _dbSet
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync();

        if (lastEmployee == null) return "EMP-0001";

        // Parse "EMP-0042" → 42 → increment → "EMP-0043"
        var lastCode   = lastEmployee.EmployeeCode;
        var numberPart = lastCode.Split('-')[1];
        var nextNumber = int.Parse(numberPart) + 1;

        return $"EMP-{nextNumber:D4}"; // D4 = zero-padded to 4 digits
    }

    public async Task<(IEnumerable<Employee> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        int? departmentId = null,
        bool? isActive = null)
    {
        var query = _dbSet
            .Include(e => e.User)
            .Include(e => e.Department)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e =>
                e.User.FirstName.ToLower().Contains(term) ||
                e.User.LastName.ToLower().Contains(term)  ||
                e.User.Email.ToLower().Contains(term)     ||
                e.EmployeeCode.ToLower().Contains(term)   ||
                e.Designation.ToLower().Contains(term));
        }

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (isActive.HasValue)
            query = query.Where(e => e.IsActive == isActive.Value);

        // Get total BEFORE paging (for pagination metadata)
        var totalCount = await query.CountAsync();

        // Apply paging
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
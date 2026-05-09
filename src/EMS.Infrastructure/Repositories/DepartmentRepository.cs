using EMS.Application.Interfaces.Repositories;
using EMS.Domain.Entities;
using EMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Department>> GetAllWithEmployeeCountAsync()
        => await _dbSet
            .Include(d => d.Employees)
            .Where(d => d.IsActive)
            .ToListAsync();

    public async Task<Department?> GetByIdWithEmployeesAsync(int id)
        => await _dbSet
            .Include(d => d.Employees)
                .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<bool> HasEmployeesAsync(int departmentId)
        => await _context.Employees
            .AnyAsync(e => e.DepartmentId == departmentId && e.IsActive);
}
using EMS.Application.Interfaces.Repositories;
using EMS.Domain.Entities;
using EMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class LeaveTypeRepository : GenericRepository<LeaveType>, ILeaveTypeRepository
{
    public LeaveTypeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<LeaveType>> GetAllActiveAsync()
        => await _dbSet.OrderBy(lt => lt.Name).ToListAsync();
}
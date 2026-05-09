using EMS.Application.Interfaces.Repositories;
using EMS.Domain.Entities;
using EMS.Domain.Enums;
using EMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class LeaveRequestRepository : GenericRepository<LeaveRequest>, ILeaveRequestRepository
{
    public LeaveRequestRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId)
        => await _dbSet
            .Include(l => l.LeaveType)
            .Include(l => l.ReviewedBy)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.AppliedOn)
            .ToListAsync();

    public async Task<IEnumerable<LeaveRequest>> GetAllWithDetailsAsync()
        => await _dbSet
            .Include(l => l.Employee).ThenInclude(e => e.User)
            .Include(l => l.LeaveType)
            .Include(l => l.ReviewedBy)
            .OrderByDescending(l => l.AppliedOn)
            .ToListAsync();

    public async Task<LeaveRequest?> GetByIdWithDetailsAsync(int id)
        => await _dbSet
            .Include(l => l.Employee).ThenInclude(e => e.User)
            .Include(l => l.LeaveType)
            .Include(l => l.ReviewedBy)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<IEnumerable<LeaveRequest>> GetByStatusAsync(LeaveStatus status)
        => await _dbSet
            .Include(l => l.Employee).ThenInclude(e => e.User)
            .Include(l => l.LeaveType)
            .Where(l => l.Status == status)
            .ToListAsync();

    public async Task<bool> HasOverlappingLeaveAsync(
        int employeeId,
        DateTime startDate,
        DateTime endDate,
        int? excludeRequestId = null)
    {
        var query = _dbSet.Where(l =>
            l.EmployeeId == employeeId &&
            l.Status != LeaveStatus.Rejected &&
            l.Status != LeaveStatus.Cancelled &&
            l.StartDate <= endDate &&
            l.EndDate   >= startDate);

        if (excludeRequestId.HasValue)
            query = query.Where(l => l.Id != excludeRequestId.Value);

        return await query.AnyAsync();
    }

    public async Task<int> GetUsedDaysThisYearAsync(int employeeId, int leaveTypeId)
    {
        var yearStart = new DateTime(DateTime.UtcNow.Year, 1, 1);
        var yearEnd   = new DateTime(DateTime.UtcNow.Year, 12, 31);

        var leaves = await _dbSet
            .Where(l =>
                l.EmployeeId  == employeeId    &&
                l.LeaveTypeId == leaveTypeId   &&
                l.Status      == LeaveStatus.Approved &&
                l.StartDate   >= yearStart     &&
                l.EndDate     <= yearEnd)
            .ToListAsync();

        // Sum actual calendar days used
        return leaves.Sum(l => (l.EndDate - l.StartDate).Days + 1);
    }
}
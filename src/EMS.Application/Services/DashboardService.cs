using EMS.Application.DTOs.Common;
using EMS.Application.DTOs.Dashboard;
using EMS.Application.Interfaces;
using EMS.Application.Interfaces.Services;
using EMS.Domain.Enums;

namespace EMS.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;

    public DashboardService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponseDto<DashboardStatsDto>> GetStatsAsync()
    {
        // ── Fetch raw data ───────────────────────────────────────────────────
        var allEmployees = await _uow.Employees.GetAllWithDetailsAsync();
        var allDepartments = await _uow.Departments.GetAllWithEmployeeCountAsync();
        var allLeaves = await _uow.LeaveRequests.GetAllWithDetailsAsync();

        var employeeList = allEmployees.ToList();
        var leaveList = allLeaves.ToList();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        // ── Stats Cards ──────────────────────────────────────────────────────
        var stats = new DashboardStatsDto
        {
            TotalEmployees = employeeList.Count,
            ActiveEmployees = employeeList.Count(e => e.IsActive),
            TotalDepartments = allDepartments.Count(),

            PendingLeaveRequests = leaveList
                .Count(l => l.Status == LeaveStatus.Pending),

            NewJoineesThisMonth = employeeList
                .Count(e => e.DateOfJoining >= monthStart),

            // ── Department Headcount Chart ───────────────────────────────────
            DepartmentHeadcounts = allDepartments
                .Select(d => new DepartmentHeadcountDto
                {
                    DepartmentName = d.Name,
                    EmployeeCount = d.Employees.Count(e => e.IsActive)
                })
                .OrderByDescending(d => d.EmployeeCount)
                .ToList(),

            // ── Leave Status Pie Chart ───────────────────────────────────────
            LeaveStatusSummary = leaveList
                .GroupBy(l => l.Status.ToString())
                .Select(g => new LeaveStatusSummaryDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList(),

            // ── Monthly Joinings Bar Chart (last 6 months) ───────────────────
            MonthlyJoinings = Enumerable.Range(0, 6)
                .Select(i => now.AddMonths(-i))
                .Select(month => new MonthlyJoiningDto
                {
                    Month = month.ToString("MMM yyyy"),
                    Count = employeeList.Count(e =>
                        e.DateOfJoining.Year == month.Year &&
                        e.DateOfJoining.Month == month.Month)
                })
                .Reverse()
                .ToList(),

            // ── Recent Leave Requests Feed ───────────────────────────────────
            RecentLeaveRequests = leaveList
                .OrderByDescending(l => l.AppliedOn)
                .Take(10)
                .Select(l => new RecentLeaveRequestDto
                {
                    EmployeeName = l.Employee.User.FullName,
                    LeaveType = l.LeaveType.Name,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Status = l.Status.ToString(),
                    AppliedOn = l.AppliedOn
                })
                .ToList()
        };

        return ApiResponseDto<DashboardStatsDto>.Ok(stats);
    }
}
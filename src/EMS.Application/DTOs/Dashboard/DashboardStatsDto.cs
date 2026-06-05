namespace EMS.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    // Top-level numbers (the "cards" on the dashboard)
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int TodaysPresentCount { get; set; }
    public int NewJoineesThisMonth { get; set; }

    // Charts data
    public IEnumerable<DepartmentHeadcountDto> DepartmentHeadcounts { get; set; }
        = new List<DepartmentHeadcountDto>();

    public IEnumerable<LeaveStatusSummaryDto> LeaveStatusSummary { get; set; }
        = new List<LeaveStatusSummaryDto>();

    public IEnumerable<MonthlyJoiningDto> MonthlyJoinings { get; set; }
        = new List<MonthlyJoiningDto>();

    // Recent activity feed
    public IEnumerable<RecentLeaveRequestDto> RecentLeaveRequests { get; set; }
        = new List<RecentLeaveRequestDto>();
}

public class DepartmentHeadcountDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
}

public class LeaveStatusSummaryDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MonthlyJoiningDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentLeaveRequestDto
{
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedOn { get; set; }
}
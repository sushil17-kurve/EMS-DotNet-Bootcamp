using EMS.Domain.Common;
using EMS.Domain.Enums;

namespace EMS.Domain.Entities;

public class LeaveRequest : BaseEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? ReviewNote { get; set; }
    public DateTime AppliedOn { get; set; } = DateTime.UtcNow;

    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public int? ReviewedById { get; set; }

    public Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
    public User? ReviewedBy { get; set; }
}
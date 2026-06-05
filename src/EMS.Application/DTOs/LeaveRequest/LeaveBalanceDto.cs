namespace EMS.Application.DTOs.LeaveRequest;

public class LeaveBalanceDto
{
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public int TotalAllowed { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays => TotalAllowed - UsedDays;
}
namespace EMS.Application.DTOs.LeaveRequest;

public class LeaveTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxDaysAllowed { get; set; }
    public string? Description { get; set; }
}
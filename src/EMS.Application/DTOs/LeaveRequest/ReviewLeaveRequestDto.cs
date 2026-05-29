namespace EMS.Application.DTOs.LeaveRequest;

public class ReviewLeaveRequestDto
{
    public string Status { get; set; } = string.Empty;
    public string? ReviewNote { get; set; }
}
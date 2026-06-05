namespace EMS.Application.DTOs.LeaveRequest;

public class LeaveRequestDto
{
   
        public int Id { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string EmployeeCode { get; set; } = string.Empty;

        public string LeaveTypeName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int TotalDays { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? ReviewedByName { get; set; }

        public string? Comments { get; set; }
    
}
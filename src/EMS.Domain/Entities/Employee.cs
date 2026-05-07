using EMS.Domain.Common;
using EMS.Domain.Enums;

namespace EMS.Domain.Entities;

public class Employee : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public DateTime DateOfJoining { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public decimal Salary { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public int UserId { get; set; }
    public int DepartmentId { get; set; }

    public User User { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
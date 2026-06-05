namespace EMS.Application.DTOs.Employee;

public class CreateEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    public DateTime DateOfJoining { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string EmploymentType { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; internal set; }
    public string? Address { get; internal set; }
}
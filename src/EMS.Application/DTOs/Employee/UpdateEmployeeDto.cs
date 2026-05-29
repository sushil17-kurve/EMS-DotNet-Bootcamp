namespace EMS.Application.DTOs.Employee;

public class UpdateEmployeeDto
{
    public string Designation { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; }
}
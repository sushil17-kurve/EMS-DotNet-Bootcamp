namespace EMS.Application.DTOs.Employee;

public class UpdateEmployeeDto
{
    // User fields
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    // Employee fields
    public string Designation { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string EmploymentType { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}
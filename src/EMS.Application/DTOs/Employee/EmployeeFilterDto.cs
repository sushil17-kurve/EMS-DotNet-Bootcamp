namespace EMS.Application.DTOs.Employee;

// Query parameters for search/filter/pagination
public class EmployeeFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; } = true;
}
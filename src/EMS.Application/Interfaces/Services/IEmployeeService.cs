using EMS.Application.DTOs.Common;
using EMS.Application.DTOs.Employee;
using Microsoft.AspNetCore.Http;

namespace EMS.Application.Interfaces.Services;

public interface IEmployeeService
{
    Task<ApiResponseDto<PagedResultDto<EmployeeDto>>> GetPagedAsync(EmployeeFilterDto filter);
    Task<ApiResponseDto<EmployeeDto>> GetByIdAsync(int id);
    Task<ApiResponseDto<EmployeeDto>> CreateAsync(CreateEmployeeDto dto);
    Task<ApiResponseDto<EmployeeDto>> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task<ApiResponseDto<bool>> DeleteAsync(int id);
    Task<ApiResponseDto<bool>> ToggleStatusAsync(int id);
    Task<ApiResponseDto<EmployeeDto>> UploadProfilePhotoAsync(
                                                          int employeeId, IFormFile file);
}
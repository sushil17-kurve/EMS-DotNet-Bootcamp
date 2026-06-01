using EMS.Application.DTOs.Common;
using EMS.Application.DTOs.Department;

namespace EMS.Application.Interfaces.Services;

public interface IDepartmentService
{
    Task<ApiResponseDto<IEnumerable<DepartmentDto>>> GetAllAsync();
    Task<ApiResponseDto<DepartmentDto>> GetByIdAsync(int id);
    Task<ApiResponseDto<DepartmentDto>> CreateAsync(CreateDepartmentDto dto);
    Task<ApiResponseDto<DepartmentDto>> UpdateAsync(int id, UpdateDepartmentDto dto);
    Task<ApiResponseDto<bool>> DeleteAsync(int id);
}
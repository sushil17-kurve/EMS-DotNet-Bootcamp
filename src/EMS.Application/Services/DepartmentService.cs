using AutoMapper;
using EMS.Application.DTOs.Common;
using EMS.Application.DTOs.Department;
using EMS.Application.Interfaces;
using EMS.Application.Interfaces.Services;
using EMS.Domain.Entities;

namespace EMS.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public DepartmentService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponseDto<IEnumerable<DepartmentDto>>> GetAllAsync()
    {
        var departments = await _uow.Departments.GetAllWithEmployeeCountAsync();
        var dtos = _mapper.Map<IEnumerable<DepartmentDto>>(departments);
        return ApiResponseDto<IEnumerable<DepartmentDto>>.Ok(dtos);
    }

    public async Task<ApiResponseDto<DepartmentDto>> GetByIdAsync(int id)
    {
        var department = await _uow.Departments.GetByIdWithEmployeesAsync(id);
        if (department == null)
            return ApiResponseDto<DepartmentDto>.Fail($"Department with ID {id} not found.");

        return ApiResponseDto<DepartmentDto>.Ok(_mapper.Map<DepartmentDto>(department));
    }

    public async Task<ApiResponseDto<DepartmentDto>> CreateAsync(CreateDepartmentDto dto)
    {
        // Business rule: department name must be unique
        var exists = await _uow.Departments.ExistsAsync(
            d => d.Name.ToLower() == dto.Name.ToLower());

        if (exists)
            return ApiResponseDto<DepartmentDto>.Fail(
                $"Department '{dto.Name}' already exists.");

        var department = _mapper.Map<Department>(dto);
        await _uow.Departments.AddAsync(department);
        await _uow.SaveChangesAsync();

        return ApiResponseDto<DepartmentDto>.Ok(
            _mapper.Map<DepartmentDto>(department),
            "Department created successfully.");
    }

    public async Task<ApiResponseDto<DepartmentDto>> UpdateAsync(int id, UpdateDepartmentDto dto)
    {
        var department = await _uow.Departments.GetByIdAsync(id);
        if (department == null)
            return ApiResponseDto<DepartmentDto>.Fail($"Department with ID {id} not found.");

        // Check name uniqueness excluding current record
        var nameExists = await _uow.Departments.ExistsAsync(
            d => d.Name.ToLower() == dto.Name.ToLower() && d.Id != id);

        if (nameExists)
            return ApiResponseDto<DepartmentDto>.Fail(
                $"Department name '{dto.Name}' is already taken.");

        _mapper.Map(dto, department);  // Map DTO into existing entity
        department.UpdatedAt = DateTime.UtcNow;
        _uow.Departments.Update(department);
        await _uow.SaveChangesAsync();

        return ApiResponseDto<DepartmentDto>.Ok(
            _mapper.Map<DepartmentDto>(department),
            "Department updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
    {
        var department = await _uow.Departments.GetByIdAsync(id);
        if (department == null)
            return ApiResponseDto<bool>.Fail($"Department with ID {id} not found.");

        // Business rule: can't delete a department with active employees
        var hasEmployees = await _uow.Departments.HasEmployeesAsync(id);
        if (hasEmployees)
            return ApiResponseDto<bool>.Fail(
                "Cannot delete department with active employees. " +
                "Reassign or deactivate employees first.");

        // Soft delete — never hard delete in production
        department.IsActive = false;
        department.UpdatedAt = DateTime.UtcNow;
        _uow.Departments.Update(department);
        await _uow.SaveChangesAsync();

        return ApiResponseDto<bool>.Ok(true, "Department deleted successfully.");
    }
}
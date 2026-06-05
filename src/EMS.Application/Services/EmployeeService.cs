using AutoMapper;
using EMS.Application.DTOs.Common;
using EMS.Application.DTOs.Employee;
using EMS.Application.Interfaces;
using EMS.Application.Interfaces.Services;  // ← IFileService now lives here
using EMS.Domain.Entities;
using EMS.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EMS.Application.Services;
public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFileService _fileService;

    public EmployeeService(
        IUnitOfWork uow,
        IMapper mapper,
        IFileService fileService)
    {
        _uow = uow;
        _mapper = mapper;
        _fileService = fileService;
    }

    // ── GET PAGED ────────────────────────────────────────────────────────────
    public async Task<ApiResponseDto<PagedResultDto<EmployeeDto>>> GetPagedAsync(
        EmployeeFilterDto filter)
    {
        var (items, totalCount) = await _uow.Employees.GetPagedAsync(
            filter.Page,
            filter.PageSize,
            filter.SearchTerm,
            filter.DepartmentId,
            filter.IsActive);

        var result = new PagedResultDto<EmployeeDto>
        {
            Items = _mapper.Map<IEnumerable<EmployeeDto>>(items),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        return ApiResponseDto<PagedResultDto<EmployeeDto>>.Ok(result);
    }

    // ── GET BY ID ────────────────────────────────────────────────────────────
    public async Task<ApiResponseDto<EmployeeDto>> GetByIdAsync(int id)
    {
        var employee = await _uow.Employees.GetByIdWithDetailsAsync(id);

        if (employee == null)
            return ApiResponseDto<EmployeeDto>.Fail(
                $"Employee with ID {id} not found.");

        return ApiResponseDto<EmployeeDto>.Ok(
            _mapper.Map<EmployeeDto>(employee));
    }

    // ── CREATE ───────────────────────────────────────────────────────────────
    public async Task<ApiResponseDto<EmployeeDto>> CreateAsync(CreateEmployeeDto dto)
    {
        // Rule 1: Email must be unique
        var emailExists = await _uow.Users.ExistsAsync(
            u => u.Email.ToLower() == dto.Email.ToLower());

        if (emailExists)
            return ApiResponseDto<EmployeeDto>.Fail(
                $"Email '{dto.Email}' is already registered.");

        // Rule 2: Department must exist and be active
        var deptExists = await _uow.Departments.ExistsAsync(
            d => d.Id == dto.DepartmentId && d.IsActive);

        if (!deptExists)
            return ApiResponseDto<EmployeeDto>.Fail(
                "Selected department does not exist or is inactive.");

        // Rule 3: Parse EmploymentType enum safely
        if (!Enum.TryParse<EmploymentType>(dto.EmploymentType, out var empType))
            return ApiResponseDto<EmployeeDto>.Fail(
                $"Invalid employment type '{dto.EmploymentType}'. " +
                $"Valid values: {string.Join(", ", Enum.GetNames<EmploymentType>())}");

        // Step 1: Create User account
        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            Role = UserRole.Employee,
            IsActive = true
        };

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync(); // Save now to get user.Id

        // Step 2: Generate unique employee code
        var code = await _uow.Employees.GenerateEmployeeCodeAsync();

        // Step 3: Create Employee linked to User
        var employee = new Employee
        {
            UserId = user.Id,
            EmployeeCode = code,
            Designation = dto.Designation.Trim(),
            DepartmentId = dto.DepartmentId,
            EmploymentType = empType,
            Salary = dto.Salary,
            DateOfJoining = dto.DateOfJoining,
            DateOfBirth = dto.DateOfBirth,
            Address = dto.Address?.Trim(),
            IsActive = true
        };

        await _uow.Employees.AddAsync(employee);
        await _uow.SaveChangesAsync();

        // Reload with full navigation properties for clean DTO mapping
        var created = await _uow.Employees.GetByIdWithDetailsAsync(employee.Id);

        return ApiResponseDto<EmployeeDto>.Ok(
            _mapper.Map<EmployeeDto>(created),
            "Employee created successfully.");
    }

    // ── UPDATE ───────────────────────────────────────────────────────────────
    public async Task<ApiResponseDto<EmployeeDto>> UpdateAsync(
        int id, UpdateEmployeeDto dto)
    {
        var employee = await _uow.Employees.GetByIdWithDetailsAsync(id);

        if (employee == null)
            return ApiResponseDto<EmployeeDto>.Fail(
                $"Employee with ID {id} not found.");

        if (!Enum.TryParse<EmploymentType>(dto.EmploymentType, out var empType))
            return ApiResponseDto<EmployeeDto>.Fail(
                $"Invalid employment type '{dto.EmploymentType}'.");

        // Update User fields
        employee.User.FirstName = dto.FirstName.Trim();
        employee.User.LastName = dto.LastName.Trim();
        employee.User.PhoneNumber = dto.PhoneNumber;
        employee.User.IsActive = dto.IsActive;
        employee.User.UpdatedAt = DateTime.UtcNow;
        _uow.Users.Update(employee.User);

        // Update Employee fields
        employee.Designation = dto.Designation.Trim();
        employee.DepartmentId = dto.DepartmentId;
        employee.EmploymentType = empType;
        employee.Salary = dto.Salary;
        employee.DateOfBirth = dto.DateOfBirth;
        employee.Address = dto.Address?.Trim();
        employee.IsActive = dto.IsActive;
        employee.UpdatedAt = DateTime.UtcNow;
        _uow.Employees.Update(employee);

        await _uow.SaveChangesAsync();

        // Reload fresh data for response
        var updated = await _uow.Employees.GetByIdWithDetailsAsync(id);

        return ApiResponseDto<EmployeeDto>.Ok(
            _mapper.Map<EmployeeDto>(updated),
            "Employee updated successfully.");
    }

    // ── DELETE (Soft) ────────────────────────────────────────────────────────
    public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
    {
        var employee = await _uow.Employees.GetByIdWithDetailsAsync(id);

        if (employee == null)
            return ApiResponseDto<bool>.Fail(
                $"Employee with ID {id} not found.");

        // Soft delete — never hard delete employee records in production
        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.User.IsActive = false;
        employee.User.UpdatedAt = DateTime.UtcNow;

        _uow.Employees.Update(employee);
        _uow.Users.Update(employee.User);
        await _uow.SaveChangesAsync();

        return ApiResponseDto<bool>.Ok(true,
            "Employee deactivated successfully.");
    }

    // ── TOGGLE STATUS ────────────────────────────────────────────────────────
    public async Task<ApiResponseDto<bool>> ToggleStatusAsync(int id)
    {
        var employee = await _uow.Employees.GetByIdWithDetailsAsync(id);

        if (employee == null)
            return ApiResponseDto<bool>.Fail(
                $"Employee with ID {id} not found.");

        // Flip both employee and user active status together
        employee.IsActive = !employee.IsActive;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.User.IsActive = employee.IsActive;
        employee.User.UpdatedAt = DateTime.UtcNow;

        _uow.Employees.Update(employee);
        _uow.Users.Update(employee.User);
        await _uow.SaveChangesAsync();

        var status = employee.IsActive ? "activated" : "deactivated";

        return ApiResponseDto<bool>.Ok(true,
            $"Employee {status} successfully.");
    }

    // ── UPLOAD PROFILE PHOTO ─────────────────────────────────────────────────
    public async Task<ApiResponseDto<EmployeeDto>> UploadProfilePhotoAsync(
        int employeeId, IFormFile file)
    {
        var employee = await _uow.Employees.GetByIdWithDetailsAsync(employeeId);

        if (employee == null)
            return ApiResponseDto<EmployeeDto>.Fail("Employee not found.");

        // Validate file (type, size, extension)
        if (!_fileService.IsValidImageFile(file))
            return ApiResponseDto<EmployeeDto>.Fail(
                "Invalid file. Please upload a JPG, PNG, or WebP image under 5MB.");

        // Delete the old photo from disk if it exists
        _fileService.DeleteFile(employee.User.ProfilePhotoPath);

        // Save new file and get the relative URL path
        var photoPath = await _fileService.SaveProfilePhotoAsync(
            file, employee.EmployeeCode);

        employee.User.ProfilePhotoPath = photoPath;
        employee.User.UpdatedAt = DateTime.UtcNow;
        _uow.Users.Update(employee.User);
        await _uow.SaveChangesAsync();

        return ApiResponseDto<EmployeeDto>.Ok(
            _mapper.Map<EmployeeDto>(employee),
            "Profile photo uploaded successfully.");
    }
}
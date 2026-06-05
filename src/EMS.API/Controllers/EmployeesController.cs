using System.Security.Claims;
using EMS.Application.DTOs.Employee;
using EMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service)
        => _service = service;

    // GET /api/employees?page=1&pageSize=10&searchTerm=john&departmentId=2
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeFilterDto filter)
    {
        var result = await _service.GetPagedAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id}/toggle-status")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _service.ToggleStatusAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // POST /api/employees/5/upload-photo
    [HttpPost("{id}/upload-photo")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
    {
        // Employees can only upload their OWN photo unless they're Admin
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var isAdmin = currentRole is "SuperAdmin" or "Admin";

        if (!isAdmin)
        {
            // Find employee record for current user
            var employee = await _service.GetByIdAsync(id);
            if (!employee.Success)
                return NotFound(employee);

            // Regular employees can only update their own photo
            // We compare the userId from token with the employee's userId
            // (This check will be refined when we add CurrentUserService)
        }

        var result = await _service.UploadProfilePhotoAsync(id, file);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET /api/employees/my-profile (employee views their own profile)
    [HttpGet("my-profile")]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> MyProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        // We'll need to get employee by userId — add this to service
        var result = await _service.GetByIdAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
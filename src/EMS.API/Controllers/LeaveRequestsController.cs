using System.Security.Claims;
using EMS.Application.DTOs.LeaveRequest;
using EMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _service;

    public LeaveRequestsController(ILeaveRequestService service)
        => _service = service;

    // Admin: see all requests
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    // Employee: see only their own requests
    [HttpGet("my-leaves/{employeeId}")]
    public async Task<IActionResult> GetMyLeaves(int employeeId)
        => Ok(await _service.GetMyLeavesAsync(employeeId));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto dto)
    {
        // Get employeeId from token claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        // Get employee record for this user
        var result = await _service.CreateAsync(userId, dto);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    // Admin approves or rejects
    [HttpPatch("{id}/review")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewLeaveRequestDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var reviewerId))
            return Unauthorized();

        var result = await _service.ReviewAsync(id, reviewerId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Employee cancels their own request
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var employeeId))
            return Unauthorized();

        var result = await _service.CancelAsync(id, employeeId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Get all leave types (for dropdown in frontend)
    [HttpGet("leave-types")]
    public async Task<IActionResult> GetLeaveTypes()
        => Ok(await _service.GetLeaveTypesAsync());

    // Get leave balance for an employee
    [HttpGet("balance/{employeeId}")]
    public async Task<IActionResult> GetBalance(int employeeId)
        => Ok(await _service.GetLeaveBalanceAsync(employeeId));
}
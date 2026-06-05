using AutoMapper;
using EMS.Application.DTOs.Common;
using EMS.Application.DTOs.LeaveRequest;
using EMS.Application.Interfaces;
using EMS.Application.Interfaces.Services;
using EMS.Domain.Entities;
using EMS.Domain.Enums;

namespace EMS.Application.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public LeaveRequestService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<ApiResponseDto<IEnumerable<LeaveRequestDto>>> GetAllAsync()
    {
        var leaves = await _uow.LeaveRequests.GetAllWithDetailsAsync();
        return ApiResponseDto<IEnumerable<LeaveRequestDto>>.Ok(
            _mapper.Map<IEnumerable<LeaveRequestDto>>(leaves));
    }

    public async Task<ApiResponseDto<IEnumerable<LeaveRequestDto>>> GetMyLeavesAsync(
        int employeeId)
    {
        var leaves = await _uow.LeaveRequests.GetByEmployeeIdAsync(employeeId);
        return ApiResponseDto<IEnumerable<LeaveRequestDto>>.Ok(
            _mapper.Map<IEnumerable<LeaveRequestDto>>(leaves));
    }

    public async Task<ApiResponseDto<LeaveRequestDto>> GetByIdAsync(int id)
    {
        var leave = await _uow.LeaveRequests.GetByIdWithDetailsAsync(id);
        if (leave == null)
            return ApiResponseDto<LeaveRequestDto>.Fail("Leave request not found.");

        return ApiResponseDto<LeaveRequestDto>.Ok(
            _mapper.Map<LeaveRequestDto>(leave));
    }

    public async Task<ApiResponseDto<LeaveRequestDto>> CreateAsync(
        int userId, CreateLeaveRequestDto dto)
    {
        var employee = await _uow.Employees.GetByUserIdAsync(userId);

        if (employee == null)
            return ApiResponseDto<LeaveRequestDto>.Fail(
                "Employee record not found.");

        var employeeId = employee.Id;
        // Rule 1: End date must be on or after start date
        if (dto.EndDate.Date < dto.StartDate.Date)
            return ApiResponseDto<LeaveRequestDto>.Fail(
                "End date must be on or after start date.");

        // Rule 2: Cannot apply for past dates
        if (dto.StartDate.Date < DateTime.UtcNow.Date)
            return ApiResponseDto<LeaveRequestDto>.Fail(
                "Cannot apply for leave on past dates.");

        // Rule 3: Leave type must exist
        var leaveType = await _uow.LeaveTypes.GetByIdAsync(dto.LeaveTypeId);
        if (leaveType == null)
            return ApiResponseDto<LeaveRequestDto>.Fail("Invalid leave type selected.");

        // Rule 4: Check leave balance
        var usedDays = await _uow.LeaveRequests
            .GetUsedDaysThisYearAsync(employeeId, dto.LeaveTypeId);

        var requestedDays = (dto.EndDate.Date - dto.StartDate.Date).Days + 1;

        if (usedDays + requestedDays > leaveType.MaxDaysAllowed)
            return ApiResponseDto<LeaveRequestDto>.Fail(
                $"Insufficient leave balance. " +
                $"Used: {usedDays} days, " +
                $"Requested: {requestedDays} days, " +
                $"Maximum allowed: {leaveType.MaxDaysAllowed} days.");

        // Rule 5: No overlapping pending/approved leaves
        var hasOverlap = await _uow.LeaveRequests.HasOverlappingLeaveAsync(
            employeeId, dto.StartDate, dto.EndDate);

        if (hasOverlap)
            return ApiResponseDto<LeaveRequestDto>.Fail(
                "You already have a leave request for the selected dates.");

        var request = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveTypeId = dto.LeaveTypeId,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            Reason = dto.Reason.Trim(),
            Status = LeaveStatus.Pending,
            AppliedOn = DateTime.UtcNow
        };

        await _uow.LeaveRequests.AddAsync(request);
        await _uow.SaveChangesAsync();

        var created = await _uow.LeaveRequests.GetByIdWithDetailsAsync(request.Id);
        return ApiResponseDto<LeaveRequestDto>.Ok(
            _mapper.Map<LeaveRequestDto>(created),
            "Leave request submitted successfully.");
    }

    public async Task<ApiResponseDto<LeaveRequestDto>> ReviewAsync(
        int id, int reviewerId, ReviewLeaveRequestDto dto)
    {
        var leave = await _uow.LeaveRequests.GetByIdWithDetailsAsync(id);
        if (leave == null)
            return ApiResponseDto<LeaveRequestDto>.Fail("Leave request not found.");

        if (leave.Status != LeaveStatus.Pending)
            return ApiResponseDto<LeaveRequestDto>.Fail(
                $"This request is already {leave.Status}. Only pending requests can be reviewed.");

        if (!Enum.TryParse<LeaveStatus>(dto.Action, ignoreCase: true, out var newStatus)
            || (newStatus != LeaveStatus.Approved && newStatus != LeaveStatus.Rejected))
            return ApiResponseDto<LeaveRequestDto>.Fail(
                "Action must be 'Approved' or 'Rejected'.");

        leave.Status = newStatus;
        leave.ReviewNote = dto.ReviewNote?.Trim();
        leave.ReviewedById = reviewerId;
        leave.UpdatedAt = DateTime.UtcNow;

        _uow.LeaveRequests.Update(leave);
        await _uow.SaveChangesAsync();

        var updated = await _uow.LeaveRequests.GetByIdWithDetailsAsync(id);
        return ApiResponseDto<LeaveRequestDto>.Ok(
            _mapper.Map<LeaveRequestDto>(updated),
            $"Leave request {newStatus.ToString().ToLower()}.");
    }

    public async Task<ApiResponseDto<bool>> CancelAsync(int id, int employeeId)
    {
        var leave = await _uow.LeaveRequests.GetByIdAsync(id);
        if (leave == null)
            return ApiResponseDto<bool>.Fail("Leave request not found.");

        if (leave.EmployeeId != employeeId)
            return ApiResponseDto<bool>.Fail(
                "You can only cancel your own leave requests.");

        if (leave.Status != LeaveStatus.Pending)
            return ApiResponseDto<bool>.Fail(
                "Only pending requests can be cancelled.");

        leave.Status = LeaveStatus.Cancelled;
        leave.UpdatedAt = DateTime.UtcNow;

        _uow.LeaveRequests.Update(leave);
        await _uow.SaveChangesAsync();

        return ApiResponseDto<bool>.Ok(true, "Leave request cancelled successfully.");
    }

    // ── Leave Types ──────────────────────────────────────────────────────────
    public async Task<ApiResponseDto<IEnumerable<LeaveTypeDto>>> GetLeaveTypesAsync()
    {
        var types = await _uow.LeaveTypes.GetAllActiveAsync();
        return ApiResponseDto<IEnumerable<LeaveTypeDto>>.Ok(
            _mapper.Map<IEnumerable<LeaveTypeDto>>(types));
    }

    public async Task<ApiResponseDto<IEnumerable<LeaveBalanceDto>>> GetLeaveBalanceAsync(
        int employeeId)
    {
        var leaveTypes = await _uow.LeaveTypes.GetAllActiveAsync();
        var balances = new List<LeaveBalanceDto>();

        foreach (var lt in leaveTypes)
        {
            var usedDays = await _uow.LeaveRequests
                .GetUsedDaysThisYearAsync(employeeId, lt.Id);

            balances.Add(new LeaveBalanceDto
            {
                LeaveTypeId = lt.Id,
                LeaveTypeName = lt.Name,
                TotalAllowed = lt.MaxDaysAllowed,
                UsedDays = usedDays
            });
        }

        return ApiResponseDto<IEnumerable<LeaveBalanceDto>>.Ok(balances);
    }
}
using AutoMapper;
using EMS.Application.DTOs.LeaveRequest;
using EMS.Application.Interfaces;
using EMS.Application.Interfaces.Services;

namespace EMS.Application.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LeaveRequestService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetAllAsync()
    {
        var leaveRequests =
            await _unitOfWork.LeaveRequests.GetAllWithDetailsAsync();

        return _mapper.Map<IEnumerable<LeaveRequestDto>>(leaveRequests);
    }

    public async Task<LeaveRequestDto?> GetByIdAsync(int id)
    {
        var leaveRequest =
            await _unitOfWork.LeaveRequests.GetByIdWithDetailsAsync(id);

        return leaveRequest == null
            ? null
            : _mapper.Map<LeaveRequestDto>(leaveRequest);
    }
}
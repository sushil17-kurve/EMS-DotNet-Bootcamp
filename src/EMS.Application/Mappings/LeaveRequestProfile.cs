using AutoMapper;
using EMS.Application.DTOs.LeaveRequest;
using EMS.Domain.Entities;

namespace EMS.Application.Mappings;

public class LeaveRequestProfile : Profile
{
    public LeaveRequestProfile()
    {
        // LeaveType → LeaveTypeDto
        CreateMap<LeaveType, LeaveTypeDto>();

        // LeaveRequest → LeaveRequestDto
        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(dest => dest.EmployeeName,
                opt => opt.MapFrom(src => src.Employee.User.FullName))
            .ForMember(dest => dest.EmployeeCode,
                opt => opt.MapFrom(src => src.Employee.EmployeeCode))
            .ForMember(dest => dest.LeaveTypeName,
                opt => opt.MapFrom(src => src.LeaveType.Name))
            .ForMember(dest => dest.ReviewedByName,
                opt => opt.MapFrom(src =>
                    src.ReviewedBy != null ? src.ReviewedBy.FullName : null))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalDays,
                opt => opt.MapFrom(src =>
                    (src.EndDate - src.StartDate).Days + 1));

        CreateMap<CreateLeaveRequestDto, LeaveRequest>();
    }
}
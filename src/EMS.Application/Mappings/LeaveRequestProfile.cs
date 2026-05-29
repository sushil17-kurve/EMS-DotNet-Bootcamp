using AutoMapper;
using EMS.Application.DTOs.LeaveRequest;
using EMS.Domain.Entities;

namespace EMS.Application.Mappings;

public class LeaveRequestProfile : Profile
{
    public LeaveRequestProfile()
    {
        CreateMap<LeaveRequest, LeaveRequestDto>()
            .ForMember(dest => dest.EmployeeName,
                opt => opt.MapFrom(src =>
                    src.Employee.User.FirstName + " " +
                    src.Employee.User.LastName))
            .ForMember(dest => dest.LeaveTypeName,
                opt => opt.MapFrom(src => src.LeaveType.Name))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
using AutoMapper;
using EMS.Application.DTOs.Employee;
using EMS.Domain.Entities;

namespace EMS.Application.Mappings;

public class EmployeeProfile : Profile
{
    public EmployeeProfile()
    {
        CreateMap<Employee, EmployeeDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src =>
                    src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src => src.Department.Name));
    }
}
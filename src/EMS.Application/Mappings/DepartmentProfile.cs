using AutoMapper;
using EMS.Application.DTOs.Department;
using EMS.Domain.Entities;

namespace EMS.Application.Mappings;

public class DepartmentProfile : Profile
{
    public DepartmentProfile()
    {
        // Entity → DTO (for GET responses)
        CreateMap<Department, DepartmentDto>()
            .ForMember(dest => dest.EmployeeCount,
                opt => opt.MapFrom(src => src.Employees.Count(e => e.IsActive)));

        // DTO → Entity (for POST/create)
        CreateMap<CreateDepartmentDto, Department>();

        // DTO → Entity (for PUT/update)
        CreateMap<UpdateDepartmentDto, Department>();
    }
}
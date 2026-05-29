using AutoMapper;
using EMS.Application.DTOs.Department;
using EMS.Application.Interfaces;
using EMS.Application.Interfaces.Services;
using EMS.Domain.Entities;

namespace EMS.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DepartmentService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllAsync()
    {
        var departments =
            await _unitOfWork.Departments.GetAllAsync();

        return _mapper.Map<IEnumerable<DepartmentDto>>(departments);
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var department =
            await _unitOfWork.Departments.GetByIdAsync(id);

        return department == null
            ? null
            : _mapper.Map<DepartmentDto>(department);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
    {
        // Replace 'Departments' with the correct entity type, likely 'Department'.
        // Assuming your entity is named 'Department' (singular), update the following line:

        var department = _mapper.Map<Department>(dto);

        // Add this using directive at the top of the file (replace or add as needed)
        
        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateDepartmentDto dto)
    {
        var department =
            await _unitOfWork.Departments.GetByIdAsync(id);

        if (department == null)
            return false;

        _mapper.Map(dto, department);

        _unitOfWork.Departments.Update(department);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department =
            await _unitOfWork.Departments.GetByIdAsync(id);

        if (department == null)
            return false;

        _unitOfWork.Departments.Remove(department);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
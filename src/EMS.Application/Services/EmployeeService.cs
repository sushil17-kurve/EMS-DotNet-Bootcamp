using AutoMapper;
using EMS.Application.DTOs.Employee;
using EMS.Application.Interfaces;
using EMS.Application.Interfaces.Services;

namespace EMS.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EmployeeService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
    {
        var employees =
            await _unitOfWork.Employees.GetAllWithDetailsAsync();

        return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var employee =
            await _unitOfWork.Employees.GetByIdWithDetailsAsync(id);

        return employee == null
            ? null
            : _mapper.Map<EmployeeDto>(employee);
    }
}
using EMS.Application.DTOs.Employee;
using EMS.Domain.Enums;
using FluentValidation;

namespace EMS.Application.Validators;

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeDto>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Designation)
            .NotEmpty().WithMessage("Designation is required.")
            .MaximumLength(150);

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Please select a valid department.");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0.");

        RuleFor(x => x.EmploymentType)
            .NotEmpty()
            .Must(type => Enum.TryParse<EmploymentType>(type, out _))
                .WithMessage($"Valid values: " +
                    $"{string.Join(", ", Enum.GetNames<EmploymentType>())}");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today.AddYears(-18))
                .WithMessage("Employee must be at least 18 years old.")
            .When(x => x.DateOfBirth.HasValue);
    }
}
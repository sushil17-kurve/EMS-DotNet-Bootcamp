using EMS.Application.DTOs.Employee;
using EMS.Domain.Enums;
using FluentValidation;

namespace EMS.Application.Validators;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Must contain an uppercase letter.")
            .Matches(@"\d").WithMessage("Must contain a number.")
            .Matches(@"[!@#$%^&*]").WithMessage("Must contain a special character.");

        RuleFor(x => x.Designation)
            .NotEmpty().WithMessage("Designation is required.")
            .MaximumLength(150);

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Please select a valid department.");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than 0.")
            .LessThan(10_000_000).WithMessage("Salary value seems incorrect.");

        RuleFor(x => x.DateOfJoining)
            .NotEmpty().WithMessage("Date of joining is required.")
            .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Date of joining cannot be in the future.");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today.AddYears(-18))
                .WithMessage("Employee must be at least 18 years old.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.EmploymentType)
            .NotEmpty()
            .Must(type => Enum.TryParse<EmploymentType>(type, out _))
                .WithMessage($"Employment type must be one of: " +
                    $"{string.Join(", ", Enum.GetNames<EmploymentType>())}");
    }
}
using EMS.Application.DTOs.Department;
using FluentValidation;

namespace EMS.Application.Validators;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentDto>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Department name is required.")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters.")
            .MaximumLength(150).WithMessage("Name cannot exceed 150 characters.")
            .Matches(@"^[a-zA-Z0-9\s\-&]+$")
                .WithMessage("Name can only contain letters, numbers, spaces, hyphens and &.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
using EMS.Application.DTOs.LeaveRequest;
using FluentValidation;

namespace EMS.Application.Validators;

public class LeaveRequestValidator : AbstractValidator<CreateLeaveRequestDto>
{
    public LeaveRequestValidator()
    {
        RuleFor(x => x.LeaveTypeId)
            .GreaterThan(0).WithMessage("Please select a leave type.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .GreaterThanOrEqualTo(DateTime.Today)
                .WithMessage("Start date cannot be in the past.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date must be on or after start date.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MinimumLength(10).WithMessage("Please provide a more detailed reason (min 10 chars).")
            .MaximumLength(1000);
    }
}
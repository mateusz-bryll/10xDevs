using FluentValidation;
using TaskFlow.Modules.WorkItems.Application.Requests;
using TaskFlow.Modules.WorkItems.Domain.Enums;

namespace TaskFlow.Modules.WorkItems.Application.Validators;

/// <summary>
/// Validator for UpdateWorkItemStatusRequest enforcing business rules.
/// Rules:
/// - Status: Required, must be one of: "New", "Ready", "InProgress", "Done" (case-insensitive)
/// - All status transitions are allowed (no workflow restrictions per PRD)
/// </summary>
public sealed class UpdateWorkItemStatusRequestValidator : AbstractValidator<UpdateWorkItemStatusRequest>
{
    public UpdateWorkItemStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(BeValidStatus)
            .WithMessage("Status must be New, Ready, InProgress, or Done");
    }

    private static bool BeValidStatus(string status)
    {
        return Enum.TryParse<WorkItemStatus>(status, ignoreCase: true, out _);
    }
}

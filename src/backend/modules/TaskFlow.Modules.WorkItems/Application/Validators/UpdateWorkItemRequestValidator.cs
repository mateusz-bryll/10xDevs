using FluentValidation;
using TaskFlow.Modules.WorkItems.Application.Requests;

namespace TaskFlow.Modules.WorkItems.Application.Validators;

/// <summary>
/// Validator for UpdateWorkItemRequest enforcing business rules.
/// Rules:
/// - Title: Required, max 200 characters, not empty or whitespace
/// - Description: Optional, max 5000 characters
/// - ParentId: Hierarchy validation performed in service layer based on existing work item type
/// </summary>
public sealed class UpdateWorkItemRequestValidator : AbstractValidator<UpdateWorkItemRequest>
{
    public UpdateWorkItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Work item title is required and cannot be empty or whitespace only")
            .MaximumLength(200)
            .WithMessage("Work item title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .WithMessage("Work item description cannot exceed 5000 characters")
            .When(x => x.Description is not null);

        // Note: ParentId hierarchy validation is complex and depends on the existing work item's type,
        // so it's performed in the service layer rather than here
    }
}

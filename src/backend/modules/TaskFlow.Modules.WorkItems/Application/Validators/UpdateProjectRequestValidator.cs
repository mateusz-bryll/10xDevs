using FluentValidation;
using TaskFlow.Modules.WorkItems.Application.Requests;

namespace TaskFlow.Modules.WorkItems.Application.Validators;

/// <summary>
/// Validator for UpdateProjectRequest enforcing business rules.
/// Rules:
/// - Name: Required, max 200 characters, not empty or whitespace
/// - Description: Optional, max 2000 characters
/// </summary>
public sealed class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Project name is required and cannot be empty or whitespace only")
            .MaximumLength(200)
            .WithMessage("Project name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Project description cannot exceed 2000 characters")
            .When(x => x.Description is not null);
    }
}

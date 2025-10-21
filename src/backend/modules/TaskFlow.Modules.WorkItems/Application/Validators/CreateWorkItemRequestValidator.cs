using FluentValidation;
using TaskFlow.Modules.WorkItems.Application.Requests;
using TaskFlow.Modules.WorkItems.Domain.Enums;

namespace TaskFlow.Modules.WorkItems.Application.Validators;

/// <summary>
/// Validator for CreateWorkItemRequest enforcing business rules.
/// Rules:
/// - Title: Required, max 200 characters, not empty or whitespace
/// - Description: Optional, max 5000 characters
/// - WorkItemType: Required, must be "Epic", "Story", or "Task" (case-insensitive)
/// - ParentId: Epic must be null, Story/Task must be provided (actual hierarchy validation in service)
/// </summary>
public sealed class CreateWorkItemRequestValidator : AbstractValidator<CreateWorkItemRequest>
{
    public CreateWorkItemRequestValidator()
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

        RuleFor(x => x.WorkItemType)
            .NotEmpty()
            .WithMessage("Work item type is required")
            .Must(BeValidWorkItemType)
            .WithMessage("Work item type must be Epic, Story, or Task");

        // Epic-specific validation: parentId must be null
        RuleFor(x => x.ParentId)
            .Null()
            .When(x => IsWorkItemType(x.WorkItemType, WorkItemType.Epic))
            .WithMessage("Epic cannot have a parent");

        // Story/Task-specific validation: parentId must be provided
        RuleFor(x => x.ParentId)
            .NotNull()
            .When(x => IsWorkItemType(x.WorkItemType, WorkItemType.Story) ||
                       IsWorkItemType(x.WorkItemType, WorkItemType.Task))
            .WithMessage("Story and Task must have a parent");
    }

    private static bool BeValidWorkItemType(string workItemType)
    {
        return Enum.TryParse<WorkItemType>(workItemType, ignoreCase: true, out _);
    }

    private static bool IsWorkItemType(string workItemType, WorkItemType expectedType)
    {
        return Enum.TryParse<WorkItemType>(workItemType, ignoreCase: true, out var parsed) &&
               parsed == expectedType;
    }
}

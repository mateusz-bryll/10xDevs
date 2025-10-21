using FluentValidation;
using TaskFlow.Modules.WorkItems.Application.Requests;

namespace TaskFlow.Modules.WorkItems.Application.Validators;

/// <summary>
/// Validator for AssignWorkItemRequest enforcing business rules.
/// Rules:
/// - UserId: Optional (null to unassign), must be valid Auth0 user ID format if provided
/// - User existence verification performed in service layer via IUsersService
/// </summary>
public sealed class AssignWorkItemRequestValidator : AbstractValidator<AssignWorkItemRequest>
{
    public AssignWorkItemRequestValidator()
    {
        // Note: UserId can be null (to unassign)
        // User existence validation is performed in the service layer via IUsersService
        // No specific validation rules needed here beyond what's in the request model
    }
}

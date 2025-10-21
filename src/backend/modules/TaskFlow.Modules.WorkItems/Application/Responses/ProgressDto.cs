namespace TaskFlow.Modules.WorkItems.Application.Responses;

/// <summary>
/// Represents progress information for parent work items (Epics and Stories with children).
/// For leaf items (Tasks with no children), all values should be 0.
/// </summary>
/// <param name="Completed">Number of direct children with status "Done".</param>
/// <param name="Total">Total number of direct children.</param>
/// <param name="Percentage">Percentage of completion (0-100), calculated as (Completed / Total) * 100.</param>
public sealed record ProgressDto(
    int Completed,
    int Total,
    int Percentage
);

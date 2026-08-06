namespace AssignmentSystem.Domain.Enums;

/// <summary>
/// Read-only, time-derived view of <see cref="AssignmentStatus"/> for display/dashboard purposes.
/// SubmissionOpen/DeadlinePassed are computed from <see cref="Domain.Entities.Assignment.DeadlineUtc"/> rather than
/// stored, since publishing an assignment opens submissions immediately in this system.
/// </summary>
public enum AssignmentLifecycleStatus
{
    Draft = 1,
    Published = 2,
    SubmissionOpen = 3,
    DeadlinePassed = 4,
    Graded = 5,
    Archived = 6
}

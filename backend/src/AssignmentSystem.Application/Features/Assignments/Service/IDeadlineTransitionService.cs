namespace AssignmentSystem.Application.Features.Assignments;

public interface IDeadlineTransitionService
{
    Task<int> ProcessDeadlinesAsync(DateTime nowUtc, CancellationToken cancellationToken);
}

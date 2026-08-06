using System.Security.Claims;
using AssignmentSystem.Application.Features.Assignments;
using Microsoft.AspNetCore.Authorization;

namespace AssignmentSystem.Api.Authorization;

public class AssignmentOwnershipAuthorizationHandler
    : AuthorizationHandler<AssignmentOwnershipRequirement, AssignmentDto>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AssignmentOwnershipRequirement requirement,
        AssignmentDto resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.FindFirstValue("sub");

        if (userId is not null && Guid.TryParse(userId, out var parsedUserId) && parsedUserId == resource.TeacherUserId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

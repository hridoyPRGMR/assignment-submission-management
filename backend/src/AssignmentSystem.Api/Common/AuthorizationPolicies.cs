using AssignmentSystem.Domain.Enums;

namespace AssignmentSystem.Api.Common;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string TeacherOnly = "TeacherOnly";
    public const string StudentOnly = "StudentOnly";
    public const string TeacherOrAdmin = "TeacherOrAdmin";
    public const string AssignmentOwner = "AssignmentOwner";

    public static void AddAppAuthorizationPolicies(this Microsoft.AspNetCore.Authorization.AuthorizationOptions options)
    {
        options.AddPolicy(AdminOnly, policy => policy.RequireRole(UserRole.Admin.ToString()));
        options.AddPolicy(TeacherOnly, policy => policy.RequireRole(UserRole.Teacher.ToString()));
        options.AddPolicy(StudentOnly, policy => policy.RequireRole(UserRole.Student.ToString()));
        options.AddPolicy(TeacherOrAdmin, policy => policy.RequireRole(UserRole.Teacher.ToString(), UserRole.Admin.ToString()));
    }
}

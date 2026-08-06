namespace AssignmentSystem.Application.Common;

public static class SubmissionFileConstraints
{
    public static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".docx", ".zip" };

    public const long MaxFileSizeBytes = 20 * 1024 * 1024;
}

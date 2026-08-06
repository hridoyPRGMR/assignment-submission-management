namespace AssignmentSystem.Application.Abstractions;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string originalFileName, CancellationToken cancellationToken);

    Stream OpenRead(string storedFileName);

    bool Exists(string storedFileName);

    void Delete(string storedFileName);
}

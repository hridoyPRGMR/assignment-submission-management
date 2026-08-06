using AssignmentSystem.Application.Abstractions;

namespace AssignmentSystem.Application.Tests.TestDoubles;

public class InMemoryFileStorageService : IFileStorageService
{
    public List<string> DeletedFiles { get; } = [];

    public Task<string> SaveAsync(Stream content, string originalFileName, CancellationToken cancellationToken) =>
        Task.FromResult($"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}");

    public Stream OpenRead(string storedFileName) => new MemoryStream();

    public bool Exists(string storedFileName) => true;

    public void Delete(string storedFileName) => DeletedFiles.Add(storedFileName);
}

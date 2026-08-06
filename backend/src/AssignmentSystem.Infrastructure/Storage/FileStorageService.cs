using AssignmentSystem.Application.Abstractions;
using Microsoft.Extensions.Hosting;

namespace AssignmentSystem.Infrastructure.Storage;

public class FileStorageService : IFileStorageService
{
    private readonly string _uploadsRootPath;

    public FileStorageService(IHostEnvironment hostEnvironment)
    {
        _uploadsRootPath = Path.Combine(hostEnvironment.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(_uploadsRootPath);
    }

    public async Task<string> SaveAsync(Stream content, string originalFileName, CancellationToken cancellationToken)
    {
        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
        var destinationPath = Path.Combine(_uploadsRootPath, storedFileName);

        await using var destination = File.Create(destinationPath);
        await content.CopyToAsync(destination, cancellationToken);

        return storedFileName;
    }

    public Stream OpenRead(string storedFileName) => File.OpenRead(GetPhysicalPath(storedFileName));

    public bool Exists(string storedFileName) => File.Exists(GetPhysicalPath(storedFileName));

    public void Delete(string storedFileName)
    {
        var path = GetPhysicalPath(storedFileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string GetPhysicalPath(string storedFileName) => Path.Combine(_uploadsRootPath, storedFileName);
}

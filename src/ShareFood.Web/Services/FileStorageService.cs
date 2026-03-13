namespace ShareFood.Web.Services;

public class FileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> SaveRecipeImageAsync(IFormFile? file, string? currentPath = null, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return currentPath;
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Chỉ cho phép tải lên ảnh JPG, PNG hoặc WEBP.");
        }

        var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "recipes");
        Directory.CreateDirectory(uploadFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        DeleteFile(currentPath);
        return $"/uploads/recipes/{fileName}";
    }

    public void DeleteFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        var normalizedPath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var absolutePath = Path.Combine(_environment.WebRootPath, normalizedPath);
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }
    }
}

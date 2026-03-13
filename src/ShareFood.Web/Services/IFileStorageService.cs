namespace ShareFood.Web.Services;

public interface IFileStorageService
{
    Task<string?> SaveRecipeImageAsync(IFormFile? file, string? currentPath = null, CancellationToken cancellationToken = default);

    void DeleteFile(string? relativePath);
}

using ImageManager.Data.Models;

namespace ImageManager.Services;

public interface IImageImportManager
{
    /// <summary>
    /// Imports all the new images related to a platform token.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task ImportAsync(PlatformToken token);
}
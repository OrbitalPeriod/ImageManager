using SixLabors.ImageSharp;

namespace ImageManager.Services;

public interface IFileService
{
    public Task<Guid> SaveFile(Image image);
}

public class FileService(string directory) : IFileService
{
    private readonly string _directory = directory;


    public async Task<Guid> SaveFile(Image file)
    {
        var id = Guid.NewGuid();

        var filePath = Path.Combine(_directory, id.ToString() + ".png");
        await using var fileStream = File.Open(filePath, FileMode.CreateNew);
        await file.SaveAsPngAsync(fileStream);

        return id;
    }
}
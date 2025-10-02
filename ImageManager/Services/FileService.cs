using SixLabors.ImageSharp;

namespace ImageManager.Services;

public interface IFileService
{
    public Task<byte[]> LoadFile(Guid id);
    public Task<Guid> SaveFile(Image image);
}

public class FileService(string directory) : IFileService
{
    public async Task<Guid> SaveFile(Image file)
    {
        var id = Guid.NewGuid();

        var filePath = Path.Combine(directory, id.ToString() + ".png");
        await using var fileStream = File.Open(filePath, FileMode.CreateNew);
        await file.SaveAsPngAsync(fileStream);

        return id;
    }
    public async Task<byte[]> LoadFile(Guid id)
    {
        var filePath = Path.Combine(directory, id.ToString() + ".png");
        return await File.ReadAllBytesAsync(filePath);
    }
}
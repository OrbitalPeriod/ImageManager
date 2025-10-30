#region Usings

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

#endregion

namespace ImageManager.Services;

/// <summary>
/// Service responsible for persisting and retrieving image files on disk.
/// Images are stored as PNGs with a GUID filename.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Loads the raw bytes of an image identified by its GUID.
    /// Throws <see cref="FileNotFoundException"/> if no file exists for the given id.
    /// </summary>
    /// <param name="id">The identifier that was returned when the image was saved.</param>
    /// <returns>The PNG byte array.</returns>
    Task<byte[]> LoadFullImage(Guid id);
    /// <summary>
    /// Loads the raw bytes of a jpg thumbnail image identified by its GUID
    /// </summary>
    /// <param name="id">The identifier that was returned when the image was saved.</param>
    /// <returns>The jpg byte array.</returns>
    Task<byte[]> LoadThumbnailImage(Guid id);

    /// <summary>
    /// Persists an <see cref="Image"/> instance as a PNG file and returns the generated GUID.
    /// </summary>
    /// <param name="image">The image to be saved.</param>
    /// <returns>The GUID that can later be used with <see cref="LoadFullImage"/>.</returns>
    Task<Guid> SaveFile(Image image);
}

#region Implementation

/// <summary>
/// File‑system implementation of <see cref="IFileService"/>.
/// The constructor requires a directory path; it will be created if it does not already exist.
/// </summary>
public class FileService(IConfiguration config, ILogger<FileService> logger) : IFileService
{
    // Implicit readonly field `directory` is available to the class.
    private readonly string _rootDirectory = config.GetValue<string>("FILE_DIRECTORY", "./images");
    private readonly int _thumbWidth = config.GetValue("THUMBNAIL_WIDTH", 300);
    private readonly int _thumbHeight = config.GetValue("THUMBNAIL_HEIGHT", 600);

    /// <inheritdoc />
    public async Task<Guid> SaveFile(Image image)
    {
        if (image == null) throw new ArgumentNullException(nameof(image));

        var id = Guid.NewGuid();

        var filePath = Path.Combine(_rootDirectory, $"{id}.png");
        await SaveFullImageAsync(image, filePath);

        filePath = Path.Combine(_rootDirectory, $"{id}_thumb.jpg");
        await SaveThumbnailImageAsync(image, filePath);

        return id;
    }

    private async Task SaveFullImageAsync(Image image, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var stream = File.Open(path, FileMode.Create);
        await image.SaveAsPngAsync(stream);
    }
    private async Task SaveThumbnailImageAsync(Image image, string path)
    {
        image.Mutate(x => x.Resize(new ResizeOptions()
        {
            Size = new Size(_thumbWidth, _thumbHeight),
            Mode = ResizeMode.Max,
        }));

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var stream = File.Open(path, FileMode.Create);
        await image.SaveAsJpegAsync(stream);
    }

    /// <inheritdoc />
    public async Task<byte[]> LoadFullImage(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(id));

        var filePath = Path.Combine(_rootDirectory, $"{id}.png");
        // The call will throw FileNotFoundException if the file does not exist,
        // which callers can catch to indicate a missing image.
        return await File.ReadAllBytesAsync(filePath);
    }

    /// <inheritdoc />
    public async Task<byte[]> LoadThumbnailImage(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(id));

        var filePath = Path.Combine(_rootDirectory, $"{id}_thumb.jpg");

        if (!Path.Exists(filePath))
        {
            logger.LogInformation($"Thumbnail image not found: {id}, defaulting to full image");
            return await LoadFullImage(id);
        }
        return await File.ReadAllBytesAsync(filePath);
    }
}

#endregion

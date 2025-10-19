#region Usings
using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
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
    Task<byte[]> LoadFile(Guid id);

    /// <summary>
    /// Persists an <see cref="Image"/> instance as a PNG file and returns the generated GUID.
    /// </summary>
    /// <param name="image">The image to be saved.</param>
    /// <returns>The GUID that can later be used with <see cref="LoadFile(Guid)"/>.</returns>
    Task<Guid> SaveFile(Image image);
}

#region Implementation

/// <summary>
/// File‑system implementation of <see cref="IFileService"/>.
/// The constructor requires a directory path; it will be created if it does not already exist.
/// </summary>
public class FileService(string directory) : IFileService
{
    // Implicit readonly field `directory` is available to the class.
    private readonly string _rootDirectory = Path.GetFullPath(directory);

    /// <inheritdoc />
    public async Task<Guid> SaveFile(Image image)
    {
        if (image == null) throw new ArgumentNullException(nameof(image));

        var id = Guid.NewGuid();
        var filePath = Path.Combine(_rootDirectory, $"{id}.png");

        // Ensure the target directory exists. CreateDirectory is idempotent.
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using var stream = File.Open(filePath, FileMode.CreateNew);
        await image.SaveAsPngAsync(stream);

        return id;
    }

    /// <inheritdoc />
    public async Task<byte[]> LoadFile(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException("ID cannot be empty.", nameof(id));

        var filePath = Path.Combine(_rootDirectory, $"{id}.png");
        // The call will throw FileNotFoundException if the file does not exist,
        // which callers can catch to indicate a missing image.
        return await File.ReadAllBytesAsync(filePath);
    }
}

#endregion

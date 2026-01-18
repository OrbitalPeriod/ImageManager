using ImageManager.Data.Models;

namespace ImageManager.Workers;

/// <summary>
/// Represents an image import that was queued because AnimeTagger was not ready.
/// </summary>
/// <param name="ImageBytes">The raw image bytes to import.</param>
/// <param name="Publicity">The publicity level for the image.</param>
/// <param name="UserId">The ID of the user performing the import.</param>
/// <param name="QueuedAt">When the import was queued.</param>
public record QueuedImageImport(
    byte[] ImageBytes,
    Publicity Publicity,
    string UserId,
    DateTime QueuedAt
);

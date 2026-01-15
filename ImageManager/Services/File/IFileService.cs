// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using SixLabors.ImageSharp;

namespace ImageManager.Services.File;

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
    /// Loads the raw bytes of a jpg compressed image identified by its GUID
    /// </summary>
    /// <param name="id">The identifier that was returned when the image was saved.</param>
    /// <returns>The jpg byte array.</returns>
    Task<byte[]> LoadCompressedImage(Guid id);

    /// <summary>
    /// Persists an <see cref="Image"/> instance as a PNG file and returns the generated GUID.
    /// </summary>
    /// <param name="image">The image to be saved.</param>
    /// <returns>The GUID that can later be used with <see cref="LoadFullImage"/>.</returns>
    Task<Guid> SaveFile(Image image);


}

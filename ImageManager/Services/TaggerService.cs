#region Usings

using Google.Protobuf;
using Grpc.Net.Client;
#endregion

namespace ImageManager.Services;

/// <summary>
/// Contract for a service that returns tags for an image.
/// </summary>
public interface ITaggerService
{
    /// <summary>
    /// Asynchronously retrieves tags for the supplied image data.
    /// </summary>
    /// <param name="image">Byte array containing the image.</param>
    /// <returns>A task that resolves to an <see cref="ImageResponse"/> with the detected tags.</returns>
    Task<ImageResponse> GetTags(byte[] image);
}

#region Implementation
/// <summary>
/// Client for communicating with the AnimeTagger gRPC service.
/// </summary>
public sealed class TaggerService : ITaggerService
{
    /// <summary>
    /// The underlying gRPC client.
    /// </summary>
    private readonly AnimeTagger.AnimeTaggerClient _client;

    /// <summary>
    /// Creates a new instance of <see cref="TaggerService"/>.
    /// </summary>
    /// <param name="serviceUrl">The base URL of the gRPC service (e.g. https://localhost:5001).</param>
    /// <exception cref="ArgumentException"><paramref name="serviceUrl"/> is null, empty or whitespace.</exception>
    public TaggerService(string serviceUrl)
    {
        if (string.IsNullOrWhiteSpace(serviceUrl))
            throw new ArgumentException("The gRPC service URL cannot be null or empty.", nameof(serviceUrl));

        var channelOptions = new GrpcChannelOptions
        {
            MaxReceiveMessageSize = 124 * 1024 * 1024, // 
            MaxSendMessageSize = 124 * 1024 * 1024
        };

        var channel = GrpcChannel.ForAddress(serviceUrl, channelOptions);
        _client = new AnimeTagger.AnimeTaggerClient(channel);
    }

    /// <inheritdoc/>
    public async Task<ImageResponse> GetTags(byte[] image)
    {
        if (image == null || image.Length == 0)
            throw new ArgumentException("Image data must not be null or empty.", nameof(image));

        var request = new ImageMessage
        {
            ImageData = ByteString.CopyFrom(image)
        };

        return await _client.GetTagsAsync(request);
    }
}
#endregion

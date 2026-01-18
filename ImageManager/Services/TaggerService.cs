#region Usings

using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using System.Collections.Concurrent;
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
    
    /// <summary>
    /// Gets whether the AnimeTagger service is currently ready.
    /// </summary>
    bool IsReady { get; }
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
    private readonly ILogger<TaggerService> _logger;
    private readonly int _maxRetries;
    private readonly TimeSpan _initialRetryDelay;
    
    /// <summary>
    /// Thread-safe flag indicating whether the service is ready.
    /// </summary>
    private volatile bool _isReady;

    /// <summary>
    /// Gets whether the AnimeTagger service is currently ready.
    /// </summary>
    public bool IsReady => _isReady;

    /// <summary>
    /// Creates a new instance of <see cref="TaggerService"/>.
    /// </summary>
    /// <param name="configuration">Configuration containing service settings.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    /// <exception cref="ArgumentException">The gRPC service URL is null, empty or whitespace.</exception>
    public TaggerService(IConfiguration configuration, ILogger<TaggerService> logger)
    {
        _logger = logger;
        var serviceUrl = configuration["ANIMETAGGER_URL"];

        if (string.IsNullOrWhiteSpace(serviceUrl))
            throw new ArgumentException("The gRPC service URL cannot be null or empty.", nameof(serviceUrl));
        
        // Retry configuration - can be overridden via configuration
        _maxRetries = configuration.GetValue<int>("ANIMETAGGER_MAX_RETRIES", 5);
        var initialDelaySeconds = configuration.GetValue<int>("ANIMETAGGER_INITIAL_RETRY_DELAY_SECONDS", 2);
        _initialRetryDelay = TimeSpan.FromSeconds(initialDelaySeconds);
        
        _logger.LogInformation("Starting gRPC service with url {serviceUrl}, max retries: {maxRetries}, initial delay: {initialDelay}",
            serviceUrl, _maxRetries, _initialRetryDelay);

        var channelOptions = new GrpcChannelOptions
        {
            MaxReceiveMessageSize = 124 * 1024 * 1024,
            MaxSendMessageSize = 124 * 1024 * 1024
        };

        var channel = GrpcChannel.ForAddress(serviceUrl, channelOptions);
        _client = new AnimeTagger.AnimeTaggerClient(channel);
        
        // Initially assume service is not ready
        _isReady = false;
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

        Exception? lastException = null;
        var delay = _initialRetryDelay;

        for (int attempt = 0; attempt <= _maxRetries; attempt++)
        {
            try
            {
                var response = await _client.GetTagsAsync(request);
                
                // Success - mark service as ready
                if (!_isReady)
                {
                    _isReady = true;
                    _logger.LogInformation("AnimeTagger service is now ready");
                }
                
                return response;
            }
            catch (RpcException rpcEx)
            {
                lastException = rpcEx;
                
                // Check if this is a connection/availability issue
                bool isConnectionError = rpcEx.StatusCode == StatusCode.Unavailable ||
                                       rpcEx.StatusCode == StatusCode.DeadlineExceeded ||
                                       rpcEx.StatusCode == StatusCode.Internal;
                
                if (isConnectionError)
                {
                    _isReady = false;
                    
                    if (attempt < _maxRetries)
                    {
                        _logger.LogWarning(
                            "AnimeTagger service not available (attempt {attempt}/{maxRetries}, status: {status}). Retrying in {delay}ms...",
                            attempt + 1, _maxRetries + 1, rpcEx.StatusCode, delay.TotalMilliseconds);
                        
                        await Task.Delay(delay);
                        delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
                    }
                    else
                    {
                        _logger.LogError(
                            "AnimeTagger service unavailable after {maxRetries} retries. Status: {status}",
                            _maxRetries + 1, rpcEx.StatusCode);
                    }
                }
                else
                {
                    // Non-connection errors (e.g., invalid request) should not be retried
                    _logger.LogError(rpcEx, "AnimeTagger returned error that should not be retried. Status: {status}", rpcEx.StatusCode);
                    throw;
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                _isReady = false;
                
                // Check if this looks like a connection issue
                if (ex is HttpRequestException || ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase))
                {
                    if (attempt < _maxRetries)
                    {
                        _logger.LogWarning(
                            "Connection error when calling AnimeTagger (attempt {attempt}/{maxRetries}). Retrying in {delay}ms...",
                            attempt + 1, _maxRetries + 1, delay.TotalMilliseconds);
                        
                        await Task.Delay(delay);
                        delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                    }
                    else
                    {
                        _logger.LogError(ex, "Connection error after {maxRetries} retries", _maxRetries + 1);
                    }
                }
                else
                {
                    // Unexpected exception - don't retry
                    _logger.LogError(ex, "Unexpected error calling AnimeTagger");
                    throw;
                }
            }
        }

        // All retries exhausted
        _logger.LogError(lastException, "Failed to get tags from AnimeTagger after {maxRetries} retries", _maxRetries + 1);
        throw new InvalidOperationException("AnimeTagger service is not available after multiple retry attempts.", lastException);
    }
}
#endregion

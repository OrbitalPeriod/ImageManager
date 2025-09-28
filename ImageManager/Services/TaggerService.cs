using Google.Protobuf;
using Grpc.Net.Client;

namespace ImageManager.Services;

public interface ITaggerService
{
    public Task<ImageResponse> GetTags(byte[] image);
}

public class TaggerService : ITaggerService
{
    private readonly AnimeTagger.AnimeTaggerClient _client;
    public TaggerService(string serviceUrl)
    {
        var channel = GrpcChannel.ForAddress(serviceUrl);
        _client = new AnimeTagger.AnimeTaggerClient(channel);
    }

    public async Task<ImageResponse> GetTags(byte[] image)
    {
        var request = new ImageMessage
        {
            ImageData = ByteString.CopyFrom(image)
        };
        return await _client.GetTagsAsync(request);
    }
}
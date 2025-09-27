import grpc
import AnimeTagger_pb2
import AnimeTagger_pb2_grpc


def run(image_path: str):
    # Read image as bytes
    with open(image_path, "rb") as f:
        image_bytes = f.read()

    # Connect to server
    channel = grpc.insecure_channel("localhost:50051")
    stub = AnimeTagger_pb2_grpc.AnimeTaggerStub(channel)

    # Correct message type is ImageMessage
    request = AnimeTagger_pb2.ImageMessage(image_data=image_bytes)
    response = stub.GetTags(request)


    print("Character tags:", response.character_tags)
    print("General tags:", response.general_tags)
    print("Rating:", AnimeTagger_pb2.Rating.Name(response.rating))


if __name__ == "__main__":
    run("test.png")  # Replace with your image path

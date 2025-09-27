from wdtagger import Tagger
from PIL import Image
import io
import AnimeTagger_pb2
import AnimeTagger_pb2_grpc
from concurrent import futures
import grpc
import signal
import sys
from grpc_health.v1 import health, health_pb2, health_pb2_grpc


class AnimeTagger(AnimeTagger_pb2_grpc.AnimeTaggerServicer):
    def __init__(self):
        self.tagger = Tagger()

    def GetTags(self, request, context):
        try:
            # request.image_data, not request.image
            image = Image.open(io.BytesIO(request.image_data))
            image.load()

            # Run tagger
            result = self.tagger.tag(image)
            # You probably need to adapt this depending on what Tagger returns.
            # Let's assume it returns {"character": [...], "general": [...], "rating": "general"}
            character_tags = result.character_tags
            general_tags = result.general_tags
            rating_str = result.rating.capitalize()

            # Map string -> enum
            rating_map = {
                "General": AnimeTagger_pb2.General,
                "Sensitive": AnimeTagger_pb2.Sensitive,
                "Questionable": AnimeTagger_pb2.Questionable,
                "Explicit": AnimeTagger_pb2.Explicit,
            }
            rating = rating_map.get(rating_str, AnimeTagger_pb2.General)

            return AnimeTagger_pb2.ImageResponse(
                character_tags=character_tags,
                general_tags=general_tags,
                rating=rating,
            )

        except Exception as e:
            context.set_details(f"Error processing image: {e}")
            context.set_code(grpc.StatusCode.INTERNAL)
            return AnimeTagger_pb2.ImageResponse()


def serve():
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    AnimeTagger_pb2_grpc.add_AnimeTaggerServicer_to_server(AnimeTagger(), server)

    health_servicer = health.HealthServicer()
    health_pb2_grpc.add_HealthServicer_to_server(health_servicer, server)

    server.add_insecure_port("[::]:50051")

    def handle_sigterm(*_):
        print("Shutting down server...")
        server.stop(0)
        sys.exit(0)

    signal.signal(signal.SIGINT, handle_sigterm)
    signal.signal(signal.SIGTERM, handle_sigterm)

    server.start()
    print("Server started on port 50051")
    server.wait_for_termination()


if __name__ == "__main__":
    serve()

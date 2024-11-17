from fastapi import FastAPI, APIRouter, Request
from app.routes import image, predict, place
from contextlib import asynccontextmanager
from starlette.middleware.cors import CORSMiddleware
from app.middlewares.logger import LoggingMiddleware
import google.generativeai as genai
import random
import os


from app.middlewares.error_handler import ErrorHandlers

from gensim.models import KeyedVectors


MODEL_PATH = os.path.join(os.path.dirname(__file__), "../", "GoogleNews-vectors-negative300.bin")

app = FastAPI()


@asynccontextmanager
async def lifespan(app: FastAPI):
    """
    Context manager for managing application lifespan.
    """
    # Preload the model during startup
    print("Loading Word2Vec model. This might take a while...")
    word_vectors = KeyedVectors.load_word2vec_format(MODEL_PATH, binary=True)
    print("Word2Vec model loaded successfully.")
    API_KEY =os.environ.get("API_KEY")
    genai.configure(api_key=API_KEY)

    app.state.word_vectors = word_vectors
    app.state.genai = genai
    # app.state.word_vectors = "word_vectors"

    yield  # Application runs during this point

    # Perform cleanup (if needed) during shutdown
    print("Shutting down application...")


# Create FastAPI app with lifespan context manager
app = FastAPI(lifespan=lifespan)


app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Allows all origins or you can list specific ones
    allow_credentials=True,
    allow_methods=["*"],  # Allows all HTTP methods like GET, POST, etc.
    allow_headers=["*"],  # Allows all headers
)
app.add_middleware(LoggingMiddleware)


api_router = APIRouter()

api_router.include_router(image.router, prefix="/image", tags=["image"])
api_router.include_router(predict.router, prefix="/predict", tags=["predict"])
api_router.include_router(place.router, prefix="/place", tags=["place"])

app.include_router(api_router, prefix="/api", tags=["api"])

@app.get("/api/health", tags=["api_health"])
async def root():
    return {"message": "working fine", "ok": True}

@app.get("/api/throw_random_error", tags=["api_health"])
async def err(request: Request):
    rand_int = random.randint(1,3)
    if rand_int== 1:
        return await ErrorHandlers.unauthorized_error_handler(request)
    if rand_int== 2:
        return await ErrorHandlers.bad_request_error_handler(request)
    if rand_int== 3:
        return await ErrorHandlers.not_found_error_handler(request)

    return {"message": "working fine", "ok": True}

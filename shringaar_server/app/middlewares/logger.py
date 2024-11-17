from starlette.middleware.base import BaseHTTPMiddleware
from starlette.requests import Request
from starlette.responses import JSONResponse
import logging

# Set up logging
logging.basicConfig(level=logging.DEBUG)


class LoggingMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):
        print(f"!! ==== Request: {request.method} {request.url.path} =========================")
        response = await call_next(request)
        print(f"!! ==== Response status code: {response.status_code} =========================")
        response.headers["X-Request-Processed"] = "True"
        return response

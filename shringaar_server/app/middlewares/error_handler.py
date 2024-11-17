from fastapi.responses import JSONResponse
from starlette.exceptions import HTTPException


class ErrorHandlers:
    @staticmethod
    async def unauthorized_error_handler(request):
        print(f"!! ==== Unauthorized error - Path: {request.url.path}")
        return JSONResponse(
            status_code=401, 
            content={"detail": "Unauthorized error: "},
        )

    @staticmethod
    async def bad_request_error_handler(request):
        print(f"!! ==== Bad Request Error - Path: {request.url.path}")
        return JSONResponse(
            status_code=400,
            content={"detail": "Bad request error. Please try again later."},
        )

    @staticmethod
    async def not_found_error_handler(request):
        print(f"!! ==== 404 Not found route - Path: {request.url.path}")
        return JSONResponse(
            status_code=404,
            content={"detail": "Not found route. Please try again later."},
        )

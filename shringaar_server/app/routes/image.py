from fastapi import APIRouter, File, UploadFile
from fastapi.responses import JSONResponse
import shutil
import os

router = APIRouter()

UPLOAD_DIR = "uploads"

# Create the uploads directory if it doesn't exist
os.makedirs(UPLOAD_DIR, exist_ok=True)


@router.post("/upload/multi")
async def upload_images(files: list[UploadFile] = File(...)):
    uploaded_files = []

    for file in files:
        file_location = os.path.join(UPLOAD_DIR, file.filename)

        # Save the uploaded file
        with open(file_location, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)

        uploaded_files.append({"filename": file.filename, "filepath": file_location})

    return JSONResponse(content={"uploaded_files": uploaded_files})


@router.post("/upload")
async def upload_images(file: UploadFile = File(...)):

    file_location = os.path.join(UPLOAD_DIR, file.filename)

    # Save the uploaded file
    with open(file_location, "wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    return JSONResponse(
        content={
            "uploaded_files": {"filename": file.filename, "filepath": file_location},
            "success":True
        }
    )

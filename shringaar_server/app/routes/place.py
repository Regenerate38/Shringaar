from fastapi import APIRouter, Request
from fastapi.responses import JSONResponse
import json
import re
import os

router = APIRouter()

# Create the uploads directory if it doesn't exist


@router.post("/")
async def gemini(request: Request):

    temp = await request.body()  # Access the raw body
    body = json.loads(temp)

    url = body["url"]
    room_coord = body["room_coord"]

    # Use relative path for the metadata file
    metadata_file_path = os.path.join(
        os.path.dirname(__file__), "..", "..", "furn_metadata.json"
    )

    try:
        with open(metadata_file_path, "r") as file:
            metadata = json.load(file)
    except FileNotFoundError:
        return JSONResponse(
            status_code=500, content={"message": "furn_metadata.json file not found"}
        )
    except json.JSONDecodeError:
        return JSONResponse(
            status_code=500,
            content={"message": "Error decoding JSON from furn_metadata.json"},
        )

    prompt = f"{url}. Given is an image of a well decorated room. I want to have a similar vibe of furniture around for an empty room with corners {str(room_coord)} in metres and I have these only furnitures: {metadata}. Now, just return the coordinates in 2D and orientation in degrees of few of these furniture so that it matches aesthetic and pattern of the well decorated room. Just return in json format with the furniture as key and x,y,orientation as their values "

    model = request.app.state.genai.GenerativeModel("gemini-1.5-flash")

    response = model.generate_content(prompt)
    text_content = response.text

    # Clean up the text content
    cleaned_text = text_content.replace("\\n", "\n").replace('\\"', '"')

    json_content = None
    try:
        # Look for JSON content within code blocks
        json_match = re.search(r"```json\s*(.*?)\s*```", cleaned_text, re.DOTALL)
        if json_match:
            json_str = json_match.group(1)
            json_content = json.loads(json_str)
    except json.JSONDecodeError:
        pass

    return {
        "data": json_content,  # Extracted JSON if found, else None
    }

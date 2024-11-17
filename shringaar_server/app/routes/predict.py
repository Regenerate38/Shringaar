from fastapi import APIRouter, Request
from fastapi.responses import JSONResponse
import json

import torchvision.transforms as transforms
from PIL import Image
import os
import numpy as np
from pathlib import Path
import cv2
from skimage.metrics import structural_similarity as ssim
from sklearn.metrics.pairwise import cosine_similarity
from gensim.models import KeyedVectors

router = APIRouter()

# Create the uploads directory if it doesn't exist


def extract_image_features(model, image_path):

    preprocess = transforms.Compose(
        [
            transforms.Resize((224, 224)),
            transforms.ToTensor(),
        ]
    )

    try:
        image = Image.open(image_path).convert("RGB")

        processed_image = preprocess(image).numpy()
        processed_image = np.transpose(processed_image, (1, 2, 0))

        gray_image = cv2.cvtColor(
            (processed_image * 255).astype(np.uint8), cv2.COLOR_RGB2GRAY
        )
        return gray_image
    except Exception as e:
        print(f"Error processing image {image_path}: {str(e)}")
        return None


def compute_SSI_similarity(image1, image2):

    try:
        similarity_score, _ = ssim(image1, image2, full=True)
        return similarity_score
    except Exception as e:
        print(f"Error computing SSIM: {str(e)}")
        return 0.0


def compute_cosine_similarity(input, furniture, model):

    with open(
        os.path.join(os.path.dirname(__file__), "..", "furn_metadata.json"),
        "r",
    ) as file:
        metadata = json.load(file)

    similarity = 0

    for each in ["style", "room_type"]:
        max_similarity = 0
        attribute_values = metadata[furniture].get(each, [])

        for one in attribute_values:
            if one in model.key_to_index and input[each] in model.key_to_index:
                vec1 = model[one].reshape(1, -1)
                vec2 = model[input[each]].reshape(1, -1)
                cos_sim = cosine_similarity(vec1, vec2)[0][0]
                max_similarity = max(max_similarity, cos_sim)
            else:
                max_similarity = max(max_similarity, 0.5)

        similarity += max_similarity

    return similarity / 2


def extract_furniture_features(model, furniture_dir):

    furniture_features = {}
    furniture_dir = Path(furniture_dir)

    valid_extensions = {".jpg", ".jpeg", ".png", ".webp"}

    for img_path in furniture_dir.rglob("*"):
        if img_path.suffix.lower() in valid_extensions:
            features = extract_image_features(None, img_path)
            if features is not None:
                furniture_features[img_path] = features

    return furniture_features


def recommend_furniture(
    room_image_path, input, furniture_dir, top_n, word_vectors, similarity_threshold=0.3
):
    room_features = extract_image_features(None, room_image_path)
    if room_features is None:
        return []

    furniture_features = extract_furniture_features(None, furniture_dir)

    recommendations = []

    for furniture_path, features in furniture_features.items():
        furniture_name = str(os.path.basename(furniture_path)).split(".")[0]
        ssi = compute_SSI_similarity(room_features, features)
        cosine = compute_cosine_similarity(input, furniture_name, model=word_vectors)
        score = 0.65 * cosine + 0.35 * ssi
        recommendations.append((furniture_path, ssi, cosine, score))

    recommendations.sort(key=lambda x: x[3], reverse=True)
    return recommendations[0:3]


@router.post("/now")
async def predictor(request:Request):
    
    temp = await request.body()  # Access the raw body
    print(type(temp), temp)
    body = json.loads(temp)
    # room_image_path = "room.jpg"
    furniture_dir = "../../furniture_models"
    print(body)

    style = body["style"]
    room_image_path = body["room_image_path"]
    room_type = body["room_type"]
    input = dict()
    input['style']=style
    input['room_type']=room_type
    recommendations = recommend_furniture(
        # room_image_path="/home/sujanbaskota/Desktop/python/decorators/decorators_server/uploads/Empty-Room-Decluttering-R.jpg",
        room_image_path=room_image_path,
        furniture_dir=furniture_dir,
        top_n=3,
        similarity_threshold=0,
        input=input,
        word_vectors=request.app.state.word_vectors
    )

    print("\nTop Recommendations:")
    print(recommendations)
    data = []  # Initialize an empty list

    for furniture_path, ssi, cosine, score in recommendations:
        print(f"Furniture: {furniture_path.name}")
        print(f"Overall Score: {score:.3f}\n")

        # Create a dictionary with proper key-value pairs
        temp = {
            "furniture_path": furniture_path.name,  # Use the .name attribute
            "score": score,
        }
        data.append(temp)

    return JSONResponse({"response": data, "success": True, })
    # return JSONResponse(content={"message": "sent"})

@router.post("/test")
async def predictor(request:Request):
    temp = await request.body()  # Access the raw body
   

    return JSONResponse({"response": temp, "success": True, })
    # return JSONResponse(content={"message": "sent"})


"""

room_image_path = "room.jpg"
furniture_dir = "./furniture_models"

input = dict()
input['style']='modern'
input['room_type']='living'

    
    
recommendations = recommend_furniture(
        room_image_path=room_image_path,
        furniture_dir=furniture_dir,
        top_n=3,
        similarity_threshold=0 ,
        input=input
    )
    
   
print("\nTop Recommendations:")
for furniture_path, ssi, cosine, score in recommendations:
        print(f"Furniture: {furniture_path.name}")
        print(f"Overall Score: {score:.3f}\n")
    
   
visualize_recommendations(room_image_path, recommendations)


"""

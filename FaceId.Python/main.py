from contextlib import asynccontextmanager

import cv2
import numpy as np

from fastapi import FastAPI, File, HTTPException, UploadFile
from insightface.app import FaceAnalysis


MAX_FILE_SIZE = 5 * 1024 * 1024 # 5 MB

face_analyzer = None


@asynccontextmanager
async def lifespan(app: FastAPI):
    global face_analyzer

    print("Loading face recognition model...")

    face_analyzer = FaceAnalysis(
        name="buffalo_l",
        providers=["CPUExecutionProvider"],
    )

    face_analyzer.prepare(
        ctx_id=-1,
        det_size=(640, 640),
    )

    print("Face recognition maodel loaded.")

    yield

app = FastAPI(
    title="FaceId Python Service",
    description="Face detection and embedding extraction service",
    version="1.0.0",
    lifespan=lifespan,
)

@app.get("/health")
def heaith():
    return {
        "status": "ok",
        "service": "FaceId Python",
        "model_loaded": face_analyzer is not None,
    }

@app.post("/extract-embedding")
async def extract_embedding(file: UploadFile = File(...)):
    contents = await file.read(MAX_FILE_SIZE + 1)

    if not contents:
        raise HTTPException(
            status_code=400,
            detail="The uplodad file is empty.",
        )

    if len(contents) > MAX_FILE_SIZE:
        raise HTTPException(
            status_code=413,
            detail="The image must not 5 MB.",
        )

    image_bytes = np.frombuffer(contents, dtype=np.uint8)
    image = cv2.imdecode(image_bytes, cv2.IMREAD_COLOR)

    if image is None:
        raise HTTPException(
            status_code=400,
            detail="The uploaded file is not a valid image.",
        )

    faces = face_analyzer.get(image)

    if len(faces) == 0:
        raise HTTPException(
            status_code=422,
            detail="No face deteated in the image",
        )

    if len(faces) > 1:
        raise HTTPException(
            status_code=422,
            detail="The image must contain exactly one face."
        )

    embedding = faces[0].embedding.tolist()

    return {
        "embedding": embedding,
        "dimensions": len(embedding),
    }
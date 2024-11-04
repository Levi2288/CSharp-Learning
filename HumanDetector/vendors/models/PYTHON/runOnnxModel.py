import onnxruntime as ort
import cv2
import numpy as np

# Configuration
model_path = "yolov8x-face-lindevs.onnx"  # Path to your ONNX model
image_path = "testPic.jpg"             # Path to the input image
input_size = (640, 640)                   # Set the input size expected by the model (e.g., 640x640 for YOLOv8)

# Load the ONNX model
session = ort.InferenceSession(model_path)

# Get model input and output details
input_name = session.get_inputs()[0].name
output_name = session.get_outputs()[0].name
print("Model input name:", input_name)
print("Model output name:", output_name)

# Load and preprocess the image
image = cv2.imread(image_path)
original_height, original_width = image.shape[:2]
input_image = cv2.resize(image, input_size)
input_image = cv2.cvtColor(input_image, cv2.COLOR_BGR2RGB)
input_image = input_image.astype(np.float32) / 255.0
input_image = np.transpose(input_image, (2, 0, 1))  # Change to (C, H, W)
input_image = np.expand_dims(input_image, axis=0)  # Add batch dimension

# Run the model
outputs = session.run([output_name], {input_name: input_image})[0]
print("Output shape:", outputs.shape)

# Now, parse the output based on its shape
confidence_threshold = 0.5
num_classes = 80  # Adjust according to the model

# Parse detections
detections = []
for detection in outputs:
    # Assumes detection has bounding box and confidence structure in YOLO format
    if len(detection) < 5:
        continue  # Skip invalid detections
    
    # Extract bounding box and confidence
    bbox = detection[:4]
    confidence = detection[4]
    if isinstance(confidence, np.ndarray):
        print("Warning: confidence is an array. Taking the first element.")
        confidence = confidence[0]  # Extract first element if confidence is an array

    # Only process if confidence is above threshold
    if confidence > confidence_threshold:
        # YOLO format: [center_x, center_y, width, height]
        center_x = int(bbox[0] * original_width)
        center_y = int(bbox[1] * original_height)
        width = int(bbox[2] * original_width)
        height = int(bbox[3] * original_height)
        
        x1 = int(center_x - width / 2)
        y1 = int(center_y - height / 2)
        x2 = int(center_x + width / 2)
        y2 = int(center_y + height / 2)
        
        # Process class scores
        class_scores = detection[5:]
        class_id = np.argmax(class_scores)
        class_score = class_scores[class_id]
        
        if class_score > 0.5:  # Threshold for class confidence
            detections.append((x1, y1, x2, y2, class_id, confidence))

# Draw detections on the image
for (x1, y1, x2, y2, class_id, confidence) in detections:
    label = f"Class {class_id} {confidence:.2f}"
    cv2.rectangle(image, (x1, y1), (x2, y2), (0, 255, 0), 2)
    cv2.putText(image, label, (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)

# Display the image with detections
cv2.imshow("Detections", image)
cv2.waitKey(0)
cv2.destroyAllWindows()

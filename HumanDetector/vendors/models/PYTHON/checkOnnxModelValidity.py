import onnx
import os

try:
    # Load the ONNX model
    onnx_model = onnx.load('yolov8x-face-lindevs.onnx')

    # Check if the model is valid according to ONNX specifications
    onnx.checker.check_model(onnx_model)

    print("ONNX model is valid and correctly formatted.")
except onnx.onnx_cpp2py_export.checker.ValidationError as e:
    # Handle model validation errors
    print(f"Model validation failed: {e}")
except Exception as e:
    # Handle other errors, such as file not found or loading issues
    print(f"An error occurred: {e}")

os.system("PAUSE")

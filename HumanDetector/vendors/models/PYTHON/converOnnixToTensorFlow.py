import onnx

from onnx_tf.backend import prepare

onnx_model = onnx.load("E:\!Levi\VisualStudio\#Other Projects\C#\HumanDetector\vendors\models\PYTHON\models\yolov8n-face.onnx")  # load onnx model
tf_rep = prepare(onnx_model)  # prepare tf representation
tf_rep.export_graph("E:\!Levi\VisualStudio\#Other Projects\C#\HumanDetector\vendors\models\PYTHON\models")  # export the model
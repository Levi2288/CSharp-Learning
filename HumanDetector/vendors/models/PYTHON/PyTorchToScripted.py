import torch
import torch.nn as nn
import torch.nn.functional as F

# Step 1: Load the Scripted Model
scripted_model_path = "E:\\!Levi\\VisualStudio\\#Other Projects\\C#\\HumanDetector\\vendors\\models\\pytorch\\yolov8l-face.pt"
scripted_model = torch.jit.load(scripted_model_path)

# Step 2: Extract the State Dictionary from the Scripted Model
state_dict = scripted_model.state_dict()

# Step 3: Define the Non-Scripted Version of the Model
class NonScriptedModel(nn.Module):
    def __init__(self):
        super(NonScriptedModel, self).__init__()
        self.fc1 = nn.Linear(784, 128)  # Adjust input/output sizes based on your model
        self.fc2 = nn.Linear(128, 64)
        self.fc3 = nn.Linear(64, 10)

    def forward(self, x):
        x = F.relu(self.fc1(x))
        x = F.relu(self.fc2(x))
        x = self.fc3(x)
        return x

# Step 4: Initialize the Non-Scripted Model and Load the State Dictionary
non_scripted_model = NonScriptedModel()
non_scripted_model.load_state_dict(state_dict)

# Step 5: Save the Non-Scripted Model
non_scripted_model_path = "model_non_scripted.pth"
torch.save(non_scripted_model.state_dict(), non_scripted_model_path)

# Step 6: Load the Non-Scripted Model for Future Use
loaded_model = NonScriptedModel()
loaded_model.load_state_dict(torch.load(non_scripted_model_path))
loaded_model.eval()  # Set the model to evaluation mode if needed

# Optional: Verify if the Loaded Model Works Correctly
# (Test with a dummy input)
dummy_input = torch.randn(1, 784)  # Adjust input size based on your model's input
output = loaded_model(dummy_input)
print(output)  # Should produce a valid output tensor if the model is correctly loaded

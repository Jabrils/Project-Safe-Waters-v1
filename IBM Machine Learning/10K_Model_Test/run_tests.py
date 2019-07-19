import os
import json
import numpy as np
import tensorflow as tf
from PIL import Image

DIRECTORY = '10k'

LABEL_MAP = { 
    'ankle deep': 0.5,               # 0.0 -> 0.5
    'knee deep': 2.0,                # 0.5 -> 2.0
    'waist deep': 3.0,               # 2.0 -> 3.0
    'feet-dont-touch deep': 5.0,     # 3.0 -> 5.0
    'dangerously deep': float('inf') # 5.0 -> inf
}

# Results
with open('model_android/labels.json', 'r') as f:
    labels = json.load(f)


def label_for_depth(label, label_map):
    # Sort the keys by their value.
    sorted_keys = sorted(label_map, key=lambda k: label_map[k])
    for key in sorted_keys:
        if label < label_map[key]:
            return key

def get_image_list(dirname):
    def is_image(name):
        n = name.lower()
        return n.endswith('.jpeg') or n.endswith('.jpg') or n.endswith('.png')
    return [os.path.join(dirname, i)  for i in os.listdir(dirname) if is_image(i)]

input_data = [[np.array(Image.open(filename), dtype=np.float32)] for filename in get_image_list(DIRECTORY)]

with open('labels.txt', 'r') as f:
    ground_truth = f.read()[:-1].split('\n')

ground_truth = [label_for_depth(float(i), LABEL_MAP) for i in ground_truth]

i = 0

TotalAcc = 0

distDictGT = {i:0 for i in labels}
distDictP = {i:0 for i in labels}

print("Calculating...")

for item in input_data:
    # Model loading grossness
    interpreter = tf.lite.Interpreter(model_path='model_android/model.tflite')
    interpreter.allocate_tensors()
    input_details = interpreter.get_input_details()
    output_details = interpreter.get_output_details()
    interpreter.set_tensor(input_details[0]['index'], item)
    interpreter.invoke()
    output_data = interpreter.get_tensor(output_details[0]['index'])[0]

    output_data = list(output_data)

    GT = ground_truth[i]
    P = labels[output_data.index(max(output_data))]
    A = 1 if P == GT else 0
    TotalAcc += A

    distDictGT[GT] += 1
    distDictP[P] += 1

    if i % 100 == 0:
        print(f"Progress: {i}/{len(ground_truth)}\nGT: {distDictGT}\nP: {distDictP}")
 
    # print(f'GT: {GT}\tP: {P}\tA: {A}')

    i += 1

print(TotalAcc/len(ground_truth))

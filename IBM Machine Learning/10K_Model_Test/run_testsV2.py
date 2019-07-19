import os
import json
import seaborn as sn
import pandas as pd
import numpy as np
import tensorflow as tf
import matplotlib.pyplot as plt
from PIL import Image
import sys

EXPORTED_BUCKET = sys.argv[1]
MODEL_PATH = 'model_android'

def get_image_list(dirname):
    def is_image(name):
        n = name.lower()
        return n.endswith('.jpeg') or n.endswith('.jpg') or n.endswith('.png')
    return [filename for filename in os.listdir(dirname) if is_image(filename)]

def testable_images():
    with open(os.path.join(EXPORTED_BUCKET, '_annotations.json'), 'r') as f:
        return json.load(f)['annotations'].keys()

def get_list_of_labels():
    with open(os.path.join(EXPORTED_BUCKET, '_annotations.json'), 'r') as f:
        annotations = json.load(f)['annotations']
        return [annotations[filename][0]['label'] for filename in list_of_filenames if filename in annotations]

def loadLabels():
    with open(os.path.join(MODEL_PATH, 'labels.json'), 'r') as f:
        return json.load(f)

def int_labels_for_strings(raw_labels):
    labels = loadLabels()
    return [labels.index(label.replace('-', ' ')) for label in raw_labels]

print(f'Loading Images')
list_of_filenames = [filename for filename in get_image_list(EXPORTED_BUCKET) if filename in testable_images()]
print(f'Next1')
list_of_paths = [os.path.join(EXPORTED_BUCKET, name) for name in list_of_filenames]
print(f'Next2')
input_data = [[np.array(Image.open(path), dtype=np.float32) / 255] for path in list_of_paths ]

# Model loading grossness
interpreter = tf.lite.Interpreter(model_path=os.path.join(MODEL_PATH, 'model.tflite'))
interpreter.allocate_tensors()
input_details = interpreter.get_input_details()
output_details = interpreter.get_output_details()

results = []
for (i, item) in enumerate(input_data):
    interpreter.set_tensor(input_details[0]['index'], item)
    interpreter.invoke()
    output_data = interpreter.get_tensor(output_details[0]['index'])[0]

    results.append(output_data)

    if (i+1) % 100 == 0:
        print(i+1)

results = np.array(results)
predictions = np.argmax(results, axis=1)


raw_labels = get_list_of_labels()
labels = int_labels_for_strings(raw_labels)

is_equal = np.equal(labels, predictions)

print('\n', 'num predictions: {}'.format(len(predictions)))
print('\n', 'accuracy: {}'.format(np.sum(is_equal) / len(predictions)), '\n')

confusion = tf.math.confusion_matrix(labels, predictions)

# pylint: disable=not-context-manager
with tf.Session().as_default():
    normalize = True
    cm = confusion.eval()

    if normalize:
        cm = cm.astype('float') / cm.sum(axis=1)[:, np.newaxis]
        cm*=100

    classes = loadLabels()

    # Cleaning up a bit for visualization
    classes[1] = 'floating deep'

    # classes = np.array(classes)
    # classes[[2, 3]] = classes[[3, 2]]
    # classes = classes[::-1]
    # classes = list(classes)

    fig, ax = plt.subplots(figsize=(5, 5))
    ax.xaxis.tick_top()
    ax.set_title(f"{sys.argv[2]} % Heatmap - {EXPORTED_BUCKET}")

    # cm = np.identity(4)*100
    sn.heatmap(cm,
        annot=True,
        cmap="Blues",
        # fmt='g',
        ax=ax,
        xticklabels=classes,
        yticklabels=classes,
        vmin=0,
        vmax=100,
        square=False)

    plt.tight_layout()
    plt.savefig(f"IBM_Model_{EXPORTED_BUCKET}_Perc.png", dpi=300)

###

    sn.set(font_scale=.5)
    fig, ax = plt.subplots(figsize=(5, 5),)
    ax.xaxis.tick_top()
    ax.set_title(f"{sys.argv[2]} Count Heatmap - {EXPORTED_BUCKET}")
    cm = confusion.eval()

    total = np.append(cm,0)

    # print(cm, '\n')
    cm = np.array([np.append(i, [float('nan'), np.sum(i), round((i[j]/np.sum(i))*100)], axis=0) for (j, i) in enumerate(cm)])
    # print(cm, '\n')
    cm = cm.transpose()
    # print(cm, '\n')
    # Gotta figure out how to get rid of that max dependency
    cm = np.array([np.append(i, [float('nan'), np.sum(i), round((max(i)/np.sum(i))*100)], axis=0) for (j, i) in enumerate(cm)])
    # print(cm, '\n')
    cm = cm.transpose()
    # print(cm, '\n')


    last = len(cm)-1
    cm[last][len(cm[last])-2] = np.sum(cm[last][:4] / len(cm[last][:4]))
    cm = cm.transpose()
    cm[last][len(cm[last])-2] = np.sum(cm[last][:4] / len(cm[last][:4]))
    cm = cm.transpose()
    cm[last][len(cm[last])-1] = float('nan')
    # print(cm, '\n')


    # print(max(total),total)
    classes.append(None)
    classesX = classes[:]#.append("Recall")
    classesY = classes[:]#.append("Percision")

    classesX.append('Total Ground Truth')
    classesY.append('Total Predictions')
    classesX.append("Recall")
    classesY.append("Percision")
    
    sn.heatmap(cm,
        annot=True,
        cmap="Blues",
        fmt='g',
        ax=ax,
        xticklabels=classesX,
        yticklabels=classesY,
        vmin=0,
        vmax=max(total),
        square=True)

    plt.tight_layout()
    plt.savefig(f"IBM_Model_{EXPORTED_BUCKET}_Count.png", dpi=300)
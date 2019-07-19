import os
import re
import json

import ibm_boto3
from botocore.client import Config
from PIL import Image

# Things to change:
BUCKET_NAME = 'simple-sim-10k-jpg' #
RESOURCE_INSTANCE_ID = ''
ENDPOINT = ''
API_KEY = ''

IMAGE_DIRECTORY = 'grabs500/Imgs'
LABELS_PATH = 'grabs500/Data/labels.txt'

SCALED_IMAGE_DIRECTORY = '_tmpImgs500' # Generated output.
ANNOTATIONS_PATH = '_annotations.json' # Generated output.

LABEL_MAP = { 
    'ankle deep': 0.5,               # 0.0 -> 0.5
    'knee deep': 2.0,                # 0.5 -> 2.0
    'waist deep': 3.0,               # 2.0 -> 3.0
    'feet-dont-touch deep': 5.0,     # 3.0 -> 5.0
    'dangerously deep': float('inf') # 5.0 -> inf
}


# Helper methods
def index_from_filename(name):
    regex = r"^\D*(\d*).(?:png|jpeg|jpg)$"
    matches = re.search(regex, name, re.IGNORECASE)
    return int(matches.group(1))

def get_image_list(dirname):
    print("fetching img")
    def is_image(name):
        n = name.lower()
        return n.endswith('.jpeg') or n.endswith('.jpg') or n.endswith('.png')
    return [i for i in os.listdir(dirname) if is_image(i)]

def resize_all_images(dirname, output):
    print("resizing")
    if not os.path.exists(output):
        os.mkdir(output)
    for filename in os.listdir(dirname):
        image_path = os.path.join(dirname, filename)
        if os.path.isfile(image_path):
            img = Image.open(image_path)
            small_img = img.resize((224, 224), Image.ANTIALIAS)
            filename = filename.split('.')[0]+'.jpg'
            scaled_image_path = os.path.join(output, filename)
            small_img.save(scaled_image_path,"JPEG")

def get_label_list(path):
    print("fetching labels")
    with open(path, 'r') as f:
        return [float(line) for line in f.readlines()]

def label_for_depth(label, label_map):
    # Sort the keys by their value.
    sorted_keys = sorted(label_map, key=lambda k: label_map[k])
    for key in sorted_keys:
        if label < label_map[key]:
            return key

def build_annotations(images, labels, index):
    print("annotating")
    annotations = {}
    for image in images:
        i = index(image)
        if i < len(labels):
             annotations[image] = [{'label': labels[i]}]
    return {
        'version': '1.0',
        'type': 'classification',
        'labels': list(set(labels)), # De-dupe labels.
        'annotations': annotations
    }


def main():
    print("init")
    # Start by shrinking all images to 224x224 (default for sota cnn).
    resize_all_images(IMAGE_DIRECTORY, SCALED_IMAGE_DIRECTORY)


    # Load labels and images.
    images = get_image_list(SCALED_IMAGE_DIRECTORY)
    labels = get_label_list(LABELS_PATH)

    # Create the _annotations.json
    mapped_labels = [label_for_depth(label, LABEL_MAP) for label in labels]
    json_content = build_annotations(images, mapped_labels, index_from_filename)
    with open(ANNOTATIONS_PATH, 'w') as f:
        json.dump(json_content, f, indent=2)

    quit()
    # Upload to object storage.
    credentials = {
        'ibm_auth_endpoint': 'https://iam.ng.bluemix.net/oidc/token',
        'ibm_service_instance_id': RESOURCE_INSTANCE_ID,
        'endpoint_url': ENDPOINT,
        'ibm_api_key_id': API_KEY,
        'config': Config(signature_version='oauth')
    }

    bucket = ibm_boto3.resource('s3', **credentials).Bucket(BUCKET_NAME)

    print('uploading {}...'.format(ANNOTATIONS_PATH))
    bucket.upload_file(ANNOTATIONS_PATH, ANNOTATIONS_PATH)

    for filename in images:
        print('uploading {}...'.format(filename))
        bucket.upload_file(os.path.join(SCALED_IMAGE_DIRECTORY, filename), filename)


if __name__ == "__main__":
  main()
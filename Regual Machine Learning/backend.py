from keras.preprocessing import image
from PIL import Image
from resizeimage import resizeimage
import numpy as np
from skimage import color
import keras
import time

def LoadNReshape(file):
    # Load the image from file
    img = image.load_img(file)#, color_mode='grayscale')

    # Turn the image into an array
    img = image.img_to_array(img)

    # Normalize the image by /255 using float32
    img =  img.astype('float32')/255

    # Reshape the image so its Keras ready
    img = img.reshape(1, 3, 1920, 1080)

    return img

def Resize(file,size):
    with open(file, 'r+b') as f:
        with Image.open(f) as image:
            cover = resizeimage.resize_cover(image, size)
            # cover.save(file[1], image.format)
        
            # pixels = list(cover.getdata())
            # width, height = cover.size
            # pixels = [pixels[i * width:(i + 1) * width] for i in range(height)]

            a = np.asarray(cover)

            return color.rgb2gray(a)

class Histories(keras.callbacks.Callback):
    def on_train_begin(self, logs={}):
        self.start = time.time()

    def on_train_end(self, logs={}):
        return

    def on_epoch_begin(self, epoch, logs={}):
        return

    def on_epoch_end(self, epoch, logs={}):
        if epoch % 25 == 0:
            print(f'Time Elapsed: {int(time.time() - self.start)}')
        return

    def on_batch_begin(self, batch, logs={}):
        return

    def on_batch_end(self, batch, logs={}):
        return
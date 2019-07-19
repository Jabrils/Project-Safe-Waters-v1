'''Trains a simple convnet on the MNIST dataset.
Gets to 99.25% test accuracy after 12 epochs
(there is still a lot of margin for parameter tuning).
16 seconds per epoch on a GRID K520 GPU.
'''

from __future__ import print_function
import keras
from keras.datasets import mnist
from keras.models import Sequential
from keras.layers import Dense, Dropout, Flatten
from keras.layers import Conv2D, MaxPooling2D
from keras import backend as K
from keras.preprocessing.image import ImageDataGenerator
from keras.preprocessing import image
import numpy as np
import backend as B
import os
import math

batch_size = 512
epochs = 100
stepsPE = 10

# input image dimensions
iDim = (192, 108)

X_dir = 'data/imgs'
Y_dir = 'data/data'

X_loc = os.listdir(X_dir)

x = np.zeros([len(X_loc),iDim[0], iDim[1],1])

for i in range(len(x)):
    theImg = f'{X_dir}/{X_loc[i]}'

    g = B.Resize([theImg, theImg.replace('.','_t.')], iDim)
    imp = image.img_to_array(g)
    x[i] = imp.reshape(iDim[0], iDim[1],1)

load = Y_dir + '/labels.txt'

with open(load, 'r') as f:
    store = f.read()[:-1].split('\n')

y = np.array(store).reshape(len(store),1)

# print(y.shape, y)

input_shape = (iDim[0], iDim[1], 1)

x = x.astype('float32')
x /= 255
print('x_train shape:', x.shape)
print('y_train shape:', y.shape)
print(x.shape[0], 'train samples')

model = Sequential()
model.add(Conv2D(32, kernel_size=(3, 3),
                 activation='relu',
                 input_shape=input_shape))
model.add(Conv2D(64, (3, 3), activation='relu'))
model.add(MaxPooling2D(pool_size=(2, 2)))
model.add(Dropout(0.25))
model.add(Flatten())
model.add(Dense(512, activation='relu'))
model.add(Dropout(0.5))
model.add(Dense(1, activation='linear'))

model.compile(loss=keras.losses.mean_squared_error,
              optimizer=keras.optimizers.Adadelta())

model.fit(x, y,
        steps_per_epoch=stepsPE,
        epochs=epochs,
        verbose=1,
        shuffle=True)
          
# score = model.evaluate(x, y, verbose=0)

model.save('model.h5')
# print('Test loss:', score[0])
# print('Test accuracy:', score[1])

p = model.predict(x)

print("Pred: ", p)
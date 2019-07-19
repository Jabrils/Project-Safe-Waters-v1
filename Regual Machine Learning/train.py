from __future__ import print_function
import keras
from keras.datasets import mnist
from keras.models import Sequential, load_model
from keras.layers import Dense, Dropout, Flatten
from keras.layers import Conv2D, MaxPooling2D
from keras import backend as K
from keras.preprocessing.image import ImageDataGenerator
from keras.preprocessing import image
from keras.callbacks import TensorBoard, ReduceLROnPlateau, ModelCheckpoint, LambdaCallback
import numpy as np
import backend as B
import os
import math
import datetime
import time
import random

batch_size = 64
epochs = 200
stepsPE = 25

tStart = time.time()
div = 1
# input image dimensions
iDim = (int(192/div), int(108/div))

print("INITING...")

X_dir = 'G:/Unity/Jabrils/IBM Safe Waters/Builds/Quick Build/IBM Safe Waters_Data/Grabs100/imgs'
Y_dir = 'G:/Unity/Jabrils/IBM Safe Waters/Builds/Quick Build/IBM Safe Waters_Data/Grabs100/data'

X_loc = os.listdir(X_dir)

x = np.zeros([len(X_loc),iDim[0], iDim[1],1])

print("LOADING DATA...")

for i in range(len(x)):
    theImg = f'{X_dir}/{X_loc[i]}'

    g = B.Resize(theImg, iDim)
    imp = image.img_to_array(g)
    x[i] = imp.reshape(iDim[0], iDim[1],1)

load = Y_dir + '/labels.txt'

now = int(time.time() - tStart)
print(f'Loading Data took: {int(now)} Seconds')

print("LOADING LABELS...")

with open(load, 'r') as f:
    store = f.read()[:-1].split('\n')

y = np.array(store).reshape(len(store),1)

print("FORMING ARCHITECTURE...")

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

l = keras.losses.mean_squared_error


# print("LOADING MODEL...")
# model = load_model("model.h5")

found = False

model.compile(loss=l, optimizer=keras.optimizers.Adadelta())
while not found:

    timestamp = datetime.datetime.fromtimestamp(time.time()).strftime('%Y-%m-%d_%H-%M-%S')

    # For tensor board
    tbCallBack = TensorBoard(log_dir=f'./Graphs/G_{timestamp}', histogram_freq=0,  
            write_graph=True, write_images=True)

    reduceLR = ReduceLROnPlateau(monitor='loss', verbose=1, min_delta=.01, factor=.99, patience=25)

    mc = ModelCheckpoint(f'model.h5', period=100)

    lCB = B.Histories()
    print("BEGINNING TRAINING...")

    tTrain = time.time()

    w = model.get_weights()

    model.fit(x, y,
            steps_per_epoch=stepsPE,
            # batch_size = batch_size,
            epochs=epochs,
            verbose=1,
            shuffle=True,
            callbacks=[tbCallBack, reduceLR, mc, lCB])
            
    score = model.evaluate(x, y, verbose=1)

    print(f"Cost: {score}")

    found = True# if score <= 5 else False

print("SAVING MODEL...")

model.save('model.h5')


# print(model.predict(x))
'''
Originally lifted from this article https://machinelearningmastery.com/tutorial-first-neural-network-python-keras/
'''

# Create your first MLP in Keras
from keras.models import Sequential
from keras.layers import Dense, Conv2D, Flatten, Dropout, MaxPooling2D
from keras.constraints import maxnorm
import numpy
import backend as B
import os

# fix random seed for reproducibility
numpy.random.seed(7)

# load pima indians labels
labels = numpy.loadtxt("data/data/labels.txt", delimiter="\n")

dataLoc = 'data/imgs'
imgs = os.listdir(dataLoc) # dir is your directory path

# split into input (X) and output (Y) variables
X = [None] * len(labels)
Y = labels
inp_dim = [1920,1080]

# 
for i in range(len(X)):
    X[i] = B.LoadNReshape(f"{dataLoc}/{imgs[i]}")

# create model
model = Sequential()
model.add(Conv2D(64, (5, 5), input_shape=(3, inp_dim[0], inp_dim[1]), padding='same', activation='relu'))
model.add(MaxPooling2D(pool_size=(2, 2), padding='same'))

model.add(Conv2D(128, (5, 5), padding='same', activation='relu'))
model.add(MaxPooling2D(pool_size=(2, 2), padding='same'))

model.add(Conv2D(256, (1, 1), padding='same', activation='relu'))
model.add(MaxPooling2D(pool_size=(2, 2), padding='same'))

model.add(Flatten())

model.add(Dense(1024, activation='relu', kernel_constraint=maxnorm(3)))
model.add(Dropout(0.5))
model.add(Dense(512, activation='relu', kernel_constraint=maxnorm(3)))
model.add(Dropout(0.5))

model.add(Dense(1, activation='linear'))

# Compile model
model.compile(loss='binary_crossentropy', optimizer='adam')

# Fit the model
model.fit(X[0], Y[0], epochs=25, batch_size=8)

# # evaluate the model
# scores = model.evaluate(X, Y)
# print("\n%s: %.2f%%" % (model.metrics_names[1], scores[1]*100))

# save the model
model.save("mod.h5")

# calculate predictions
predictions = model.predict(X)
input()
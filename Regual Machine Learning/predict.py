from keras.models import load_model
from keras.preprocessing import image
import backend as B
import numpy as np
import os

div = 1

iDim = (int(192/div), int(108/div))
theDir = "G:/Unity/Jabrils/IBM Safe Waters/Builds/Quick Build/IBM Safe Waters_Data/Grabs100/"
# theDir = "data/"
theImg = os.listdir(theDir+"Imgs")

x = np.zeros([len(theImg),iDim[0], iDim[1],1])

for i in range(len(x)):
    g = B.Resize(f'{theDir+"Imgs"}/{theImg[i]}', iDim)
    imp = image.img_to_array(g)
    x[i] = imp.reshape(iDim[0], iDim[1], 1)

x = x.astype('float32')
x /= 255

with open(theDir+"data/labels.txt", 'r') as f:
    store = f.read()[:-1].split('\n')

y = np.array(store).reshape(len(store),1)

mname = 'model_T100'
model = load_model(f"{mname}.h5")

p = model.predict(x)

errT = []

for i in range(len(p)):
    l = float(y[i][0])
    g = float(p[i][0])
    errT.append(abs(l-g))

    print(f"Pred: {g}\nLabel: {l}\nError: {g-l}\n{'~'*10}\n")

avg = 0
target = 0
half = 0
one = 0

for e in errT:
    avg += e
    target += 0 if e > .1 else 1
    half += 0 if e > .5 else 1
    one += 0 if e > 1 else 1

avg /= len(errT)
target /= len(errT)
half /= len(errT)
one /= len(errT)

print(f"Model: {mname}\nSample Count: {len(p)}\nAverage Error (inch): {avg*12}\nOn Target: {target}%\n6 Inch: {half}%\n1 Foot: {one}%\n")
import keras
import numpy

# # load pima indians dataset
dataset = numpy.loadtxt("commentT.csv", delimiter=",")

# # split into input (X) and output (Y) variables
X = dataset[:,0:5]

model = keras.models.load_model("mod.h5")
predictions = model.predict(X)

# round predictions
rounded = [round(x[0]) for x in predictions]
print(rounded)
input()
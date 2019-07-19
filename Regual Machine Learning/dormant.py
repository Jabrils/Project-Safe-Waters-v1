print(input_shape)

input_img = Input(shape=input_shape)

network = Conv2D(32, kernel_size=(3, 3), activation='relu',input_shape=input_shape) (input_img)
network = Conv2D(64, kernel_size=(3, 3), activation='relu') (network)
network = Flatten() (network)
network = Dense(512, activation='relu') (network)
network = Dense(1, activation='linear') (network)

model = Model(input_img, network)
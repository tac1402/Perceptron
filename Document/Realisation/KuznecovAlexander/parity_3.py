import datetime as dt
import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import DataLoader, TensorDataset
import numpy as np

num_inputs = 16
num_samples = 2**num_inputs

indices = torch.arange(num_samples, dtype=torch.float32).unsqueeze(1)

bit_positions = torch.arange(num_inputs, dtype=torch.float32)
periods = 2 ** (bit_positions + 1)

X_train = ((indices % periods) >= (periods / 2)).float()
y_train = ((torch.sum(X_train, dim=1) + 1) % 2).float().unsqueeze(1)

batch_size = 8
train_dataset = TensorDataset(X_train, y_train)
train_loader = DataLoader(train_dataset, batch_size=batch_size, shuffle=True)

class ParityNetwork(nn.Module):
    def __init__(self, input_size):
        super(ParityNetwork, self).__init__()
        self.fc1 = nn.Linear(input_size, 256)
        self.fc2 = nn.Linear(256, 256)
        self.fc3 = nn.Linear(256, 1)
        self.relu = nn.ReLU()
        self.sigmoid = nn.Sigmoid()
        
    def forward(self, x):
        x = self.relu(self.fc1(x))
        x = self.relu(self.fc2(x))
        x = self.sigmoid(self.fc3(x))
        return x

model = ParityNetwork(num_inputs).cuda()
criterion = nn.BCELoss()
optimizer = optim.Adam(model.parameters(), lr=0.001)

num_epochs = 500
for epoch in range(num_epochs):

    model.train()
    train_loss = 0.0
    correct = 0
    
    for i_batch, (batch_X, batch_y) in enumerate(train_loader):

        batch_X_cuda = batch_X.cuda()
        batch_y_cuda = batch_y.cuda()
        
        optimizer.zero_grad()
        outputs = model(batch_X_cuda)
        loss = criterion(outputs, batch_y_cuda)
        loss.backward()
        optimizer.step()
        train_loss += loss.item()

        predictions = (outputs > 0.5).float()
        correct += (predictions == batch_y_cuda).sum().item()

    time = dt.datetime.now()
    print(f'{time} {epoch=}, train_loss={train_loss/i_batch:0.4f}, accuracy {correct / num_samples * 100:.2f}')
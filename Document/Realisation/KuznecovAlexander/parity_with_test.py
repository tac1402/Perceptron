import datetime as dt
import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import DataLoader, TensorDataset
import numpy as np

num_inputs = 10
num_samples = 2**num_inputs

indices = torch.arange(num_samples, dtype=torch.float32).unsqueeze(1)

bit_positions = torch.arange(num_inputs, dtype=torch.float32)
periods = 2 ** (bit_positions + 1)

X = ((indices % periods) >= (periods / 2)).float()
y = ((torch.sum(X, dim=1) + 1) % 2).float().unsqueeze(1)

# Перемешаем
ix = torch.randperm(len(indices))
X = X[ix]
y = y[ix]

train_size = int(0.7 * num_samples)
X_train, X_test = X[:train_size], X[train_size:]
y_train, y_test = y[:train_size], y[train_size:]

batch_size = 8
train_dataset = TensorDataset(X_train, y_train)
train_loader = DataLoader(train_dataset, batch_size=batch_size, shuffle=True)
test_dataset = TensorDataset(X_test, y_test)
test_loader = DataLoader(test_dataset, batch_size=1000, shuffle=False)

class ParityNetwork(nn.Module):
    def __init__(self, input_size):
        super(ParityNetwork, self).__init__()
        self.fc1 = nn.Linear(input_size, 192)
        self.fc2 = nn.Linear(192, 8)
        self.fc3 = nn.Linear(8, 1)
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

num_epochs = 3000
for epoch in range(num_epochs):

    model.train()
    train_loss = 0.0
    train_correct = 0
    
    for batch_X, batch_y in train_loader:

        batch_X_cuda = batch_X.cuda()
        batch_y_cuda = batch_y.cuda()
        
        optimizer.zero_grad()
        outputs = model(batch_X_cuda)
        loss = criterion(outputs, batch_y_cuda)
        loss.backward()
        optimizer.step()
        train_loss += loss.item()

        train_predictions = (outputs > 0.5).float()
        train_correct += (train_predictions == batch_y_cuda).sum().item()
        
    train_loss /= len(train_loader)
    train_accuracy = train_correct * 100 / len(X_train)
    
    time = dt.datetime.now()
    print(f'{time} {epoch=}, {train_loss=:0.4f}, accuracy {train_accuracy:.2f}')

    model.eval()
    test_loss = 0.0
    test_correct = 0
    
    with torch.no_grad():
        for batch_X, batch_y in test_loader:
            batch_y_cuda = batch_y.cuda()
            outputs = model(batch_X.cuda())
            loss = criterion(outputs, batch_y_cuda)
            test_loss += loss.item()
            
            predictions = (outputs > 0.5).float()
            test_correct += (predictions == batch_y_cuda).sum().item()

    test_loss /= len(train_loader)
    test_accuracy = test_correct * 100 / len(X_test)
    print(f'{test_loss=:0.4f}, accuracy {test_accuracy:.2f}')

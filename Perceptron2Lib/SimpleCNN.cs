// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;

namespace Tac.Perceptron
{
	public class SimpleCNN
	{
		// Структура для хранения весов фильтров
		struct Filter
		{
			public double[,] weights;
			public double bias;
		}

		// Параметры сети
		private Filter[] filters;
		private double[,] fullyConnectedWeights;
		private double[] fullyConnectedBiases;

		public SimpleCNN()
		{
			InitializeNetwork();
		}

		private void InitializeNetwork()
		{
			// Инициализируем 2 фильтра 2x2 для обнаружения паттернов
			filters = new Filter[2];

			// Фильтр для обнаружения квадратов
			filters[0] = new Filter
			{
				weights = new double[,] { { 0.25, 0.25 }, { 0.25, 0.25 } },
				bias = 0.1
			};

			// Фильтр для обнаружения треугольников
			filters[1] = new Filter
			{
				weights = new double[,] { { 0.3, -0.1 }, { -0.1, 0.3 } },
				bias = 0.1
			};

			// Полносвязный слой: 8 входов (2 фильтра * 2x2) -> 4 выхода (класса)
			fullyConnectedWeights = new double[8, 4];
			fullyConnectedBiases = new double[4];

			// Инициализация случайными небольшими значениями
			Random rand = new Random();
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					fullyConnectedWeights[i, j] = rand.NextDouble() * 0.1 - 0.05;
				}
			}

			for (int i = 0; i < 4; i++)
			{
				fullyConnectedBiases[i] = rand.NextDouble() * 0.1 - 0.05;
			}
		}

		// Сверточный слой
		private double[][,] ConvolutionalLayer(double[,] input)
		{
			int inputSize = 4;
			int filterSize = 2;
			int outputSize = inputSize - filterSize + 1; // 3
			double[][,] outputs = new double[filters.Length][,];

			for (int f = 0; f < filters.Length; f++)
			{
				outputs[f] = new double[outputSize, outputSize];

				for (int i = 0; i < outputSize; i++)
				{
					for (int j = 0; j < outputSize; j++)
					{
						double sum = 0;

						// Применяем фильтр
						for (int ki = 0; ki < filterSize; ki++)
						{
							for (int kj = 0; kj < filterSize; kj++)
							{
								sum += input[i + ki, j + kj] * filters[f].weights[ki, kj];
							}
						}

						// Добавляем смещение и применяем ReLU
						outputs[f][i, j] = Math.Max(0, sum + filters[f].bias);
					}
				}
			}

			return outputs;
		}

		// Подвыборка (Max Pooling)
		private double[][,] PoolingLayer(double[][,] inputs)
		{
			int poolSize = 2;
			double[][,] outputs = new double[inputs.Length][,];

			for (int f = 0; f < inputs.Length; f++)
			{
				int inputSize = inputs[f].GetLength(0);
				int outputSize = inputSize / poolSize;
				outputs[f] = new double[outputSize, outputSize];

				for (int i = 0; i < outputSize; i++)
				{
					for (int j = 0; j < outputSize; j++)
					{
						double maxVal = double.MinValue;

						for (int pi = 0; pi < poolSize; pi++)
						{
							for (int pj = 0; pj < poolSize; pj++)
							{
								int x = i * poolSize + pi;
								int y = j * poolSize + pj;
								if (x < inputSize && y < inputSize)
								{
									maxVal = Math.Max(maxVal, inputs[f][x, y]);
								}
							}
						}

						outputs[f][i, j] = maxVal;
					}
				}
			}

			return outputs;
		}

		// Полносвязный слой с Softmax
		private double[] FullyConnectedLayer(double[] input)
		{
			double[] outputs = new double[4];
			double maxOutput = double.MinValue;

			// Линейная комбинация
			for (int i = 0; i < 4; i++)
			{
				outputs[i] = fullyConnectedBiases[i];

				for (int j = 0; j < input.Length; j++)
				{
					outputs[i] += input[j] * fullyConnectedWeights[j, i];
				}

				maxOutput = Math.Max(maxOutput, outputs[i]);
			}

			// Softmax для стабильности
			double sum = 0;
			for (int i = 0; i < 4; i++)
			{
				outputs[i] = Math.Exp(outputs[i] - maxOutput);
				sum += outputs[i];
			}

			for (int i = 0; i < 4; i++)
			{
				outputs[i] /= sum;
			}

			return outputs;
		}

		// Прямой проход через сеть
		public double[] Forward(double[,] input)
		{
			// Сверточный слой
			double[][,] convOutput = ConvolutionalLayer(input);

			// Слой подвыборки
			double[][,] poolOutput = PoolingLayer(convOutput);

			// Вытягиваем в вектор
			double[] flattened = new double[8]; // 2 фильтра * 2x2
			int index = 0;

			for (int f = 0; f < poolOutput.Length; f++)
			{
				for (int i = 0; i < poolOutput[f].GetLength(0); i++)
				{
					for (int j = 0; j < poolOutput[f].GetLength(1); j++)
					{
						flattened[index++] = poolOutput[f][i, j];
					}
				}
			}

			// Полносвязный слой
			return FullyConnectedLayer(flattened);
		}

		// Обучение сети
		public void Train(double[][,] trainingData, int[] labels, int epochs, double learningRate)
		{
			for (int epoch = 0; epoch < epochs; epoch++)
			{
				double totalLoss = 0;

				for (int sample = 0; sample < trainingData.Length; sample++)
				{
					// Прямой проход
					double[] output = Forward(trainingData[sample]);

					// Вычисляем градиенты (упрощенная версия)
					double[] error = new double[4];
					for (int i = 0; i < 4; i++)
					{
						double target = (i == labels[sample]) ? 1.0 : 0.0;
						error[i] = output[i] - target;
						totalLoss += Math.Abs(error[i]);
					}

					// Простое обновление весов (упрощенный backprop)
					UpdateWeights(error, learningRate);
				}

				if (epoch % 100 == 0)
				{
					Console.WriteLine($"Epoch {epoch}, Loss: {totalLoss / trainingData.Length}");
				}
			}
		}

		private void UpdateWeights(double[] error, double learningRate)
		{
			// Упрощенное обновление весов
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					fullyConnectedWeights[i, j] -= learningRate * error[j];
				}
			}

			for (int i = 0; i < 4; i++)
			{
				fullyConnectedBiases[i] -= learningRate * error[i];
			}
		}

		public int Predict(double[,] input)
		{
			double[] output = Forward(input);
			int maxIndex = 0;

			for (int i = 1; i < output.Length; i++)
			{
				if (output[i] > output[maxIndex])
				{
					maxIndex = i;
				}
			}

			return maxIndex;
		}
	}

}

using System;
using System.Collections.Generic;


namespace BackProp
{
	public class Layer
	{
		public int Precision;

		private int inputSize;
		private int outputSize;

		private Dictionary<int, NArray> weights;
		private NArray biases;

		private Adam weightsOptimizer;
		private Adam biasesOptimizer;

		public Layer(int argInputSize, int argOutputSize, float argLearningRate, Random argRandom, int argPrecision)
		{
			Precision = argPrecision;
			inputSize = argInputSize;
			outputSize = argOutputSize;

			// Инициализация весов и смещений
			weights = new Dictionary<int, NArray>();
			for (int i = 0; i < argInputSize; i++)
			{
				weights.Add(i, new NArray(argOutputSize, Precision));
			}
			biases = new NArray(argOutputSize, Precision);

			for (int j = 0; j < argOutputSize; j++)
			{
				biases[j] = ((float)argRandom.NextDouble() * 2 - 1) * 0.1f;
				for (int i = 0; i < argInputSize; i++)
				{
					weights[i][j] = ((float)argRandom.NextDouble() * 2 - 1) * 0.1f;
				}
			}

			// Инициализация оптимизаторов
			weightsOptimizer = new Adam(weights, Precision, argLearningRate);
			biasesOptimizer = new Adam(biases, Precision, argLearningRate);
		}

		public void SetLearningRate(float learningRate)
		{ 
			weightsOptimizer.LearningRate = learningRate;
			biasesOptimizer.LearningRate = learningRate;
		}

		public void SetAdam(bool isAdam)
		{
			weightsOptimizer.isAdam = isAdam;
			biasesOptimizer.isAdam = isAdam;
		}

		public void ResetAdam()
		{
			weightsOptimizer.TimeStep = 0;
			biasesOptimizer.TimeStep = 0;
		}

		public (NArray preActivation, NArray output) Forward(NArray input, FN activation)
		{
			// Вычисление взвешенной суммы
			NArray preActivation = new NArray(outputSize, Precision);
			for (int j = 0; j < outputSize; j++)
			{
				preActivation[j] = biases[j];
				for (int i = 0; i < inputSize; i++)
				{
					preActivation[j] += input[i] * weights[i][j];
				}
			}

			// Применение функции активации
			NArray output = new NArray(outputSize, Precision);
			for (int j = 0; j < outputSize; j++)
			{
				output[j] = activation(preActivation[j]);
			}

			return (preActivation, output);
		}

		public NArray Backward(NArray outputGradient, NArray previousLayerOutput, NArray preActivation, FN activationDerivative)
		{

			// Вычисляем градиент до активации (умножаем на производную функции активации)
			NArray preActivationGradient = new NArray(outputSize, Precision);
			for (int j = 0; j < outputSize; j++)
			{
				preActivationGradient[j] = outputGradient[j] * activationDerivative(preActivation[j]);
			}

			// Установка градиентов в оптимизаторы
			SetGradients(preActivationGradient, previousLayerOutput);

			// Вычисление ошибки для предыдущего слоя
			NArray previousLayerErrors = new NArray(inputSize, Precision);
			for (int i = 0; i < inputSize; i++)
			{
				for (int j = 0; j < outputSize; j++)
				{
					previousLayerErrors[i] += preActivationGradient[j] * weights[i][j];
				}
			}

			return previousLayerErrors;
		}

		private void SetGradients(NArray outputGradients, NArray input)
		{
			// Установка градиентов для весов
			for (int i = 0; i < inputSize; i++)
			{
				for (int j = 0; j < outputSize; j++)
				{
					weightsOptimizer[i, j] = outputGradients[j] * input[i];
				}
			}

			// Установка градиентов для смещений
			for (int j = 0; j < outputSize; j++)
			{
				biasesOptimizer[j] = outputGradients[j];
			}
		}

		public void Update()
		{
			// Обновление весов и смещений
			weightsOptimizer.Update();
			biasesOptimizer.Update();
		}
	}
}

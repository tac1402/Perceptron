using System;
using System.Collections.Generic;


namespace BackProp
{
	public class Layer
	{
		private int inputSize;
		private int outputSize;

		private Dictionary<int, float[]> weights;
		private float[] biases;

		private Adam weightsOptimizer;
		private Adam biasesOptimizer;

		public Layer(int inputSize, int outputSize, float learningRate, Random random)
		{
			this.inputSize = inputSize;
			this.outputSize = outputSize;

			// Инициализация весов и смещений
			weights = new Dictionary<int, float[]>();
			for (int i = 0; i < inputSize; i++)
			{
				weights.Add(i, new float[outputSize]);
			}
			biases = new float[outputSize];

			for (int j = 0; j < outputSize; j++)
			{
				biases[j] = ((float)random.NextDouble() * 2 - 1) * 0.1f;
				for (int i = 0; i < inputSize; i++)
				{
					weights[i][j] = ((float)random.NextDouble() * 2 - 1) * 0.1f;
				}
			}

			// Инициализация оптимизаторов
			weightsOptimizer = new Adam(weights, learningRate);
			biasesOptimizer = new Adam(biases, learningRate);
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

		public (float[] preActivation, float[] output) Forward(float[] input, Func<float, float> activation)
		{
			// Вычисление взвешенной суммы
			float[] preActivation = new float[outputSize];
			for (int j = 0; j < outputSize; j++)
			{
				preActivation[j] = biases[j];
				for (int i = 0; i < inputSize; i++)
				{
					preActivation[j] += input[i] * weights[i][j];
				}
			}

			// Применение функции активации
			float[] output = new float[outputSize];
			for (int j = 0; j < outputSize; j++)
			{
				output[j] = activation(preActivation[j]);
			}

			return (preActivation, output);
		}

		public float[] Backward(float[] outputGradient, float[] previousLayerOutput, float[] preActivation, Func<float, float> activationDerivative)
		{

			// Вычисляем градиент до активации (умножаем на производную функции активации)
			float[] preActivationGradient = new float[outputSize];
			for (int j = 0; j < outputSize; j++)
			{
				preActivationGradient[j] = outputGradient[j] * activationDerivative(preActivation[j]);
			}

			// Установка градиентов в оптимизаторы
			SetGradients(preActivationGradient, previousLayerOutput);

			// Вычисление ошибки для предыдущего слоя
			float[] previousLayerErrors = new float[inputSize];
			for (int i = 0; i < inputSize; i++)
			{
				for (int j = 0; j < outputSize; j++)
				{
					previousLayerErrors[i] += preActivationGradient[j] * weights[i][j];
				}
			}

			return previousLayerErrors;
		}

		private void SetGradients(float[] outputGradients, float[] input)
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

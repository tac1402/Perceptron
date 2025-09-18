using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackProp
{


	public class NeuralNetwork
	{
		private readonly int inputSize;
		private readonly int hiddenSize;
		private readonly int outputSize;
		private readonly double learningRate;

		private double[,] weightsInputHidden;
		private double[] biasHidden;
		private double[,] weightsHiddenOutput;
		private double[] biasOutput;

		private readonly Random random;

		public NeuralNetwork(int inputSize, int hiddenSize, int outputSize, double learningRate, int seed = 1)
		{
			this.inputSize = inputSize;
			this.hiddenSize = hiddenSize;
			this.outputSize = outputSize;
			this.learningRate = learningRate;

			random = new Random(seed);
			InitializeWeights();
		}

		private void InitializeWeights()
		{
			// Инициализация весов между входным и скрытым слоем
			weightsInputHidden = new double[inputSize, hiddenSize];
			biasHidden = new double[hiddenSize];

			for (int i = 0; i < inputSize; i++)
			{
				for (int j = 0; j < hiddenSize; j++)
				{
					weightsInputHidden[i, j] = random.NextDouble() * 2 - 1;
				}
			}

			for (int i = 0; i < hiddenSize; i++)
			{
				biasHidden[i] = random.NextDouble() * 2 - 1;
			}

			// Инициализация весов между скрытым и выходным слоем
			weightsHiddenOutput = new double[hiddenSize, outputSize];
			biasOutput = new double[outputSize];

			for (int i = 0; i < hiddenSize; i++)
			{
				for (int j = 0; j < outputSize; j++)
				{
					weightsHiddenOutput[i, j] = random.NextDouble() * 2 - 1;
				}
			}

			for (int i = 0; i < outputSize; i++)
			{
				biasOutput[i] = random.NextDouble() * 2 - 1;
			}
		}

		public (double[] hiddenInputs, double[] hiddenOutputs, double[] outputInputs, double[] outputs) Forward(double[] input)
		{
			// Прямое распространение через скрытый слой
			double[] hiddenInputs = new double[hiddenSize];
			double[] hiddenOutputs = new double[hiddenSize];

			for (int j = 0; j < hiddenSize; j++)
			{
				hiddenInputs[j] = biasHidden[j];
				for (int i = 0; i < inputSize; i++)
				{
					hiddenInputs[j] += input[i] * weightsInputHidden[i, j];
				}
				hiddenOutputs[j] = ReLU(hiddenInputs[j]);
			}

			// Прямое распространение через выходной слой
			double[] outputInputs = new double[outputSize];
			double[] outputs = new double[outputSize];

			for (int j = 0; j < outputSize; j++)
			{
				outputInputs[j] = biasOutput[j];
				for (int i = 0; i < hiddenSize; i++)
				{
					outputInputs[j] += hiddenOutputs[i] * weightsHiddenOutput[i, j];
				}
				outputs[j] = Sigmoid(outputInputs[j]);
			}

			return (hiddenInputs, hiddenOutputs, outputInputs, outputs);
		}

		public void Backward(double[] input, double target, double[] hiddenInputs, double[] hiddenOutputs, double[] outputs)
		{
			// Вычисление ошибки выходного слоя
			double error = outputs[0] - target;
			double outputGradient = error * SigmoidDerivative(outputs[0]);

			// Обновление весов между скрытым и выходным слоем
			for (int i = 0; i < hiddenSize; i++)
			{
				weightsHiddenOutput[i, 0] -= learningRate * outputGradient * hiddenOutputs[i];
			}
			biasOutput[0] -= learningRate * outputGradient;

			// Вычисление градиентов для скрытого слоя
			double[] hiddenGradients = new double[hiddenSize];
			for (int j = 0; j < hiddenSize; j++)
			{
				double sum = outputGradient * weightsHiddenOutput[j, 0];
				hiddenGradients[j] = sum * ReLUDerivative(hiddenInputs[j]);
			}

			// Обновление весов между входным и скрытым слоем
			for (int i = 0; i < inputSize; i++)
			{
				for (int j = 0; j < hiddenSize; j++)
				{
					weightsInputHidden[i, j] -= learningRate * hiddenGradients[j] * input[i];
				}
			}

			for (int j = 0; j < hiddenSize; j++)
			{
				biasHidden[j] -= learningRate * hiddenGradients[j];
			}
		}

		public void Train(double[][] inputs, double[] targets, int epochs)
		{
			for (int epoch = 0; epoch < epochs; epoch++)
			{
				double totalLoss = 0;
				int errorCount = 0;

				for (int i = 0; i < inputs.Length; i++)
				{
					// Прямое распространение
					var (hiddenInputs, hiddenOutputs, _, outputs) = Forward(inputs[i]);

					// Вычисление ошибки
					double error = 0;
					error = (outputs[0] - targets[i]) * (outputs[0] - targets[i]);
					totalLoss += error;

					// Подсчет ошибок классификации
					double prediction = outputs[0] > 0.5 ? 1 : 0;
					if (Math.Abs(prediction - targets[i]) > 0.1) // Порог для классификации
					{
						errorCount++;
					}

					// Обратное распространение и обновление весов
					Backward(inputs[i], targets[i], hiddenInputs, hiddenOutputs, outputs);
				}

				// Вывод статистики каждую эпоху (или реже, если нужно)
				//if (epoch % 100 == 0) // Можно изменить частоту вывода
				{
					double averageLoss = totalLoss / inputs.Length;
					double accuracy = (double)(inputs.Length - errorCount) / inputs.Length * 100;
					Console.WriteLine($"Epoch {epoch}, Loss: {averageLoss:F6}, Errors: {errorCount}/{inputs.Length}, Accuracy: {accuracy:F2}%");
				}

				if (errorCount == 0)
				{ 
					break;
				}
			}
		}

		public double Predict(double[] input)
		{
			var (_, _, _, outputs) = Forward(input);
			return outputs[0];
		}

		// Функции активации
		private double ReLU(double x) => Math.Max(0, x);
		private double ReLUDerivative(double x) => x > 0 ? 1 : 0;
		private double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
		private double SigmoidDerivative(double x) => x * (1 - x);
	}

}

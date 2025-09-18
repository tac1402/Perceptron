using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackProp
{
	public class NeuralNetwork
	{

		private Layer inputHiddenLayer;
		private Layer hiddenOutputLayer;

		private Random random;

		public NeuralNetwork(int inputSize, int hiddenSize, int outputSize, float learningRate, int seed = 1)
		{
			random = new Random(seed);

			// Создаем слои с соответствующими оптимизаторами
			inputHiddenLayer = new Layer(inputSize, hiddenSize, learningRate, random);
			hiddenOutputLayer = new Layer(hiddenSize, outputSize, learningRate, random);
		}

		public (float[] hiddenInputs, float[] hiddenOutputs, float[] outputInputs, float[] outputs) Forward(float[] input)
		{
			// Прямое распространение через скрытый слой
			var (hiddenInputs, hiddenOutputs) = inputHiddenLayer.Forward(input, ReLU);

			// Прямое распространение через выходной слой
			var (outputInputs, outputs) = hiddenOutputLayer.Forward(hiddenOutputs, Sigmoid);

			return (hiddenInputs, hiddenOutputs, outputInputs, outputs);
		}

		public void Backward(float[] input, float target, float[] hiddenInputs, float[] hiddenOutputs, float[] outputs)
		{
			// Вычисление градиента выходного слоя
			float[] error = new float[1];
			error[0] = outputs[0] - target;

			// Обратное распространение через выходной слой
			float[] hiddenErrors = hiddenOutputLayer.Backward(error, hiddenOutputs, outputs, SigmoidDerivative);

			// Обратное распространение через скрытый слой
			inputHiddenLayer.Backward(hiddenErrors, input, hiddenInputs, ReLUDerivative);
		}


		public void Update()
		{
			// Обновление весов всех слоев
			inputHiddenLayer.Update();
			hiddenOutputLayer.Update();
		}

		bool is98 = false;
		bool is99 = false;

		public void Train(float[][] inputs, float[] targets, int epochs)
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
					double error = (outputs[0] - targets[i]) * (outputs[0] - targets[i]);
					totalLoss += error;

					// Подсчет ошибок классификации
					double prediction = outputs[0] > 0.5 ? 1 : 0;
					if (Math.Abs(prediction - targets[i]) > 0.1)
					{
						errorCount++;
					}

					// Обратное распространение
					Backward(inputs[i], targets[i], hiddenInputs, hiddenOutputs, outputs);

					// Обновление весов
					Update();
				}

				// Вывод статистики
				if (epoch % 100 == 0)
				{
					double averageLoss = totalLoss / inputs.Length;
					double accuracy = (double)(inputs.Length - errorCount) / inputs.Length * 100;
					Console.WriteLine($"Epoch {epoch}, Loss: {averageLoss:F6}, Errors: {errorCount}/{inputs.Length}, Accuracy: {accuracy:F2}%");

					if (accuracy > 97 && is98 == false)
					{
						inputHiddenLayer.SetLearningRate(0.001f);
						hiddenOutputLayer.SetLearningRate(0.001f);

						inputHiddenLayer.SetAdam(true);
						hiddenOutputLayer.SetAdam(true);

						is98 = true;
					}
					else if (accuracy > 99 && is99 == false)
					{
						inputHiddenLayer.SetLearningRate(0.0005f);
						hiddenOutputLayer.SetLearningRate(0.0005f);

						inputHiddenLayer.ResetAdam();
						hiddenOutputLayer.ResetAdam();

						is99 = true;
					}


				}

				if (errorCount == 0)
				{
					break;
				}
			}
		}

		public float Predict(float[] input)
		{
			var (_, _, _, outputs) = Forward(input);
			return outputs[0];
		}

		public float ReLU(float x) => Math.Max(0, x);
		public float ReLUDerivative(float x) => x > 0 ? 1 : 0;
		public float Sigmoid(float x) => (float)(1.0 / (1.0 + Math.Exp(-x)));
		public float SigmoidDerivative(float x) => x * (1 - x);

	}
}

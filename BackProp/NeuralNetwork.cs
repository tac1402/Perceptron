using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackProp
{
	public class NeuralNetwork
	{
		public int Precision;

		private Layer inputHiddenLayer;
		private Layer hiddenOutputLayer;

		private Random random;

		public NeuralNetwork(int argInputSize, int argHiddenSize, int argOutputSize, float argLearningRate, int argPrecision, int argSeed = 1)
		{
			Precision = argPrecision;
			random = new Random(argSeed);

			// Создаем слои с соответствующими оптимизаторами
			inputHiddenLayer = new Layer(argInputSize, argHiddenSize, argLearningRate, random, argPrecision);
			hiddenOutputLayer = new Layer(argHiddenSize, argOutputSize, argLearningRate, random, argPrecision);
		}

		public (NArray hiddenInputs, NArray hiddenOutputs, NArray outputInputs, NArray outputs) Forward(NArray input)
		{
			// Прямое распространение через скрытый слой
			var (hiddenInputs, hiddenOutputs) = inputHiddenLayer.Forward(input, ReLU);

			// Прямое распространение через выходной слой
			var (outputInputs, outputs) = hiddenOutputLayer.Forward(hiddenOutputs, Sigmoid);

			return (hiddenInputs, hiddenOutputs, outputInputs, outputs);
		}

		public void Backward(NArray input, double lossGradient, NArray hiddenInputs, NArray hiddenOutputs, NArray outputs)
		{
			// Вычисление градиента выходного слоя
			NArray error = new NArray(1, outputs.Precision);
			error[0] = lossGradient; // outputs[0] - target;

			// Обратное распространение через выходной слой
			NArray hiddenErrors = hiddenOutputLayer.Backward(error, hiddenOutputs, outputs, SigmoidDerivative);

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

		public void Train(NArray[] inputs, NArray targets, int epochs)
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

					// Вычисление градиента функции потерь (MSE derivative)
					double lossGradient = 2.0f * (outputs[0] - targets[i]);

					// Обратное распространение
					Backward(inputs[i], lossGradient, hiddenInputs, hiddenOutputs, outputs);

					// Обновление весов
					Update();
				}

				// Вывод статистики
				//if (epoch % 100 == 0)
				{
					//double averageLoss = totalLoss / inputs.Length;
					double accuracy = (double)(inputs.Length - errorCount) / inputs.Length * 100;
					Console.WriteLine($"Epoch {epoch}, Loss: {totalLoss}, Errors: {errorCount}/{inputs.Length}, Accuracy: {accuracy:F2}%");

					/*if (accuracy > 97 && is98 == false)
					{
						inputHiddenLayer.SetLearningRate(0.001f);
						hiddenOutputLayer.SetLearningRate(0.001f);

						inputHiddenLayer.SetAdam(true);
						hiddenOutputLayer.SetAdam(true);

						is98 = true;
					}
					else if (accuracy > 99 && is99 == false)
					{
						inputHiddenLayer.SetLearningRate(0.0002f);
						hiddenOutputLayer.SetLearningRate(0.0002f);

						inputHiddenLayer.ResetAdam();
						hiddenOutputLayer.ResetAdam();

						is99 = true;
					}*/


				}

				if (errorCount == 0)
				{
					break;
				}
			}
		}

		public double Predict(NArray input)
		{
			var (_, _, _, outputs) = Forward(input);
			return outputs[0];
		}

		public dynamic ReLU(dynamic x) => Math.Max(0, x);
		public dynamic ReLUDerivative(dynamic x) => x > 0 ? 1 : 0;
		public dynamic Sigmoid(dynamic x) => (1.0 / (1.0 + Math.Exp(-x)));
		public dynamic SigmoidDerivative(dynamic x) => x * (1 - x);

	}

	public delegate dynamic FN(dynamic number);
}

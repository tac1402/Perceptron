using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackProp
{
	internal class Program
	{
		static void Main(string[] args)
		{
			/*
			// Данные для обучения (XOR)
			double[][] inputs = new double[][]
			{
				new double[] {0, 0},
				new double[] {0, 1},
				new double[] {1, 0},
				new double[] {1, 1}
			};

			double[] targets = new double[] { 0, 1, 1, 0 };
			*/

			//ParityDataset dataset = ParityDataset.CreateXORDataset();
			ParityDataset dataset = ParityDataset.CreateNParityDataset(10);


			// Создание и обучение нейронной сети
			NeuralNetwork network = new NeuralNetwork(
				inputSize: 10,
				hiddenSize: 30,
				outputSize: 1,
				learningRate: 0.01
			);

			network.Train(dataset.features, dataset.labels, epochs: 100000);

			// Тестирование обученной сети
			Console.WriteLine("\nTesting the trained network:");
			int errorCount = 0; 
			for (int i = 0; i < dataset.features.Length; i++)
			{
				double prediction = network.Predict(dataset.features[i]);

				// Округляем предсказание до 0 или 1
				int roundedPrediction = prediction > 0.5 ? 1 : 0;
				int expected = (int)dataset.labels[i];

				if (roundedPrediction != expected)
				{
					errorCount++;
					Console.WriteLine($"Ошибка #{errorCount}: Input: [{dataset.features[i][0]}, {dataset.features[i][1]}], " +
									  $"Prediction: {prediction:F6} ({roundedPrediction}), Expected: {expected}");
				}
			}

		}
	}
}

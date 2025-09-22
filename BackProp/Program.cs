using System;
using System.Collections.Generic;


namespace BackProp
{
	internal class Program
	{
		static void Main(string[] args)
		{
			/*
			Number n = new Number(0.001, 6);
			Number n2 = new Number(10.001, 6);

			Number sum = n * n2;
			*/

			int a = 1;

			//ParityDataset dataset = ParityDataset.CreateXORDataset();
			ParityDataset dataset = ParityDataset.CreateNParityDataset(10);


			// Создание и обучение нейронной сети
			NeuralNetwork network = new NeuralNetwork(
				argInputSize: 10,
				argHiddenSize: 50,
				argOutputSize: 1,
				argLearningRate: 0.01f,
				argPrecision: 7
			);

			network.Train(dataset.features, dataset.labels, epochs: 100000);

		}
	}
}

using System;
using System.Collections.Generic;


namespace BackProp
{
	internal class Program
	{
		static void Main(string[] args)
		{
			//ParityDataset dataset = ParityDataset.CreateXORDataset();
			ParityDataset dataset = ParityDataset.CreateNParityDataset(12);


			// Создание и обучение нейронной сети
			NeuralNetwork network = new NeuralNetwork(
				inputSize: 12,
				hiddenSize: 30,
				outputSize: 1,
				learningRate: 0.01f
			);

			network.Train(dataset.features, dataset.labels, epochs: 100000);

		}
	}
}

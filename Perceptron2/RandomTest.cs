using MathNet.Numerics.Distributions;
using System;
using Tac.Perceptron;

public class RandomTest
{
	public void Run()
	{
		//BinaryDataGenerator.Main();

		int N1 = 100;
		int L = 200;
		int E = 100;

		int Set = 100;

		PerceptronDT net = new PerceptronDT(L, 200, 2, N1, 100);
		net.IsAnalyze = false;
		//net.SinapsXCount = 100;
		//net.SinapsYCount = 100;
		net.sinapsType = PerceptronDT.SinapsType.Random;
		

		//PerceptronTLNL net = new PerceptronTLNL(L, 1000, 2, N1, E);


		string[] dataset = File.ReadAllLines("randomdata.txt");


		float[][] input = new float[N1][];
		sbyte[][] output = new sbyte[N1][];
		for (int i = 0; i < N1; i++)
		{
			input[i] = new float[L];
			output[i] = new sbyte[2];

			string[] set = dataset[i+1 + Set].Split(',');
			for (int j = 0; j < L; j++)
			{
				input[i][j] = int.Parse(set[j + 3]);
			}
			if (set[1] == "True")
			{
				output[i][0] = 1;
				output[i][1] = 0;
			}
			else
			{
				output[i][0] = 0;
				output[i][1] = 1;
			}

			net.JoinStimul(i, input[i], output[i]);
		}

		float[][] inputE = new float[E][];
		sbyte[][] outputE = new sbyte[E][];
		for (int i = 0; i < E; i++)
		{
			inputE[i] = new float[L];
			outputE[i] = new sbyte[2];

			string[] set = dataset[i + 1 + 100 + Set].Split(',');
			for (int j = 0; j < L; j++)
			{
				inputE[i][j] = int.Parse(set[j + 3]);
			}
			if (set[1] == "True")
			{
				outputE[i][0] = 1;
				outputE[i][1] = 0;
			}
			else
			{
				outputE[i][0] = 0;
				outputE[i][1] = 1;
			}

			net.JoinEStimul(i, inputE[i], outputE[i]);
		}

		net.Learned();

	}

}

public class BinaryDataGenerator
{
	public static (int[][] trainX, int[] trainY, int[][] testX, int[] testY) GenerateDataWithIndicatorFunction(int trainSize, int testSize)
	{
		// Генерация исходных данных
		var uniform = new ContinuousUniform(-1.0, 1.0);
		var normal = new Normal(0.0, 1.0);

		// Генерация обучающих данных
		double[] trainX_original = new double[trainSize];
		int[] trainY_original = new int[trainSize];

		for (int i = 0; i < trainSize; i++)
		{
			double z = uniform.Sample();
			trainX_original[i] = z + normal.Sample();
			trainY_original[i] = z > 0 ? 1 : 0;
		}

		// Генерация тестовых данных
		double[] testX_original = new double[testSize];
		int[] testY_original = new int[testSize];

		for (int i = 0; i < testSize; i++)
		{
			double z = uniform.Sample();
			testX_original[i] = z + normal.Sample();
			testY_original[i] = z > 0 ? 1 : 0;
		}

		// Сортировка обучающей выборки для создания порогов
		double[] sortedTrainX = trainX_original.OrderBy(x => x).ToArray();

		// Применение индикаторной функции к обучающим данным
		int[][] trainX = new int[trainSize][];
		for (int i = 0; i < trainSize; i++)
		{
			trainX[i] = new int[trainSize];
			for (int j = 0; j < trainSize; j++)
			{
				trainX[i][j] = trainX_original[i] <= sortedTrainX[j] ? 1 : 0;
			}
		}

		// Применение индикаторной функции к тестовым данным (используем те же пороги - sortedTrainX)
		int[][] testX = new int[testSize][];
		for (int i = 0; i < testSize; i++)
		{
			testX[i] = new int[trainSize]; // используем trainSize порогов
			for (int j = 0; j < trainSize; j++)
			{
				testX[i][j] = testX_original[i] <= sortedTrainX[j] ? 1 : 0;
			}
		}

		return (trainX, trainY_original, testX, testY_original);
	}

	// Альтернативная версия с объединенной сортировкой обучающей и тестовой выборки
	public static (int[][] trainX, int[] trainY, int[][] testX, int[] testY) GenerateDataWithCombinedSort(int trainSize, int testSize)
	{
		// Генерация исходных данных
		var uniform = new ContinuousUniform(-1.0, 1.0);
		var normal = new Normal(0.0, 1.0);

		// Генерация данных
		double[] trainX_original = new double[trainSize];
		int[] trainY_original = new int[trainSize];
		double[] testX_original = new double[testSize];
		int[] testY_original = new int[testSize];

		for (int i = 0; i < trainSize; i++)
		{
			double z = uniform.Sample();
			trainX_original[i] = z + normal.Sample();
			trainY_original[i] = z > 0 ? 1 : 0;
		}

		for (int i = 0; i < testSize; i++)
		{
			double z = uniform.Sample();
			testX_original[i] = z + normal.Sample();
			testY_original[i] = z > 0 ? 1 : 0;
		}

		// Объединение и сортировка всех данных для создания порогов
		double[] allX = trainX_original.Concat(testX_original).ToArray();
		Array.Sort(allX);

		// Применение индикаторной функции
		int[][] trainX = new int[trainSize][];
		for (int i = 0; i < trainSize; i++)
		{
			trainX[i] = new int[allX.Length];
			for (int j = 0; j < allX.Length; j++)
			{
				trainX[i][j] = trainX_original[i] <= allX[j] ? 1 : 0;
			}
		}

		int[][] testX = new int[testSize][];
		for (int i = 0; i < testSize; i++)
		{
			testX[i] = new int[allX.Length];
			for (int j = 0; j < allX.Length; j++)
			{
				testX[i][j] = testX_original[i] <= allX[j] ? 1 : 0;
			}
		}

		return (trainX, trainY_original, testX, testY_original);
	}

	// Пример использования
	public static void Main()
	{
		int trainSize = 5;  // малый размер для демонстрации
		int testSize = 3;

		var (trainX, trainY, testX, testY) = GenerateDataWithIndicatorFunction(trainSize, testSize);

		Console.WriteLine("Обучающие данные (бинарные признаки):");
		for (int i = 0; i < trainSize; i++)
		{
			Console.Write($"Пример {i}: X = [");
			Console.Write(string.Join(" ", trainX[i]));
			Console.WriteLine($"], Y = {trainY[i]}");
		}

		Console.WriteLine("\nТестовые данные (бинарные признаки):");
		for (int i = 0; i < testSize; i++)
		{
			Console.Write($"Пример {i}: X = [");
			Console.Write(string.Join(" ", testX[i]));
			Console.WriteLine($"], Y = {testY[i]}");
		}

		// Демонстрация с объединенной сортировкой
		Console.WriteLine("\n=== С объединенной сортировкой ===");
		var (trainX2, trainY2, testX2, testY2) = GenerateDataWithCombinedSort(trainSize, testSize);

		Console.WriteLine("Обучающие данные (бинарные признаки):");
		for (int i = 0; i < trainSize; i++)
		{
			Console.Write($"Пример {i}: X = [");
			Console.Write(string.Join(" ", trainX2[i]));
			Console.WriteLine($"], Y = {trainY2[i]}");
		}
	}

	// Вспомогательный метод для преобразования в одномерный массив (если нужно)
	public static int[] FlattenFeatures(int[][] data)
	{
		return data.SelectMany(x => x).ToArray();
	}
}
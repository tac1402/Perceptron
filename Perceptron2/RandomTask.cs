using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Tac.Perceptron;

public class RandomTask
{
	float radius = 2.0f;

	public void Run()
	{
		int nSamples = 1600;
		Random random = new Random(42);

		// Генерация точек в круге
		var points = new List<(float x, float y, float r)>();
		for (int i = 0; i < nSamples; i++)
		{
			float r = (float)Math.Sqrt((float)random.NextDouble() * (radius * radius));
			float theta = (float)random.NextDouble() * 2 * (float)Math.PI;
			float x = r * (float)Math.Cos(theta);
			float y = r * (float)Math.Sin(theta);
			points.Add((x, y, r));
		}

		// Создание меток в формате bool
		bool[] labels = points.Select(p => p.r < 1.0f).ToArray();
		int noiseCount = (int)(nSamples * 0.2f);
		var noiseIndices = Enumerable.Range(0, nSamples)
			.OrderBy(_ => random.Next())
			.Take(noiseCount);

		// Добавление шума (инверсия меток)
		foreach (int idx in noiseIndices)
		{
			labels[idx] = !labels[idx];
		}
		

		// Создание матрицы признаков
		//float[][] X = points.Select(p => new[] { p.x, p.y }).ToArray();

		// Создание матрицы признаков и преобразование в бинарный формат
		BinaryPoint[] X_binary = points.Select(p => PointToBinaryVector(p.x, p.y)).ToArray();

		// Стратифицированное разделение
		var stratifiedData = Enumerable.Range(0, nSamples)
			.GroupBy(i => labels[i])
			.SelectMany(g => g.OrderBy(_ => random.Next()))
			.ToArray();

		int trainSize = nSamples / 2;
		var trainIndices = new List<int>();
		var testIndices = new List<int>();

		// Группируем по классам и обрабатываем каждую группу отдельно
		var groups = Enumerable.Range(0, nSamples)
			.GroupBy(i => labels[i])
			.ToList();

		// Распределение с сохранением пропорций
		foreach (var group in groups)
		{
			var shuffledGroup = group.OrderBy(_ => random.Next()).ToList();
			int groupCount = shuffledGroup.Count;
			int trainCount = groupCount / 2;

			// Для четного распределения добавляем по одному элементу в train из групп с нечетным количеством
			if (groupCount % 2 == 1 && trainIndices.Count + trainCount < nSamples / 2)
			{
				trainCount++;
			}

			trainIndices.AddRange(shuffledGroup.Take(trainCount));
			testIndices.AddRange(shuffledGroup.Skip(trainCount));
		}


		// Формирование окончательных выборок
		var X_train = trainIndices.Select(i => X_binary[i]).ToArray();
		var X_test = testIndices.Select(i => X_binary[i]).ToArray();
		var y_train = trainIndices.Select(i => labels[i]).ToArray();
		var y_test = testIndices.Select(i => labels[i]).ToArray();

		int N1 = nSamples / 2;
		int L = 64;
		int E = nSamples / 2;

		NeironNetTree net = new NeironNetTree(L, 800, 2, N1, E);
		net.IsAnalyze = false;
		net.sinapsType = NeironNetTree.SinapsType.Full;


		BitBlock[] outputE = new BitBlock[E];

		float[][] x_testBin = new float[E][];
		for (int i = 0; i < E; i++)
		{
			outputE[i] = new BitBlock(2);
			if (y_test[i] == false)
			{
				outputE[i][0] = true;
				outputE[i][1] = false;
			}
			else
			{
				outputE[i][0] = false;
				outputE[i][1] = true;
			}

			X_test[i].X.To();
			X_test[i].Y.To();

			x_testBin[i] = new float[L];
			for (int j = 0; j < L; j++)
			{
				if (j < 32)
				{
					x_testBin[i][j] = X_test[i].X.DataF[j];
				}
				else
				{
					x_testBin[i][j] = X_test[i].Y.DataF[j - 32];
				}
			}

			net.JoinEStimul(i, x_testBin[i], outputE[i]);
		}

		BitBlock[] output = new BitBlock[N1];
		float[][] x_trainBin = new float[N1][];
		for (int i = 0; i < N1; i++)
		{
			output[i] = new BitBlock(2);
			if (y_train[i] == false)
			{
				output[i][0] = true;
				output[i][1] = false;
			}
			else
			{
				output[i][0] = false;
				output[i][1] = true;
			}

			X_train[i].X.To();
			X_train[i].Y.To();

			x_trainBin[i] = new float[L];
			for (int j = 0; j < L; j++)
			{
				if (j < 32)
				{
					x_trainBin[i][j] = X_train[i].X.DataF[j];
				}
				else
				{
					x_trainBin[i][j] = X_train[i].Y.DataF[j - 32];
				}
			}


			net.JoinStimul(i, x_trainBin[i], output[i]);
		}

		net.Learned();
		net.Examin(E);

	}

	// Структура для хранения бинарного представления точки
	public struct BinaryPoint
	{
		public BitBlock X;
		public BitBlock Y;
	}

	// Преобразование точки в бинарное представление
	BinaryPoint PointToBinaryVector(float x, float y)
	{
		return new BinaryPoint
		{
			X = FloatToBinary(x, -radius, radius, 32),  // Диапазон для x
			Y = FloatToBinary(y, -radius, radius, 32)   // Диапазон для y
		};
	}

	// Преобразование float в 16-битное представление с использованием BitVector32
	static BitBlock FloatToBinary(float value, float min, float max, int bitCount = 16)
	{
		// Нормализация значения в диапазон [0, 65535]
		float normalized = (value - min) / (max - min);
		normalized = Math.Max(0, Math.Min(1, normalized)); // Ограничение в диапазоне [0,1]
		ushort intValue = (ushort)(normalized * 65535);

		BitBlock vector = new BitBlock(bitCount);
		for (int i = 0; i < bitCount; i++)
		{
			int bit = (intValue >> (bitCount - 1 - i)) & 1;
			if (bit == 1)
			{
				vector[i] = true;
			}
		}

		return vector;
	}


}
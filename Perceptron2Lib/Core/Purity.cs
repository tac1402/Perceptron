// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Perceptron
{
	public class Purity
	{

		private Dictionary<int, sbyte[]> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public int[] reactions;
		public int[] reactions2;

		//private Hamming Hamming = new Hamming();

		public Purity(Dictionary<int, sbyte[]> argNecessaryReactions)
		{
			NecessaryReactions = argNecessaryReactions;

			if (NecessaryReactions != null)
			{
				Dictionary<string, int> r = new Dictionary<string, int>();
				reactions = new int[argNecessaryReactions.Count];
				int rCount = 0;
				for (int i = 0; i < NecessaryReactions.Count; i++)
				{
					string key = ReactionToString(NecessaryReactions[i]);

					if (r.ContainsKey(key) == false)
					{
						r.Add(key, rCount);
						rCount++;
					}
					reactions[i] = r[key];
				}
			}
		}

		public string ReactionToString(sbyte[] sample)
		{
			string s = "";
			for (int i = 0; i < sample.Length; i++)
			{
				if (sample[i] == 1)
					s += "1";
				else
					s += "0";
			}
			return s;
		}

		public void SelectReaction(List<int> select)
		{
			reactions2 = new int[select.Count];
			int j = 0;
			foreach (int i in select)
			{ 
				reactions2[j] = reactions[i];
				j++;
			}
		}

		/// <summary>
		/// Считает количество линейных регионов (комбинаций бинарных состояний, предоставляемых скрытым слоем)
		/// </summary>
		public int LinearRegions(Dictionary<int, float[]> argActivations)
		{
			List<string> regions = new List<string>();
			for (int i = 0; i < argActivations.Count; i++)
			{
				string aLine = "";
				for (int j = 0; j < argActivations[i].Length; j++)
				{
					if (argActivations[i][j] > 0)
					{
						aLine += "1";
					}
					else
					{
						aLine += "0";
					}
				}
				if (regions.Contains(aLine) == false)
				{
					regions.Add(aLine);
				}
			}
			return regions.Count;
		}


		/*
		private const float HammingThreshold = 0.000001f; // 1e-6

		/// <summary>
		/// Расстояние Хэмминга для бинарных векторов
		/// </summary>
		public int HammingDistance(float[] a, float[] b)
		{
			if (a.Length != b.Length)
				throw new ArgumentException("Vectors must have the same length");

			int distance = 0;
			for (int i = 0; i < a.Length; i++)
			{
				if (Math.Abs(a[i] - b[i]) > HammingThreshold) // считаем как различие 0/1
					distance++;
			}
			return distance;
		}*/


		public float avgPairwise = 0; // среднее попарное расстояние (Хэмминг)
		public float avgPurity = 0;
		public float minPurity = 0;
		public float maxPurity = 0;

		public PurityDistribution Distribution = new PurityDistribution();

		public List<(int index, int distance)>[] neighborsByPoint;

		/// <summary>
		/// Анализ линейной разделимости на основе "чистоты окрестностей"
		/// </summary>
		public void NeighborhoodPurity(Dictionary<int, float[]> activations, int k = 512, int ClassesCount = 2)
		{
			int nSamples = activations.Count;

			// Сразу создаем отсортированные списки соседей для каждой точки
			neighborsByPoint = ComputeSortedNeighbors(activations);

			float[] purityScores = new float[nSamples];

			for (int i = 0; i < nSamples; i++)
			{
				// Берем k ближайших соседей из предварительно отсортированного списка
				var kNearest = neighborsByPoint[i].Take(k).ToList();

				// Определяем "чистоту" окрестности
				int sameClassCount = kNearest.Count(neighbor => reactions2[neighbor.index] == reactions2[i]);
				float purity = (float)sameClassCount / k;
				purityScores[i] = purity;
			}

			avgPurity = purityScores.Average();
			minPurity = purityScores.Min();
			maxPurity = purityScores.Max();

			if (ClassesCount > 2)
			{
				Distribution = CalculatePurityDistribution(purityScores, ClassesCount);
			}
		}

		/// <summary>
		/// Создает отсортированные списки соседей для каждой точки
		/// </summary>
		private List<(int index, int distance)>[] ComputeSortedNeighbors(Dictionary<int, float[]> activations)
		{
			int nSamples = activations.Count;
			var neighborsByPoint = new List<(int index, int distance)>[nSamples];

			// Инициализируем списки
			for (int i = 0; i < nSamples; i++)
			{
				neighborsByPoint[i] = new List<(int index, int distance)>();
			}

			double totalDistance = 0.0;
			int pairCount = 0;

			// Заполняем списки, используя симметричность расстояния
			for (int i = 0; i < nSamples; i++)
			{
				for (int j = i + 1; j < nSamples; j++)
				{
					int distance = Hamming.Calculate(activations[i], activations[j]);

					neighborsByPoint[i].Add((j, distance));
					neighborsByPoint[j].Add((i, distance));

					totalDistance += distance;
					pairCount++;
				}
			}
			avgPairwise = (float) (totalDistance / pairCount);

			// Сортируем списки соседей по расстоянию
			for (int i = 0; i < nSamples; i++)
			{
				neighborsByPoint[i] = neighborsByPoint[i]
					.OrderBy(nd => nd.distance)
					.ToList();
			}

			return neighborsByPoint;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Рассчитывает распределение purity scores с конвертацией в бинарный эквивалент
		/// </summary>
		public static PurityDistribution CalculatePurityDistribution(float[] purityScores, int numClasses)
		{
			// Конвертируем все значения в бинарный эквивалент
			var binaryScores = purityScores.Select(p => PurityToBinary(p, numClasses)).ToArray();
			var sortedBinaryScores = binaryScores.OrderBy(p => p).ToArray();
			int n = sortedBinaryScores.Length;

			return new PurityDistribution
			{
				// Перцентили конвертированных значений
				Percentile10 = GetPercentile(sortedBinaryScores, 0.10f),
				Percentile25 = GetPercentile(sortedBinaryScores, 0.25f),
				Percentile50 = GetPercentile(sortedBinaryScores, 0.50f),
				Percentile75 = GetPercentile(sortedBinaryScores, 0.75f),
				Percentile90 = GetPercentile(sortedBinaryScores, 0.90f),

				// Основные статистики
				Mean = binaryScores.Average(),
				StdDev = CalculateStdDev(binaryScores)
			};
		}


		/// <summary>
		/// Преобразует purity из N-классовой задачи в эквивалентное значение для 2 классов
		/// </summary>
		/// <param name="purityN">Измеренная чистота для N классов</param>
		/// <param name="numClasses">Количество классов в исходной задаче (N)</param>
		/// <returns>Эквивалентное значение purity для 2 классов</returns>
		public static float PurityToBinary(float purityN, int numClasses)
		{
			if (numClasses < 2)
				throw new ArgumentException("Number of classes must be at least 2", nameof(numClasses));

			if (purityN < 0f || purityN > 1f)
				throw new ArgumentException("Purity must be between 0 and 1", nameof(purityN));

			// Случайный уровень для N классов
			float baselineN = 1f / numClasses;

			// Случайный уровень для 2 классов
			float baseline2 = 0.5f;

			// Если измеренное значение равно случайному уровню, возвращаем 0.5
			if (Math.Abs(purityN - baselineN) < float.Epsilon)
				return baseline2;

			// Нормализованная чистота (сколько процентов от максимума сверх случайного уровня)
			float normalizedPurity = (purityN - baselineN) / (1f - baselineN);

			// Преобразуем в эквивалент для 2 классов
			float purity2 = baseline2 + normalizedPurity * (1f - baseline2);

			// Обеспечиваем границы [0, 1]
			return Math.Max(0f, Math.Min(1f, purity2));
		}

		private static float GetPercentile(float[] sortedData, float percentile)
		{
			int index = (int)Math.Ceiling(percentile * sortedData.Length) - 1;
			index = Math.Max(0, Math.Min(index, sortedData.Length - 1));
			return sortedData[index];
		}

		private static float CalculateStdDev(float[] values)
		{
			float avg = values.Average();
			float sumSq = values.Sum(v => (v - avg) * (v - avg));
			return (float)Math.Sqrt(sumSq / values.Length);
		}


	}

	public class PurityDistribution
	{
		public float Percentile10; // 10% самых проблемных точек
		public float Percentile25; // 25% самых проблемных точек
		public float Percentile50; // Медиана
		public float Percentile75; // 25% лучших точек
		public float Percentile90; // 10% лучших точек
		public float Mean; // Среднее
		public float StdDev; // Стандартное отклонение

		public string InfoA
		{
			get 
			{
				return "\tP: " + Mean.ToString("F4") + " ± " + StdDev.ToString("F4")
					+ "\t10:" + Percentile10.ToString("F4") + "\t25:" + Percentile25.ToString("F4") + "\t50:" + Percentile50.ToString("F4")
					+ "\t75:" + Percentile75.ToString("F4") + "\t90:" + Percentile90.ToString("F4");
			}
		}
		public string InfoB
		{
			get
			{
				return "\t" + Mean.ToString("F4") + "\t" + StdDev.ToString("F4")
					+ "\t" + Percentile10.ToString("F4") + "\t" + Percentile25.ToString("F4") + "\t" + Percentile50.ToString("F4")
					+ "\t" + Percentile75.ToString("F4") + "\t" + Percentile90.ToString("F4");
			}
		}

	}

}

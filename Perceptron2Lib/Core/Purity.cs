// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Perceptron
{
	public class Purity
	{

		private Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public int[] reactions;

		//private Hamming Hamming = new Hamming();

		public Purity(Dictionary<int, BitBlock> argNecessaryReactions)
		{
			NecessaryReactions = argNecessaryReactions;

			if (NecessaryReactions != null)
			{
				Dictionary<string, int> r = new Dictionary<string, int>();
				reactions = new int[argNecessaryReactions.Count];
				int rCount = 0;
				for (int i = 0; i < NecessaryReactions.Count; i++)
				{
					string key = NecessaryReactions[i].ToString();

					if (r.ContainsKey(key) == false)
					{
						r.Add(key, rCount);
						rCount++;
					}
					reactions[i] = r[key];
				}
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
		}


		public float avgPairwise = 0; // среднее попарное расстояние (Хэмминг)
		public float avgPurity = 0;
		public float minPurity = 0;
		public float maxPurity = 0;

		public List<(int index, int distance)>[] neighborsByPoint;

		/// <summary>
		/// Анализ линейной разделимости на основе "чистоты окрестностей"
		/// </summary>
		public void NeighborhoodPurity(Dictionary<int, float[]> activations, int k = 512)
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
				int sameClassCount = kNearest.Count(neighbor => reactions[neighbor.index] == reactions[i]);
				float purity = (float)sameClassCount / k;
				purityScores[i] = purity;
			}

			avgPurity = purityScores.Average();
			minPurity = purityScores.Min();
			maxPurity = purityScores.Max();
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
					int distance = HammingDistance(activations[i], activations[j]);
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

	}
}

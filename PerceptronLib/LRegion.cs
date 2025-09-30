using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tac.Perceptron;

namespace PerceptronLib
{
	public class LRegion
	{
		private Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public int[] reactions;

		public LRegion(Dictionary<int, BitBlock> argNecessaryReactions)
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
			int a = 1;
		}


		public int Calc(Dictionary<int, float[]> argActivations)
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

		// Расстояние Хэмминга для бинарных векторов
		public static int HammingDistance(float[] a, float[] b)
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

		/// <summary>
		/// Вычисляет изменение расстояния Хэмминга при изменении одного признака
		/// </summary>
		private int HammingChange(float otherValue, float oldValue, float newValue)
		{
			// Определяем, изменилось ли расстояние для этой пары
			bool oldMatch = Math.Abs(oldValue - otherValue) < HammingThreshold;
			bool newMatch = Math.Abs(newValue - otherValue) < HammingThreshold;

			int distanceChange = 0; // Без изменений
			if (oldMatch && !newMatch)
			{
				distanceChange = 1; // Расстояние увеличилось на 1
			}
			else if (!oldMatch && newMatch)
			{
				distanceChange = -1; // Расстояние уменьшилось на 1
			}
			return distanceChange;
		}

		public double avgPairwise = 0; // среднее попарное расстояние (Хэмминг)
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
			avgPairwise = totalDistance / pairCount;

			// Сортируем списки соседей по расстоянию
			for (int i = 0; i < nSamples; i++)
			{
				neighborsByPoint[i] = neighborsByPoint[i]
					.OrderBy(nd => nd.distance)
					.ToList();
			}

			return neighborsByPoint;
		}

		public List<(int index, int distance)>[] CopyNeighborsByPoint()
		{
			if (neighborsByPoint == null) return null;

			var copy = new List<(int index, int distance)>[neighborsByPoint.Length];

			for (int i = 0; i < neighborsByPoint.Length; i++)
			{
				if (neighborsByPoint[i] != null)
				{
					// Создаем новый список с теми же элементами
					copy[i] = new List<(int index, int distance)>(neighborsByPoint[i]);
				}
			}

			return copy;
		}

		public bool IsImproved(
			List<(int index, int distance)>[] neighborsByPointBefore,
			List<(int index, int distance)>[] neighborsByPointAfter,
			int targetStimulNumber)
		{
			var neighborsBefore = neighborsByPointBefore[targetStimulNumber];
			var neighborsAfter = neighborsByPointAfter[targetStimulNumber];

			if (neighborsBefore == null || neighborsAfter == null)
				return false;

			// Простая эвристика: улучшение, если уменьшилось расстояние до 5 ближайших соседей
			double scoreBefore = neighborsBefore.Take(512).Average(n => n.distance);
			double scoreAfter = neighborsAfter.Take(512).Average(n => n.distance);

			return scoreAfter < scoreBefore;
		}

	}

}

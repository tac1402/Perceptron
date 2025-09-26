using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptronLib
{
	public class LRegion
	{

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

		// Функция для вычисления минимального попарного расстояния (Хэмминг)
		public double MinPairwiseDistance(Dictionary<int, float[]> activations)
		{
			var keys = activations.Keys.ToList();
			int n = keys.Count;
			if (n < 2) return 0.0;

			int minDistance = int.MaxValue;
			for (int i = 0; i < n; i++)
			{
				for (int j = i + 1; j < n; j++)
				{
					float[] vec1 = activations[keys[i]];
					float[] vec2 = activations[keys[j]];

					if (IsEmpty(vec1) == false && IsEmpty(vec2) == false)
					{
						int distance = HammingDistance(vec1, vec2);
						if (distance < minDistance)
						{
							minDistance = distance;
						}
					}
				}
			}

			return minDistance;
		}

		private bool IsEmpty(float[] vec)
		{
			bool ret = false;
			int k = 0;
			for (int i = 0; i < vec.Length; i++)
			{
				if (vec[i] == 0)
				{ 
					k++;
				}
			}
			if (k == vec.Length)
			{ 
				ret = true;
			}
			return ret;
		}

		// Функция для вычисления среднего попарного расстояния (Хэмминг)
		public double MeanPairwiseDistance(Dictionary<int, float[]> activations)
		{
			var keys = activations.Keys.ToList();
			int n = keys.Count;
			if (n < 2) return 0.0;

			double totalDistance = 0.0;
			int pairCount = 0;

			for (int i = 0; i < n; i++)
			{
				for (int j = i + 1; j < n; j++)
				{
					float[] vec1 = activations[keys[i]];
					float[] vec2 = activations[keys[j]];

					totalDistance += HammingDistance(vec1, vec2);
					pairCount++;
				}
			}

			return totalDistance / pairCount;
		}

		// Расстояние Хэмминга для бинарных векторов
		int HammingDistance(float[] a, float[] b)
		{
			if (a.Length != b.Length)
				throw new ArgumentException("Vectors must have the same length");

			int distance = 0;
			for (int i = 0; i < a.Length; i++)
			{
				if (Math.Abs(a[i] - b[i]) > 1e-6) // считаем как различие 0/1
					distance++;
			}
			return distance;
		}

	}

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptronLib
{
	public class NeighborhoodAnalyzer
	{
		public class ClusterAnalysisResult
		{
			public int ClusterId;
			public int[] Center;
			public double Purity;
			public int Size;
			public List<int> MemberIndices;
		}

		public List<ClusterAnalysisResult> AnalyzeClusters(Dictionary<int, float[]> activations, int[] labels)
		{
			var results = new List<ClusterAnalysisResult>();
			int n = activations.Count;
			var visited = new bool[n];

			for (int i = 0; i < n; i++)
			{
				if (!visited[i])
				{
					var cluster = new ClusterAnalysisResult
					{
						ClusterId = results.Count,
						MemberIndices = new List<int>()
					};

					// Находим все точки в окрестности текущей точки
					FindCluster(activations, visited, i, cluster);

					if (cluster.MemberIndices.Count > 0)
					{
						cluster.Center = CalculateClusterCenter(activations, cluster.MemberIndices);
						cluster.Purity = CalculateClusterPurity(labels, cluster.MemberIndices);
						cluster.Size = cluster.MemberIndices.Count;
						results.Add(cluster);
					}
				}
			}

			return results.OrderByDescending(c => c.Purity).ToList();
		}

		private void FindCluster(Dictionary<int, float[]> activations, bool[] visited, int startIndex, ClusterAnalysisResult cluster)
		{
			var queue = new Queue<int>();
			queue.Enqueue(startIndex);
			visited[startIndex] = true;

			while (queue.Count > 0)
			{
				int current = queue.Dequeue();
				cluster.MemberIndices.Add(current);

				// Ищем соседей в радиусе 2 (по Хэммингу)
				for (int i = 0; i < activations.Count; i++)
				{
					if (!visited[i] && LRegion.HammingDistance(activations[current], activations[i]) <= 2)
					{
						visited[i] = true;
						queue.Enqueue(i);
					}
				}
			}
		}

		private int[] CalculateClusterCenter(Dictionary<int, float[]> activations, List<int> members)
		{
			if (members.Count == 0) return new int[0];

			int length = activations[0].Length;
			int[] center = new int[length];

			for (int i = 0; i < length; i++)
			{
				int sum = 0;
				foreach (int idx in members)
				{
					int s = 0; if (activations[idx][i] > 0) s = 1;
					sum += s;
				}
				center[i] = sum > members.Count / 2 ? 1 : 0;
			}

			return center;
		}

		private double CalculateClusterPurity(int[] labels, List<int> members)
		{
			if (members.Count == 0) return 0;

			Dictionary<int, int> labelCounts = new Dictionary<int, int>();
			foreach (int idx in members)
			{
				int label = labels[idx];
				if (labelCounts.ContainsKey(label) == false)
				{ 
					labelCounts.Add(label, 0);
				}
				labelCounts[label] ++;
			}

			int maxCount = labelCounts.Values.Max();
			return (double)maxCount / members.Count;
		}

	}
}

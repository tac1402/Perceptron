// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


namespace Tac.Perceptron
{
	/// <summary>
	/// Анализатор перцептрона с помощью ID3
	/// </summary>
	public class PerceptronID3
	{
		private DecisionTreeID3 id3;
		public Graph graphP;
		public Graph graphN;

		private int ACount;

		public int[] Result;

		public void printNode(List<int> root)
		{
			for (int i = 0; i < root.Count; i++)
			{
				int index = root[i];
				Result[index] = 1;
			}
			Console.WriteLine(root.Count.ToString());
		}

		(Dictionary<int, BitBlock>, bool[]) getSamples(int ReactionCount, Dictionary<int, float[]> argActivations, Dictionary<int, BitBlock> argNecessaryReactions,
			int RNumber, int argFrom, int argTill)
		{
			Dictionary<int, BitBlock> result = new Dictionary<int, BitBlock>();

			bool[] samplesClass = new bool[ReactionCount];

			for (int i = 0; i < ReactionCount; i++)
			{
				BitBlock sensor = new BitBlock(argTill - argFrom);
				bool reaction;

				for (int j = 0; j < argActivations[i].Length; j++)
				{
					if (argActivations[i][j] > 0)
					{
						sensor[j] = true;
					}
				}

				reaction = argNecessaryReactions[i][RNumber];

				result.Add(i, sensor);
				samplesClass[i] = reaction;
			}


			return (result, samplesClass);
		}

		public void Analyze(int argACount, int argHCount, Dictionary<int, float[]> argActivations,
			Dictionary<int, BitBlock> argNecessaryReactions, int argRNumber, int argFrom, int argTill)
		{
			ACount = argACount;
			Result = new int[ACount];

			int[] attributes = new int[argTill - argFrom];

			int k = 0;
			for (int i = 0; i < ACount; i++)
			{
				if (i >= argFrom && i < argTill)
				{
					attributes[k] = i - argFrom;
					k++;
				}
			}

			Console.Write(".");

			Dictionary<int, BitBlock> samples;
			bool[] samplesClass;
			(samples, samplesClass) = getSamples(argHCount, argActivations, argNecessaryReactions, argRNumber, argFrom, argTill);

			//id3 = new DecisionTreeID3(argFrom);
			id3 = new DecisionTreeID3(0);
			id3.graphP = graphP;
			id3.mountTree(samples, samplesClass, attributes, 0, null);

			printNode(id3.root);

			graphP = id3.graphP;

			/*for (int i = 0; i < ACount; i++)
			{
				if (Result[i] == 1)
				{
					Console.WriteLine(i.ToString());
				}
			}*/
			//Console.ReadLine();
		}
	}


	/// <summary>
	/// Класс, реализующий дерево решений с использованием алгоритма ID3
	/// </summary>
	internal class DecisionTreeID3
	{
		public List<int> root = new List<int>();
		public Graph graphP;
		public Graph graphN;

		private int total = 0;
		private double entropySet = 0.0;
		private int from = 0;

		public DecisionTreeID3(int argFrom)
		{
			from = argFrom;
			graphP = new Graph();
			graphN = new Graph();
		}

		/// <summary>
		/// Возвращает общее количество положительных образцов в таблице образцов
		/// </summary>
		private int countTotalPositives(bool[] samplesClass)
		{
			int result = 0;
			foreach (bool value in samplesClass)
			{
				if (value == true) { result++; }
			}
			return result;
		}

		/// <summary>
		/// Рассчитавает энтропию по следующей формуле:
		/// -p+log2p+ - p-log2p-
		/// 
		/// где: p+ — доля положительных значений
		///		 p- — доля отрицательных значений
		/// </summary>
		/// <param name="positives">Количество положительных значений</param>
		/// <param name="negatives">Количество отрицательных значений</param>
		/// <returns>Возвращает значение энтропии</returns>
		private float calcEntropy(int positives, int negatives)
		{
			int total = positives + negatives;
			float ratioPositive = (float)positives / total;
			float ratioNegative = (float)negatives / total;

			if (ratioPositive != 0)
			{
				ratioPositive = -(ratioPositive) * (float)Math.Log(ratioPositive, 2);
			}
			if (ratioNegative != 0)
			{
				ratioNegative = -(ratioNegative) * (float)Math.Log(ratioNegative, 2);
			}

			float result = ratioPositive + ratioNegative;

			return result;
		}

		/// <summary>
		/// Просматривает таблицу образцов, проверяя атрибут и является ли результат положительным или отрицательным
		/// </summary>
		/// <param name="value">допустимое значение для атрибута</param>
		/// <param name="positives">количество всех атрибутов с положительным значением</param>
		/// <param name="negatives">количество всех атрибутов с отрицательным значением</param>
		private void getValuesToAttribute(Dictionary<int, BitBlock> argSamples, bool[] argSamplesClass,
			int attribute, bool value, out int positives, out int negatives)
		{
			positives = 0;
			negatives = 0;
			for (int i = 0; i < argSamples.Count; i++)
			{
				if (argSamples[i][attribute] == value)
				{
					if (argSamplesClass[i] == true)
					{
						positives++;
					}
					else
					{
						negatives++;
					}
				}
			}
		}

		/// <summary>
		/// Рассчитывает gain атрибута
		/// </summary>
		/// <param name="attribute">Атрибут для расчета</param>
		private double gain(Dictionary<int, BitBlock> samples, bool[] samplesClass, int attribute)
		{
			double sum = 0.0;

			int positives, negatives;
			double entropy;

			positives = negatives = 0;

			getValuesToAttribute(samples, samplesClass, attribute, true, out positives, out negatives);

			entropy = calcEntropy(positives, negatives);
			sum += -(double)(positives + negatives) / total * entropy;

			positives = negatives = 0;

			getValuesToAttribute(samples, samplesClass, attribute, false, out positives, out negatives);

			entropy = calcEntropy(positives, negatives);
			sum += -(double)(positives + negatives) / total * entropy;

			return entropySet + sum;
		}

		/// <summary>
		/// Возвращает лучший атрибут (с наибольшим gain)
		/// </summary>
		/// <param name="attributes">Вектор с атрибутами</param>
		private int getBestAttribute(Dictionary<int, BitBlock> samples, bool[] samplesClass, int[] attributes)
		{
			double maxGain = 0.0;
			int result = attributes[0];

			for (int i = 0; i < attributes.Length; i++)
			{
				double locGain = gain(samples, samplesClass, attributes[i]);

				if (locGain > maxGain)
				{
					maxGain = locGain;
					result = attributes[i];
				}
			}
			return result;
		}

		/// <summary>
		/// Возвращает true, если все примеры в выборке положительные
		/// </summary>
		private bool allSamplesPositives(bool[] samplesClass) => allSamples(samplesClass, true);

		/// <summary>
		/// Возвращает true, если все примеры в выборке отрицательные
		/// </summary>
		private bool allSamplesNegatives(bool[] samplesClass) => allSamples(samplesClass, false);

		private bool allSamples(bool[] samplesClass, bool argValue)
		{
			bool ret = true;
			foreach (bool value in samplesClass)
			{
				if (value != argValue)
				{
					ret = false;
					break;
				}
			}
			return ret;
		}


		/// <summary>
		/// Построить дерево решений на основе представленных образцов
		/// </summary>
		public int mountTree(Dictionary<int, BitBlock> samples, bool[] samplesClass, int[] attributes, int Level, Graph graph)
		{
			if (Level == 1)
			{
				Console.WriteLine();
			}
			else if (Level > 1 && Level <= 9)
			{
				Console.Write(Level.ToString());
			}
			else if (Level > 9)
			{
				if (Level % 100 == 0)
				{
					Console.Write(".");
				}
			}

			if (allSamplesPositives(samplesClass) == true) { return -1; }
			if (allSamplesNegatives(samplesClass) == true) { return -1; }
			if (attributes.Length == 0) { return -1; }
			if (Level > 100) { return -1; }

			total = samples.Count;
			int totalPositives = countTotalPositives(samplesClass);

			entropySet = calcEntropy(totalPositives, total - totalPositives);

			int bestAttribute = getBestAttribute(samples, samplesClass, attributes);


			root.Add(bestAttribute);

			List<int> at = new List<int>(attributes.Length - 1);
			for (int i = 0; i < attributes.Length; i++)
			{
				if (attributes[i] != bestAttribute)
				{
					at.Add(attributes[i]);
				}
			}


			int Count = 0;
			int Count2 = 0;
			for (int i = 0; i < samples.Count; i++)
			{
				if (samples[i][bestAttribute] == true)
				{
					Count++;
				}
				if (samples[i][bestAttribute] == false)
				{
					Count2++;
				}
			}

			Dictionary<int, BitBlock> s;
			bool[] sc;

			// 1 проход построения дерева, основываясь на положительном классе
			s = new Dictionary<int, BitBlock>(Count);
			sc = new bool[Count];
			int k = 0;
			for (int i = 0; i < samples.Count; i++)
			{
				if (samples[i][bestAttribute] == true)
				{
					s[k] = samples[i];
					sc[k] = samplesClass[i];
					k++;
				}
			}

			int positiveLink = -1;
			if (s.Count != 0)
			{
				if (Level == 0)
				{
					graph = graphP;
				}
				positiveLink = mountTree(s, sc, at.ToArray(), Level + 1, graph);
			}


			// 2 проход построения дерева, основываясь на отрицательном классе
			s = new Dictionary<int, BitBlock>(Count2);
			sc = new bool[Count2];
			k = 0;
			for (int i = 0; i < samples.Count; i++)
			{
				if (samples[i][bestAttribute] == false)
				{
					s[k] = samples[i];
					sc[k] = samplesClass[i];
					k++;
				}
			}

			int negativeLink = -1;
			if (s.Count != 0)
			{
				if (Level == 0)
				{
					graph = graphP;
				}
				negativeLink = mountTree(s, sc, at.ToArray(), Level + 1, graph);
			}

			if (Level >= 0)
			{
				if (positiveLink != -1)
				{
					graph.AddBranch(bestAttribute + from, positiveLink, "+");
				}
				if (negativeLink != -1)
				{
					graph.AddBranch(bestAttribute + from, negativeLink, "-");
				}
			}

			return bestAttribute + from;
		}

	}

}

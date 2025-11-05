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

		(sbyte[][], sbyte[]) getSamples(int ReactionCount, Dictionary<int, float[]> argActivations, Dictionary<int, sbyte[]> argNecessaryReactions,
			int RNumber, int argFrom, int argTill)
		{
			sbyte[][] result = new sbyte[ReactionCount][];
			sbyte[] samplesClass = new sbyte[ReactionCount];

			for (int i = 0; i < ReactionCount; i++)
			{
				result[i] = new sbyte[argTill - argFrom];

				for (int j = 0; j < argActivations[i].Length; j++)
				{
					if (argActivations[i][j] > 0)
					{
						result[i][j] = 1;
					}
					else
					{
						result[i][j] = -1;
					}
				}

				if (argNecessaryReactions[i][RNumber] == 1)
				{
					samplesClass[i] = 1;
				}
				else
				{
					samplesClass[i] = -1;
				}
			}

			return (result, samplesClass);
		}

		public void Analyze(int argACount, int argHCount, Dictionary<int, float[]> argActivations,
			Dictionary<int, sbyte[]> argNecessaryReactions, int argRNumber, int argFrom, int argTill)
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

			sbyte[][] samples;
			sbyte[] samplesClass;
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

		private ID3 id3;

		public DecisionTreeID3(int argFrom)
		{
			from = argFrom;
			graphP = new Graph();
			graphN = new Graph();
			id3 = new ID3();
		}

		/// <summary>
		/// Возвращает общее количество положительных образцов в таблице образцов
		/// </summary>
		private int GetTotalPositives(sbyte[] samplesClass)
		{
			return id3.GetTotalPositives(samplesClass);
			
			/*int result = 0;
			foreach (sbyte value in samplesClass)
			{
				if (value == 1) { result++; }
			}

			if (ret != result)
			{
				int a = 1;
			}

			return result;*/
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
		private void getValuesToAttribute(sbyte[][] argSamples, sbyte[] argSamplesClass,
			int attribute, sbyte value, out int positives, out int negatives)
		{
			positives = 0;
			negatives = 0;
			for (int i = 0; i < argSamples.Length; i++)
			{
				if (argSamples[i][attribute] == value)
				{
					if (argSamplesClass[i] == 1)
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
		private double gain(sbyte[][] samples, sbyte[] samplesClass, int attribute)
		{
			double sum = 0.0;

			int positives, negatives;
			double entropy;

			positives = negatives = 0;

			getValuesToAttribute(samples, samplesClass, attribute, 1, out positives, out negatives);

			entropy = calcEntropy(positives, negatives);
			sum += -(double)(positives + negatives) / total * entropy;

			positives = negatives = 0;

			getValuesToAttribute(samples, samplesClass, attribute, -1, out positives, out negatives);

			entropy = calcEntropy(positives, negatives);
			sum += -(double)(positives + negatives) / total * entropy;

			return entropySet + sum;
		}

		/// <summary>
		/// Возвращает лучший атрибут (с наибольшим gain)
		/// </summary>
		/// <param name="attributes">Вектор с атрибутами</param>
		private int getBestAttribute(sbyte[][] samples, sbyte[] samplesClass, int[] attributes)
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
		private sbyte allSamplesPositives(sbyte[] samplesClass) => allSamples(samplesClass, 1);

		/// <summary>
		/// Возвращает true, если все примеры в выборке отрицательные
		/// </summary>
		private sbyte allSamplesNegatives(sbyte[] samplesClass) => allSamples(samplesClass, -1);

		private sbyte allSamples(sbyte[] samplesClass, sbyte argValue)
		{
			return id3.AllSamples(samplesClass, argValue);
			
			/*sbyte ret = 1;
			foreach (sbyte value in samplesClass)
			{
				if (value != argValue)
				{
					ret = -1;
					break;
				}
			}

			if (r1 != ret)
			{
				int a = 1;
			}

			return ret;*/
		}


		/// <summary>
		/// Построить дерево решений на основе представленных образцов
		/// </summary>
		public int mountTree(sbyte[][] samples, sbyte[] samplesClass, int[] attributes, int Level, Graph graph)
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

			if (allSamplesPositives(samplesClass) == 1) { return -1; }
			if (allSamplesNegatives(samplesClass) == 1) { return -1; }
			if (attributes.Length == 0) { return -1; }
			if (Level > 100) { return -1; }

			total = samples.Length;
			int totalPositives = GetTotalPositives(samplesClass);

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
			for (int i = 0; i < samples.Length; i++)
			{
				if (samples[i][bestAttribute] == 1)
				{
					Count++;
				}
				if (samples[i][bestAttribute] == -1)
				{
					Count2++;
				}
			}

			sbyte[][] s;
			sbyte[] sc;

			// 1 проход построения дерева, основываясь на положительном классе
			s = new sbyte[Count][];
			sc = new sbyte[Count];
			int k = 0;
			for (int i = 0; i < samples.Length; i++)
			{
				if (samples[i][bestAttribute] == 1)
				{
					s[k] = samples[i];
					sc[k] = samplesClass[i];
					k++;
				}
			}

			int positiveLink = -1;
			if (s.Length != 0)
			{
				if (Level == 0)
				{
					graph = graphP;
				}
				positiveLink = mountTree(s, sc, at.ToArray(), Level + 1, graph);
			}


			// 2 проход построения дерева, основываясь на отрицательном классе
			s = new sbyte[Count2][];
			sc = new sbyte[Count2];
			k = 0;
			for (int i = 0; i < samples.Length; i++)
			{
				if (samples[i][bestAttribute] == -1)
				{
					s[k] = samples[i];
					sc[k] = samplesClass[i];
					k++;
				}
			}

			int negativeLink = -1;
			if (s.Length != 0)
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

using System;
using System.Collections.Generic;
using System.Linq;
using Tac.Perceptron;

public class InformationGainCalculator
{
	//private float[] targets;
	private float totalEntropy;
	private int totalCount;

	private Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

	public InformationGainCalculator(Dictionary<int, BitBlock> argNecessaryReactions)
	{
		NecessaryReactions = argNecessaryReactions;
		totalCount = argNecessaryReactions.Count;

		// Подсчитываем количество положительных примеров
		//int positiveCount = argTargets.Count(t => t > 0.5f);
		int positiveCount = 0;
		for (int i = 0; i < NecessaryReactions.Count; i++)
		{
			if (NecessaryReactions[i][0] == true)
			{ 
				positiveCount++;
			}
		}

		// Вычисляем общую энтропию один раз
		float positiveProbability = (float)positiveCount / totalCount;
		totalEntropy = CalculateEntropy(positiveProbability);
	}

	public float[] CalculateInformationGain(Dictionary<int, float[]> activations, int argACount)
	{
		float[] informationGain = new float[argACount];

		for (int i = 0; i < argACount; i++)
		{
			// Бинаризуем активации
			int[] binaryActivation = new int[activations.Count];
			for (int j = 0; j < binaryActivation.Length; j++)
			{
				if (activations[j][i] > 0)
				{
					binaryActivation[j] = 1;
				}
				else
				{
					binaryActivation[j] = 0;
				}
			}

			// Вычисляем информационную значимость для этого нейрона
			informationGain[i] = CalculateNeuronInformationGain(binaryActivation);
		}

		return informationGain;
	}

	private float CalculateNeuronInformationGain(int[] binaryActivation)
	{
		int count0 = 0, count1 = 0; // Количество примеров в каждом подмножестве
		int positive0 = 0, positive1 = 0; // Количество положительных примеров в каждом подмножестве

		for (int j = 0; j < binaryActivation.Length; j++)
		{
			if (binaryActivation[j] == 0)
			{
				count0++;
				if (NecessaryReactions[j][0] == true) positive0++;
			}
			else
			{
				count1++;
				if (NecessaryReactions[j][0] == true) positive1++;
			}
		}

		// Вычисляем энтропию для каждого подмножества
		float entropy0 = CalculateSubsetEntropy(positive0, count0);
		float entropy1 = CalculateSubsetEntropy(positive1, count1);

		// Вычисляем взвешенную энтропию
		float subsetEntropy = (count0 * entropy0 + count1 * entropy1) / totalCount;

		// Информационная значимость = уменьшение энтропии
		return totalEntropy - subsetEntropy;
	}

	private float CalculateSubsetEntropy(int argPositiveCount, int argTotalCount)
	{
		if (argTotalCount == 0) return 0;

		float positiveProbability = (float)argPositiveCount / argTotalCount;
		return CalculateEntropy(positiveProbability);
	}

	private float CalculateEntropy(float p)
	{
		if (p == 0f || p == 1f)
			return 0f;

		return -p * (float)Math.Log(p) - (1 - p) * (float)Math.Log(1 - p);
	}
}
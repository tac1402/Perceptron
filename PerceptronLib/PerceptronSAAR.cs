// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


using PerceptronLib;
using System;
using System.Collections.Generic;

namespace Tac.Perceptron
{

	/// <summary>
	/// SAAR-Perceptron (Self-Recursive Associative Adaptive Reservoir Perceptron)
	/// </summary>
	public class PerceptronSAAR
	{
		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров

		public BitBlock SensorsField; /* Сенсорное поле */
		public BitBlock ReactionsOutput; /* Реагирующие поле */

		public float[] AField;
		public float[] AFieldNorm;
		public Dictionary<int, float[]> Activations = new Dictionary<int, float[]>();

		public Dictionary<int, int[]> WeightAA;
		public Dictionary<int, List<int>> IndexAA;
		private int[] Threshold;
		private int AACount; // Количество внутрених связей


		public Dictionary<int, BitBlock> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Dictionary<int, float[]> WeightSA; // Веса между S-A элементами
		public Dictionary<int, float[]> WeightAR; // Веса между A-R элементами

		private sbyte[] ReactionError;
		private Random rnd = new Random(20);

		public PerceptronSAAR(int argSCount, int argACount, int argRCount, int argHCount)
		{
			ACount = argACount;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;

			SensorsField = new BitBlock(SCount);

			ReactionsOutput = new BitBlock(RCount);
			ReactionError = new sbyte[RCount];

			AField = new float[ACount];

			for (int i = 0; i < HCount; i++)
			{
				Activations.Add(i, new float[ACount]);
			}
			gainValue = new float[ACount][];
			for (int i = 0; i < ACount; i++)
			{
				gainValue[i] = new float[RCount];
			}

			LearnedStimuls = new Dictionary<int, BitBlock>();
			NecessaryReactions = new Dictionary<int, BitBlock>();

			WeightSA = new Dictionary<int, float[]>(SCount);
			for (int i = 0; i < SCount; i++)
			{
				WeightSA[i] = new float[ACount];
			}

			WeightAR = new Dictionary<int, float[]>(ACount);
			for (int i = 0; i < ACount; i++)
			{
				WeightAR[i] = new float[RCount];
			}

			AACount = ACount / 10;
			WeightAA = new Dictionary<int, int[]>(ACount);
			IndexAA = new Dictionary<int, List<int>>(ACount);
			for (int i = 0; i < ACount; i++)
			{
				WeightAA[i] = new int[ACount];
				IndexAA[i] = new List<int>();
			}
			for (int i = 0; i < ACount; i++)
			{
				InitAA(i);
			}
			Threshold = new int[ACount];
			for (int i = 0; i < ACount; i++)
			{
				Threshold[i] = rnd.Next(-10, 11);
			}
		}

		private void InitAA(int argAId)
		{
			int sinapsCount = rnd.Next(2, AACount);

			int sensorNumber = 0;
			sbyte sensorType = 0;

			for (int j = 0; j < sinapsCount; j++)
			{
				sensorNumber = rnd.Next(ACount);
				if (rnd.Next(2) == 0) sensorType = 1; else sensorType = -1;
				WeightAA[argAId][sensorNumber] = sensorType;

				if (IndexAA[argAId].Contains(sensorNumber) == false)
				{
					IndexAA[argAId].Add(sensorNumber);
				}
			}
			if (sinapsCount != AACount) { IndexAA[argAId].Add(-1); }
		}


		/// <summary>
		/// Добавить на обработку новый пример из обучающей выборки
		/// </summary>
		/// <param name="argStimulNumber">Номер примера из обучающей выборки</param>
		/// <param name="argPerception">Стимулы (входы) из примера обучающей выборки</param>
		/// <param name="argReaction">Нужная реакция (выходы) из примера обучающей выборки</param>
		public void JoinStimul(int argStimulNumber, BitBlock argPerception, BitBlock argReaction)
		{
			// Запомним обучающий стимул
			LearnedStimuls.Add(argStimulNumber, argPerception);

			// Запомним какая реакция должна быть на этот пример
			NecessaryReactions.Add(argStimulNumber, argReaction);
		}


		public int[] Shuffle(int[] list)
		{
			int n = list.Length;
			while (n > 1)
			{
				n--;
				int k = rnd.Next(n + 1);
				int value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
			return list;
		}

		/// <summary>
		/// Когда все примеры добавлены, вызывается чтобы перцептрон их выучил
		/// </summary>
		public void Learned()
		{
			InformationGainCalculator gain = new InformationGainCalculator(NecessaryReactions);

			int[] indexL = new int[HCount];
			for (int i = 0; i < HCount; i++)
			{
				indexL[i] = i;
			}

			int OldError = 0;
			int Error = 0;


			// Делаем очень много итераций
			for (int n = 0; n < 100000; n++)
			{
				Error = 0;

				DateTime begin = DateTime.Now;
				aTime = 0;

				indexL = Shuffle(indexL);

				// За каждую итерацию прокручиваем все примеры из обучающей выборки
				for (int i = 0; i < HCount; i++)
				{
					int index = indexL[i];

					AActivation(index);
					// Активируем R-элементы, т.е. рассчитываем выходы
					RActivation(index);
					// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
					bool e = GetError(index);
					if (e == true)
					{
						LearnedStimulAR(index);

						RActivation(index);
						bool e2 = GetError(index);
						if (e2 == true)
						{
							if (learnedSA)
							{
								LearnedStimulSA(index);
							}
						}


						Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.
					}
				}


				OldError = Error;

				gainValue = gain.CalculateInformationGain(Activations, ACount, RCount);

				LRegion region = new LRegion();
				//int rCount = region.Calc(Activations);
				double rCount = region.MeanPairwiseDistance(Activations);


				stop = 0;

				for (int j = 0; j < ACount; j++)
				{
					bool isEmpty = true;
					for (int i = 0; i < RCount; i++)
					{
						if (gainValue[j][i] > 0)
						{
							isEmpty = false;
							break;
						}
					}
					if (isEmpty == false)
					{
						stop++;
					}
				}

				if (T == 1)
				{
					learnedSA = false;
				}
				if (T == 1)
				{ 
					learnedSA= true;
					T = 0;
				}
				T++;

				double t = (DateTime.Now - begin).TotalMilliseconds;
				Console.WriteLine(n.ToString() + " - " + Error.ToString() + " - " + t.ToString() + " ms " + stop.ToString("F0") + "\t" + learnedSA.ToString()
								+ "\t" + rCount.ToString("F2"));
				if (Error == 0) { break; }
			}
		}

		int T = 0;

		float[][] gainValue;

		double aTime = 0;


		/// <summary>
		/// Вычисляем бинарную кросс-энтропию (BinaryCrossEntropy)
		/// </summary>
		public static float BCE(float logit, float target)
		{
			return Math.Max(logit, 0) - logit * target + (float)Math.Log(1 + Math.Exp(-Math.Abs(logit)));
		}


		float maxAField = 0;
		public float[] Normalize(float[] AField)
		{
			// Находим максимальное по модулю значение
			float maxAbs = 0;
			for (int i = 0; i < AField.Length; i++)
			{
				float absValue = Math.Abs(AField[i]);
				if (absValue > maxAbs) maxAbs = absValue;
			}

			// Если все значения нулевые, возвращаем исходный массив
			if (maxAbs == 0)
				return AField;

			// Нормализуем значения
			float[] normalized = new float[AField.Length];

			for (int i = 0; i < AField.Length; i++)
			{
				normalized[i] = AField[i] / maxAbs;
			}

			if (maxAField < maxAbs)
			{
				maxAField = maxAbs;
			}

			return normalized;
		}


		private void AActivation(int argStimulNumber)
		{
			// Кинем на сенсоры обучающий пример
			SensorsField = LearnedStimuls[argStimulNumber];

			AField = new float[ACount];
			for (int j = 0; j < ACount; j++)
			{
				for (int i = 0; i < SCount; i++)
				{
					if (SensorsField[i] == true)
					{
						AField[j] += WeightSA[i][j];
					}
				}
			}

			if (isAA)
			{
				float[] AFieldTmp = new float[ACount];
				for (int i = 0; i < ACount; i++)
				{
					for (int j = 0; j < AACount; j++)
					{
						int index = IndexAA[i][j];

						if (index == -1) break;

						if (AField[index] > Threshold[i])
						{
							AFieldTmp[i] += WeightAA[i][index];
						}
					}
				}
				for (int i = 0; i < ACount; i++)
				{
					AField[i] += AFieldTmp[i];
				}
			}

			Activations[argStimulNumber] = AField;

			AFieldNorm = Normalize(AField);
		}


		float[] RField;

		private void RActivation(int argStimulNumber)
		{
			RField = new float[RCount];
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < ACount; i++)
				{
					if (AField[i] > 0)
					{
						RField[j] += WeightAR[i][j];
					}
				}
			}

			for (int i = 0; i < RCount; i++)
			{
				if (RField[i] > 0) { ReactionsOutput[i] = true; }
				if (RField[i] <= 0) { ReactionsOutput[i] = false; }
			}
		}


		private bool GetError(int argStimulNumber)
		{
			bool IsError = false;
			for (int i = 0; i < RCount; i++)
			{
				bool v = NecessaryReactions[argStimulNumber][i];

				if (ReactionsOutput[i] == v)
				{
					ReactionError[i] = 0;
				}
				else
				{
					IsError = true;
					sbyte v2 = -1; if (v == true) { v2 = 1; }
					ReactionError[i] = v2;
				}
			}
			return IsError;
		}


		float p1 = 1.0f;
		float p2 = 1.0f;
		float p3 = 0.0001f;
		float correct1 = 1.0f; // rnd=12,  0.0003f;
		float correct2 = 1.0f;
		float correct3 = 0.01f;
		int stop;

		bool correct12 = true;
		bool isAA = false;
		bool learnedSA = true;

		private void LearnedStimulSA(int argStimulNumber)
		{
			for (int j = 0; j < ACount; j++)
			{
				float[] w = new float[SCount];

				if (AField[j] > 0)
				{
					if (correct12)
					{
						for (int r = 0; r < RCount; r++)
						{
							if (ReactionError[r] != 0 && Math.Sign(WeightAR[j][r]) != Math.Sign(ReactionError[r]))
							{
								for (int i = 0; i < SCount; i++)
								{
									if (SensorsField[i] == true)
									{
										float p = (float)rnd.NextDouble();
										float entropy = BCE(AFieldNorm[j], -1);

										if (p < p1 * entropy)
										{
											w[i] -= correct1 * AFieldNorm[j];
										}
									}
								}
							}
						}
					}
				}
				else
				{
					for (int r = 0; r < RCount; r++)
					{
						if (correct12)
						{
							if (Math.Sign(WeightAR[j][r]) == Math.Sign(ReactionError[r]))
							{
								for (int i = 0; i < SCount; i++)
								{
									if (SensorsField[i] == true)
									{
										float p = (float)rnd.NextDouble();
										float entropy = BCE(AFieldNorm[j], 1);

										if (p < p2 * entropy)
										{
											w[i] += correct2 * AFieldNorm[j];
										}
									}
								}
							}
						}
						if (stop < ACount * 1.0f && Math.Sign(WeightAR[j][r]) != Math.Sign(ReactionError[r]))
						{
							for (int i = 0; i < SCount; i++)
							{
								if (SensorsField[i] == true)
								{
									float p = (float)rnd.NextDouble();
									if (p < p3)
									{
										w[i] += correct3;
									}
								}
							}
						}
					}
				}

				for (int i = 0; i < SCount; i++)
				{
					WeightSA[i][j] += w[i];
				}

			}
		}


		private void LearnedStimulAR(int argStimulNumber)
		{
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < ACount; i++)
				{
					if (AField[i] > 0)
					{
						WeightAR[i][j] = WeightAR[i][j] +  ReactionError[j];
					}
				}
			}
		}
	}
}

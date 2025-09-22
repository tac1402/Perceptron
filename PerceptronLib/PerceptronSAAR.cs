// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


using System;
using System.Collections.Generic;

namespace Tac.Perceptron
{

	/// <summary>
	/// SAAR-Perceptron (Self-Recursive Associative Adaptive Reservoir Perceptron)
	/// </summary>
	public class PerceptronSAAR
	{
		public BitBlock SensorsField; /* Сенсорное поле */
		//public int[] AssociationsField; /* Ассоциативное поле */
		public BitBlock ReactionsOutput; /* Реагирующие поле */

		public float[] AField;
		public float[] AFieldNorm;
		public Dictionary<int, float[]> Activations = new Dictionary<int, float[]>();

		public Dictionary<int, float[]> WeightAA;
		private int[] Threshold;

		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров, запоминается реакция A-элементов на каждый пример из обучающей выборки


		public Dictionary<int, BitBlock> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Dictionary<int, float[]> WeightSA; // Веса между S-A элементами
		public Dictionary<int, float[]> WeightAR; // Веса между A-R элементами

		private sbyte[] ReactionError;
		private Random rnd = new Random(10);

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
			gainValue = new float[ACount];
			gainNorm = new float[ACount];
			gainNormAvg = new float[ACount];
			gainNormCount = new int[ACount];

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

			WeightAA = new Dictionary<int, float[]>(ACount);
			for (int i = 0; i < ACount; i++)
			{
				WeightAA[i] = new float[ACount];
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
			int sinapsCount = rnd.Next(2, ACount/10);

			int sensorNumber = 0;
			sbyte sensorType = 0;

			for (int j = 0; j < sinapsCount; j++)
			{
				sensorNumber = rnd.Next(ACount);
				if (rnd.Next(2) == 0) sensorType = 1; else sensorType = -1;
				WeightAA[argAId][sensorNumber] = sensorType;
			}
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
				OldError = Error;
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
						float p = (float)rnd.NextDouble();
						//if (p > 0.99f && OldError < ACount || OldError >= ACount)
						{
							LearnedStimulSA(index);
						}

						LearnedStimulAR(index);
						Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.
					}
				}
				gainValue = gain.CalculateInformationGain(Activations, ACount);
				gainNorm = Normalize(gainValue);
				stop = 0;
				string gainTxt = "";
				for (int j = 0; j < ACount; j++)
				{
					if (gainNorm[j] > 0)
					{
						stop++;
						gainNormCount[j] ++;
					}
					else
					{
						gainNormCount[j] = 0;
					}

					if (gainNormCount[j] == 1)
					{
						gainNormAvg[j] = gainNorm[j];
					}
					else if (gainNormCount[j] > 1)
					{
						gainNormAvg[j] = (gainNormAvg[j] + gainNorm[j]) / 2;
					}
				}
				gainTxt += "\n";

				//ClearGain();


				double t = (DateTime.Now - begin).TotalMilliseconds;
				Console.WriteLine(n.ToString() + " - " + Error.ToString() + " - " + t.ToString() + " ms " + stop.ToString() + "\t" + clearCount.ToString());
				if (Error == 0) { break; }
			}
		}

		float[] gainValue;
		float[] gainNorm;
		float[] gainNormAvg;
		int[] gainNormCount;
		int clearCount = 0;

		double aTime = 0;

		private void ClearGain()
		{
			clearCount = 0;
			for (int i = 0; i < ACount; i++)
			{
				if (gainNormCount[i] > 100 && gainNormAvg[i] < 0.0001f)
				{
					for (int j = 0; j < SCount; j++)
					{
						WeightSA[j][i] = 0;
					}
					clearCount++;
				}
			}
		}


		/// <summary>
		/// Вычисляем бинарную кросс-энтропию
		/// BinaryCrossEntropy
		/// </summary>
		public static float BCE(float logit, float target)
		{
			// Стабильная реализация бинарной кросс-энтропии с логитами
			return Math.Max(logit, 0) - logit * target + (float)Math.Log(1 + Math.Exp(-Math.Abs(logit)));
		}

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

			return normalized;
		}


		private void AActivation(int argStimulNumber)
		{
			// Кинем на сенсоры обучающий пример
			SensorsField = LearnedStimuls[argStimulNumber];

			AField = new float[ACount];
			//float[] Summa = new float[ACount];
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

			float[] AFieldTmp = new float[ACount];

			for (int i = 0; i < ACount; i++)
			{
				for (int j = 0; j < ACount; j++)
				{
					if (AField[j] > Threshold[i])
					{
						AFieldTmp[i] += WeightAA[i][j];
					}
				}
			}
			for (int i = 0; i < ACount; i++)
			{
				AField[i] += AFieldTmp[i];
			}


			Activations[argStimulNumber] = AField;

			AFieldNorm = Normalize(AField);

			int a = 1;
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
		float p3 = 0.00001f;
		float correct1 = 0.0001f;
		float correct2 = 0.0001f;
		float correct3 = 0.000001f;
		int stop = 0;

		private void LearnedStimulSA(int argStimulNumber)
		{
			for (int j = 0; j < ACount; j++)
			{
				if (AField[j] > 0)
				{
					if (Math.Sign(WeightAR[j][0]) != Math.Sign(ReactionError[0]))
					{
						for (int i = 0; i < SCount; i++)
						{
							if (SensorsField[i] == true)
							{
								float p = (float)rnd.NextDouble();
								float entropy = BCE(AFieldNorm[j], -1);

								if (p < p1 * entropy)
								{
									WeightSA[i][j] -= correct1 * AFieldNorm[j];
								}
							}
						}
					}
				}
				else
				{
					if (Math.Sign(WeightAR[j][0]) == Math.Sign(ReactionError[0]))
					{
						for (int i = 0; i < SCount; i++)
						{
							if (SensorsField[i] == true)
							{
								float p = (float)rnd.NextDouble();
								float entropy = BCE(AFieldNorm[j], 1);

								if (p < p2 * entropy)
								{
									WeightSA[i][j] += correct2 * AFieldNorm[j];
								}
							}
						}
					}
					if (stop < ACount *0.5f  && Math.Sign(WeightAR[j][0]) != Math.Sign(ReactionError[0]))
					{
						for (int i = 0; i < SCount; i++)
						{
							if (SensorsField[i] == true)
							{
								float p = (float)rnd.NextDouble();
								if (p < p3)
								{
									WeightSA[i][j] += correct3;
								}
							}
						}
					}
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

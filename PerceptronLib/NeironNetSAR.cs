// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Tac.Perceptron
{

	/// <summary>
	/// Версия перцептрона Розенблатта, с обучаемым SA слоем
	/// </summary>
	public class NeironNetSAR
	{
		public BitBlock SensorsField; /* Сенсорное поле */
		//public int[] AssociationsField; /* Ассоциативное поле */
		public BitBlock ReactionsOutput; /* Реагирующие поле */

		public float[] AField;
		public float[] AFieldNorm;
		public Dictionary<int, float[]> Activations = new Dictionary<int, float[]>();


		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров, запоминается реакция A-элементов на каждый пример из обучающей выборки


		//public Dictionary<int, List<int>> AHConnections; // Как реагируют A-элементы на каждый стимул из обучающей выборки

		public Dictionary<int, BitBlock> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Dictionary<int, float[]> WeightSA; // Веса между S-A элементами
		public Dictionary<int, float[]> WeightAR; // Веса между A-R элементами

		private sbyte[] ReactionError;
		private Random rnd = new Random(10);

		public NeironNetSAR(int argSCount, int argACount, int argRCount, int argHCount)
		{
			ACount = argACount;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;

			SensorsField = new BitBlock(SCount);

			/*WeightSA = new Dictionary<int, int[]>(SCount);
			for (int i = 0; i < SCount; i++)
			{
				WeightSA[i] = new int[A1Count];
			}*/

			/*AHConnections = new Dictionary<int, List<int>>();
			for (int i = 0; i < HCount; i++)
			{
				AHConnections[i] = new List<int>();
			}*/

			//AssociationsField = new int[A1Count];
			/*for (int i = 0; i < A1Count; i++)
			{
				InitSA(i);
			}*/

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
		}

		/*private void InitSA(int argAId)
		{
			int sinapsXCount = 16;
			int sinapsYCount = 16;
			int sinapsCount = sinapsXCount + sinapsYCount;

			int sensorNumber = 0;
			sbyte sensorType = 0;

			for (int j = 0; j < sinapsCount; j++)
			{
				sensorNumber = rnd.Next(SCount);

				if (j < sinapsXCount)
				{
					sensorType = 1;
				}
				else
				{
					sensorType = -1;
				}
				//if (rnd.Next(2) == 0) sensorType = 1; else sensorType = -1;

				WeightSA[sensorNumber][argAId] = sensorType;
			}
		}*/


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


		/// <summary>
		/// Когда все примеры добавлены, вызывается чтобы перцептрон их выучил
		/// </summary>
		public void Learned()
		{
			InformationGainCalculator gain = new InformationGainCalculator(NecessaryReactions);


			// Делаем очень много итераций
			for (int n = 0; n < 100000; n++)
			{
				int Error = 0;

				DateTime begin = DateTime.Now;
				aTime = 0;


				// За каждую итерацию прокручиваем все примеры из обучающей выборки
				for (int i = 0; i < HCount; i++)
				{
					// Активируем S-элементы, т.е. подаем входы и рассчитываем средний слой A-элементы
					//if (n == 0) { SActivation(i); }

					AActivation(i);
					// Активируем R-элементы, т.е. рассчитываем выходы
					RActivation(i);
					// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
					bool e = GetError(i);
					if (e == true)
					{
						float p = (float)rnd.NextDouble();
						//if (p > 0.7f)
						{
							LearnedStimulSA(i);
						}

						LearnedStimulAR(i);
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

					//gainTxt += gainNorm[j].ToString() + "|";
				}
				gainTxt += "\n";
				//File.AppendAllText("gain.txt", gainTxt);
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


		//Ограничивает значение x диапазоном[x_min, x_max]
		private float clip(float x, float min = 1e-6f, float max = 1 - 1e-6f)
		{
			if (x < min)
				return min;
			else if (x > max)
				return max;
			else
				return x;
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

		public float[] LogNormalize(float[] AField)
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

			// Вычисляем логарифмический нормализующий делитель
			float logDenom = (float)Math.Log(1 + maxAbs);

			// Нормализуем значения с использованием логарифма
			float[] normalized = new float[AField.Length];
			for (int i = 0; i < AField.Length; i++)
			{
				float x = AField[i];
				if (x == 0)
				{
					normalized[i] = 0;
				}
				else
				{
					float sign = Math.Sign(x);
					float absX = Math.Abs(x);
					normalized[i] = sign * (float)(Math.Log(1 + absX ) / logDenom);
				}
			}

			return normalized;
		}


		public float[] SigmoidLogNormalize(float[] AField)
		{
			float value = AField[0];

			// Логарифмическое преобразование с последующим сигмоидным отображением
			//float sign = Math.Sign(value);
			float absValue = Math.Abs(value);

			// Логарифмическое преобразование
			float logValue = (float)Math.Log(1 + absValue);

			// Сигмоидное отображение для приведения к диапазону [0, 1]
			float sigmoid = 1f / (1f + (float)Math.Exp(-logValue));

			return new float[] { sigmoid };
		}

		/*private void SActivation(int argStimulNumber)
		{

			for (int i = 0; i < A1Count; i++)
			{
				AssociationsField[i] = 0;
			}

			// Кинем на сенсоры обучающий пример
			SensorsField = LearnedStimuls[argStimulNumber];

			DateTime begin = DateTime.Now;

			for (int i = 0; i < SCount; i++)
			{
				if (SensorsField[i] == true)
				{
					for (int j = 0; j < A1Count; j++)
					{
						AssociationsField[j] += WeightSA[i][j];
					}
				}
			}
			double t = (DateTime.Now - begin).TotalMilliseconds;
			aTime += t;

			// Запомним как на этот пример реагировали A - элементы
			for (int j = 0; j < A1Count; j++)
			{
				if (AssociationsField[j] > 0)
				{
					AHConnections[argStimulNumber].Add(j);
				}
			}
		}*/


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
			/*for (int i = 0; i < ACount; i++)
			{
				if (Summa[i] > 0) { AField[i] = true; }
				if (Summa[i] <= 0) { AField[i] = false; }
			}*/

			Activations[argStimulNumber] = AField;


			//AFieldNorm = LogNormalize(AField);
			AFieldNorm = Normalize(AField);

			int a = 1;
		}


		float[] RField;
		float[] RFieldNorm;

		private void RActivation(int argStimulNumber)
		{
			RField = new float[RCount];
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < ACount; i++)
				{
					if (AField[i] > 0)
					{
						RField[j] += WeightAR[i][j]/* * AFieldNorm[i]*/;
					}
				}
			}

			RFieldNorm = SigmoidLogNormalize(RField);

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


		float p1 = 0.5f;
		float p2 = 0.5f;
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

								//float g = p4 + gainNorm[j];
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

								//float g = p4 + gainNorm[j];
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
								//float entropy = BCE(AFieldNorm[j], -1);

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

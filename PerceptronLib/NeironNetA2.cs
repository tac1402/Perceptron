// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

namespace Tac.Perceptron
{

	/// <summary>
	/// Оптимизированная версия перцептрона Розенблатта, засчет сохранения реакций A элементов для каждого примера из обучающей выборки (AHConnections)
	/// </summary>
	public class NeironNetA2
	{
		public BitBlock SensorsField; /* Сенсорное поле */
		public int[] AssociationsField; /* Ассоциативное поле */
		public BitBlock ReactionsField; /* Реагирующие поле */

		public BitBlock A2Field;

		private int SCount; // Количество сенсоров
		private int A1Count; // Количество ассоциаций
		private int A2Count; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров, запоминается реакция A-элементов на каждый пример из обучающей выборки


		public Dictionary<int, List<int>> AHConnections; // Как реагируют A-элементы на каждый стимул из обучающей выборки

		public Dictionary<int, BitBlock> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Dictionary<int, int[]> WeightSA; // Веса между S-A1 элементами
		public Dictionary<int, float[]> WeightA1A2; // Веса между A1-A2 элементами
		public Dictionary<int, float[]> WeightAR; // Веса между A2-R элементами

		private sbyte[] ReactionError;
		private Random rnd = new Random(10);

		public NeironNetA2(int argSCount, int argA1Count, int argA2Count, int argRCount, int argHCount)
		{
			A1Count = argA1Count;
			A2Count = argA2Count;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;

			SensorsField = new BitBlock(SCount);

			WeightSA = new Dictionary<int, int[]>(SCount);
			for (int i = 0; i < SCount; i++)
			{
				WeightSA[i] = new int[A1Count];
			}

			AHConnections = new Dictionary<int, List<int>>();
			for (int i = 0; i < HCount; i++)
			{
				AHConnections[i] = new List<int>();
			}

			AssociationsField = new int[A1Count];
			for (int i = 0; i < A1Count; i++)
			{
				InitSA(i);
			}

			ReactionsField = new BitBlock(RCount);
			ReactionError = new sbyte[RCount];

			A2Field =  new BitBlock(A2Count);


			LearnedStimuls = new Dictionary<int, BitBlock>();
			NecessaryReactions = new Dictionary<int, BitBlock>();


			WeightA1A2 = new Dictionary<int, float[]>(A1Count);
			for (int i = 0; i < A1Count; i++)
			{
				WeightA1A2[i] = new float[A2Count];
			}

			for (int i = 0; i < A1Count; i++)
			{
				for (int j = 0; j < A2Count; j++)
				{
					WeightA1A2[i][j] = (float) (1 - (rnd.NextDouble() * 2));
				}
			}


			WeightAR = new Dictionary<int, float[]>(A2Count);
			for (int i = 0; i < A2Count; i++)
			{
				WeightAR[i] = new float[RCount];
			}
		}

		private void InitSA(int argAId)
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


		/// <summary>
		/// Когда все примеры добавлены, вызывается чтобы перцептрон их выучил
		/// </summary>
		public void Learned()
		{
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
					if (n == 0) { SActivation(i); }

					A2Activation(i);
					// Активируем R-элементы, т.е. рассчитываем выходы
					RActivation(i);
					// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
					bool e = GetError(i);
					if (e == true)
					{
						LearnedStimulA1A2(i);
						LearnedStimulA2R(i);
						Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.
					}
				}
				double t = (DateTime.Now - begin).TotalMilliseconds;
				Console.WriteLine(n.ToString() + " - " + Error.ToString() + " - " + t.ToString() + " ms");
				Console.WriteLine("\t" + aTime.ToString() + " ms");
				if (Error == 0) { break; }
			}
		}

		double aTime = 0;

		private void SActivation(int argStimulNumber)
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
		}


		private void A2Activation(int argStimulNumber)
		{
			float[] Summa = new float[A2Count];
			for (int j = 0; j < A2Count; j++)
			{
				for (int i = 0; i < AHConnections[argStimulNumber].Count; i++)
				{
					int index = AHConnections[argStimulNumber][i];
					Summa[j] += WeightA1A2[index][j];
				}
			}
			for (int i = 0; i < A2Count; i++)
			{
				if (Summa[i] > 0) { A2Field[i] = true; }
				if (Summa[i] <= 0) { A2Field[i] = false; }
			}

			int a = 1;
		}

		private void RActivation(int argStimulNumber)
		{
			float[] Summa = new float[RCount];
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < A2Count; i++)
				{
					if (A2Field[i] == true)
					{
						Summa[j] += WeightAR[i][j];
					}
				}
			}
			for (int i = 0; i < RCount; i++)
			{
				if (Summa[i] > 0) { ReactionsField[i] = true; }
				if (Summa[i] <= 0) { ReactionsField[i] = false; }
			}
		}


		private bool GetError(int argStimulNumber)
		{
			bool IsError = false;
			for (int i = 0; i < RCount; i++)
			{
				bool v = NecessaryReactions[argStimulNumber][i];

				if (ReactionsField[i] == v)
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


		float maxSumma = 1;

		private void LearnedStimulA1A2(int argStimulNumber)
		{
			for (int j = 0; j < A2Count; j++)
			{
				/*float summa = 0;
				for (int i = 0; i < AHConnections[argStimulNumber].Count; i++)
				{
					int index = AHConnections[argStimulNumber][i];
					summa += WeightA1A2[index][j];
				}
				summa /= AHConnections[argStimulNumber].Count;

				if (summa > maxSumma) { maxSumma = summa; }

				float x_norm = summa / maxSumma;*/

				for (int i = 0; i < AHConnections[argStimulNumber].Count; i++)
				{
					int index = AHConnections[argStimulNumber][i];

					// 0.0001f
					//WeightA1A2[index][j] = WeightA1A2[index][j] + ReactionError[0] * 0.001f;
				}
			}
		}


		private void LearnedStimulA2R(int argStimulNumber)
		{
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < A2Count; i++)
				{
					if (A2Field[i] == true)
					{
						WeightAR[i][j] = WeightAR[i][j] + ReactionError[j]*1.0f;
					}
				}
			}
		}
	}
}

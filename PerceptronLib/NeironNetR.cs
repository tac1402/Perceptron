// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Tac.Perceptron
{
	/// <summary>
	/// Классическая версия элементарного перцептрона Розенблатта
	/// </summary>
	public class NeironNetR
	{
		public BitBlock SensorsField; /* Сенсорное поле */
		public int[] AssociationsField; /* Ассоциативное поле */
		public BitBlock ReactionsField; /* Реагирующие поле */

		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров, запоминается реакция A-элементов на каждый пример из обучающей выборки


		public List<int> AHConnections; // Как реагируют A-элементы на каждый стимул из обучающей выборки

		public Dictionary<int, BitBlock> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Dictionary<int, int[]> WeightSA; // Веса между S-A элементами
		public Dictionary<int, int[]> WeightAR; // Веса между A-R элементами

		private sbyte[] ReactionError;
		private Random rnd = new Random();

		public NeironNetR(int argSCount, int argACount, int argRCount, int argHCount)
		{
			ACount = argACount;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;

			SensorsField = new BitBlock(SCount);

			WeightSA = new Dictionary<int, int[]>(SCount);
			for (int i = 0; i < SCount; i++)
			{
				WeightSA[i] = new int[ACount];
			}

			AssociationsField = new int[ACount];
			for (int i = 0; i < ACount; i++)
			{
				InitSA(i);
			}
			ReactionsField = new BitBlock(RCount);

			ReactionError = new sbyte[RCount];

			AHConnections = new List<int>();

			LearnedStimuls = new Dictionary<int, BitBlock>();
			NecessaryReactions = new Dictionary<int, BitBlock>();


			WeightAR = new Dictionary<int, int[]>(ACount);
			for (int i = 0; i < ACount; i++)
			{
				WeightAR[i] = new int[RCount];
			}
		}

		private void InitSA(int argAId)
		{
			int SinapsCount = 16;

			int sensorNumber = 0;
			sbyte sensorType = 0;

			for (int j = 0; j < SinapsCount; j++)
			{
				sensorNumber = rnd.Next(SCount);

				if (WeightSA[sensorNumber][argAId] == 0)
				{
					if (rnd.Next(2) == 0) sensorType = 1; else sensorType = -1;

					WeightSA[sensorNumber][argAId] = sensorType;
				}
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

				// За каждую итерацию прокручиваем все примеры из обучающей выборки
				for (int i = 0; i < HCount; i++)
				{
					// Активируем S-элементы, т.е. подаем входы и рассчитываем средний слой A-элементы
					SActivation(i);
					// Активируем R-элементы, т.е. рассчитываем выходы
					RActivation(i);
					// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
					bool e = GetError(i);
					if (e == true)
					{
						LearnedStimul(i);
						Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.
					}
				}
				double t = (DateTime.Now - begin).TotalMilliseconds;
				Console.WriteLine(n.ToString() + " - " + Error.ToString() + " - " + t.ToString() + " ms");
				if (Error == 0) { break; }
			}
		}

		private void SActivation(int argStimulNumber)
		{
			for (int i = 0; i < ACount; i++)
			{
				AssociationsField[i] = 0;
			}

			// Кинем на сенсоры обучающий пример
			SensorsField = LearnedStimuls[argStimulNumber];

			for (int i = 0; i < SCount; i++)
			{
				if (SensorsField[i] == true)
				{
					for (int j = 0; j < ACount; j++)
					{
						AssociationsField[j] += WeightSA[i][j];
					}
				}
			}

			// Запомним как на этот пример реагировали A - элементы
			AHConnections.Clear();
			for (int j = 0; j < ACount; j++)
			{
				if (AssociationsField[j] > 0)
				{
					AHConnections.Add(j);
				}
			}
		}


		private void RActivation(int argStimulNumber)
		{
			int[] Summa = new int[RCount];
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < AHConnections.Count; i++)
				{
					int index = AHConnections[i];
					Summa[j] += WeightAR[index][j];
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

		private void LearnedStimul(int argStimulNumber)
		{
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < AHConnections.Count; i++)
				{
					int index = AHConnections[i];
					WeightAR[index][j] = WeightAR[index][j] + ReactionError[j];
				}
			}
		}
	}
}

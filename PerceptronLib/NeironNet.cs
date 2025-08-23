// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Tac.Perceptron
{

	public class NeironNet
	{
		public SElement SensorsField; /* Сенсорное поле */
		public AElement[] AssociationsField; /* Ассоциативное поле */
		public BitBlock ReactionsField; /* Реагирующие поле */

		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров, запоминается реакция A-элементов на каждый пример из обучающей выборки


		public ArrayList[] AHConnections; // Как реагируют A-элементы на каждый стимул из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки
		public ArrayList[] Weight; // Веса между A-R элементами

		private sbyte[] ReactionError;
		private Random rnd = new Random();

		public NeironNet(int argSCount, int argACount, int argRCount, int argHCount)
		{
			ACount = argACount;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;

			SensorsField = new SElement(SCount);

			AssociationsField = new AElement[ACount];
			for (int i = 0; i < ACount; i++)
			{
				AssociationsField[i] = new AElement(i, SensorsField, SCount, rnd);
			}
			ReactionsField = new BitBlock(RCount);

			ReactionError = new sbyte[RCount];

			AHConnections = new ArrayList[HCount];
			for (int i = 0; i < HCount; i++)
			{
				AHConnections[i] = new ArrayList();
			}

			NecessaryReactions = new Dictionary<int, BitBlock>();

			Weight = new ArrayList[ACount];
			for (int i = 0; i < ACount; i++)
			{
				Weight[i] = new ArrayList();
				for (int j = 0; j < RCount; j++)
				{
					Weight[i].Add(0);
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
			for (int i = 0; i < ACount; i++)
			{
				AssociationsField[i].ActivationLevel = 0;
			}
			// Кинем на сенсоры полученный пример
			SensorsField.State = argPerception;

			// Запомним как на этот пример реагировали A - элементы
			for (int i = 0; i < ACount; i++)
			{
				if (AssociationsField[i].ActivationLevel > 0)
				{
					AHConnections[argStimulNumber].Add(i);
				}
			}

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

				// За каждую итерацию прокручиваем все примеры из обучающей выборки
				for (int i = 0; i < HCount; i++)
				{
					// Активируем R-элементы, т.е. рассчитываем выходы
					RAktivation(i);
					// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
					bool e = GetError(i);
					if (e == true)
					{
						LearnedStimul(i);
						Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.
					}
				}
				Console.WriteLine(n.ToString() + " - " + Error.ToString());
				if (Error == 0) { break; }
			}
		}

		private void RAktivation(int argStimulNumber)
		{
			int[] Summa = new int[RCount];
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < AHConnections[argStimulNumber].Count; i++)
				{
					int index = (int) AHConnections[argStimulNumber][i];
					Summa[j] += (int) Weight[index][j];
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
				for (int i = 0; i < AHConnections[argStimulNumber].Count; i++)
				{
					int index = (int)AHConnections[argStimulNumber][i];
					Weight[index][j] = (int)Weight[index][j] + ReactionError[j];
				}
			}
		}
	}
}

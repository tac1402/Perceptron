// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace Tac.Perceptron
{

	/// <summary>
	/// Оптимизированная версия перцептрона Розенблатта, засчет сохранения реакций A элементов для каждого примера из обучающей выборки (AHConnections)
	/// </summary>
	public class NeironNetTree
	{
		public BitBlock SensorsField; /* Сенсорное поле */
		public int[] AssociationsField; /* Ассоциативное поле */
		public BitBlock ReactionsField; /* Реагирующие поле */

		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров, запоминается реакция A-элементов на каждый пример из обучающей выборки


		public Dictionary<int, List<int>> AHConnections; // Как реагируют A-элементы на каждый стимул из обучающей выборки

		public Dictionary<int, BitBlock> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Dictionary<int, int[]> WeightSA; // Веса между S-A элементами
		public Dictionary<int, int[]> WeightAR; // Веса между A-R элементами

		private sbyte[] ReactionError;
		private Random rnd = new Random(10);

		public NeironNetTree(int argSCount, int argACount, int argRCount, int argHCount)
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

			AHConnections = new Dictionary<int, List<int>>();
			for (int i = 0; i < HCount; i++)
			{
				AHConnections[i] = new List<int>();
			}

			AssociationsField = new int[ACount];
			for (int i = 0; i < ACount; i++)
			{
				InitSA(i);
			}
			ReactionsField = new BitBlock(RCount);

			ReactionError = new sbyte[RCount];


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



		int MaxTreeCount = 2;
		int batchCount = 2500;
		ArrayList AElement = new ArrayList();
		int From = 0, Till = 0;
		int Flag = 1;
		public void Analyze()
		{

			PerceptronAnalyze id3 = new PerceptronAnalyze();

			Till += batchCount;

			id3.Analyze(ACount, HCount, AHConnections, NecessaryReactions, 0, From, Till);


			for (int n = 0; n < ACount; n++)
			{
				if (id3.Result[n] == 0)
				{
				}
				else
				{
					AElement.Add(n);
				}
			}


			if (Flag == MaxTreeCount)
			{
				Dictionary<int, string> mask = new Dictionary<int, string>();
				for (int i = 0; i < ACount; i++)
				{
					if (AElement.Contains(i) == false)
					{
						for (int j = 0; j < SCount; j++)
						{
							WeightSA[j][i] = 0;
						}
					}
					else
					{
						string maskLine = "";
						for (int j = 0; j < SCount; j++)
						{
							if (WeightSA[j][i] == 0)
							{
								maskLine += "0";
							}
							else if (WeightSA[j][i] == 1)
							{
								maskLine += "+";
							}
							else if (WeightSA[j][i] == -1)
							{
								maskLine += "-";
							}
						}
						mask.Add(i, maskLine);
					}
				}

				foreach (var m in mask)
				{
					File.AppendAllText("mask.txt", m.Value + "=" + m.Key + "\n");
				}
			}

			From += batchCount;

			Flag++;

			//Console.WriteLine(AElement.Count.ToString());

			/*for (int i = 1; i < ACount + 1; i++)
			{
				AssociationsFiled[i].ActivationLevel = 0;
			}*/

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

				if (n == 0 || n == 1)
				{
					for (int i = 0; i < HCount; i++)
					{
						// Активируем S-элементы, т.е. подаем входы и рассчитываем средний слой A-элементы
						SActivation(i);
					}
				}
				if (n >= 2)
				{
					// За каждую итерацию прокручиваем все примеры из обучающей выборки
					for (int i = 0; i < HCount; i++)
					{
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
				}
				if (n == 0)
				{
					Analyze();
					Analyze();
					//Analyze();
					//Analyze();
					//Analyze();
					//Analyze();

					AHConnections = new Dictionary<int, List<int>>();
					for (int i = 0; i < HCount; i++)
					{
						AHConnections[i] = new List<int>();
					}
				}
				if (n == 1)
				{
					int a = 1;
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

			for (int i = 0; i < ACount; i++)
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
					for (int j = 0; j < ACount; j++)
					{
						AssociationsField[j] += WeightSA[i][j];
					}
				}
			}
			double t = (DateTime.Now - begin).TotalMilliseconds;
			aTime += t;

			// Запомним как на этот пример реагировали A - элементы
			for (int j = 0; j < ACount; j++)
			{
				if (AssociationsField[j] > 0)
				{
					AHConnections[argStimulNumber].Add(j);
				}
			}
		}


		private void RActivation(int argStimulNumber)
		{
			int[] Summa = new int[RCount];
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < AHConnections[argStimulNumber].Count; i++)
				{
					int index = AHConnections[argStimulNumber][i];
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
				for (int i = 0; i < AHConnections[argStimulNumber].Count; i++)
				{
					int index = AHConnections[argStimulNumber][i];
					WeightAR[index][j] = WeightAR[index][j] + ReactionError[j];
				}
			}
		}
	}
}

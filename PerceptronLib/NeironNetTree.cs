// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

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

		public Dictionary<int, BitBlock> ExaminStimuls; // Стимулы для экзамена
		public Dictionary<int, BitBlock> ExaminReactions; // Требуемая реакция на каждый стимул во время экзамена

		public Dictionary<int, int[]> WeightSA; // Веса между S-A элементами
		public Dictionary<int, int[]> WeightAR; // Веса между A-R элементами

		private sbyte[] ReactionError;
		private Random rnd = new Random(10);

		/// <summary>
		/// Количество выходов, которые уже обучились (используется для оптимизации)
		/// </summary>
		private int ROk = 0;
		/// <summary>
		/// Количество ошибок на каждом R выходе в процессе обучения (используется для оптимизации)
		/// </summary>
		private int[] RErrorCount;


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

			ReactionsField = new BitBlock(RCount);

			ReactionError = new sbyte[RCount];


			LearnedStimuls = new Dictionary<int, BitBlock>();
			NecessaryReactions = new Dictionary<int, BitBlock>();

			ExaminStimuls = new Dictionary<int, BitBlock>();
			ExaminReactions = new Dictionary<int, BitBlock>();

			WeightAR = new Dictionary<int, int[]>(ACount);
			for (int i = 0; i < ACount; i++)
			{
				WeightAR[i] = new int[RCount];
			}

			RErrorCount = new int[RCount];
		}

		public bool IsAnalyze = false;
		public SinapsType sinapsType = SinapsType.Full;

		public enum SinapsType
		{ 
			Custom = 1,
			Sinaps2x2 = 2,
			Full = 10
		}

		public int SinapsXCount = 0;
		public int SinapsYCount = 0;

		private void InitSA(int argAId)
		{

			if (sinapsType == SinapsType.Full)
			{
				SinapsXCount = SCount /2;
				SinapsYCount = SCount /2;
			}
			else if (sinapsType == SinapsType.Sinaps2x2)
			{
				SinapsXCount = 2;
				SinapsYCount = 2;
			}


			int sinapsCount = SinapsXCount + SinapsYCount;

			int sensorNumber = 0;
			sbyte sensorType = 0;

			for (int j = 0; j < sinapsCount; j++)
			{
				sensorNumber = rnd.Next(SCount);

				if (rnd.Next(2) == 0) sensorType = 1; else sensorType = -1;

				/*
				if (j < sinapsXCount)
				{
					sensorType = 1;
				}
				else
				{
					sensorType = -1;
				}*/

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

		public void JoinEStimul(int argStimulNumber, BitBlock argPerception, BitBlock argReaction)
		{
			// Запомним обучающий стимул
			ExaminStimuls.Add(argStimulNumber, argPerception);

			// Запомним какая реакция должна быть на этот пример
			ExaminReactions.Add(argStimulNumber, argReaction);
		}


		int MaxTreeCount = 1;
		int batchCount = 5000;
		ArrayList AElement = new ArrayList();
		int From = 0, Till = 0;
		int Flag = 1;
		//public List<Graph> graph = new List<Graph>();
		public Graph graph = new Graph();


		private void StartAnalyze()
		{
			id3.Analyze(ACount, HCount, AHConnections, NecessaryReactions, rNumber, From, Till);
		}

		PerceptronAnalyze id3;
		int rNumber;

		public void Analyze(int argRNumber, int argBatchNumber)
		{
			id3 = new PerceptronAnalyze();

			From = argBatchNumber * batchCount;
			Till = batchCount + argBatchNumber * batchCount;
			rNumber = argRNumber;

			id3.graphP = graph;



			//id3.Analyze(ACount, HCount, AHConnections, NecessaryReactions, argRNumber, From, Till);

			Thread t = new Thread(StartAnalyze, 16 * 1024 * 1024); // 16 МБ
			t.Start();
			t.Join();


			for (int n = 0; n < ACount; n++)
			{
				if (id3.Result[n] == 0)
				{
				}
				else
				{
					if (AElement.Contains(n) == false)
					{
						AElement.Add(n);
					}
				}
			}

			Console.WriteLine("AElement = " + AElement.Count.ToString());
			graph = id3.graphP;
			//graph.Add(id3.graphN);

			if (argBatchNumber == MaxTreeCount)
			{
				//graph.Add(id3.graphP);

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
					/*else
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
					}*/
				}

				/*foreach (var m in mask)
				{
					File.AppendAllText("mask.txt", m.Value + "=" + m.Key.ToString() + "\n");
				}*/
			}



			//Flag++;

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
			AHMinimum = ACount;
			for (int i = 0; i < ACount; i++)
			{
				InitSA(i);
			}
			RErrorClear(true);

			int nb = 0;
			if (IsAnalyze == false) { nb = 1; }

			// Делаем очень много итераций
			for (int n = nb; n < 100000; n++)
			{
				int Error = 0;
				RErrorClear();

				DateTime begin = DateTime.Now;
				aTime = 0;

				if (n == 0 || n == 1)
				{
					for (int i = 0; i < HCount; i++)
					{
						// Активируем S-элементы, т.е. подаем входы и рассчитываем средний слой A-элементы
						SActivation(i);

						if (i % 10000 == 0)
							Console.WriteLine("AHMinimum = " + AHMinimum.ToString());
					}
					Console.WriteLine("AHMinimum = " + AHMinimum.ToString());
					Console.WriteLine("\t" + aTime.ToString() + " ms");
				}
				if (n >= 2)
				{
					// За каждую итерацию прокручиваем все примеры из обучающей выборки
					for (int i = 1; i < HCount; i++)
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
					for (int i = 0; i < RCount; i++)
					{
						if (RErrorCount[i] == 0)
						{
							RErrorCount[i] = -1;
							ROk++;
						}
					}

					double t = (DateTime.Now - begin).TotalMilliseconds;
					Console.WriteLine(n.ToString() + " - " + Error.ToString() + " - " + t.ToString() + " ms, ROk = " + ROk.ToString());
					if (Error == 0) { break; }
				}
				if (n == 0)
				{
					if (IsAnalyze == true)
					{
						for (int i = 0; i < MaxTreeCount; i++)
						{
							for (int j = 0; j < RCount; j++)
							{
								Analyze(j, i);
							}
						}

						AHConnections = new Dictionary<int, List<int>>();
						for (int i = 0; i < HCount; i++)
						{
							AHConnections[i] = new List<int>();
						}
					}
				}
				if (n == 1)
				{
					int a = 1;
				}
			}

			graph.Save("tree_");
		}


		public void Examin(int argECount)
		{
			Console.WriteLine("Begin Examination");

			int[] ErrorCount = new int[RCount];
			int AllErrorCount = 0;

			RErrorClear(true);
			AHConnections = new Dictionary<int, List<int>>();
			for (int i = 0; i < HCount; i++)
			{
				AHConnections[i] = new List<int>();
			}

			for (int n = 0; n < argECount; n++)
			{

				if (n % 100 == 0)
					Console.WriteLine("n=" + n.ToString() + "; Error=" + AllErrorCount.ToString());

				bool isError = ExaminOne(n);
				
				for (int i = 0; i < RCount; i++)
				{
					ErrorCount[i] += Math.Abs(ReactionError[i]);
				}
				if (isError == true)
				{
					AllErrorCount++;
					//Console.WriteLine("#"+n.ToString());
				}
			}

			
			for (int i = 0; i < RCount; i++)
			{
				Console.WriteLine("Error = " + i.ToString() + " - " + ErrorCount[i].ToString());
				File.AppendAllText("Result.txt", "Error = " + i.ToString() + " - " + ErrorCount[i].ToString() + "\n");
			}
			Console.WriteLine("Error = " + AllErrorCount.ToString());
			File.AppendAllText("Result.txt", "Error=" + AllErrorCount.ToString() + "\n");
		}

		public bool ExaminOne(int argNumber)
		{
			SActivation(argNumber, 1);

			// Активируем R-элементы, т.е. рассчитываем выходы
			RActivation(argNumber);
			// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
			bool isError = GetError(argNumber, 1);

			/*
			int[] e = new int[RCount + 1];
			for (int i = 1; i < RCount + 1; i++)
			{
				e[i] += ReactionError[i];
			}*/

			return isError;
		}



		double aTime = 0;

		int AHMinimum = 0;

		/// <summary>
		/// Активация S-A слоя
		/// </summary>
		/// <param name="argStimulNumber">Номер примера в выборке</param>
		/// <param name="argMode">0 - обучение, 1 - экзамен</param>
		private void SActivation(int argStimulNumber, int argMode = 0)
		{

			for (int i = 0; i < ACount; i++)
			{
				AssociationsField[i] = 0;
			}

			// Кинем на сенсоры обучающий пример
			if (argMode == 0)
			{
				SensorsField = LearnedStimuls[argStimulNumber];
			}
			else if (argMode == 1)
			{
				SensorsField = ExaminStimuls[argStimulNumber];
			}

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

			// Check
			if (AHConnections[argStimulNumber].Count < AHMinimum)
			{
				AHMinimum = AHConnections[argStimulNumber].Count;
			}
		}

		private void RErrorClear(bool argFull = false)
		{
			if (argFull == true)
			{
				for (int i = 0; i < RCount; i++)
				{
					RErrorCount[i] = 0;
				}
			}
			else
			{
				for (int i = 0; i < RCount; i++)
				{
					if (RErrorCount[i] != -1)
					{
						RErrorCount[i] = 0;
					}
				}
			}
		}


		private void RActivation(int argStimulNumber)
		{
			int[] Summa = new int[RCount];
			for (int j = 0; j < RCount; j++)
			{
				if (RErrorCount[j] != -1)
				{
					for (int i = 0; i < AHConnections[argStimulNumber].Count; i++)
					{
						int index = AHConnections[argStimulNumber][i];
						Summa[j] += WeightAR[index][j];
					}
				}
			}
			for (int i = 0; i < RCount; i++)
			{
				if (Summa[i] > 0) { ReactionsField[i] = true; }
				if (Summa[i] <= 0) { ReactionsField[i] = false; }
			}
		}

		private bool GetError(int argStimulNumber, int argMode = 0)
		{
			bool IsError = false;
			for (int i = 0; i < RCount; i++)
			{
				if (RErrorCount[i] != -1)
				{
					bool v = NecessaryReactions[argStimulNumber][i];

					if (argMode == 0)
					{
						v = NecessaryReactions[argStimulNumber][i];
					}
					else if (argMode == 1)
					{
						v = ExaminReactions[argStimulNumber][i];
					}

					if (ReactionsField[i] == v)
					{
						ReactionError[i] = 0;
					}
					else
					{
						IsError = true;
						sbyte v2 = -1; if (v == true) { v2 = 1; }
						ReactionError[i] = v2;

						RErrorCount[i]++;
					}
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

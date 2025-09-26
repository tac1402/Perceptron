// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using PerceptronLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Tac.Perceptron
{

	/// <summary>
	/// Оптимизированная версия перцептрона Розенблатта, засчет сохранения реакций A элементов для каждого примера из обучающей выборки (AHConnections)
	/// </summary>
	public class NeironNetTree
	{
		public BitBlock SensorsField; /* Сенсорное поле */
		public BitBlock ReactionsField; /* Реагирующие поле */

		protected int SCount; // Количество сенсоров
		public int ACount; // Количество ассоциаций
		protected int RCount; // Количество реакций
		protected int HCount; // Количество примеров, запоминается реакция A-элементов на каждый пример из обучающей выборки


		public Dictionary<int, List<int>> AHConnections; // Как реагируют A-элементы на каждый стимул из обучающей выборки

		public float[] AField;
		public Dictionary<int, float[]> Activations = new Dictionary<int, float[]>();

		public Dictionary<int, BitBlock> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Dictionary<int, BitBlock> ExaminStimuls; // Стимулы для экзамена
		public Dictionary<int, BitBlock> ExaminReactions; // Требуемая реакция на каждый стимул во время экзамена

		public Dictionary<int, int[]> WeightSA; // Веса между S-A элементами
		public Dictionary<int, int[]> WeightAR; // Веса между A-R элементами

		protected sbyte[] ReactionError;

		public int RndNumber = 10;
		protected Random rnd;

		protected double aTime = 0;

		private bool isSR = false;

		public NeironNetTree(int argSCount, int argACount, int argRCount, int argHCount, bool argIsSR = false)
		{
			rnd = new Random(RndNumber);
			ACount = argACount;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;

			isSR = argIsSR;
			if (argIsSR == true) { ACount += SCount * 2; }

			batchCount = ACount;

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

			for (int i = 0; i < HCount; i++)
			{
				Activations.Add(i, new float[ACount]);
			}

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
		}

		public bool IsAnalyze = false;
		public SinapsType sinapsType = SinapsType.Full;

		public enum SinapsType
		{ 
			Custom = 1,
			Sinaps2x2 = 2,
			Random = 3,
			Full = 10
		}

		public int SinapsXCount = 0;
		public int SinapsYCount = 0;

		protected void InitSA(int argAId)
		{
			if (isSR == true)
			{
				if (argAId < SCount)
				{
					WeightSA[argAId][argAId] = 1;
					return;
				}
				else if (argAId >= SCount && argAId < SCount * 2)
				{
					WeightSA[argAId - SCount][argAId] = -1;
					return;
				}
			}

			if (sinapsType == SinapsType.Full)
			{
				SinapsXCount = SCount;
				SinapsYCount = SCount;
			}
			else if (sinapsType == SinapsType.Sinaps2x2)
			{
				SinapsXCount = 2;
				SinapsYCount = 2;
			}

			int sinapsCount = SinapsXCount + SinapsYCount;

			int sensorNumber = 0;
			sbyte sensorType = 0;

			if (sinapsType == SinapsType.Random)
			{
				sinapsCount = rnd.Next(0, SinapsXCount);
				for (int j = 0; j < sinapsCount; j++)
				{
					sensorNumber = rnd.Next(SCount);
					WeightSA[sensorNumber][argAId] = 1;
				}
				sinapsCount = rnd.Next(0, SinapsYCount);
				for (int j = 0; j < sinapsCount; j++)
				{
					sensorNumber = rnd.Next(SCount);
					WeightSA[sensorNumber][argAId] = -1;
				}
			}
			else
			{
				//var square = GetRandomSquare();
				for (int j = 0; j < sinapsCount; j++)
				{
					sensorNumber = rnd.Next(SCount);

					if (j < SinapsXCount)
					{
						sensorType = 1;
					}
					else
					{
						sensorType = -1;
					}

					//sensorNumber = GetRandomPointInSquare(square);

					//if (rnd.Next(2) == 0) sensorType = 1; else sensorType = -1;
					//if (j < SinapsXCount) sensorType = 1; else sensorType = -1;

					WeightSA[sensorNumber][argAId] = sensorType;
				}
			}
		}

		private (int x, int y) GetRandomSquare()
		{
			// Шаг 1: Выбор случайного квадрата
			/*var squares = new (int x, int y)[] {
				(0, 0),  // Верхний левый
				(14, 0), // Верхний правый
				(7, 7),  // Центральный
				(0, 14), // Нижний левый
				(14, 14) // Нижний правый
			};*/

			List<(int x, int y)> squares = new List<(int, int)>();

			// Генерируем квадраты с шагом 7 по обеим осям
			for (int y = 0; y < 28; y += 7)
			{
				for (int x = 0; x <28; x += 7)
				{
					squares.Add((x, y));
				}
			}
			for (int y = 0; y < 28 - 7; y += 7)
			{
				for (int x = 0; x < 28 - 7; x += 7)
				{
					squares.Add((x + 3, y + 3));
				}
			}


			(int x, int y) selectedSquare = squares[rnd.Next(0, squares.Count)];

			return selectedSquare;
		}


		private int GetRandomPointInSquare((int x, int y) argSquare)
		{
			// Шаг 2: Генерация точки внутри квадрата
			int xCoord = argSquare.x + rnd.Next(0, 7);
			int yCoord = argSquare.y + rnd.Next(0, 7);

			// Шаг 3: Преобразование в индекс (если требуется)
			int globalIndex = yCoord * 28 + xCoord;
			return globalIndex;
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


		public int MaxTreeCount = 2;
		public int batchCount = 0;

		List<int> AElement = new List<int>();
		int From = 0, Till = 0;
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

			if (argBatchNumber + 1 == MaxTreeCount)
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
				}
			}
		}

		public int SANumber = 0;
		public int SASelectCount = 0;

		public void SaveSA(string argFileName, List<int> argActive)
		{
			Console.Write(".");
			FileStream file = new FileStream(argFileName, FileMode.Create);
			BinaryWriter writer = new BinaryWriter(file);

			for (int j = 0; j < argActive.Count; j++)
			{
				int index = argActive[j];
				for (int i = 0; i < SCount; i++)
				{
					if (WeightSA[i][index] != 0)
					{
						writer.Write(i);
						writer.Write(WeightSA[i][index]);
					}
				}
				writer.Write(-2);
			}
			writer.Close();
			file.Close();
			Console.WriteLine("SaveSA");
		}


		private bool isLoaded = false;


		public void LoadSA(string argFileName)
		{
			isLoaded = false;
			Console.Write(".");
			FileStream file = new FileStream(argFileName, FileMode.Open);
			BinaryReader reader = new BinaryReader(file);

			int i = 0;
			while (reader.BaseStream.Position != reader.BaseStream.Length)
			{
				int v = reader.ReadInt32();
				if (v != -2)
				{
					int type = reader.ReadInt32();
					WeightSA[v][i] = type;
				}
				else
				{
					i++;
					ACount++;
				}
			}
			reader.Close();
			file.Close();
			Console.WriteLine("LoadAH");
			isLoaded = true;
		}


		/// <summary>
		/// Когда все примеры добавлены, вызывается чтобы перцептрон их выучил
		/// </summary>
		public virtual void Learned()
		{
			if (isLoaded == false)
			{
				for (int i = 0; i < ACount; i++)
				{
					InitSA(i);
				}
			}

			int nb = 0;
			if (IsAnalyze == false) { nb = 1; }

			int stopError = 5;
			int stopCount = 0;
			int oldError = 0;

			// Делаем очень много итераций
			for (int n = nb; n < 100000; n++)
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

					double t = (DateTime.Now - begin).TotalMilliseconds;
					Console.WriteLine(n.ToString() + " - " + Error.ToString() + " - " + t.ToString() + " ms");

					if (oldError == Error) { stopCount++; }	else { stopCount = 0; }
					oldError = Error;

					if (Error == 0) { break; }
				}
				if (n == 0)
				{
					if (IsAnalyze == true)
					{
						if (SASelectCount == 0)
						{
							batchCount = ACount/2;
							Analyze();
						}
						else
						{
							for (int s = 0; s < SASelectCount; s++)
							{
								rnd = new Random(RndNumber + s);

								Analyze();
								SANumber++;

								SaveSA("SA_" + SANumber.ToString() + ".bin", AElement);


								AElement = new List<int>();
								WeightSA = new Dictionary<int, int[]>(SCount);
								for (int i = 0; i < SCount; i++)
								{
									WeightSA[i] = new int[ACount];
								}
								for (int i = 0; i < ACount; i++)
								{
									InitSA(i);
								}
							}
						}

						AHConnections = new Dictionary<int, List<int>>();
						for (int i = 0; i < HCount; i++)
						{
							AHConnections[i] = new List<int>();
						}

						for (int i = 0; i < HCount; i++)
						{
							// Активируем S-элементы, т.е. подаем входы и рассчитываем средний слой A-элементы
							SActivation(i);
						}

					}
				}
				if (n == 1)
				{
					int a = 1;
				}
			}

			CalcInfo();

			graph.Save("tree_");
		}

		private void CalcInfo()
		{
			InformationGainCalculator gain = new InformationGainCalculator(NecessaryReactions);
			float[][] gainValue = gain.CalculateInformationGain(Activations, ACount, RCount);

			LRegion region = new LRegion(NecessaryReactions);
			int rCount = region.Calc(Activations);
			region.NeighborhoodPurity(Activations);

			int stop = 0;
			for (int j = 0; j < ACount; j++)
			{
				bool isEmpty = true;
				for (int i = 0; i < RCount; i++)
				{
					if (gainValue[j] != null && gainValue[j][i] > 0)
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
			Console.WriteLine("R: " + rCount.ToString("F0") + "/" + region.avgPairwise.ToString("F0") 
					+ "\tP: " + region.minPurity.ToString("F4")+ "-" + region.avgPurity.ToString("F4") + "-" + region.maxPurity.ToString("F4")
					+ "\tAN: " + stop.ToString("F0"));
		}



		private void Analyze()
		{
			for (int i = 0; i < MaxTreeCount; i++)
			{
				for (int j = 0; j < RCount; j++)
				{
					Analyze(j, i);
				}
			}
		}


		public void Examin(int argECount)
		{
			Console.WriteLine("Begin Examination");

			int[] ErrorCount = new int[RCount];
			int AllErrorCount = 0;

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



		/// <summary>
		/// Активация S-A слоя
		/// </summary>
		/// <param name="argStimulNumber">Номер примера в выборке</param>
		/// <param name="argMode">0 - обучение, 1 - экзамен</param>
		protected void SActivation(int argStimulNumber, int argMode = 0)
		{

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

			AField = new float[ACount];
			for (int i = 0; i < SCount; i++)
			{
				if (SensorsField[i] == true)
				{
					for (int j = 0; j < ACount; j++)
					{
						AField[j] += WeightSA[i][j];
					}
				}
			}
			double t = (DateTime.Now - begin).TotalMilliseconds;
			aTime += t;

			// Запомним как на этот пример реагировали A - элементы
			for (int j = 0; j < ACount; j++)
			{
				if (AField[j] > 0)
				{
					AHConnections[argStimulNumber].Add(j);
				}
			}

			Activations[argStimulNumber] = AField;

		}


		protected void RActivation(int argStimulNumber)
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

		protected bool GetError(int argStimulNumber, int argMode = 0)
		{
			bool IsError = false;
			for (int i = 0; i < RCount; i++)
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

				}
			}
			return IsError;
		}

		protected void LearnedStimul(int argStimulNumber)
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

// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tac.Perceptron
{

	/// <summary>
	/// Оптимизированная версия перцептрона Розенблатта, засчет сохранения реакций A элементов для каждого примера из обучающей выборки (AHConnections)
	/// </summary>
	public class NeironNetTree
	{
		public float[] SensorsField; /* Сенсорное поле */
		public sbyte[] ReactionsOutput; /* Реагирующие поле */

		protected int SCount; // Количество сенсоров
		public int ACount; // Количество ассоциаций
		protected int RCount; // Количество реакций
		protected int HCount; // Количество примеров, запоминается реакция A-элементов на каждый пример из обучающей выборки
		private int ECount; // Количество примеров


		public float[] AField;
		public Dictionary<int, float[]> Activations = new Dictionary<int, float[]>();

		public Dictionary<int, float[]> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Dictionary<int, float[]> ExaminStimuls; // Стимулы для экзамена
		public Dictionary<int, BitBlock> ExaminReactions; // Требуемая реакция на каждый стимул во время экзамена

		protected float[] ReactionError;

		public int RndNumber = 10;
		protected Random rnd;

		private string Name = "";
		private string Arh
		{
			get { return "[" + HCount.ToString() + "]" + SCount.ToString() + "x" + ACount.ToString(); }
		}


		private PerceptronAA AB;

		public NeironNetTree(int argSCount, int argACount, int argRCount, int argHCount, int argECount, string argName = "")
		{
			rnd = new Random(RndNumber);
			ACount = argACount;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;
			ECount = argECount;
			Name = argName;

			batchCount = ACount;

			SensorsField = new float[SCount];

			for (int i = 0; i < HCount; i++)
			{
				Activations.Add(i, new float[ACount]);
			}

			ReactionsOutput = new sbyte[RCount];
			ReactionError = new float[RCount];


			LearnedStimuls = new Dictionary<int, float[]>();
			NecessaryReactions = new Dictionary<int, BitBlock>();

			ExaminStimuls = new Dictionary<int, float[]>();
			ExaminReactions = new Dictionary<int, BitBlock>();

			AB = new PerceptronAA(SCount, ACount, RCount);

		}

		public bool IsAnalyze = false;
		public SinapsType sinapsType = SinapsType.Full;

		public enum SinapsType
		{ 
			Custom = 1,
			Sinaps1x1 = 2,
			Sinaps2x2 = 3,
			Random = 4,
			Full = 10
		}

		public int SinapsXCount = 0;
		public int SinapsYCount = 0;

		protected void InitSA(int argAId)
		{

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
			else if (sinapsType == SinapsType.Sinaps1x1)
			{
				SinapsXCount = 1;
				SinapsYCount = 1;
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
					//WeightSA[sensorNumber][argAId] = 1;
					AB.SA(sensorNumber, argAId, 1);
				}
				sinapsCount = rnd.Next(0, SinapsYCount);
				for (int j = 0; j < sinapsCount; j++)
				{
					sensorNumber = rnd.Next(SCount);
					//WeightSA[sensorNumber][argAId] = -1;
					AB.SA(sensorNumber, argAId, -1);
				}
			}
			else
			{
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

					AB.SA(sensorNumber, argAId, sensorType);
					//WeightSA[sensorNumber][argAId] = sensorType;
				}
			}
		}


		/// <summary>
		/// Добавить на обработку новый пример из обучающей выборки
		/// </summary>
		/// <param name="argStimulNumber">Номер примера из обучающей выборки</param>
		/// <param name="argPerception">Стимулы (входы) из примера обучающей выборки</param>
		/// <param name="argReaction">Нужная реакция (выходы) из примера обучающей выборки</param>
		public void JoinStimul(int argStimulNumber, float[] argPerception, BitBlock argReaction)
		{
			// Запомним обучающий стимул
			LearnedStimuls.Add(argStimulNumber, argPerception);

			// Запомним какая реакция должна быть на этот пример
			NecessaryReactions.Add(argStimulNumber, argReaction);
		}

		public void JoinEStimul(int argStimulNumber, float[] argPerception, BitBlock argReaction)
		{
			// Запомним обучающий стимул
			ExaminStimuls.Add(argStimulNumber, argPerception);

			// Запомним какая реакция должна быть на этот пример
			ExaminReactions.Add(argStimulNumber, argReaction);
		}


		public int MaxTreeCount = 3;
		public int batchCount = 0;

		List<int> AElement = new List<int>();
		int From = 0, Till = 0;
		public Graph graph = new Graph();


		private void StartAnalyze()
		{
			id3.Analyze(ACount, HCount, Activations, NecessaryReactions, rNumber, From, Till);
		}

		PerceptronID3 id3;
		int rNumber;

		public void Analyze(int argRNumber, int argBatchNumber)
		{
			id3 = new PerceptronID3();

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

			string output = "AElement = " + AElement.Count.ToString();

			Console.WriteLine(output);
			File.AppendAllText("Error_" + Arh + ".txt", output + "\n");

			Console.WriteLine();
			graph = id3.graphP;

			if (argBatchNumber + 1 == MaxTreeCount)
			{
				Dictionary<int, string> mask = new Dictionary<int, string>();

				Dictionary<int, float[]> oldWeightSA = new Dictionary<int, float[]>();
				for (int i = 0; i < ACount; i++)
				{
					for (int j = 0; j < SCount; j++)
					{
						if (oldWeightSA.ContainsKey(j) == false)
						{
							oldWeightSA.Add(j, new float[ACount]);
						}
						oldWeightSA[j][i] = AB.SA_(j, i);
					}
				}

				ACount = AElement.Count;

				AB = new PerceptronAA(SCount, ACount, RCount);

				for (int i = 0; i < ACount; i++)
				{
					for (int j = 0; j < SCount; j++)
					{
						AB.SA(j, i, oldWeightSA[j][AElement[i]]);
					}
				}

				int a = 1;
			}
		}

		public int SANumber = 0;


		/// <summary>
		/// Когда все примеры добавлены, вызывается чтобы перцептрон их выучил
		/// </summary>
		public virtual void Learned()
		{
			DateTime beginFull = DateTime.Now;

			for (int i = 0; i < ACount; i++)
			{
				InitSA(i);
			}

			int nb = 0;
			if (IsAnalyze == false) { nb = 1; }

			int stopCount = 0;
			int oldError = 0;

			foreach (var rr in NecessaryReactions)
			{
				rr.Value.To();
			}
			for (int i = 0; i < ExaminStimuls.Count; i++)
			{
				ExaminReactions[i].To();
			}

			// Делаем очень много итераций
			for (int n = nb; n < 100000; n++)
			{
				int Error = 0;

				DateTime begin = DateTime.Now;
				DateTime beginA;
				DateTime beginR;

				double tA = 0;
				double tR = 0;

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

						beginA = DateTime.Now;
						SActivation(i);
						tA += (DateTime.Now - beginA).TotalMilliseconds;

						// Активируем R-элементы, т.е. рассчитываем выходы
						beginR = DateTime.Now;
						RActivation(i);
						tR += (DateTime.Now - beginR).TotalMilliseconds;

						// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
						bool e = GetError(i);
						if (e == true)
						{
							LearnedStimulAR(i);
							Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.
						}
					}

					double t = (DateTime.Now - begin).TotalMilliseconds;

					string output = n.ToString() + " - " + Error.ToString() + " - " 
						+ t.ToString("F0") + " ms " + tA.ToString("F0") + "/" + tR.ToString("F0");

					Console.WriteLine(output);
					File.AppendAllText("Error_" + Arh + ".txt", output + "\n");

					if (oldError == Error) { stopCount++; }	else { stopCount = 0; }
					oldError = Error;

					if (Error == 0) { break; }
				}
				if (n == 0)
				{
					if (IsAnalyze == true)
					{
						batchCount = ACount / MaxTreeCount;
						DateTime beginAn = DateTime.Now;
						Analyze();
						double t = (DateTime.Now - beginAn).TotalMilliseconds;

						string output = "\tAnalyzeTime = " + t.ToString() + " ms ";

						Console.WriteLine(output);
						File.AppendAllText("Error_" + Arh + ".txt", output + "\n");
					}
				}
				if (n == 1)
				{
					int a = 1;
				}
			}

			double tFull = (DateTime.Now - beginFull).TotalMilliseconds;

			string outputF = "\tFullTime = " + tFull.ToString() + " ms ";

			Console.WriteLine(outputF);
			File.AppendAllText("Error_" + Arh + ".txt", outputF + "\n");

			graph.Save("tree_" + Arh);

			//CalcInfo();
		}

		/*private void CalcInfo()
		{
			Gain gain = new Gain(NecessaryReactions);
			float[][] gainValue = gain.CalculateInformationGain(Activations, ACount, RCount);

			LRegion region = new LRegion(NecessaryReactions);
			int rCount = region.Calc(Activations);
			region.NeighborhoodPurity(Activations, HCount / 2);

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
			string output = "R: " + rCount.ToString("F0")
					+ "\tP: " + region.minPurity.ToString("F4") + "-" + region.avgPurity.ToString("F4") + "-" + region.maxPurity.ToString("F4")
					+ "\tAN: " + stop.ToString("F0");

			Console.WriteLine(output);
			File.AppendAllText("Error_" + SCount.ToString() + "x" + StartACount.ToString() + ".txt", output + "\n");
		}*/



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

			for (int n = 0; n < argECount; n++)
			{

				if (n % 100 == 0)
					Console.WriteLine("n=" + n.ToString() + "; Error=" + AllErrorCount.ToString());

				bool isError = ExaminOne(n);
				
				for (int i = 0; i < RCount; i++)
				{
					ErrorCount[i] += (int)Math.Abs(ReactionError[i]);
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

			AField = AB.AActivation(SensorsField);

			// Запомним как на этот пример реагировали A - элементы
			Activations[argStimulNumber] = AField;

		}


		float[] RField;
		protected void RActivation(int argStimulNumber)
		{
			RField = AB.RActivation(AField);

			/*int[] Summa = new int[RCount];
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < AHConnections[argStimulNumber].Count; i++)
				{
					int index = AHConnections[argStimulNumber][i];
					Summa[j] += WeightAR[index][j];
				}
			}*/

			for (int i = 0; i < RCount; i++)
			{
				if (RField[i] > 0) { ReactionsOutput[i] = 1; }
				else if (RField[i] < 0) { ReactionsOutput[i] = -1; }
				else { ReactionsOutput[i] = 0; }
			}
		}

		private bool GetError(int argStimulNumber, int argMode = 0)
		{
			bool IsError = false;
			for (int i = 0; i < RCount; i++)
			{
				sbyte v = 0;
				if (argMode == 0)
				{
					v = NecessaryReactions[argStimulNumber].DataB[i];
				}
				else if (argMode == 1)
				{
					v = ExaminReactions[argStimulNumber].DataB[i];
				}

				if (ReactionsOutput[i] != 0 && ReactionsOutput[i] == v)
				{
					ReactionError[i] = 0;
				}
				else
				{
					IsError = true;
					ReactionError[i] = v;
				}
			}
			return IsError;
		}

		protected void LearnedStimulAR(int argStimulNumber)
		{
			AB.LearnedStimulAR(ReactionError, AField);
		}
	}
}

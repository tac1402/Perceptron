// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Perceptron
{

	/// <summary>
	/// Оптимизированная версия перцептрона Розенблатта, засчет сохранения реакций A элементов для каждого примера из обучающей выборки (AHConnections)
	/// </summary>
	public class PerceptronDT : Perceptron
	{
		public float[] SensorsField; /* Сенсорное поле */
		//public sbyte[] ReactionsOutput; /* Реагирующие поле */


		public float[] AField;
		public Dictionary<int, float[]> Activations = new Dictionary<int, float[]>();

		//protected float[] ReactionError;

		//public int RndNumber = 10;

		private string Name = "";
		private string Arh
		{
			get { return "[" + HCount.ToString() + "]" + SCount.ToString() + "x" + ACount.ToString(); }
		}

		public int MaxTreeCount = 1;
		public bool InitSetSA = false; // влияет на иницициализацию SA связей, можно или устанавливать (SetSA()) или прибавлять (SA())

		public int batchCount = 0;
		int PCount = 1000;

		private Purity purity;

		private bool isLoaded = false;

		public PerceptronDT(int argSCount, int argACount, int argRCount, int argHCount, int argECount, int argMaxTreeCount = 1, string argName = "")
			: base(argSCount, argACount, argRCount, argHCount, argECount)
		{
			MaxTreeCount = argMaxTreeCount;
			Name = argName;

			batchCount = ACount;

			SensorsField = new float[SCount];

			for (int i = 0; i < HCount; i++)
			{
				Activations.Add(i, new float[ACount/ MaxTreeCount]);
			}

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
			else if (sinapsType == SinapsType.Random)
			{
				if (SinapsXCount == 0) { SinapsXCount = SCount; }
				if (SinapsYCount == 0) { SinapsYCount = SCount; }
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
				for (int j = 0; j < sinapsCount; j++)
				{
					sensorNumber = rnd.Next(SCount);
					if (rnd.Next(2) == 0) sensorType = 1; else sensorType = -1;

					AB.SetSA(sensorNumber, argAId, sensorType);
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

					if (InitSetSA == true)
					{
						AB.SetSA(sensorNumber, argAId, sensorType);
					}
					else
					{
						AB.SA(sensorNumber, argAId, sensorType);
					}
					//WeightSA[sensorNumber][argAId] = sensorType;
				}
			}

			/*
			string s = "";
			for (int i = 0; i < SCount; i++)
			{
				s += AB.SA_(i, argAId);
			}
			int a = 1;*/
		}



		List<int> AElement = new List<int>();
		int From = 0, Till = 0;
		public Graph graph = new Graph();


		private void StartAnalyze()
		{
			id3.Analyze(ACount, HCount, Activations, NecessaryReactions, From, Till);
		}

		PerceptronID3 id3;

		public void Analyze(int argBatchNumber)
		{
			id3 = new PerceptronID3();

			From = argBatchNumber * batchCount;
			Till = batchCount + argBatchNumber * batchCount;

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

		//public int SANumber = 0;

		private int[] Shuffle(int[] list)
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

		List<int> PuritySamples;
		Dictionary<int, int> PuritySamples_;
		public void Get1000(int totalSamples, int sampleSize)
		{
			// Создаем массив всех возможных индексов
			int[] allIndices = Enumerable.Range(0, totalSamples).ToArray();

			allIndices = Shuffle(allIndices);

			PuritySamples = allIndices.Take(sampleSize).ToList();

			PuritySamples_ = new Dictionary<int, int>();
			for (int i = 0; i < PuritySamples.Count; i++)
			{
				PuritySamples_.Add(PuritySamples[i], i);
			}
		}

		public void LoadWeights()
		{
			string weightFileName = WeightFileName();

			if (File.Exists(weightFileName))
			{
				ACount = 12613;
				AB = new PerceptronAA(SCount, ACount, RCount);

				int ret = AB.LoadWeights(weightFileName);
				if (ret != 0)
				{
					isLoaded = true;
				}
				else
				{
					int a = 1;
				}
			}
		}

		/// <summary>
		/// Когда все примеры добавлены, вызывается чтобы перцептрон их выучил
		/// </summary>
		public virtual void Learned()
		{
			DateTime beginFull = DateTime.Now;

			string weightFileName = WeightFileName();
			LoadWeights();
			if (isLoaded)
			{
				IsAnalyze = false;
			}
			else
			{
				for (int i = 0; i < ACount; i++)
				{
					InitSA(i);
				}
			}

			int nb = 0;
			if (IsAnalyze == false) { nb = 1; }

			int stopCount = 0;
			int oldError = 0;

			purity = new Purity(NecessaryReactions);
			int[] indexL = new int[HCount];

			// Делаем очень много итераций
			for (int n = nb; n < 100000; n++)
			{
				int Error = 0;

				DateTime begin = DateTime.Now;
				DateTime beginA;
				DateTime beginR;

				double tA = 0;
				double tR = 0;

				if (n >= 2)
				{
					indexL = Shuffle(indexL);
					Get1000(HCount, PCount);
					purity.SelectReaction(PuritySamples);

					// За каждую итерацию прокручиваем все примеры из обучающей выборки
					for (int i = 0; i < HCount; i++)
					{
						int index = indexL[i];
						//int index = i;

						beginA = DateTime.Now;
						SActivation(index);
						tA += (DateTime.Now - beginA).TotalMilliseconds;

						// Активируем R-элементы, т.е. рассчитываем выходы
						beginR = DateTime.Now;
						RActivation(index);
						tR += (DateTime.Now - beginR).TotalMilliseconds;

						// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
						GetError(index);
						if (r.E == true)
						{
							LearnedStimulAR(index, r.Error);
							Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.
						}
					}

					//if (n % 10 == 0)
					//{
						bool ret = AB.SaveWeights(weightFileName);
					//}

					int er = 0; int fer = 0;
					(er, fer) = Examin(ECount, false);


					//purity.NeighborhoodPurity(Activations, PCount / 2);
					string outputA = "\tP: " + purity.minPurity.ToString("F4") + "-" + purity.avgPurity.ToString("F4") + "-" + purity.maxPurity.ToString("F4");


					double t = (DateTime.Now - begin).TotalMilliseconds;

					string output = n.ToString() + " - " + Error.ToString() + " - " 
						+ t.ToString("F0") + " ms " + tA.ToString("F0") + "/" + tR.ToString("F0")
						+"\tE: " + er.ToString() + " / " + fer.ToString();

					Console.WriteLine(output);
					Console.WriteLine(outputA);
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

						int r = 0;
						foreach (var rr in NecessaryReactions)
						{
							indexL[r] = rr.Key;
							r++;
						}

						Activations = new Dictionary<int, float[]>();
						for (int i = 0; i < PCount; i++)
						{
							Activations.Add(i, new float[ACount]);
						}

						AB.SaveWeights("Begin" + weightFileName);
						int a = 1;
					}
				}
				if (n == 1)
				{
					int r = 0;
					foreach (var rr in NecessaryReactions)
					{
						indexL[r] = rr.Key;
						r++;
					}

					Activations = new Dictionary<int, float[]>();
					for (int i = 0; i < PCount; i++)
					{
						Activations.Add(i, new float[ACount]);
					}
				}
			}

			AB.SaveWeights(weightFileName);


			double tFull = (DateTime.Now - beginFull).TotalMilliseconds;

			string outputF = "\tFullTime = " + tFull.ToString() + " ms ";

			Console.WriteLine(outputF);
			File.AppendAllText("Error_" + Arh + ".txt", outputF + "\n");

			graph.Save("tree_" + Arh);

			//CalcInfo();
		}

		public string WeightFileName()
		{
			string weightFileName = "weight" + Arh;
			if (Name != "")
			{
				weightFileName += "_" + Name;
			}
			weightFileName += ".bin";
			return weightFileName;
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
				int a = 0;
				for (int j = 0; j < HCount; j++)
				{
					// Активируем S-элементы, т.е. подаем входы и рассчитываем средний слой A-элементы
					SActivation(j, 0, batchCount*i, batchCount * (i+1));
				}

				Analyze(i);
			}
		}


		bool logErrorType = true;
		public (int, int) Examin(int argECount, bool log = true)
		{
			//Console.WriteLine("Begin Examination");
			
			string weightFileName = WeightFileName();
			//LoadWeights();

			int[] ErrorCount = new int[RCount];
			int AllErrorCount = 0;
			int AllFastErrorCount = 0;
			Dictionary<int, int> error = new Dictionary<int, int>();

			for (int n = 0; n < argECount; n++)
			{
				ExaminOne(n);

				/*
				string output = "";
				for (int i = 0; i < RCount; i++)
				{
					if (ReactionsOutput[i] == -1)
					{
						output += "0";
					}
					else
					{
						output += "1";
					}
				}
				File.AppendAllText("Output_" + Arh + ".txt", output);
				*/

				for (int i = 0; i < RCount; i++)
				{
					ErrorCount[i] += (int)Math.Abs(r.Error[i]);
				}
				if (r.IsErrorHard == true)
				{
					AllErrorCount++;
				}
				if (r.IsErrorSoft == true)
				{
					AllFastErrorCount++;
				}

				if (logErrorType)
				{
					if (r.IsErrorSoft == true)
					{
						error.Add(n, 2);
					}
					else if (r.IsErrorHard == true)
					{
						error.Add(n, 1);
					}
				}
			}

			if (log == true)
			{
				for (int i = 0; i < RCount; i++)
				{
					Console.WriteLine("Error = " + i.ToString() + " - " + ErrorCount[i].ToString());
					File.AppendAllText("Result_" + Arh + ".txt",
							"Error = " + i.ToString() + " - " + ErrorCount[i].ToString() + "\n");
				}
				Console.WriteLine("Error = " + AllErrorCount.ToString());
				File.AppendAllText("Result_" + Arh + ".txt", "Error=" + AllErrorCount.ToString() + "\n");

				if (logErrorType)
				{
					foreach (var item in error)
					{
						File.AppendAllText("ErrorLog.txt", item.Key.ToString() + "\t" + item.Value.ToString() + "\n");
					}
				}
			}
			return (AllErrorCount, AllFastErrorCount);
		}

		public void ExaminOne(int argNumber)
		{
			SActivation(argNumber, 1);
			// Активируем R-элементы, т.е. рассчитываем выходы
			RActivation(argNumber);
			// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
			GetError(argNumber, 1);
		}


		/// <summary>
		/// Активация S-A слоя
		/// </summary>
		/// <param name="argStimulNumber">Номер примера в выборке</param>
		/// <param name="argMode">0 - обучение, 1 - экзамен</param>
		protected void SActivation(int argStimulNumber, int argMode = 0, int startA = 0, int endA = -1)
		{

			// Кинем на сенсоры обучающий пример
			if (argMode == 0)
			{
				SensorsField = LearnedStimuls[0].Stimuls[argStimulNumber];
			}
			else if (argMode == 1)
			{
				SensorsField = ExaminStimuls[0].Stimuls[argStimulNumber];
			}

			AField = AB.AActivation(SensorsField, startA, endA);

			if (endA != -1)
			{
				// Запомним как на этот пример реагировали A - элементы
				Activations[argStimulNumber] = AField;
			}
			else
			{
				if (PuritySamples != null && PuritySamples.Contains(argStimulNumber))
				{
					int index = PuritySamples_[argStimulNumber];
					Activations[index] = AField;
				}
			}
			/*
			string s = "";
			for (int i = 0; i < ACount; i++)
			{
				if (AField[i] > 0)
				{
					s += "1";
				}
				else
				{
					s += "0";
				}
			}
			AA.Add(s, 0);
			*/
			int a2 = 1;
		}

		//Dictionary<string, int> AA = new Dictionary<string, int>();

		protected void RActivation(int argStimulNumber)
		{
			RField = AB.RActivation(AField);
		}


		protected void LearnedStimulAR(int argStimulNumber, float[] rError)
		{
			AB.LearnedStimulAR(rError, AField);
		}

	}
}

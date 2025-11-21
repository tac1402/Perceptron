// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Collections.Generic;
using System.IO;

namespace Tac.Perceptron
{

	/// <summary>
	/// Перцептрон TL&NL - Двухуровневое и нормализированное обучение
	/// Perceptron TL&NL – Perceptron witch Two-Level & Normalization Learning
	/// </summary>
	public class PerceptronTLNL : Perceptron
	{

		public float[] SensorsField; /* Сенсорное поле */

		public float[] AField;
		public float[] AFieldNorm;
		public float[][] AFieldT;


		public List<int> ExceptStimul = new List<int>();
		public List<int> OnlyStimul = new List<int>();

		public int[] ErrorLog;
		private int OldError = 0;

		public Dictionary<int, float[]> Activations = new Dictionary<int, float[]>();
		//private Gain gain;
		private Purity purity;

		private string Name = "";

		private string Arh
		{
			get { return "[" + HCount.ToString() + "]" + SCount.ToString() + "x" + ACount.ToString(); }
		}

		private LayerSA LayerSA;
		private LayerAR LayerAR;


		public PerceptronTLNL(int argSCount, int argACount, int argRCount, int argHCount, int argECount, string argName = "") 
			: base (argSCount, argACount, argRCount, argHCount, argECount)
		{
			Name = argName;


			SensorsField = new float[SCount];

			AFieldT = new float[1][];

			ErrorLog = new int[HCount];

			AB = new PerceptronAA(SCount, ACount, RCount);
		}

		public PerceptronTLNL(int argSCount, int argACount, int argRCount, int argHCount, int argECount, LayerSA argLayerSA, string argName = "", int argTSet = 1)
			: base(argSCount, argACount, argRCount, argHCount, argECount, argTSet)
		{
			Name = argName;

			SensorsField = new float[SCount];
			ErrorLog = new int[HCount];

			AField = new float[ACount];
			AFieldT = new float[TSet][];

			LayerSA = argLayerSA;
			AB = new PerceptronAA(SCount, ACount, 0);
			LayerAR = new LayerAR((ACount + LayerSA.ACount) * TSet, RCount);
		}



		int PCount = 1000;
		private void InitAnalyze()
		{
			for (int i = 0; i < PCount; i++)
			{
				Activations.Add(i, new float[ACount]);
			}

			purity = new Purity(NecessaryReactions);
			//gain = new Gain(NecessaryReactions);
		}




		/*public void ExaminAB(int argECount, PerceptronTLNL perceptronB)
		{
			int AllErrorCount = 0;
			int AllFastErrorCount = 0;
			int NotSure = 0;
			int SureB = 0;
			int SureB2 = 0;
			int s1 = 0;
			int s2 = 0;
			int s3 = 0;
			int s4 = 0;

			for (int i = 0; i < ExaminStimuls.Count; i++)
			{
				ExaminReactions[i].To();
			}

			errorType.Add("s1", 0);
			errorType.Add("s2", 0);
			errorType.Add("s3", 0);
			errorType.Add("s4", 0);

			for (int n = 0; n < argECount; n++)
			{

				//if (n % 100 == 0)
				//	Console.WriteLine("n=" + n.ToString() + "; Error=" + AllErrorCount.ToString());

				int index = ArgMax(ExaminReactions[n].DataB);

				(bool isError, bool isFastError) = ExaminOne(n);
				int indexA = ArgMax(RField);
				bool isSureA = IsSure(ReactionsOutput);

				(bool isErrorB, bool isFastErrorB) = perceptronB.ExaminOne(n);
				int indexB = ArgMax(perceptronB.RField);
				bool isSureB = IsSure(perceptronB.ReactionsOutput);

				string output = "";
				if (isError)
				{
					AllErrorCount++;
				}
				if (isFastError)
				{
					AllFastErrorCount++;
				}
				string outputA = "";
				for (int i = 0; i < RCount; i++)
				{
					if (ReactionsOutput[i] == 1) { outputA += "1"; } else { outputA += "0"; }
				}
				string outputB = "";
				for (int i = 0; i < RCount; i++)
				{
					if (perceptronB.ReactionsOutput[i] == 1) { outputB += "1"; } else { outputB += "0"; }
				}


				if (isSureA == false)
				{
					NotSure++;

					if (isSureB == true)
					{
						if (isErrorB == true)
						{
							AddError(n, "s3", index, outputA, outputB, isError, isFastError, isErrorB, isFastErrorB);
							s3++;
						}
					}
					else
					{
						if (isFastErrorB == true)
						{
							AddError(n, "s4", index, outputA, outputB, isError, isFastError, isErrorB, isFastErrorB);
							s4++;
						}
					}
				}
				else
				{
					if (isSureB == true && isError == false)
					{
						SureB++;
					}

					if (isError == true)
					{
						AddError(n, "s1", index, outputA, outputB, isError, isFastError, isErrorB, isFastErrorB);
						s1++;
					}
				}

			}
			File.AppendAllText("Exam.txt", AllErrorCount.ToString() + "\t" + AllFastErrorCount.ToString() + "\t" + NotSure.ToString() + 
				"\t" + SureB.ToString() + "\t" + SureB2.ToString() +
				 "\t S: " + s1.ToString() + "\t" + s2.ToString() + "\t" + s3.ToString() + "\t" + s4.ToString() + "\n");
			
			foreach (var error in errorType)
			{
				File.AppendAllText("Exam.txt", error.Key + " = " + error.Value.ToString() + "\n");
			}


		}*/

		Dictionary<string, int> errorType = new Dictionary<string, int>();

		public void AddError(int n, string type, int index, string outputA, string outputB,
			bool isError, bool isFastError, bool isErrorB, bool isFastErrorB)
		{
			string output = n.ToString() + "-" + type + ". " + index.ToString() + "\t" + ExaminReactions[n].ToString() + "\t" + outputA + "\t" + outputB;
			output += "\n\t" + isError.ToString() + "\t" + isFastError.ToString();
			output += "\n\t" + isErrorB.ToString() + "\t" + isFastErrorB.ToString();

			if (isError && isFastError && isErrorB && isFastErrorB)
			{
				File.AppendAllText("ExamE.txt", output + "\n");
			}
			else
			{
				errorType[type]++;
				File.AppendAllText("Exam.txt", output + "\n");
			}
		}


		bool logErrorType = false;
		public (int, int) Examin(int argECount, bool log = true)
		{
			//Console.WriteLine("Begin Examination");

			int[] ErrorCount = new int[RCount];
			int AllErrorCount = 0;
			int AllFastErrorCount = 0;
			Dictionary<int, int> error = new Dictionary<int, int>();

			for (int n = 0; n < argECount; n++)
			{

				//if (n % 100 == 0)
				//	Console.WriteLine("n=" + n.ToString() + "; Error=" + AllErrorCount.ToString());

				ExaminOne2(n);

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
				File.AppendAllText("Result_" + Arh + ".txt",
						"p3 = " + p3.ToString("F16").TrimEnd('0') + "; c3 = " + correct3.ToString("F16").TrimEnd('0') + "\n");

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

		public void ExaminOne2(int argNumber)
		{
			// Активируем A-элементы
			for (int t = 0; t < TSet; t++)
			{
				LayerSA.SActivation(ExaminStimuls[t].Stimuls[argNumber], 0, -1, t);
				SActivation(argNumber, 1, t);
			}

			// Активируем R-элементы, т.е. рассчитываем выходы
			float[] AField2 = LayerSA.AFieldSum(LayerSA.AField, AField);
			LayerAR.RActivation(AField2);
			RField = LayerAR.RField;

			// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
			GetError(argNumber, 1);
		}



		private List<int> top(int[] argErrorLog, int N)
		{
			return Enumerable.Range(0, argErrorLog.Length)
							.OrderByDescending(i => argErrorLog[i])
							.Take(N)
							.ToList();
		}

		public List<int> TopError(int argCount)
		{
			return top(ErrorLog, argCount);
		}


		int Error = 0;
		int Error2 = 0;

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

		public void LoadWeights()
		{
			string weightFileName = WeightFileName();

			if (File.Exists(weightFileName))
			{
				int ret = AB.LoadWeights(weightFileName);
			}
		}

		public void LoadWeights2()
		{
			string weightFileName = WeightFileName();

			if (File.Exists(weightFileName))
			{
				int ret = AB.LoadWeights(weightFileName);
			}

			LayerSA.LoadWeights();
			LayerAR.LoadWeights();
		}


		public void Learned2()
		{
			DateTime beginFull = DateTime.Now;
			string weightFileName = WeightFileName();
			LoadWeights2();

			int[] indexL = new int[HCount];
			int indexR = 0;
			foreach (var rr in NecessaryReactions)
			{
				indexL[indexR] = rr.Key;
				indexR++;
			}

			for (int n = 0; n < 100000; n++)
			{
				DateTime begin = DateTime.Now;
				DateTime beginA;
				DateTime beginR;
				DateTime beginLar;
				DateTime beginLsa;
				DateTime beginRnd;

				double tA = 0;
				double tR = 0;
				double tLar = 0;
				double tLsa = 0;
				double tRnd = 0;


				Error = 0;
				Error2 = 0;

				indexL = Shuffle(indexL);

				// За каждую итерацию прокручиваем все примеры из обучающей выборки
				for (int i = 0; i < HCount; i++)
				{
					int index = indexL[i];

					beginA = DateTime.Now;

					for (int t = 0; t < TSet; t++)
					{
						LayerSA.SActivation(LearnedStimuls[t].Stimuls[index], 0, -1, t);
						SActivation(index, 0, t);
					}

					tA += (DateTime.Now - beginA).TotalMilliseconds;

					// Активируем R-элементы, т.е. рассчитываем выходы
					beginR = DateTime.Now;
					float[] AField2 = LayerSA.AFieldSum(LayerSA.AField, AField);
					LayerAR.RActivation(AField2);
					RField = LayerAR.RField;
					tR += (DateTime.Now - beginR).TotalMilliseconds;

					// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
					GetError(index);
					if (r.E == true)
					{
						beginLar = DateTime.Now;
						LayerAR.LearnedStimulAR(AField2, r.Error);
						tLar += (DateTime.Now - beginLar).TotalMilliseconds;

						beginR = DateTime.Now;
						LayerAR.RActivation(AField2);
						RField = LayerAR.RField;
						tR += (DateTime.Now - beginR).TotalMilliseconds;

						GetError(index);
						if (r.E == true)
						{
							beginRnd = DateTime.Now;
							RandomChange(index);
							tRnd += (DateTime.Now - beginRnd).TotalMilliseconds;

							beginLsa = DateTime.Now;
							LearnedStimulSA(index, r.Error);
							tLsa += (DateTime.Now - beginLsa).TotalMilliseconds;

							/*beginLar = DateTime.Now;
							LayerAR.LearnedStimulAR(AField2, r.Error);
							tLar += (DateTime.Now - beginLar).TotalMilliseconds;*/

							Error2++;
						}
						Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.
					}
				}

				OldError = Error;

				int er = 0; int fer = 0;
				//if (Error < 1000)
				{
					(er, fer) = Examin(ECount, false);
				}

				double time = (DateTime.Now - begin).TotalMilliseconds;
				string output = n.ToString() + ". E1/E2 \t" + Error.ToString() + " / " + Error2.ToString()  
								+ "\t" + time.ToString("F0") + " ms " + tA.ToString("F0") + "/" + tR.ToString("F0")
								+ " " + tLar.ToString("F0") + "-" + tLsa.ToString("F0") + "-" + tRnd.ToString("F0")
								+ "\tE: " + er.ToString() + " / " + fer.ToString();

				Console.WriteLine(output);
				File.AppendAllText("Error_" + Arh + ".txt", output + "\n");

				LayerSA.SaveWeights();
				LayerAR.SaveWeights();
				AB.SaveWeights(weightFileName);

				if (Error == 0) { break; }
			}

			double tFull = (DateTime.Now - beginFull).TotalMilliseconds;

			string outputF = "\tFullTime = " + tFull.ToString() + " ms ";

			Console.WriteLine(outputF);
			File.AppendAllText("Error_" + Arh + ".txt", outputF + "\n");
		}



		/// <summary>
		/// Когда все примеры добавлены, вызывается чтобы перцептрон их выучил
		/// </summary>
		public void Learned()
		{
			DateTime beginFull = DateTime.Now;

			InitAnalyze();

			string weightFileName = WeightFileName();
			//LoadWeights();
			//LoadLog();

			int heCount = HCount - ExceptStimul.Count;
			if (OnlyStimul.Count != 0)
			{ 
				heCount = OnlyStimul.Count;
			}

			int[] indexL = new int[heCount];
			int indexR = 0;
			foreach (var rr in NecessaryReactions)
			{
				if (ExceptStimul.Count != 0)
				{
					if (ExceptStimul.Contains(rr.Key)) { continue; }
				}
				if (OnlyStimul.Count != 0)
				{
					if (!OnlyStimul.Contains(rr.Key)) { continue; }
				}

				indexL[indexR] = rr.Key;

				indexR++;
			}


			//float k1 = 1.0f;
			int minError2 = int.MaxValue;

			// Делаем очень много итераций
			for (int n = 0; n < 100000; n++)
			{
				DateTime begin = DateTime.Now;
				DateTime beginA;
				DateTime beginR;
				DateTime beginLar;
				DateTime beginLsa;
				DateTime beginRnd;

				double tA = 0;
				double tR = 0;
				double tLar = 0;
				double tLsa = 0;
				double tRnd = 0;


				Error = 0;
				Error2 = 0;

				indexL = Shuffle(indexL);

				Get1000(HCount, PCount);
				purity.SelectReaction(PuritySamples);

				// За каждую итерацию прокручиваем все примеры из обучающей выборки
				for (int i = 0; i < heCount; i++)
				{
					int index = indexL[i];

					//if (ExceptStimul.Contains(index)) { continue; }

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
						beginLar = DateTime.Now;
						LearnedStimulAR(index, r.Error);
						tLar += (DateTime.Now - beginLar).TotalMilliseconds;

						beginR = DateTime.Now;
						RActivation(index);
						tR += (DateTime.Now - beginR).TotalMilliseconds;

						GetError(index);
						if (r.E == true)
						{
							beginRnd = DateTime.Now;
							RandomChange(index);
							tRnd += (DateTime.Now - beginRnd).TotalMilliseconds;

							beginLsa = DateTime.Now;
							LearnedStimulSA(index, r.Error);
							tLsa += (DateTime.Now - beginLsa).TotalMilliseconds;

							beginLar = DateTime.Now;
							LearnedStimulAR(index, r.Error);
							tLar += (DateTime.Now - beginLar).TotalMilliseconds;

							Error2++;
						}
						Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.

						ErrorLog[index]++;
					}
				}

				if (Error2 > 0)
				{
					//Analyze();
				}

				//k1 = K1(Error2);
				OldError = Error;

				int er = 0; int fer = 0;
				//if (Error < 1000)
				{
					(er, fer) = Examin(ECount, false);
				}

				double t = (DateTime.Now - begin).TotalMilliseconds;
				string output = n.ToString() + ". E1/E2 \t" + Error.ToString() + " / " + Error2.ToString() 
								+ "\t" + t.ToString("F0") + " ms " + tA.ToString("F0") + "/" + tR.ToString("F0")
								+ " " + tLar.ToString("F0") + "-" + tLsa.ToString("F0") + "-" + tRnd.ToString("F0")
								+ "\tE: " + er.ToString() + " / " + fer.ToString();

				Console.WriteLine(output);
				File.AppendAllText("Error_" + Arh + ".txt", output + "\n");



				AB.SaveWeights(weightFileName);
				SaveLog();


				//if (Error == 0) { break; }
				//if (Error < 44000) { break; }
				break;
			}

			AB.SaveWeights(weightFileName);

			double tFull = (DateTime.Now - beginFull).TotalMilliseconds;

			string outputF = "\tFullTime = " + tFull.ToString() + " ms ";

			Console.WriteLine(outputF);
			File.AppendAllText("Error_" + Arh + ".txt", outputF + "\n");

		}

		private void LoadLog()
		{
			string[] log = File.ReadAllLines("ErrorLog.txt");
			for (int i = 0; i < log.Length; i++)
			{
				string[] l = log[i].Split('\t');
				int index = int.Parse(l[0]);
				int count = int.Parse(l[1]);
				ErrorLog[index] = count;
			}
		}


		private void SaveLog()
		{
			string s = "";
			List<int> log = TopError(HCount);
			for (int i = 0; i < HCount; i++)
			{
				s += log[i].ToString() + "\t" + ErrorLog[log[i]].ToString() + "\n"; 
			}
			File.WriteAllText("ErrorLog.txt", s);
		}


		private void Analyze()
		{
			int regionCount = 0;
			//int regionCount = purity.LinearRegions(Activations);

			/*float[][] gainValue = gain.CalculateGain(Activations, ACount, RCount);
			int activeNeiron = 0;
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
					activeNeiron++;
				}
			}*/

			//purity.NeighborhoodPurity(Activations, PCount / RCount, RCount); // /2
			purity.NeighborhoodPurity(Activations, PCount / 2);

			//string outputA = "\tAN: " + activeNeiron.ToString("F0") + "\tR: " + regionCount.ToString("F0") + "/" + purity.avgPairwise.ToString("F0")
			//	+ "\tP: " + purity.minPurity.ToString("F4") + "-" + purity.avgPurity.ToString("F4") + "-" + purity.maxPurity.ToString("F4");

			string outputA = "\tP: " + purity.minPurity.ToString("F4") + "-" + purity.avgPurity.ToString("F4") + "-" + purity.maxPurity.ToString("F4");

			//string outputA2 = "\t" + activeNeiron.ToString("F0") + "\t" + purity.avgPairwise.ToString("F2")
			//	+ "\t " + purity.minPurity.ToString("F4") + "\t" + purity.avgPurity.ToString("F4") + "\t" + purity.maxPurity.ToString("F4")
			//	+ "\t" + FPurity.Avg.ToString("F4");


			//Console.WriteLine(purity.Distribution.InfoA);
			Console.WriteLine(outputA + "\n" + purity.Distribution.InfoA);
			File.AppendAllText("Purity_" + Arh + ".txt", outputA + "\n");
			//File.AppendAllText("PurityD_" + Arh + ".txt", purity.Distribution.InfoB + "\n");
		}



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


		//float sum;

		/*private float[] Normalize(float[] argAField)
		{
			//sum = 0;
			float maxAbs = 0;
			// Находим максимальное по модулю значение
			for (int i = 0; i < argAField.Length; i++)
			{
				float absValue = Math.Abs(argAField[i]);
				if (absValue > maxAbs) maxAbs = absValue;

				//sum += argAField[i];
			}

			// Если все значения нулевые, возвращаем исходный массив
			if (maxAbs == 0)
				return argAField;

			// Нормализуем значения
			float[] normalized = new float[argAField.Length];

			for (int i = 0; i < argAField.Length; i++)
			{
				normalized[i] = argAField[i] / maxAbs;
			}

			return normalized;
		}*/

		/// <summary>
		/// Активация S-A слоя
		/// </summary>
		/// <param name="argStimulNumber">Номер примера в выборке</param>
		/// <param name="argMode">0 - обучение, 1 - экзамен</param>
		private void SActivation(int argStimulNumber, int argMode = 0, int t = 0)
		{
			// Кинем на сенсоры обучающий пример
			if (argMode == 0)
			{
				SensorsField = LearnedStimuls[t].Stimuls[argStimulNumber];
			}
			else if (argMode == 1)
			{
				SensorsField = ExaminStimuls[t].Stimuls[argStimulNumber];
			}

			AFieldT[t] = AB.AActivation(SensorsField);

			//Activations[argStimulNumber] = AField;

			/*if (PuritySamples != null && PuritySamples.Contains(argStimulNumber))
			{
				int index = PuritySamples_[argStimulNumber];
				Activations[index] = AField;
			}*/

			if (t == 0)
			{
				AField = AFieldT[0];
			}
			else if (t > 0)
			{
				AField = LayerSA.AFieldSum(AField, AFieldT[t]);
			}

			if (t + 1 == TSet)
			{
				AFieldNorm = AB.Normalize(AField);
			}
		}

		private void RActivation(int argStimulNumber)
		{
			RField = AB.RActivation(AField);
		}

		//RandomTest
		//float p3 = 0.0002f;
		//float correct3 = 0.001f;


		float p3 = 0.0000001f;
		float correct3 = 0.00001f;

		//float p3 = 0.01f;
		//float correct3 = 0.001f;

		private void RandomChange(int argStimulNumber)
		{
			float d = p3;
			if (OldError != 0) d = p3 * ((float)OldError / (float)HCount);

			AB.RandomChange(d, correct3, AField);
		}


		private void LearnedStimulSA(int argStimulNumber, float[] rError)
		{
			AB.LearnedStimulSA(rError, AField, AFieldNorm);
		}


		private void LearnedStimulAR(int argStimulNumber, float[] rError)
		{
			AB.LearnedStimulAR(rError, AField);
		}
	}
}

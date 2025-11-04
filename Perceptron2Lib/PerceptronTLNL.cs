// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Collections.Generic;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Tac.Perceptron
{

	/// <summary>
	/// Перцептрон TL&NL - Двухуровневое и нормализированное обучение
	/// Perceptron TL&NL – Perceptron witch Two-Level & Normalization Learning
	/// </summary>
	public class PerceptronTLNL
	{
		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров
		private int ECount; // Количество примеров

		public float[] SensorsField; /* Сенсорное поле */
		//public sbyte[] ReactionsOutput; /* Реагирующие поле */

		public float[] AField;
		public float[] AFieldNorm;

		//public Dictionary<int, BitBlock> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, float[]> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		//public Dictionary<int, BitBlock> ExaminStimuls; // Стимулы для экзамена
		public Dictionary<int, float[]> ExaminStimuls; // Стимулы для экзамена
		public Dictionary<int, BitBlock> ExaminReactions; // Требуемая реакция на каждый стимул во время экзамена

		public int[] ErrorLog;
		public List<int> ExceptStimul = new List<int>();
		public List<int> OnlyStimul = new List<int>();

		private float[] ReactionError;
		private int OldError = 0;

		private Random rnd = new Random(24);
		private PerceptronAA AB;
		//private PerceptronC AC;
		//private PerceptronT AT;
		//private PerceptronL AL;

		//private Conv3x3 conv;

		public Dictionary<int, float[]> Activations = new Dictionary<int, float[]>();
		//private Gain gain;
		private Purity purity;

		private string Name = "";

		private string Arh
		{
			get { return "[" + HCount.ToString() + "]" + SCount.ToString() + "x" + ACount.ToString(); }
		}

		public PerceptronTLNL(int argSCount, int argACount, int argRCount, int argHCount, int argECount, string argName = "")
		{
			ACount = argACount;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;
			ECount = argECount;
			Name = argName;

			SensorsField = new float[SCount];

			//ReactionsOutput = new sbyte[RCount];
			ReactionError = new float[RCount];

			AField = new float[ACount];

			ErrorLog = new int[HCount];

			LearnedStimuls = new Dictionary<int, float[]>();
			NecessaryReactions = new Dictionary<int, BitBlock>();

			ExaminStimuls = new Dictionary<int, float[]>();
			ExaminReactions = new Dictionary<int, BitBlock>();

			AB = new PerceptronAA(SCount, ACount, RCount);
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



		public void JoinStimul(int argStimulNumber, BitBlock argPerception, BitBlock argReaction)
		{
			//TODO конвертировать  BitBlock в float[]
			// Запомним обучающий стимул
			//LearnedStimuls.Add(argStimulNumber, argPerception);

			// Запомним какая реакция должна быть на этот пример
			NecessaryReactions.Add(argStimulNumber, argReaction);
		}

		
		/*public void JoinEStimul(int argStimulNumber, BitBlock argPerception, BitBlock argReaction)
		{
			// Запомним обучающий стимул
			ExaminStimuls.Add(argStimulNumber, argPerception);

			// Запомним какая реакция должна быть на этот пример
			ExaminReactions.Add(argStimulNumber, argReaction);
		}*/


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

				(bool isError, bool isFastError) = ExaminOne(n);

				for (int i = 0; i < RCount; i++)
				{
					ErrorCount[i] += (int)Math.Abs(ReactionError[i]);
				}
				if (isError == true)
				{
					AllErrorCount++;
				}
				if (isFastError == true)
				{
					AllFastErrorCount++;
				}

				if (logErrorType)
				{
					if (isFastError == true)
					{
						error.Add(n, 2);
					}
					else if (isError == true)
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

		public (bool, bool) ExaminOne(int argNumber)
		{
			AActivation(argNumber, 1);

			// Активируем R-элементы, т.е. рассчитываем выходы
			RActivation(argNumber);
			// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
			Reaction r = GetError(argNumber, 1);

			/*
			int[] e = new int[RCount + 1];
			for (int i = 1; i < RCount + 1; i++)
			{
				e[i] += ReactionError[i];
			}*/

			return (r.IsErrorHard, r.IsErrorSoft);
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
				AB.LoadWeights(weightFileName);
			}
		}

		/// <summary>
		/// Когда все примеры добавлены, вызывается чтобы перцептрон их выучил
		/// </summary>
		public void Learned()
		{
			DateTime beginFull = DateTime.Now;

			InitAnalyze();

			string weightFileName = WeightFileName();
			LoadWeights();


			int heCount = HCount - ExceptStimul.Count;
			if (OnlyStimul.Count != 0)
			{ 
				heCount = OnlyStimul.Count;
			}

			int[] indexL = new int[heCount];
			int indexR = 0;
			foreach (var rr in NecessaryReactions)
			{
				if (ExaminStimuls.Count != 0)
				{
					if (ExceptStimul.Contains(rr.Key)) { continue; }
				}
				if (OnlyStimul.Count != 0)
				{
					if (!OnlyStimul.Contains(rr.Key)) { continue; }
				}

				indexL[indexR] = rr.Key;

				rr.Value.To();
				indexR++;
			}
			for (int i = 0; i < ExaminStimuls.Count; i++)
			{
				ExaminReactions[i].To();
			}

			float k1 = 1.0f;
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
					AActivation(index);
					tA += (DateTime.Now - beginA).TotalMilliseconds;

					// Активируем R-элементы, т.е. рассчитываем выходы
					beginR = DateTime.Now;
					RActivation(index);
					tR += (DateTime.Now - beginR).TotalMilliseconds;

					// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
					Reaction r = GetError(index);
					if (r.E == true)
					{
						beginLar = DateTime.Now;
						LearnedStimulAR(index);
						tLar += (DateTime.Now - beginLar).TotalMilliseconds;

						beginR = DateTime.Now;
						RActivation(index);
						tR += (DateTime.Now - beginR).TotalMilliseconds;

						Reaction r2 = GetError(index);
						if (r2.E == true)
						{
							beginRnd = DateTime.Now;
							RandomChange(index, k1);
							tRnd += (DateTime.Now - beginRnd).TotalMilliseconds;

							beginLsa = DateTime.Now;
							LearnedStimulSA(index);
							tLsa += (DateTime.Now - beginLsa).TotalMilliseconds;

							beginLar = DateTime.Now;
							LearnedStimulAR(index);
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
				if (Error < 1000)
				{
					(er, fer) = Examin(ECount, false);
				}

				double t = (DateTime.Now - begin).TotalMilliseconds;
				string output = n.ToString() + ". E1/E2 \t" + Error.ToString() + " / " + Error2.ToString() + " (" + k1.ToString("F4") + ")"
								+ "\t" + t.ToString("F0") + " ms " + tA.ToString("F0") + "/" + tR.ToString("F0")
								+ " " + tLar.ToString("F0") + "-" + tLsa.ToString("F0") + "-" + tRnd.ToString("F0")
								+ "\tE: " + er.ToString() + " / " + fer.ToString();

				Console.WriteLine(output);
				File.AppendAllText("Error_" + Arh + ".txt", output + "\n");



				if (n % 10 == 0 && n > 0)
				//if (Error2 < minError2)
				{
					AB.SaveWeights(weightFileName);
					//minError2 = Error2;
				}

				//Console.WriteLine("\tminError2: " + minError2.ToString());


				if (Error == 0) { break; }
				//if (Error < 13000) { break; }
			}

			AB.SaveWeights(weightFileName);

			double tFull = (DateTime.Now - beginFull).TotalMilliseconds;

			string outputF = "\tFullTime = " + tFull.ToString() + " ms ";

			Console.WriteLine(outputF);
			File.AppendAllText("Error_" + Arh + ".txt", outputF + "\n");

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


		float sum;

		private float[] Normalize(float[] argAField)
		{
			sum = 0;
			float maxAbs = 0;
			// Находим максимальное по модулю значение
			for (int i = 0; i < argAField.Length; i++)
			{
				float absValue = Math.Abs(argAField[i]);
				if (absValue > maxAbs) maxAbs = absValue;

				sum += argAField[i];
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
		}

		/// <summary>
		/// Активация S-A слоя
		/// </summary>
		/// <param name="argStimulNumber">Номер примера в выборке</param>
		/// <param name="argMode">0 - обучение, 1 - экзамен</param>
		private void AActivation(int argStimulNumber, int argMode = 0)
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

			/*AField = new float[ACount];
			for (int j = 0; j < ACount; j++)
			{
				for (int i = 0; i < SCount; i++)
				{
					//if (SensorsField[i] == true)
					{
						AField[j] += WeightSA[i][j] * SensorsField.DataByte[i];
					}
				}
			}*/

			AField = AB.AActivation(SensorsField);

			//Activations[argStimulNumber] = AField;

			if (PuritySamples != null && PuritySamples.Contains(argStimulNumber))
			{
				int index = PuritySamples_[argStimulNumber];
				Activations[index] = AField;
			}


			AFieldNorm = Normalize(AField);

		}

		float[] RField;
		private void RActivation(int argStimulNumber)
		{
			/*float[] RField = new float[RCount];
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < ACount; i++)
				{
					if (AField[i] > 0)
					{
						RField[j] += AL.AR_(i, j);
					}
				}
			}*/

			RField = AB.RActivation(AField);

			/*for (int i = 0; i < RCount; i++)
			{
				if (RField[i] > 0) { ReactionsOutput[i] = 1; }
				else if (RField[i] < 0) { ReactionsOutput[i] = -1; }
				else { ReactionsOutput[i] = 0; }
			}*/
		}

		/*private bool GetFastError(int argStimulNumber, int argMode = 0)
		{
			bool IsError = false;
			int index = ArgMax(RField);

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

				if (v == 1)
				{
					if (i != index)
					{ 
						IsError = true;
					}
					break;
				}
			}
			return IsError;
		}*/

		/*public bool IsSure(sbyte[] array)
		{
			int count = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] > 0)
				{
					count++;
				}
			}
			return count == 1;
		}

		public int ArgMax(sbyte[] array)
		{ 
			float[] a = new float[array.Length];
			for (int i = 0; i < array.Length; i++)
			{ 
				a[i] = array[i];
			}
			return ArgMax(a);
		}*/

		public int ArgMax(float[] array)
		{

			int maxIndex = 0;
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i] > array[maxIndex])
					maxIndex = i;
			}
			return maxIndex;
		}

		private Reaction GetError(int argStimulNumber, int argMode = 0)
		{
			Reaction r = new Reaction(RField, RCount);

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

				int output = (RField[i] > 0) ? 1 : -1;
				if (output != v)
				{
					r.IsErrorHard = true;
					r.Error[i] = v;
				}

				if (v == 1 && i != r.RMax)
				{
					r.IsErrorSoft = true;
				}
			}
			return r;
		}



		float p3 = 0.0000001f;
		float correct3 = 0.00001f;

		//float p3 = 0.01f;
		//float correct3 = 0.001f;

		/*
		public float K1(float k2)
		{
			float maxK1 = 1.0f;
			float minK1 = 0.001f;
			float threshold = 150f;

			if (k2 >= threshold)
				return maxK1;

			float normalized = k2 / threshold;
			float factor = (float)Math.Pow(normalized, 3);
			return minK1 + (maxK1 - minK1) * factor;
		}*/

		private void RandomChange(int argStimulNumber, float argK1)
		{
			float d = p3;
			if (OldError != 0) d = p3 * ((float)OldError / (float)HCount);


			//float p = (float)rnd.NextDouble();
			//if (p < argK1)
			{
				AB.RandomChange(d, correct3, AField);

				/*for (int r = 0; r < RCount; r++)
				{
					for (int j = 0; j < ACount; j++)
					{
						if (AField[j] <= th)
						{
							for (int i = 0; i < SCount; i++)
							{
								float p0 = (float)rnd.NextDouble();
								if (p0 < d)
								{
									//WeightSA[i][j] += correct3;
									AL.SA(i, j, correct3);
								}
							}
						}
					}
				}*/
			}
		}


		private void LearnedStimulSA(int argStimulNumber)
		{
			AB.LearnedStimulSA(ReactionError, AField, AFieldNorm);

			/*for (int j = 0; j < ACount; j++)
			{
				float[] w = new float[SCount];

				if (AField[j] > th)
				{
					for (int r = 0; r < RCount; r++)
					{
						if (ReactionError[r] != 0 && Math.Sign(AL.AR_(j, r)) != Math.Sign(ReactionError[r]))
						{
							for (int i = 0; i < SCount; i++)
							{
								w[i] -= AFieldNorm[j];
							}
						}
					}
				}
				else
				{
					for (int r = 0; r < RCount; r++)
					{
						if (Math.Sign(AL.AR_(j, r)) == Math.Sign(ReactionError[r]))
						{
							for (int i = 0; i < SCount; i++)
							{
								w[i] += AFieldNorm[j];
							}
						}
					}
				}

				for (int i = 0; i < SCount; i++)
				{
					//WeightSA[i][j] += w[i];
					AL.SA(i, j, w[i]);
				}
			}*/
		}


		private void LearnedStimulAR(int argStimulNumber)
		{
			AB.LearnedStimulAR(ReactionError, AField);

			/*for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < ACount; i++)
				{
					if (AField[i] > 0)
					{
						//WeightAR[i][j] = WeightAR[i][j] + ReactionError[j];
						AL.AR(i, j, ReactionError[j]);
					}
				}
			}*/
		}
	}
}

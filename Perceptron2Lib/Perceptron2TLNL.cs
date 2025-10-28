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
	public class Perceptron2TLNL
	{
		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int A2Count; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров
		private int ECount; // Количество примеров

		public float[] SensorsField; /* Сенсорное поле */
		public sbyte[] ReactionsOutput; /* Реагирующие поле */

		public float[] AField;
		public float[] AFieldNorm;

		public float[] A2Field;
		public float[] A2FieldNorm;

		public Dictionary<int, float[]> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Dictionary<int, float[]> ExaminStimuls; // Стимулы для экзамена
		public Dictionary<int, BitBlock> ExaminReactions; // Требуемая реакция на каждый стимул во время экзамена

		public int[] ErrorLog;

		private float[] ReactionError;
		private int OldError = 0;

		private Random rnd = new Random(24);
		private PerceptronAA AB;

		public Dictionary<int, float[]> Activations = new Dictionary<int, float[]>();
		//private Gain gain;
		private Purity purity;

		private string Arh
		{
			get { return "[" + HCount.ToString() + "]" + SCount.ToString() + "x" + ACount.ToString() + "x" + A2Count.ToString(); }
		}


		public Perceptron2TLNL(int argSCount, int argACount, int argA2Count, int argRCount, int argHCount, int argECount)
		{
			ACount = argACount;
			A2Count = argA2Count;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;
			ECount = argECount;

			SensorsField = new float[SCount];

			ReactionsOutput = new sbyte[RCount];
			ReactionError = new float[RCount];

			AField = new float[ACount];
			A2Field = new float[A2Count];

			ErrorLog = new int[HCount];

			LearnedStimuls = new Dictionary<int, float[]>();
			NecessaryReactions = new Dictionary<int, BitBlock>();

			ExaminStimuls = new Dictionary<int, float[]>();
			ExaminReactions = new Dictionary<int, BitBlock>();

			AB = new PerceptronAA(SCount, ACount, RCount, A2Count);
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
			bool isError = GetError(argNumber, 1);

			bool isFastError = GetFastError(argNumber, 1);

			/*
			int[] e = new int[RCount + 1];
			for (int i = 1; i < RCount + 1; i++)
			{
				e[i] += ReactionError[i];
			}*/

			return (isError, isFastError);
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

		/// <summary>
		/// Когда все примеры добавлены, вызывается чтобы перцептрон их выучил
		/// </summary>
		public void Learned()
		{
			DateTime beginFull = DateTime.Now;

			InitAnalyze();

			string weightFileName = "weight" + Arh + ".bin";
			if (File.Exists(weightFileName))
			{
				//AB.LoadWeights(weightFileName);
			}

			int[] indexL = new int[HCount];
			for (int i = 0; i < HCount; i++)
			{
				indexL[i] = i;

				//LearnedStimuls[i].To();
				NecessaryReactions[i].To();
			}
			for (int i = 0; i < ExaminStimuls.Count; i++)
			{
				//ExaminStimuls[i].To();
				ExaminReactions[i].To();
			}

			float k1 = 0.05f;

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
				for (int i = 0; i < HCount; i++)
				{
					int index = indexL[i];

					beginA = DateTime.Now;
					AActivation(index);
					tA += (DateTime.Now - beginA).TotalMilliseconds;

					// Активируем R-элементы, т.е. рассчитываем выходы
					beginR = DateTime.Now;
					RActivation(index);
					tR += (DateTime.Now - beginR).TotalMilliseconds;

					// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
					bool e = GetError(index);
					if (e == true)
					{
						beginLar = DateTime.Now;
						LearnedStimulAR(index);
						tLar += (DateTime.Now - beginLar).TotalMilliseconds;

						beginR = DateTime.Now;
						RActivation(index);
						tR += (DateTime.Now - beginR).TotalMilliseconds;

						bool e2 = GetError(index);

						if (e2 == true)
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

						ErrorLog[i]++;
					}
				}

				if (Error2 > 0)
				{
					Analyze();
				}

				k1 = K1(Error2);
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
				{
					//AB.SaveWeights("weight" + SCount.ToString() + "x" + ACount.ToString() + ".bin");
				}


				if (Error == 0) { break; }
			}

			//AB.SaveWeights("weight" + SCount.ToString() + "x" + ACount.ToString() + ".bin");

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

			AField = AB.AActivation(SensorsField);

			if (PuritySamples.Contains(argStimulNumber))
			{
				int index = PuritySamples_[argStimulNumber];
				Activations[index] = AField;
			}

			AFieldNorm = Normalize(AField);

			A2Activation(argStimulNumber);
		}

		private void A2Activation(int argStimulNumber, int argMode = 0)
		{
			A2Field = AB.A2Activation(AField);

			/*if (PuritySamples.Contains(argStimulNumber))
			{
				int index = PuritySamples_[argStimulNumber];
				Activations[index] = AField;
			}*/

			A2FieldNorm = Normalize(A2Field);
		}



		float[] RField;
		private void RActivation(int argStimulNumber)
		{
			RField = AB.R2Activation(A2Field);

			for (int i = 0; i < RCount; i++)
			{
				if (RField[i] > 0) { ReactionsOutput[i] = 1; }
				else if (RField[i] <= 0) { ReactionsOutput[i] = -1; }
			}
		}

		private bool GetFastError(int argStimulNumber, int argMode = 0)
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
		}

		public int ArgMax(float[] array)
		{
			if (array == null || array.Length == 0)
				throw new ArgumentException("Array cannot be null or empty");

			int maxIndex = 0;
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i] > array[maxIndex])
					maxIndex = i;
			}
			return maxIndex;
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


		//float p3 = 0.000003f;		// MNIST
		//float correct3 = 0.0001f;	// MNIST

		float p3 = 0.0000001f;
		float correct3 = 0.00001f;

		public float K1(float k2)
		{
			float maxK1 = 0.05f;
			float minK1 = 0.001f;
			float threshold = 1500f;

			if (k2 >= threshold)
				return maxK1;

			float normalized = k2 / threshold;
			float factor = (float)Math.Pow(normalized, 3);
			return minK1 + (maxK1 - minK1) * factor;
		}

		private void RandomChange(int argStimulNumber, float argK1)
		{
			float d = p3;
			if (OldError != 0) d = p3 * ((float)OldError / (float)HCount);


			//float p = (float)rnd.NextDouble();
			//if (p < argK1)
			{
				AB.Random2Change(d, correct3, AField, A2Field);
			}
		}

		private void LearnedStimulSA(int argStimulNumber)
		{
			float[] ReactionErrorAA = new float[A2Count];

			AB.LearnedStimul2AA(ReactionError, A2Field, A2FieldNorm, ReactionErrorAA);

			for (int i = 0; i < ReactionErrorAA.Length; i++)
			{
				ReactionErrorAA[i] = Math.Sign(ReactionErrorAA[i]);
			}

			//AB.LearnedStimul2SA(ReactionErrorAA, AField, AFieldNorm);
		}

		private void LearnedStimulAR(int argStimulNumber)
		{
			AB.LearnedStimulAR(ReactionError, A2Field);
		}
	}
}

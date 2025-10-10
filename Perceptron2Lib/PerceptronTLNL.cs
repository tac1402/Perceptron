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
	public class PerceptronTLNL
	{
		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров

		public BitBlock SensorsField; /* Сенсорное поле */
		public sbyte[] ReactionsOutput; /* Реагирующие поле */

		public float[] AField;
		public float[] AFieldNorm;

		private float[] AThreshold;


		public Dictionary<int, BitBlock> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Dictionary<int, BitBlock> ExaminStimuls; // Стимулы для экзамена
		public Dictionary<int, BitBlock> ExaminReactions; // Требуемая реакция на каждый стимул во время экзамена

		private float[] ReactionError;
		private int OldError = 0;

		private Random rnd = new Random(24);
		private PerceptronAA AB;

		//private FastPurity FPurity;

		public Dictionary<int, float[]> Activations = new Dictionary<int, float[]>();
		//private Gain gain;
		private Purity purity;

		public PerceptronTLNL(int argSCount, int argACount, int argRCount, int argHCount)
		{
			ACount = argACount;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;

			SensorsField = new BitBlock(SCount);

			ReactionsOutput = new sbyte[RCount];
			ReactionError = new float[RCount];

			AField = new float[ACount];
			AThreshold = new float[ACount];

			LearnedStimuls = new Dictionary<int, BitBlock>();
			NecessaryReactions = new Dictionary<int, BitBlock>();

			ExaminStimuls = new Dictionary<int, BitBlock>();
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


		float cc = 0.000000001f;
		private void ChangeRThreshold()
		{
			/*for (int i = 0; i < ACount; i++)
			{
				if (AField[i] > AThreshold[i])
				{
					AThreshold[i] += cc;
				}
				else if (AField[i] < AThreshold[i])
				{
					AThreshold[i] -= cc;
				}
			}*/
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

			File.AppendAllText("Result_" + SCount.ToString() + "x" + ACount.ToString() + ".txt",
					"p3 = " + p3.ToString("F16").TrimEnd('0') + "; c3 = " + correct3.ToString("F16").TrimEnd('0') + "\n");

			for (int i = 0; i < RCount; i++)
			{
				Console.WriteLine("Error = " + i.ToString() + " - " + ErrorCount[i].ToString());
				File.AppendAllText("Result_" + SCount.ToString() + "x" + ACount.ToString()+ ".txt", 
						"Error = " + i.ToString() + " - " + ErrorCount[i].ToString() + "\n");
			}
			Console.WriteLine("Error = " + AllErrorCount.ToString());
			File.AppendAllText("Result_" + SCount.ToString() + "x" + ACount.ToString() + ".txt", "Error=" + AllErrorCount.ToString() + "\n");
		}

		public bool ExaminOne(int argNumber)
		{
			AActivation(argNumber, 1);

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
		/// Когда все примеры добавлены, вызывается чтобы перцептрон их выучил
		/// </summary>
		public void Learned()
		{
			DateTime beginFull = DateTime.Now;

			InitAnalyze();
			//FPurity = new FastPurity(SCount, ACount, RCount, NecessaryReactions);

			int[] indexL = new int[HCount];
			for (int i = 0; i < HCount; i++)
			{
				indexL[i] = i;

				LearnedStimuls[i].To();
				NecessaryReactions[i].To();
			}
			for (int i = 0; i < ExaminStimuls.Count; i++)
			{
				ExaminStimuls[i].To();
				ExaminReactions[i].To();
			}

			int Error = 0;
			int Error2 = 0;

			// Делаем очень много итераций
			for (int n = 0; n < 100000; n++)
			{
				DateTime begin = DateTime.Now;
				DateTime beginA;
				DateTime beginR;
				double tA = 0;
				double tR = 0;

				Error = 0;
				Error2 = 0;

				indexL = Shuffle(indexL);

				Get1000(HCount, PCount);
				purity.SelectReaction(PuritySamples);

				//FPurity.Clear();

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
						LearnedStimulAR(index);

						RActivation(index);
						bool e2 = GetError(index);

						if (e2 == true)
						{
							RandomChange(index);
							LearnedStimulSA(index);
							LearnedStimulAR(index);

							//AA.SetSAWeights(WeightSA);

							Error2++;
						}
						Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.
					}
					else
					{
						ChangeRThreshold();
					}

				}

				//FPurity.Calc();

				OldError = Error;

				double t = (DateTime.Now - begin).TotalMilliseconds;
				string output = n.ToString() + ". E1/E2 \t" + Error.ToString() + " / " + Error2.ToString()
								+ "\t" + t.ToString() + " ms " + tA.ToString("F0") + "/" + tR.ToString("F0");

				//output += "\tP: " + FPurity.Min.ToString("F4") + "-" + FPurity.Avg.ToString("F4") + "-" + FPurity.Max.ToString("F4");

				Console.WriteLine(output);
				File.AppendAllText("Error_" + SCount.ToString() + "x" + ACount.ToString() + ".txt", output + "\n");

				if (Error2 > 0)
				{
					Analyze();
				}

				if (Error == 0) { break; }
			}

			double tFull = (DateTime.Now - beginFull).TotalMilliseconds;

			string outputF = "\tFullTime = " + tFull.ToString() + " ms ";

			Console.WriteLine(outputF);
			File.AppendAllText("Error_" + SCount.ToString() + "x" + ACount.ToString() + ".txt", outputF + "\n");

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

			purity.NeighborhoodPurity(Activations, PCount / RCount, RCount); // /2

			//string outputA = "\tAN: " + activeNeiron.ToString("F0") + "\tR: " + regionCount.ToString("F0") + "/" + purity.avgPairwise.ToString("F0")
			//	+ "\tP: " + purity.minPurity.ToString("F4") + "-" + purity.avgPurity.ToString("F4") + "-" + purity.maxPurity.ToString("F4");

			//string outputA2 = "\t" + activeNeiron.ToString("F0") + "\t" + purity.avgPairwise.ToString("F2")
			//	+ "\t " + purity.minPurity.ToString("F4") + "\t" + purity.avgPurity.ToString("F4") + "\t" + purity.maxPurity.ToString("F4")
			//	+ "\t" + FPurity.Avg.ToString("F4");


			Console.WriteLine(purity.Distribution.InfoA);
			//Console.WriteLine(outputA + "\n" + purity.Distribution.InfoA);
			//File.AppendAllText("Purity_" + SCount.ToString() + "x" + ACount.ToString() + ".txt", outputA2 + "\n");
			File.AppendAllText("PurityD_" + SCount.ToString() + "x" + ACount.ToString() + ".txt", purity.Distribution.InfoB + "\n");

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

		private float[] Normalize(float[] AField)
		{
			sum = 0;
			float maxAbs = 0;
			// Находим максимальное по модулю значение
			for (int i = 0; i < AField.Length; i++)
			{
				float absValue = Math.Abs(AField[i]);
				if (absValue > maxAbs) maxAbs = absValue;

				sum += AField[i];
			}

			// Если все значения нулевые, возвращаем исходный массив
			if (maxAbs == 0)
				return AField;

			// Нормализуем значения
			float[] normalized = new float[AField.Length];

			for (int i = 0; i < AField.Length; i++)
			{
				normalized[i] = AField[i] / maxAbs;
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

			AField = AB.AActivation(SensorsField.DataF);

			//FPurity.Add(argStimulNumber, AField, AThreshold);

			//Activations[argStimulNumber] = AField;

			if (PuritySamples.Contains(argStimulNumber))
			{
				int index = PuritySamples_[argStimulNumber];
				Activations[index] = AField;
			}


			AFieldNorm = Normalize(AField);
		}

		private void RActivation(int argStimulNumber)
		{
			/*float[] RField = new float[RCount];
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < ACount; i++)
				{
					if (AField[i] > AThreshold[i])
					{
						RField[j] += AB.AR_(i, j);
					}
				}
			}*/

			float[] RField = AB.RActivation(AField, 0.0f);

			for (int i = 0; i < RCount; i++)
			{
				if (RField[i] > th) { ReactionsOutput[i] = 1; }
				else if (RField[i] < th * -1) { ReactionsOutput[i] = -1; }
				else { ReactionsOutput[i] = 0; }
				
				//if (RField[i] <= -0.01f) { ReactionsOutput[i] = false; }
			}
		}

		float th = 0.000001f;

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


		//float p3 = 0.000001f;		// MNIST
		//float correct3 = 0.001f;	// MNIST

		float p3 = 0.000003f;
		float correct3 = 0.000002f;

		// 57 it
		//float p3 = 0.0002f;
		//float correct3 = 0.001f;

		private void RandomChange(int argStimulNumber)
		{
			float d = p3;
			if (OldError != 0) d = p3 * ((float)OldError / (float)HCount);

			for (int r = 0; r < RCount; r++)
			{
				for (int j = 0; j < ACount; j++)
				{
					if (AField[j] <= th)
					{
						for (int i = 0; i < SCount; i++)
						{
							float p = (float)rnd.NextDouble();
							if (p < d)
							{
								//WeightSA[i][j] += correct3;
								AB.SA(i, j, correct3);
							}
						}
					}
				}
			}
		}


		private void LearnedStimulSA(int argStimulNumber)
		{
			for (int j = 0; j < ACount; j++)
			{
				float[] w = new float[SCount];

				if (AField[j] > th)
				{
					for (int r = 0; r < RCount; r++)
					{
						if (ReactionError[r] != 0 && Math.Sign(AB.AR_(j, r)) != Math.Sign(ReactionError[r]))
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
						if (Math.Sign(AB.AR_(j, r)) == Math.Sign(ReactionError[r]))
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
					AB.SA(i, j, w[i]);
				}
			}
		}


		private void LearnedStimulAR(int argStimulNumber)
		{
			AB.LearnedStimulAR(ReactionError, AField);

			/*for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < ACount; i++)
				{
					if (AField[i] > AThreshold[i])
					{
						//WeightAR[i][j] = WeightAR[i][j] + ReactionError[j];
						AB.AR(i, j, ReactionError[j]);
					}
				}
			}*/
		}
	}
}

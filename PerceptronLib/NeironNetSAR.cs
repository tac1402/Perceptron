// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


using System;
using System.Collections.Generic;

namespace Tac.Perceptron
{

	/// <summary>
	/// Версия перцептрона Розенблатта, с обучаемым SA слоем
	/// </summary>
	public class NeironNetSAR
	{
		public BitBlock SensorsField; /* Сенсорное поле */
		//public int[] AssociationsField; /* Ассоциативное поле */
		public BitBlock ReactionsField; /* Реагирующие поле */

		public float[] AField;

		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров, запоминается реакция A-элементов на каждый пример из обучающей выборки


		//public Dictionary<int, List<int>> AHConnections; // Как реагируют A-элементы на каждый стимул из обучающей выборки

		public Dictionary<int, BitBlock> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Dictionary<int, float[]> WeightSA; // Веса между S-A элементами
		public Dictionary<int, float[]> WeightAR; // Веса между A-R элементами

		private sbyte[] ReactionError;
		private Random rnd = new Random(10);

		public NeironNetSAR(int argSCount, int argACount, int argRCount, int argHCount)
		{
			ACount = argACount;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;

			SensorsField = new BitBlock(SCount);

			/*WeightSA = new Dictionary<int, int[]>(SCount);
			for (int i = 0; i < SCount; i++)
			{
				WeightSA[i] = new int[A1Count];
			}*/

			/*AHConnections = new Dictionary<int, List<int>>();
			for (int i = 0; i < HCount; i++)
			{
				AHConnections[i] = new List<int>();
			}*/

			//AssociationsField = new int[A1Count];
			/*for (int i = 0; i < A1Count; i++)
			{
				InitSA(i);
			}*/

			ReactionsField = new BitBlock(RCount);
			ReactionError = new sbyte[RCount];

			AField = new float[ACount];


			LearnedStimuls = new Dictionary<int, BitBlock>();
			NecessaryReactions = new Dictionary<int, BitBlock>();

			WeightSA = new Dictionary<int, float[]>(SCount);
			for (int i = 0; i < SCount; i++)
			{
				WeightSA[i] = new float[ACount];
			}

			WeightAR = new Dictionary<int, float[]>(ACount);
			for (int i = 0; i < ACount; i++)
			{
				WeightAR[i] = new float[RCount];
			}
		}

		/*private void InitSA(int argAId)
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
		}*/


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
				// За каждую итерацию прокручиваем все примеры из обучающей выборки
				for (int i = 0; i < HCount; i++)
				{
					// Активируем S-элементы, т.е. подаем входы и рассчитываем средний слой A-элементы
					//if (n == 0) { SActivation(i); }

					AActivation(i);
					// Активируем R-элементы, т.е. рассчитываем выходы
					RActivation(i);
					// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
					bool e = GetError(i);
					if (e == true)
					{
						float p = (float)rnd.NextDouble();
						if (p > 0.7f)
						{
							LearnedStimulSA(i);
						}

						LearnedStimulAR(i);
						Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.
					}
				}


				double t = (DateTime.Now - begin).TotalMilliseconds;
				Console.WriteLine(n.ToString() + " - " + Error.ToString() + " - " + t.ToString() + " ms");
				if (Error == 0) { break; }
			}
		}

		double aTime = 0;

		/*private void SActivation(int argStimulNumber)
		{

			for (int i = 0; i < A1Count; i++)
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
					for (int j = 0; j < A1Count; j++)
					{
						AssociationsField[j] += WeightSA[i][j];
					}
				}
			}
			double t = (DateTime.Now - begin).TotalMilliseconds;
			aTime += t;

			// Запомним как на этот пример реагировали A - элементы
			for (int j = 0; j < A1Count; j++)
			{
				if (AssociationsField[j] > 0)
				{
					AHConnections[argStimulNumber].Add(j);
				}
			}
		}*/


		private void AActivation(int argStimulNumber)
		{
			// Кинем на сенсоры обучающий пример
			SensorsField = LearnedStimuls[argStimulNumber];

			AField = new float[ACount];
			//float[] Summa = new float[ACount];
			for (int j = 0; j < ACount; j++)
			{
				for (int i = 0; i < SCount; i++)
				{
					if (SensorsField[i] == true)
					{
						AField[j] += WeightSA[i][j];
					}
				}
			}
			/*for (int i = 0; i < ACount; i++)
			{
				if (Summa[i] > 0) { AField[i] = true; }
				if (Summa[i] <= 0) { AField[i] = false; }
			}*/

			int a = 1;
		}

		private void RActivation(int argStimulNumber)
		{
			float[] Summa = new float[RCount];
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < ACount; i++)
				{
					if (AField[i] > 0)
					{
						Summa[j] += WeightAR[i][j];
					}
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


		float p1 = 0.6f;
		float p2 = 0.4f;
		float p3 = 0.01f;
		float correct = 0.001f;


		private void LearnedStimulSA(int argStimulNumber)
		{
			for (int j = 0; j < ACount; j++)
			{
				float pp = (float)rnd.NextDouble();
				if (pp > 0.95f)
				{
					if (AField[j] > 0)
					{
						if (Math.Sign(WeightAR[j][0]) != Math.Sign(ReactionError[0]))
						{
							for (int i = 0; i < SCount; i++)
							{
								if (SensorsField[i] == true)
								{
									float p = (float)rnd.NextDouble();
									if (p < p1)
									{
										WeightSA[i][j] -= correct * AField[j];
									}
								}
							}
						}
					}
					else
					{
						if (Math.Sign(WeightAR[j][0]) == Math.Sign(ReactionError[0]))
						{
							for (int i = 0; i < SCount; i++)
							{
								if (SensorsField[i] == true)
								{
									float p = (float)rnd.NextDouble();
									if (p < p2)
									{
										WeightSA[i][j] += correct * AField[j];
									}
								}
							}
						}
						if (Math.Sign(WeightAR[j][0]) != Math.Sign(ReactionError[0]))
						{
							for (int i = 0; i < SCount; i++)
							{
								if (SensorsField[i] == true)
								{
									float p = (float)rnd.NextDouble();
									if (p < p3)
									{
										WeightSA[i][j] += correct;
									}
								}
							}
						}
					}
				}
			}
		}


		private void LearnedStimulAR(int argStimulNumber)
		{
			for (int j = 0; j < RCount; j++)
			{
				for (int i = 0; i < ACount; i++)
				{
					if (AField[i] > 0)
					{
						WeightAR[i][j] = WeightAR[i][j] + ReactionError[j] * 1.0f;
					}
				}
			}
		}
	}
}

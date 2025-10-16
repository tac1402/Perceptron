// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


namespace Tac.Perceptron
{
	public class FastPurity
	{
		public float Avg = 0;
		public float Min = 0;
		public float Max = 0;
		public int Empty = 0;

		private int SCount;
		private int ACount;
		private int RCount;
		private Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		private int[] RATotal;
		private float[] RAPurity;
		private Dictionary<int, int[]> RASpec0;
		private Dictionary<int, int[]> RASpec1;



		public FastPurity(int argSCount, int argACount, int argRCount, Dictionary<int, BitBlock> argNecessaryReactions)
		{
			SCount = argSCount;
			ACount = argACount;
			RCount = argRCount;
			NecessaryReactions = argNecessaryReactions;

			RATotal = new int[ACount];
			RAPurity = new float[ACount];

			RASpec0 = new Dictionary<int, int[]>();
			RASpec1 = new Dictionary<int, int[]>();

			for (int i = 0; i < RCount; i++)
			{
				RASpec0.Add(i, new int[ACount]);
				RASpec1.Add(i, new int[ACount]);
			}
		}

		public void Clear()
		{
			for (int i = 0; i < ACount; i++)
			{
				RATotal[i] = 0;
				RAPurity[i] = 0;
				for (int j = 0; j < RCount; j++)
				{
					RASpec1[j][i] = 0;
					RASpec0[j][i] = 0;
				}
			}
		}

		public void Add(int argStimulNumber, float[] AField, float[] AThreshold)
		{
			for (int i = 0; i < ACount; i++)
			{
				if (AField[i] > AThreshold[i])
				{
					RATotal[i]++;
					for (int j = 0; j < RCount; j++)
					{
						if (NecessaryReactions[argStimulNumber][j] == true)
						{
							RASpec1[j][i]++;
						}
						if (NecessaryReactions[argStimulNumber][j] == false)
						{
							RASpec0[j][i]++;
						}
					}
				}
			}
		}

		public void Calc()
		{
			Empty = 0;
			int[] REmpty = new int[ACount];
			for (int i = 0; i < ACount; i++)
			{
				if (RATotal[i] != 0)
				{
					for (int j = 0; j < RCount; j++)
					{
						if (RASpec0[j][i] != 0)
						{
							RAPurity[i] += (float)RASpec1[j][i] / (float)RASpec0[j][i];
						}
						else
						{
							REmpty[i]++;
						}
					}
				}
				else
				{
					RAPurity[i] = 0;
					Empty++;
				}
			}

			// Усредняем по всем R
			for (int i = 0; i < ACount; i++)
			{
				RAPurity[i] = RAPurity[i] / ((float)RCount - REmpty[i]);
			}

			IEnumerable<float> purity = RAPurity.Where(x => x != 0);
			Avg = 0;
			Min = 0;
			Max = 0;
			if (purity.Count() > 0)
			{
				Avg = purity.Average();
				Min = purity.Min();
				Max = purity.Max();
			}
		}

	}
}

// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Perceptron
{
	public class Reaction
	{
		public bool IsErrorHard = false;
		public bool IsErrorSoft = false;
		public float[] Error;

		public int RMax;

		public bool E { get { return IsErrorHard; } }

		public Reaction(int RCount)
		{
			Error = new float[RCount];
		}

		public void Clear()
		{
			Error = new float[Error.Length];
			IsErrorHard = false;
			IsErrorSoft = false;
		}

		public void CalcRMax(float[] RField)
		{
			RMax = ArgMax(RField);
		}

		private int ArgMax(float[] array)
		{
			int maxIndex = 0;
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i] > array[maxIndex])
				{
					maxIndex = i;
				}
			}
			return maxIndex;
		}
	}
}

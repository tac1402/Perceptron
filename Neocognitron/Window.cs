// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Neocognitron
{
	public class Window
	{
		private Dictionary<string, float> Full = new Dictionary<string, float>();
		private int size;

		private string key(int x, int y) { return x.ToString() + "-" + y.ToString(); }

		public Window(int argSize) 
		{
			size = argSize;
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					Full.Add(key(x, y), 0);
				}
			}
		}

		public void Set(float[][] argMatrix)
		{
			int size = argMatrix.Length;
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					Full[key(x, y)] = argMatrix[x][y];
				}
			}
		}

		public void Set(int x, int y, float value)
		{
			Full[key(x, y)] = value;
		}

	}
}

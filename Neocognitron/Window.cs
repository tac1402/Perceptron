// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Neocognitron
{
	public class Window
	{
		public float[,] Full;
		public int size;

		public string key(int x, int y) { return x.ToString() + "-" + y.ToString(); }

		public (int x, int y) xy(string key) { return (int.Parse(key.Substring(0, key.IndexOf('-'))), int.Parse(key.Substring(key.IndexOf('-') + 1))); }

		public float[,] this[int wx, int wy, int wSize]
		{
			get 
			{
				float[,] win = new float[wSize, wSize];
				for (int y = 0; y < wSize; y++)
				{
					for (int x = 0; x < wSize; x++)
					{
						if (wx + x < size && wy + y < size)
						{
							win[x, y] = Full[wx + x, wy + y];
						}
					}
				}
				return win;
			}
		}

		public float[] this[int wx, int wy, int wSize, bool plane]
		{
			get
			{
				float[] win = new float[wSize * wSize];
				int index = 0;
				for (int x = 0; x < wSize; x++)
				{
					for (int y = 0; y < wSize; y++)
					{
						if (wx + x < size && wy + y < size)
						{
							win[index] = Full[wx + x, wy + y];
						}
						index++;
					}
				}
				return win;
			}
		}



		public Window(int argSize) 
		{
			size = argSize;
			Full = new float[size, size];
		}

		public void Set(float[][] argMatrix)
		{
			int size = argMatrix.Length;
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					Full[x, y] = argMatrix[x][y];
				}
			}
		}

		public void Set(int x, int y, float value)
		{
			Full[x, y] = value;
		}

	}
}

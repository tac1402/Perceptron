// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Neocognitron
{
	/// <summary>
	/// Объект, содержащий все выходные данные определенного слоя неокогнитрона. 
	/// Существует фиксированное количество плоскостей и фиксированный размер каждой плоскости.
	/// </summary>
	public class OutputConnections
	{
		// Number of planes
		private int K;
		// Matrix size of each plane (square)
		private int size;

		// Actual output values (size x size x K)
		private Window[] outputsW;

		public OutputConnections(int argK, int argSize)
		{
			K = argK;
			size = argSize;

			outputsW = new Window[K];

			// Create K outputs, each (size by size)
			for (int i = 0; i < K; i++)
			{
				outputsW[i] = new Window(size);
			}
		}

		public void Set(int k, int n, int m, float value)
		{
			outputsW[k].Set(n, m, value);
		}

		public void Set(int k, float[][] newOutputs)
		{
			outputsW[k].Set(newOutputs);
		}


		/// <summary>
		/// Получить массив окон для определенной точки в каждой плоскости и заданного окна размера. 
		/// Выходные данные форматируются следующим образом: output[planes][window]. 
		/// </summary>
		public float[][] getWindows(int n, int m, int wSize) 
		{
			float[][] ret = new float[K][];
			for (int k = 0; k < K; k++)
			{
				ret[k] = outputsW[k][n, m, wSize, true];
			}
			return ret;
		}

		/// <summary>
		/// Отбирается одна точка из каждого фильтра с максимальным выходным значением
		/// </summary>
		public Point2D[] getRepresentativeCells(int wSize)
		{
			Point2D[] rp = new Point2D[K];
			float[] maxV = new float[K];
			for (int n = 0; n < size - wSize; n++)
			{
				for (int m = 0; m < size - wSize; m++)
				{
					for (int k = 0; k < K; k++)
					{
						float[,] win = outputsW[k][n, m, wSize];

						for (int y = 0; y < wSize; y++)
						{
							for (int x = 0; x < wSize; x++)
							{
								if (win[x, y] > maxV[k])
								{
									if (rp[k] == null) { rp[k] = new Point2D(0, 0); }
									rp[k].X = n + x;
									rp[k].Y = m + y;
									maxV[k] = win[x, y];
								}
							}
						}
					}

				}
			}

			
			return rp;
		}


		public float[] getPointsOnPlanes()
		{
			float[] output = new float[K];
			for (int k = 0; k < K; k++)
			{
				output[k] = outputsW[k].Full[0, 0];
			}

			return output;
		}
	}
}

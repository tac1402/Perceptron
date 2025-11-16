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
		private float[][][] outputs;

		private Window[] outputsW;

		public OutputConnections(int argK, int argSize)
		{
			K = argK;
			size = argSize;

			outputsW = new Window[K];

			// Create K outputs, each (size by size)
			outputs = new float[K][][];
			for (int i = 0; i < K; i++)
			{
				outputs[i] = new float[size][];
				outputsW[i] = new Window(size);
				for (int j = 0; j < size; j++)
				{
					outputs[i][j] = new float[size];
				}
			}
		}

		public void setSingleOutput(int k, int n, int m, float value)
		{
			outputs[k][n][m] = value;
			outputsW[k].Set(n, m, value);
		}

		public void setPlaneOutput(int k, float[][] newOutputs)
		{
			outputs[k] = newOutputs;
			outputsW[k].Set(newOutputs);
		}


		/**
		 * For a specific location and window size, return the resulting window
		 * as a single dimensional array. 
		 * 
		 * @param k				plane location k
		 * @param n				location n
		 * @param m				location m
		 * @param windowSize	size of the square window
		 * @return				array representation of the window
		 */
		public float[] getWindowInPlane(int k, int n, int m, int windowSize) 
		{
			int ws = (int)Math.Pow(windowSize, 2);
			// Initialize the output array
			float[] ret = new float[ws];

			// If window size is the entire plane, return the entire plane
			if (windowSize == size) 
			{
				int count = 0;
				for (int x = 0; x < size; x++) 
				{
					for (int y = 0; y < size; y++) 
					{
						ret[count] = outputs[k][x][y];
						count++;
					}
				}
			// otherwise, convert the window into an array
			} 
			else 
			{
				int offset = (windowSize - 1) / 2;
				int count = 0;
				for (int x = n - offset; x <= n + offset; x++) 
				{
					for (int y = m - offset; y <= m + offset; y++) 
					{
						if (x >= 0 && x < size && y >= 0 && y < size)
						{
							ret[count] = outputs[k][x][y];
						}
						count++;
					}
				}
				int a = 1;
			}

			return ret;
		}

		/**
		 * Получить массив окон для определенной точки в каждой плоскости и заданного окна размера. 
		 * Выходные данные форматируются следующим образом: output[planes][window]. 
		 * 
		 * @param n				location n
		 * @param m				location m
		 * @param windowSize	size of the square window
		 * @return				array representation of each window in each plane
		 */
		public float[][] getWindows(int n, int m, int windowSize) 
		{
			float[][] ret = new float[K][];
			for (int k = 0; k < K; k++)
			{
				ret[k] = getWindowInPlane(k, n, m, windowSize);
			}

			return ret;
		}

		/**
		 Получить список репрезентативных ячеек для данного набора выходных данных. Для этого
		 требуется размер окна, который используется для генерации s-столбцов. Выходные данные 
		 представляют собой массив точек, по одной для каждой плоскости; некоторые плоскости будут иметь нулевое 
		 значение точки, что означает отсутствие репрезентативной ячейки.

		 * @param windowSize	window size used to generate the s-column
		 * @return				the array of representative cells
		 */
		public List<Point2D> getRepresentativeCells(int windowSize)
		{
			List<Location> points = new List<Location>();
			Location temp;

			int offset = (windowSize - 1) / 2;


			// Create a list of all possible representative cells
			if (windowSize == size)
			{
				temp = getLocationOfMax(new Point2D(size / 2, size / 2), windowSize);
				points.Add(temp);
			}
			else
			{

				for (int n = offset; n < size - offset; n++)
				{
					for (int m = offset; m < size - offset; m++)
					{
						temp = getLocationOfMax(new Point2D(n, m), windowSize);
						if (temp != null)
						{
							if (points.Contains(temp) == false)
							{
								points.Add(temp);
							}
						}
					}
				}
			}

			List<Point2D> reps = new List<Point2D>();
			for (int k = 0; k < K; k++)
			{
				Point2D point = getMaxPerPlane(k, points);
				reps.Add(point);
			}

			// Отбирается одна точка из каждого фильтра
			return reps;
		}

		/**
		 * Get array of two dimensional window matrices for a specific point in each
		 * plane and a given window size. The output is formated so that
		 * output[planes][window]. 
		 * 
		 * @param n				location n
		 * @param m				location m
		 * @param windowSize	size of the square window
		 * @return				array representation of each window in each plane
		 */
		public double[][][] getSquareWindows(int n, int m, int windowSize) 
		{
			double[][][] ret = new double[K][][];

			for (int k = 0; k < K; k++)
			{ 
				ret[k] = getSquareWindowInPlane(k, n, m, windowSize);
			}

			return ret;
		}

		/**
		 * For a specific location and window size, return the resulting window
		 * as a two dimensional array. 
		 * 
		 * @param k				plane location k
		 * @param n				location n
		 * @param m				location m
		 * @param windowSize	size of the square window
		 * @return				array representation of the window
		 */
		public double[][] getSquareWindowInPlane(int k, int n, int m, int windowSize)
		{
			double[][] ret = new double[windowSize][];
			for (int i = 0; i < windowSize; i++)
			{ 
				ret[i] = new double[windowSize];
			}

			if (windowSize == size)
			{
				for (int x = 0; x < size; x++)
				{
					for (int y = 0; y < size; y++)
					{
						ret[x][y] = outputs[k][x][y];
					}
				}
			}
			else
			{
				int offset = (windowSize - 1) / 2;
				for (int x = n - offset; x <= n + offset; x++)
				{
					for (int y = m - offset; y <= m + offset; y++)
					{
						int newX = x - n + offset;
						int newY = y - m + offset;

						if (x >= 0 && x < size && y >= 0 && y < size)
						{
							ret[newX][newY] = outputs[k][x][y];
						}
					}
				}
			}
			return ret;
		}

		/**
		 * Given a list of possible representative cells, and a desired plane, determine the location
		 * of the maximum point. Typically there will only be one possible point for a given plane.
		 * 
		 * @param plane	Plane under test
		 * @param l		list of locations for possible representative cells 
		 * @return		specific point in the given plane of the maximum output
		 */
		public Point2D getMaxPerPlane(int plane, List<Location> l)
		{
			Point2D p = null;
			double maxValue = 0;

			for (int i = 0; i < l.Count; i++)
			{
				if (l[i] != null)
				{
					if (l[i].Plane == plane)
					{
						double v = outputs[l[i].Plane][(int)l[i].Point.X][(int)l[i].Point.Y];
						if (v > maxValue)
						{
							maxValue = v;
							p = l[i].Point;
						}
					}
				}
			}
			return p;
		}


		/**
		 * For a given s-column, determine the location of the maximum output value. This
		 * requires knowledge of the window size and the center point of the s-column.
		 * 
		 * @param sColumn		three dimensional s-column array
		 * @param center		center location of the column
		 * @param windowSize	window size used to generate the s-column
		 * @return				Location of maximum value
		 */
		public Location getLocationOfMax(Point2D center, int windowSize)
		{
			Location maxL = null;
			double maxValue = 0;

			double[][][] sColumn = getSquareWindows((int)center.X, (int)center.Y, windowSize);


			// Find maximum value and corresponding location
			for (int k = 0; k < sColumn.Length; k++)
			{
				for (int n = 0; n < sColumn[0].Length; n++)
				{
					for (int m = 0; m < sColumn[0][0].Length; m++)
					{
						if (sColumn[k][n][m] > maxValue)
						{
							maxValue = sColumn[k][n][m];
							maxL = new Location(k, n, m);
						}
					}
				}
			}

			// Determine offset for calculating overall location of max
			int offset = (windowSize - (windowSize % 2)) / 2;

			// If a max exists, generate location object
			if (maxL != null)
			{
				maxL.Point.X = maxL.Point.X + center.X - offset;
				maxL.Point.Y = maxL.Point.Y + center.Y - offset;
			}

			return maxL;
		}

		public double[] getPointsOnPlanes()
		{
			double[] output = new double[K];

			for (int k = 0; k < K; k++)
			{
				output[k] = outputs[k][0][0];
			}

			return output;
		}
	}
}

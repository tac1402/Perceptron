using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Neocognitron
{
	/// <summary>
	/// Объект SLayer содержит все s-ячейки в каждой s-плоскости в пределах заданного s-слоя.
	/// S-слой получает на вход объект OutputConnection от предыдущего слоя и выводит объект OutputConnection.
	/// </summary>
	public class SLayer
	{

		// Structural values saved for speedy access
		private int planes;
		private int size;
		private int windowSize;
		private int columnSize;

		// All the layer's s-cells, organized as sCell[plane][n][m] 
		private SCell[][][] sCells;
		// The single v-plane of v-cells, organized as vcCells[n][m]
		private VSCell[][] vsCells;

		// Learning constant q
		private double q;

		// For every plane, there exists a b[k]
		private double[] b;


		/// <summary>
		/// Для каждой плоскости существует a[k][ck][v] 
		/// где k — местоположение в плоскости s
		/// ck — местоположение окна, входящего из плоскости c 
		/// а v — окно
		/// </summary>
		private double[][][] a;

		// Weights for v-cells c[window]
		private double[] c;

		private NeocognitronStructure s;

		public SLayer(int layer, NeocognitronStructure argS)
		{
			s = argS;

			// Initial values
			size = s.sLayerSizes[layer];
			planes = s.numSPlanes[layer];
			windowSize = s.sWindowSize[layer];
			columnSize = s.sColumnSize[layer];

			//sCells = new SCell[planes][size][size];
			sCells = new SCell[planes][][];
			for (int i = 0; i < planes; i++)
			{
				sCells[i] = new SCell[size][];
				for (int j = 0; j < size; j++)
				{
					sCells[i][j] = new SCell[size];
				}
			}

			//vsCells = new VSCell[size][size];
			vsCells = new VSCell[size][];
			for (int i = 0; i < size; i++)
			{
				vsCells[i] = new VSCell[size];
			}


			q = s.q[layer];

			// Determine number of planes in previous c-layer
			int previousPlanes;
			if (layer == 0)
				previousPlanes = 1;
			else
				previousPlanes = s.numCPlanes[layer - 1];

			c = s.c[layer];

			InitializeA(previousPlanes);
			InitializeB();
			InitializeCells(s.r[layer]);
		}

		/// <summary>
		/// Инициализирует каждую матрицу весов для a[planes][c-planes][window]
		/// </summary>
		/// <param name="previousPlanes">Количество плоскостей в предыдущем c-слое</param>
		public void InitializeA(int previousPlanes)
		{
			//a = new double[planes][previousPlanes][(int)Math.Pow(windowSize, 2)];
			a = new double[planes][][];

			int ws = (int)Math.Pow(windowSize, 2);

			for (int k = 0; k < planes; k++)
			{
				a[k] = new double[previousPlanes][];
				for (int ck = 0; ck < previousPlanes; ck++)
				{

					a[k][ck] = new double[ws];
					for (int w = 0; w < ws; w++)
					{
						a[k][ck][w] = s.rnd.NextDouble() * .4;
					}
				}
			}
		}

		/// <summary>
		/// Инициализирует каждый вес b нулем
		/// </summary>
		public void InitializeB()
		{
			b = new double[planes];
			for (int k = 0; k < planes; k++)
			{
				b[k] = 0;
			}
		}

		/// <summary>
		/// Инициализируйте каждую s-ячейку
		/// </summary>
		public void InitializeCells(double r)
		{
			for (int n = 0; n < size; n++)
			{
				for (int m = 0; m < size; m++)
				{
					vsCells[n][m] = new VSCell(c);
					for (int k = 0; k < planes; k++)
					{
						sCells[k][n][m] = new SCell(r);
					}
				}
			}
		}


		public OutputConnections propagate(OutputConnections input, bool argTrain)
		{

			// Initialize output object
			OutputConnections output = new OutputConnections(planes, size);

			double[][] windowsFromEachPlane;
			double[][] vOutput = new double[size][];
			double value;


			// For every cell location in each plane, propagate the input 
			for (int n = 0; n < size; n++)
			{
				vOutput[n] = new double[size];
				for (int m = 0; m < size; m++)
				{
					//DateTime beginA = DateTime.Now;
					// Get the window array for a specific location (n,m).
					windowsFromEachPlane = input.getWindows(n, m, windowSize);
					//NeocognitronStructure.tA += (DateTime.Now - beginA).TotalMilliseconds;

					// Determine v-cell output for specific location
					vOutput[n][m] = vsCells[n][m].propagate(windowsFromEachPlane);

					// Cycle through each plane and determine the output for a specific location (n,m)
					for (int k = 0; k < planes; k++)
					{
						value = sCells[k][n][m].propagate(windowsFromEachPlane, vOutput[n][m], b[k], a[k]);
						output.setSingleOutput(k, n, m, value);
					}
				}
			}

			if (argTrain)
			{
				train(input, output, vOutput);
				output = propagate(input, false);
			}

			return output;
		}

		/**
		 * Train the s-layer. Modifies the weights based on the input and output of the layer.
		 * 
		 * @param input		Input to the layer
		 * @param output	Output for the given input
		 * @param vOutput	v-plane output for the given input
		 */
		public void train(OutputConnections input, OutputConnections output, double[][] vOutput)
		{
			//DateTime beginB = DateTime.Now;

			// Determine length of the weight array that will be changed (for each window)
			int weightLength = (int)Math.Pow(windowSize, 2);
			double delta;

			// Получить репрезентативные местоположения ячеек из выходных данных
			Point2D[] repLoc = output.getRepresentativeCells(columnSize);

			// Для каждой плоскости в этом конкретном S-слое
			for (int k = 0; k < planes; k++)
			{
				// Пока есть репрезентативная ячейка, обновляйте веса плоскости.
				if (repLoc[k] != null)
				{
					// Get specific representative location
					Point2D p = repLoc[k];

					// Update b weights, one value for each plane (not dependent on (n,m) )
					delta = q / 2 * vOutput[(int)p.X][(int)p.Y];
					b[k] += delta;

					// Loop for every plane in the input (from the previous C-layer) 
					for (int ck = 0; ck < a[k].Length; ck++)
					{
						// Get the output for the previous C-layer (input for this layer)
						double[] in_ = input.getWindowInPlane(ck, (int)p.X, (int)p.Y, windowSize);

						// Loop through every weight a[k][ck][window] in the given window 
						for (int w = 0; w < weightLength; w++)
						{
							delta = q * c[w] * in_[w];
							a[k][ck][w] += delta;
						}
					}
				}
			}

			//NeocognitronStructure.tB += (DateTime.Now - beginB).TotalMilliseconds;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neocognitron
{
	public class CLayer
	{

		// Structural values saved for speedy access
		private int planes;
		private int size;
		private int windowSize;

		// All the layer's c-cells, organized as cCell[plane][n][m] 
		private CCell[][][] cCells;
		// The single v-plane of v-cells, organized as vcCells[n][m]
		private VCCell[][] vcCells;

		private NeocognitronStructure s;

		public CLayer(int layer, NeocognitronStructure argS)
		{
			s = argS;

			// Initialize values
			size = s.cLayerSizes[layer];
			planes = s.numCPlanes[layer];
			windowSize = s.cWindowSize[layer];

			cCells = new CCell[planes][][];
			for (int i = 0; i < planes; i++)
			{
				cCells[i] = new CCell[size][];
				for (int j = 0; j < size; j++)
				{
					cCells[i][j] = new CCell[size];
				}
			}

			vcCells = new VCCell[size][];
			for (int j = 0; j < size; j++)
			{
				vcCells[j] = new VCCell[size];
			}

			InitializeCells(s.d[layer], s.alpha);
		}

		/**
		 * Initializes each c-cell.
		 * 
		 * @param d		Initial d weight values
		 * @param alpha	Constant alpha
		 */
		public void InitializeCells(double[] d, double alpha)
		{
			for (int n = 0; n < size; n++)
			{
				for (int m = 0; m < size; m++)
				{
					vcCells[n][m] = new VCCell(d);

					for (int k = 0; k < planes; k++)
					{
						cCells[k][n][m] = new CCell(d, alpha);
					}
				}
			}
		}

		/**
		 * For a given input, determine the output for this layer. The input
		 * and output object are both OutputConnections. 
		 * 
		 * @param inputs	The input to this layer
		 * @return			The output from this layer
		 */
		public OutputConnections propagate(OutputConnections inputs)
		{
			OutputConnections output = new OutputConnections(planes, size);

			double[][] windowInEachPlane;
			double vOutput;
			double value;

			// For every cell location in each plane, propagate the input 
			for (int n = 0; n < size; n++)
			{
				for (int m = 0; m < size; m++)
				{
					// Get the window array for a specific location (n,m).
					windowInEachPlane = inputs.getWindows(n, m, windowSize);

					// Determine v-cell output for specific location
					vOutput = vcCells[n][m].propagate(windowInEachPlane);
					// Cycle through each plane and determine the output for a specific location (n,m)
					for (int k = 0; k < planes; k++)
					{
						value = cCells[k][n][m].propagate(windowInEachPlane[k], vOutput);
						output.setSingleOutput(k, n, m, value);
					}
				}
			}

			return output;
		}

	}
}

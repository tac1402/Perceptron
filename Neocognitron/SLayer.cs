// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Neocognitron
{
	/// <summary>
	/// Объект SLayer содержит все s-ячейки в каждой s-плоскости в пределах заданного s-слоя.
	/// S-слой получает на вход объект OutputConnection от предыдущего слоя и выводит объект OutputConnection.
	/// </summary>
	public class SLayer
	{
		private int planes;
		private int size;
		private int windowSize;
		private int columnSize;

		// Learning constant q
		private float q;

		// For every plane, there exists a b[k]
		private float[] b;


		/// <summary>
		/// Для каждой плоскости существует a[k][ck][v] 
		/// где k — местоположение в плоскости s
		/// ck — местоположение окна, входящего из плоскости c 
		/// а v — окно
		/// </summary>
		private float[][][] a;

		// Weights for v-cells c[window]
		private MWeights c;

		private float r;

		private NeocognitronStructure s;

		public SLayer(int layer, NeocognitronStructure argS)
		{
			s = argS;

			// Initial values
			size = s.sLayerSizes[layer];
			planes = s.numSPlanes[layer];
			windowSize = s.sWindowSize[layer];
			columnSize = s.sColumnSize[layer];

			q = s.q[layer];
			r = s.r[layer];

			// Determine number of planes in previous c-layer
			int previousPlanes;
			if (layer == 0)
				previousPlanes = 1;
			else
				previousPlanes = s.numCPlanes[layer - 1];

			c = s.c[layer];

			InitializeA(previousPlanes);

			b = new float[planes];

		}

		/// <summary>
		/// Инициализирует каждую матрицу весов для a[planes][c-planes][window]
		/// </summary>
		/// <param name="previousPlanes">Количество плоскостей в предыдущем c-слое</param>
		public void InitializeA(int previousPlanes)
		{
			//a = new double[planes][previousPlanes][(int)Math.Pow(windowSize, 2)];
			a = new float[planes][][];

			int ws = (int)Math.Pow(windowSize, 2);

			for (int k = 0; k < planes; k++)
			{
				a[k] = new float[previousPlanes][];
				for (int ck = 0; ck < previousPlanes; ck++)
				{

					a[k][ck] = new float[ws];
					for (int w = 0; w < ws; w++)
					{
						a[k][ck][w] = (float)s.rnd.NextDouble() * 0.4f;
					}
				}
			}
		}

		public float propagateVS(float[][] inputs, MWeights c)
		{
			float output = 0;
			for (int i = 0; i < inputs.Length; i++)
			{
				for (int j = 0; j < inputs[0].Length; j++)
				{
					output += inputs[i][j] * inputs[i][j] * c.w[j];
				}
			}
			output = (float)Math.Sqrt(output);
			return output;
		}


		public OutputConnections propagate(OutputConnections input, bool argTrain)
		{

			// Initialize output object
			OutputConnections output = new OutputConnections(planes, size);

			float[][] windowsFromEachPlane;
			float[][] vOutput = new float[size][];
			float value;


			// For every cell location in each plane, propagate the input 
			for (int n = 0; n < size; n++)
			{
				vOutput[n] = new float[size];
				for (int m = 0; m < size; m++)
				{
					//DateTime beginA = DateTime.Now;
					// Get the window array for a specific location (n,m).
					windowsFromEachPlane = input.getWindows(n, m, windowSize);
					//NeocognitronStructure.tA += (DateTime.Now - beginA).TotalMilliseconds;

					// Determine v-cell output for specific location
					vOutput[n][m] = propagateVS(windowsFromEachPlane, c);

					// Cycle through each plane and determine the output for a specific location (n,m)
					for (int k = 0; k < planes; k++)
					{
						value = propagateS(windowsFromEachPlane, vOutput[n][m], b[k], a[k], r);
						output.setSingleOutput(k, n, m, value);
					}
				}
			}

			if (argTrain)
			{
				train(input, output, vOutput);
				//output = propagate(input, false);
			}

			return output;
		}

		public float propagateS(float[][] inputs, float vInput, float b, float[][] a, float r)
		{
			float output = 0;
			for (int ck = 0; ck < inputs.Length; ck++)
			{
				output += NeocognitronStructure.arrayMultiply(a[ck], inputs[ck]);
			}

			float denominator = 1 + 2 * r / (1 + r) * b * vInput;

			output = (1 + output) / denominator - 1;

			// Output function, set to zero if negative
			if (output < 0)
			{
				output = 0;
			}

			// Final multiplication
			return r * output;
		}

		/**
		 * Train the s-layer. Modifies the weights based on the input and output of the layer.
		 * 
		 * @param input		Input to the layer
		 * @param output	Output for the given input
		 * @param vOutput	v-plane output for the given input
		 */
		public void train(OutputConnections input, OutputConnections output, float[][] vOutput)
		{
			//DateTime beginB = DateTime.Now;

			// Determine length of the weight array that will be changed (for each window)
			int weightLength = (int)Math.Pow(windowSize, 2);
			float delta;

			// Получить репрезентативные местоположения ячеек из выходных данных
			List<Point2D> repLoc = output.getRepresentativeCells(columnSize);

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
						float[] in_ = input.getWindowInPlane(ck, (int)p.X, (int)p.Y, windowSize);

						// Loop through every weight a[k][ck][window] in the given window 
						for (int w = 0; w < weightLength; w++)
						{
							delta = q * c.w[w] * in_[w];
							a[k][ck][w] += delta;
						}
					}
				}
			}

			//NeocognitronStructure.tB += (DateTime.Now - beginB).TotalMilliseconds;
		}
	}
}

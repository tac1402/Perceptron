// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Neocognitron
{
	public class CLayer
	{
		private int planes;
		private int size;
		private int windowSize;

		private MWeights d;
		private float alpha;

		public CLayer(int layer, NeocognitronStructure s)
		{
			size = s.cLayerSizes[layer];
			planes = s.numCPlanes[layer];
			windowSize = s.cWindowSize[layer];

			d = s.d[layer];
			alpha = s.alpha;
		}

		public OutputConnections propagate(OutputConnections inputs)
		{
			OutputConnections output = new OutputConnections(planes, size);

			float[][] windowInEachPlane;
			float vOutput;
			float value;

			// For every cell location in each plane, propagate the input 
			for (int n = 0; n < size; n++)
			{
				for (int m = 0; m < size; m++)
				{
					//DateTime beginB = DateTime.Now;
					// Get the window array for a specific location (n,m).
					windowInEachPlane = inputs.getWindows(n, m, windowSize);
					//NeocognitronStructure.tB += (DateTime.Now - beginB).TotalMilliseconds;

					// Determine v-cell output for specific location
					vOutput = propagateVC(windowInEachPlane, d);
					// Cycle through each plane and determine the output for a specific location (n,m)
					for (int k = 0; k < planes; k++)
					{
						value = propagateC(windowInEachPlane[k], vOutput, d, alpha);
						output.setSingleOutput(k, n, m, value);
					}
				}
			}

			return output;
		}

		public float propagateC(float[] input, float v, MWeights d, float alpha)
		{
			float output = NeocognitronStructure.arrayMultiply(d.w, input);
			output = (1f + output) / (1f + v) - 1f;

			output = output / (alpha + output);

			// For negative outputs, set to zero
			if (output < 0)
			{
				output = 0;
			}

			return output;
		}


		public float propagateVC(float[][] inputs, MWeights d)
		{
			float output = 0;

			// where input is inputs[sk][window] a window in each plane
			for (int sk = 0; sk < inputs.Length; sk++)
			{
				output += NeocognitronStructure.arrayMultiply(d.w, inputs[sk]);
			}

			return output / inputs.Length;
		}

	}
}

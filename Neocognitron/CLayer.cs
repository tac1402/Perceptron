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

			float value;

			// For every cell location in each plane, propagate the input 
			for (int n = 0; n < size; n++)
			{
				for (int m = 0; m < size; m++)
				{
					//DateTime beginB = DateTime.Now;
					// Get the window array for a specific location (n,m).
					float[][] windowInEachPlane = inputs.getWindows(n, m, windowSize);
					//NeocognitronStructure.tB += (DateTime.Now - beginB).TotalMilliseconds;

					// Determine v-cell output for specific location
					(float[] vOutput, float avg) = propagateVC(windowInEachPlane);

					// Cycle through each plane and determine the output for a specific location (n,m)
					for (int k = 0; k < planes; k++)
					{
						value = propagateC(avg, vOutput[k]);
						output.Set(k, n, m, value);
					}
				}
			}

			return output;
		}

		public float propagateC(float avg, float vOutput)
		{
			//float output = NeocognitronStructure.arrayMultiply(d.w, input);
			float output = (1f + vOutput) / (1f + avg) - 1f;

			output = output / (alpha + output);

			// For negative outputs, set to zero
			if (output < 0)
			{
				//output = 0;
			}

			return output;
		}


		public (float[], float) propagateVC(float[][] inputs)
		{
			float[] output = new float[planes];
			float avg = 0;

			// where input is inputs[sk][window] a window in each plane
			for (int i = 0; i < planes; i++)
			{
				output[i] = NeocognitronStructure.arrayMultiply(d.w, inputs[i]);
				avg += output[i];
			}

			avg = avg / planes;

			return (output, avg);
		}

	}
}

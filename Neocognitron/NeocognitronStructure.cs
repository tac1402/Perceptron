// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Neocognitron

{
	public class NeocognitronStructure
	{
		// Values which dictate the structure of the Neocognitron
		public int inputLayerSize = 16;
		public int numLayers = 3;

		// Number of planes in each layer
		private int p;
		public int[] numSPlanes;
		public int[] numCPlanes;

		// Layer specific values
		public int[] sLayerSizes = { 16, 8, 2 };
		public int[] cLayerSizes = { 10, 6, 1 };
		public int[] sWindowSize = { 5, 5, 5 };
		public int[] cWindowSize = { 5, 5, 2 };
		public int[] sColumnSize = { 5, 5, 2 };

		public float[] r;
		public MWeights[] c;
		public MWeights[] d;
		public float[] q;
		public float alpha;

		// Values used to determine c and d
		//double[] gamma;
		//double[] delta;
		//double[] delta_bar;

		public Random rnd = new Random();

		public NeocognitronStructure()
		{
			p = (int)Math.Round(rnd.NextDouble() * 10 + 10);
			numSPlanes = new int[] { p, p, 100 };
			numCPlanes = new int[] { p, p, 100 };

			//r = new double[] { rnd.NextDouble() * 4 + 1, rnd.NextDouble() * 1 + 2, rnd.NextDouble() * 2 + 2 };
			//q = new double[] { rnd.NextDouble() * .1 + .2, rnd.NextDouble() * 4 + 8, rnd.NextDouble() * 10 + 6 };
			r = new float[] { (float)rnd.NextDouble() * 10, (float)rnd.NextDouble() * 10, (float)rnd.NextDouble() * 10 };
			q = new float[] { (float)rnd.NextDouble() * 10, (float)rnd.NextDouble() * 10, (float)rnd.NextDouble() * 10 };
			//alpha = rnd.NextDouble() * .08 + .42;
			alpha = (float)rnd.NextDouble();

			generateC();
			generateD();
		}


		/// <summary>
		/// Инициализирует C с помощью gamma. Это монотонно убывающая функция. Одно значение для каждого слоя. c[layer][window]
		/// </summary>
		public void generateC()
		{
			c = new MWeights[numLayers];
			float[] gamma = new float[] { (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble() };

			// For first layer, depends on input
			c[0] = new MWeights(sWindowSize[0], gamma[0], 1, true);
			for (int i = 1; i < numLayers; i++)
			{
				c[i] = new MWeights(sWindowSize[i], gamma[i], numCPlanes[i - 1], true);
			}
		}

		/// <summary>
		/// Инициализирует D с помощью delta и delta_bar. Это монотонно убывающая функция. Для каждого слоя существует одно значение. d[layer][window]
		/// </summary>
		public void generateD()
		{
			d = new MWeights[numLayers];
			//double[] delta = new double[] { rnd.NextDouble() * .2 + .4, rnd.NextDouble() * .75 + .2, rnd.NextDouble() * .3 + .4 };
			float[] delta = new float[] { (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble() };
			float[] delta_bar = new float[] { (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble() };

			// For first layer, depends on input
			for (int i = 0; i < numLayers; i++)
			{
				d[i] = new MWeights(cWindowSize[i], delta[i], numSPlanes[i], false);
				for (int j = 0; j < d[i].w.Length; j++)
				{
					d[i].w[j] *= delta_bar[i];
				}
			}
		}

		public static double tA;
		public static double tB;


		/// <summary>
		/// Статическая функция, позволяющая умножать два массива друг на друга.
		/// </summary>
		public static float arrayMultiply(float[] a, float[] b)
		{
			//DateTime beginA = DateTime.Now;

			float output = 0;
			for (int i = 0; i < a.Length; i++)
			{
				output += a[i] * b[i];
			}

			//tA += (DateTime.Now - beginA).TotalMilliseconds;

			return output;
		}
	}
}

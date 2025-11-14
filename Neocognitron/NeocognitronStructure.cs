using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Neocognitron
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

		public double[] r;
		public double[][] c;
		public double[][] d;
		public double[] q;
		public double alpha;

		// Values used to determine c and d
		double[] gamma;
		double[] delta;
		double[] delta_bar;

		public Random rnd = new Random();

		public NeocognitronStructure()
		{
			p = (int)Math.Round(rnd.NextDouble() * 20 + 10);
			numSPlanes = new int[] { p, p, p };
			numCPlanes = new int[] { p, p, p };

			r = new double[] { rnd.NextDouble() * 4 + 1, rnd.NextDouble() * 1 + 2, rnd.NextDouble() * 2 + 2 };
			q = new double[] { rnd.NextDouble() * .1 + .2, rnd.NextDouble() * 4 + 8, rnd.NextDouble() * 10 + 6 };
			alpha = rnd.NextDouble() * .08 + .42;

			gamma = new double[] { rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() };
			delta = new double[] { rnd.NextDouble() * .2 + .4, rnd.NextDouble() * .75 + .2, rnd.NextDouble() * .3 + .4 };
			delta_bar = new double[] { rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() };

			c = new double[numLayers][];
			d = new double[numLayers][];

			generateC();
			generateD();
		}


		/// <summary>
		/// Инициализирует C с помощью gamma. Это монотонно убывающая функция. Одно значение для каждого слоя. c[layer][window]
		/// </summary>
		public void generateC()
		{
			// For first layer, depends on input
			c[0] = generateMonotonic(gamma[0], sWindowSize[0], 1, true);
			for (int l = 1; l < numLayers; l++)
			{
				c[l] = generateMonotonic(gamma[l], sWindowSize[l], numCPlanes[l - 1], true);
			}
		}

		/// <summary>
		/// Инициализирует D с помощью delta и delta_bar. Это монотонно убывающая функция. Для каждого слоя существует одно значение. d[layer][window]
		/// </summary>
		public void generateD()
		{
			// For first layer, depends on input
			for (int l = 0; l < numLayers; l++)
			{
				d[l] = generateMonotonic(delta[l], cWindowSize[l], numSPlanes[l], false);
				for (int w = 0; w < d[l].Length; w++)
				{
					d[l][w] = d[l][w] * delta_bar[l];
				}
			}
		}

		/// <summary>
		/// Сгенерировать монотонно убывающую двумерную функцию.
		/// </summary>
		/// <param name="argBase">Базовое значение для функции</param>
		/// <param name="size">Размер используемого окна</param>
		/// <param name="planes">Количество плоскостей, используемых для нормализации</param>
		/// <param name="norm">Нормализовать выход, чтобы получить сумму 1</param>
		/// <returns>Возвращает монотонную двумерную функцию</returns>
		public double[] generateMonotonic(double argBase, int size, int planes, bool norm)
		{
			double[] output = new double[(int)Math.Pow(size, 2)];
			Point2D center = new Point2D(((double)size - 1) / 2, ((double)size - 1) / 2);

			// Calculated each value
			int index = 0;
			for (int n = 0; n < size; n++)
			{
				for (int m = 0; m < size; m++)
				{
					output[index] = Math.Pow(argBase, center.Distance(n, m));
					index++;
				}
			}

			// Normalize the entire function
			if (norm)
			{
				double sum = 0;
				for (int w = 0; w < Math.Pow(size, 2); w++)
				{
					sum += output[w];
				}
				// Normalize with respect to # of planes
				double multiplier = 1 / ((double)planes * sum);
				for (int w = 0; w < Math.Pow(size, 2); w++)
				{
					output[w] = output[w] * multiplier;
				}
			}
			return output;
		}

		public static double tA;
		public static double tB;


		/// <summary>
		/// Статическая функция, позволяющая умножать два массива друг на друга.
		/// </summary>
		public static double arrayMultiply(double[] a, double[] b)
		{
			//DateTime beginA = DateTime.Now;

			double output = 0;
			for (int i = 0; i < a.Length; i++)
			{
				output += a[i] * b[i];
			}

			//tA += (DateTime.Now - beginA).TotalMilliseconds;

			return output;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neocognitron
{

	/// <summary>
	/// Ячейка vs содержит все константы, необходимые для корректного распространения сигнала для неокогнитрона.
	/// Это ячейка v, которая будет частью s-слоя.
	/// </summary>
	public class VSCell
	{
		// Weight array c[window]
		private double[] c;

		public VSCell(double[] argC) 
		{ 
			c = argC;
		}

		public double propagate(double[][] inputs)
		{
			double output = 0;
			for (int ck = 0; ck < inputs.Length; ck++)
			{
				for (int w = 0; w < inputs[0].Length; w++)
				{
					inputs[ck][w] = Math.Pow(inputs[ck][w], 2);
					output += inputs[ck][w] * c[w];
				}
			}
			output = Math.Sqrt(output);
			return output;
		}

	}
}

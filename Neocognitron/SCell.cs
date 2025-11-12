using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neocognitron
{
	/// <summary>
	/// S-ячейка содержит все константы, необходимые для корректного распространения сигнала для неокогнитрона.
	/// </summary>
	public class SCell
	{

		// Constant r for specific layer
		double r;

		public SCell(double argR)
		{
			r = argR;
		}


		public double propagate(double[][] inputs, double vInput, double b, double[][] a)
		{

			double output = 0;

			for (int ck = 0; ck < inputs.Length; ck++)
			{
				output += NeocognitronStructure.arrayMultiply(a[ck], inputs[ck]);
			}

			double denominator = 1 + 2 * r / (1 + r) * b * vInput;

			output = (1 + output) / denominator - 1;

			// Output function, set to zero if negative
			if (output < 0)
			{
				output = 0;
			}

			// Final multiplication
			return r * output;
		}

	}
}

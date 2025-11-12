using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neocognitron
{
	/// <summary>
	/// C-ячейка содержит все константы, необходимые для корректного распространения сигнала для неокогнитрона.
	/// </summary>
	public class CCell
	{

		// Weight array d[window]
		private double[] d;

		// Global constant alpha 
		private double alpha;

		public CCell(double[] argD, double argAlpha) 
		{ 
			d = argD;
			alpha = argAlpha;
		}

		public double propagate(double[] input, double v)
		{
			double output = NeocognitronStructure.arrayMultiply(d, input);
			output = (1 + output) / (1 + v) - 1;

			output = output / (alpha + output);

			// For negative outputs, set to zero
			if (output < 0)
			{
				output = 0;
			}

			return output;
		}

	}
}

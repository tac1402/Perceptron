using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neocognitron
{
	/// <summary>
	/// Ячейка vc содержит все константы, необходимые для корректного распространения сигнала для неокогнитрона.
	/// Это v-ячейка, которая будет частью c-слоя.
	/// </summary>
	public class VCCell
	{
		// Weight array d[window]
		private double[] d;

		public VCCell(double[] argD)
		{
			d = argD;
		}

		public double propagate(double[][] inputs)
		{
			double output = 0;

			// where input is inputs[sk][window] a window in each plane
			for (int sk = 0; sk < inputs.Length; sk++)
			{
				output += NeocognitronStructure.arrayMultiply(d, inputs[sk]);
			}

			return output / inputs.Length;
		}

	}
}

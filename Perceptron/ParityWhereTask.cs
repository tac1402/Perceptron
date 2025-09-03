using System;
using System.Collections.Generic;
using System.IO;
using Tac.Perceptron;

/// <summary>
/// Пример решения задачи "четность" перцептроном Розенблатта
/// </summary>
public class ParityWhereTask
{
	public void Run()
	{
		int N1 = 256;
		int N2 = 1;

		NeironNetTree net = new NeironNetTree(16, 1000, 4, N1 * N2);

		BitBlock[] input = new BitBlock[N1 * N2];
		BitBlock[] output = new BitBlock[N1 * N2];

		int max = 0;
		//List<string> o = new List<string>();
		for (int i = 0; i < N1 * N2; i++)
		{
			input[i] = new BitBlock(1, new int[] { i });

			int c = WhereParity(input[i]);
			if (max < c) { max = c; }

			output[i] = new BitBlock(1, new int[] { c });

			//o.Add(output[i].ToString(4));

			net.JoinStimul(i, input[i], output[i]);
		}

		/*o.Sort();
		string oo = "";
		for (int i = 0; i < o.Count; i++)
		{
			oo += o[i] + "\n";
		}
		File.WriteAllText("output.txt", oo);*/

		net.Learned();
	}

	private int WhereParity(BitBlock argInput)
	{
		int ret = 1;
		int p = 0;
		int sum = 0;
		int where = 0;
		string whereBit = "";

		for (int i = 0; i < argInput.Count; i++)
		{
			if (argInput[i] == true)
			{
				sum++;
			}
		}
		p = sum % 2;

		if (p == 1)
		{
			where = (sum - 1) / 2;
			whereBit += "1";
			for (int i = 0; i < where; i++)
			{
				whereBit += "0";
			}

			ret = Convert.ToInt32(whereBit, 2);
		}

		return ret;
	}

}

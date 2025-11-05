using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tac.Perceptron;

/// <summary>
/// Пример решения задачи "инверсия" перцептроном Розенблатта
/// </summary>
public class GrayCodeTask
{
	public void Run()
	{
		int N1 = 256;
		int N2 = 256;
		int L = 16;

		//NeironNetTree net = new NeironNetTree(L, 5000, L, N1 * N2);
		PerceptronTLNL net = new PerceptronTLNL(L, 1500, L, N1 * N2 - 1, 0);

		BitBlock[] input = new BitBlock[N1 * N2];
		BitBlock[] output = new BitBlock[N1 * N2];

		for (int i = 1; i < N1 * N2; i++)
		{
			input[i] = new BitBlock(1, new int[] { i });

			int c = GrayCode(input[i]);
			output[i] = new BitBlock(1, new int[] { c });

			//net.JoinStimul(i - 1, input[i], output[i]);
		}

		net.Learned();
	}


	private int GrayCode(BitBlock argInput)
	{
		int binary = argInput.Data[0].Data;
		int ret = binary ^ (binary >> 1);
		return ret;
	}
}

using System;
using System.Collections.Specialized;
using Tac.Perceptron;
using static TorchSharp.torch;

/// <summary>
/// Пример решения задачи "четность" перцептроном Розенблатта
/// </summary>
public class ParityTask
{
	public void Run()
	{
		int N = 128;

		NeironNetR net = new NeironNetR(15, 256 * N, 1, 256 * N);

		BitBlock[] input = new BitBlock[256 * N];
		BitBlock[] output = new BitBlock[256 * N];

		for (int i = 0; i < 256 * N; i++)
		{
			input[i] = new BitBlock(1, new int[] { i });
			output[i] = new BitBlock(1, new int[] { IsParity(input[i]) } );

			net.JoinStimul(i, input[i], output[i]);
		}
		net.Learned();
	}

	private int IsParity(BitBlock argInput)
	{
		int ret = 0;
		int sum = 0;

		for (int i = 0; i < argInput.Count; i++)
		{
			if (argInput[i] == true)
			{
				sum++;
			}
		}
		ret = sum % 2;
		return ret;
	}

}

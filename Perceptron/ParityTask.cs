using System;
using System.Collections.Specialized;
using Tac.Perceptron;

/// <summary>
/// Пример решения задачи "четность" перцептроном Розенблатта
/// </summary>
public class ParityTask
{
	public void Run()
	{
		int N1 = 256;
		int N2 = 256;

		//16-256*12-1
		//16-32-96-1

		//NeironNetA2 net = new NeironNetA2(14, 5000, 100, 1, N1 * N2);
		NeironNetTree net = new NeironNetTree(16, 5000, 1, N1 * N2);

		BitBlock[] input = new BitBlock[N1 * N2];
		BitBlock[] output = new BitBlock[N1 * N2];

		for (int i = 0; i < N1 * N2; i++)
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

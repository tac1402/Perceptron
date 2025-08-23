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
		NeironNet net = new NeironNet(8, 256, 1, 256);

		BitBlock[] input = new BitBlock[256];
		BitBlock[] output = new BitBlock[256];

		for (int i = 0; i < 256; i++)
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

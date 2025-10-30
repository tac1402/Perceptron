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
		int N2 = 64;
		int N3 = 1;
		int L = 14;
		int E = 0;

		//16-256*12-1
		//16-32-96-1

		//NeironNetA2 net = new NeironNetA2(14, 5000, 100, 1, N1 * N2);

		//PerceptronTLNL net = new PerceptronTLNL(12, 300, 1, N1 * N2 * N3, 0);
		//NeironNetA net = new NeironNetA(18, 6000, 1, N1 * N2 * N3);
		NeironNetTreeOld net = new NeironNetTreeOld(L, 3000, 1, N1 * N2 * N3, E);

		net.IsAnalyze = true;
		//net.SinapsXCount = 14;
		//net.SinapsYCount = 14;
		net.sinapsType = NeironNetTreeOld.SinapsType.Full;
		

		float[][] input = new float[N1 * N2 * N3][];
		BitBlock[] output = new BitBlock[N1 * N2 * N3];

		for (int i = 0; i < N1 * N2 * N3; i++)
		{
			BitBlock bitBlock = new BitBlock(1, new int[] { i });
			bitBlock.To();

			input[i] = new float[L];
			for (int j = 0; j < L; j++)
			{
				input[i][j] = bitBlock.DataF[j];
			}

			output[i] = new BitBlock(1, new int[] { IsParity(bitBlock) });

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


using Tac.Experiment;
using Tac.Perceptron;

public class CIFAR_Task : CIFARLib
{
	public void Run()
	{
		int N1 = 10000;
		int L = 1024;
		int E = 10000;


		PerceptronTLNL net = new PerceptronTLNL(L, 10000, 10, N1, E);

		Load();

		BitBlock[] outputE = new BitBlock[E];
		for (int i = 0; i < E; i++)
		{
			outputE[i] = new BitBlock(10);

			int c = (int)ExamLabels[i];
			for (int j = 0; j < 10; j++)
			{
				if (c == j)
				{
					outputE[i][j] = true;
				}
			}

			net.JoinEStimul(i, ExamSetPack[i], outputE[i]);
		}

		BitBlock[] output = new BitBlock[N1];
		for (int i = 0; i < N1; i++)
		{
			output[i] = new BitBlock(10);

			int c = (int)TrainLabels[i];
			for (int j = 0; j < 10; j++)
			{
				if (c == j)
				{
					output[i][j] = true;
				}
			}

			net.JoinStimul(i, TrainSetPack[i], output[i]);
		}

		net.Learned();
		net.Examin(E);
	}
}

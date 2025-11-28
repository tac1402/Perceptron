
using Tac.Perceptron;
using nn = Tac.Perceptron;

/// <summary>
/// Пример решения задачи распознования рукописных цифр (MNIST) перцептроном Розенблатта
/// </summary>
public class MNIST_Task
{
	public void Run()
	{
		int N1 = 60000;
		//int N1 = 10000;
		int L = 28 * 28;

		//NeironNetTree net = new NeironNetTree(L, 20000, 10, N1);
		//PerceptronTLNL net = new PerceptronTLNL(L, 10000, 10, N1);
		nn.MLP net = new nn.MLP(L, 1000, 10, N1, 500);

		MNIST_Dataset dataset = new MNIST_Dataset();
		dataset.Load();


		int E = 10000;

		sbyte[][] outputE = new sbyte[E][];
		for (int i = 0; i < E; i++)
		{
			outputE[i] = new sbyte[10];

			int c = (int)dataset.ExamLabels[i];
			for (int j = 0; j < 10; j++)
			{
				if (c == j)
				{
					outputE[i][j] = 1;
				}
			}

			net.JoinEStimul(i, dataset.ExamSet[i], outputE[i]);
		}

		sbyte[][] output = new sbyte[N1][];
		for (int i = 0; i < N1; i++)
		{
			output[i] = new sbyte[10];

			int c = (int)dataset.TrainLabels[i];
			for (int j = 0; j < 10; j++)
			{
				if (c == j)
				{
					output[i][j] = 1;
				}
			}

			net.JoinStimul(i, dataset.TrainSet[i], output[i]);
		}


		net.Learned();
		//net.Examin();
	}

}

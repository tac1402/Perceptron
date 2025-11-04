
using Tac.Experiment;
using Tac.Perceptron;


public class BlackTask
{

	public float[][] TrainSet = new float[1000][];

	public void Run()
	{

		int N1 = 10;
		int L = 128;
		int E = 10000;
		Random rnd = new Random(1);

		PerceptronDT net = new PerceptronDT(L, 300, 1, N1, E);
		net.IsAnalyze = false;
		net.SinapsXCount = 32;
		net.SinapsYCount = 32;
		net.sinapsType = PerceptronDT.SinapsType.Custom;

		BitBlock[] output = new BitBlock[N1];
		for (int i = 0; i < N1; i++)
		{
			output[i] = new BitBlock(1);

			string s = "";
			TrainSet[i] = new float[L];
			for (int j = 0; j < L; j++)
			{
				int r = rnd.Next(100);
				if (r < 90)
				{
					TrainSet[i][j] = 1;
				}
				s += TrainSet[i][j].ToString();
			}

			if (i > 5) output[i][0] = true;

			net.JoinStimul(i, TrainSet[i], output[i]);
		}


		net.Learned();


	}

}

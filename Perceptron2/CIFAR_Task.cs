
using Tac.Experiment;
using Tac.Perceptron;

public class CIFAR_Task : CIFARLib
{
	public void Run()
	{
		int N1 = 50000;
		int L = 1024 * 3;
		int E = 10000;

		LayerSA layerSA = new LayerSA(L, 12649);


		/*PerceptronDT net = new PerceptronDT(L, 100000, 1, N1, E, 5);
		net.IsAnalyze = true;
		//net.SinapsXCount = 32;
		//net.SinapsYCount = 32;
		net.sinapsType = PerceptronDT.SinapsType.Full;*/

		//PerceptronTLNL net = new PerceptronTLNL(L, 1000, 10, N1, E);
		PerceptronTLNL net = new PerceptronTLNL(L, 10000, 10, N1, E, layerSA);

		Load();

		/*
		string hardError = File.ReadAllText("HardError.txt");
		string[] hError = hardError.Split(',');
		List<int> topError = new List<int>();
		Dictionary<int, int> tE = new Dictionary<int, int>();

		for (int i = 0; i < hError.Length - 1; i++)
		{
			int k = int.Parse(hError[i]);
			topError.Add(k);
			tE.Add(k, 0);
		}*/


		sbyte[][] outputE = new sbyte[E][];
		for (int i = 0; i < E; i++)
		{
			outputE[i] = new sbyte[10];

			int c = (int)ExamLabels[i];
			for (int j = 0; j < 10; j++)
			{
				if (c == j)
				{
					outputE[i][j] = 1;
				}
			}

			net.JoinEStimul(i, ExamSetRGB[i], outputE[i]);
		}

		sbyte[][] output = new sbyte[N1][];
		for (int i = 0; i < N1; i++)
		{
			output[i] = new sbyte[10];

			int c = (int)TrainLabels[i];
			for (int j = 0; j < 10; j++)
			{
				if (c == j)
				{
					output[i][j] = 1;
				}
			}

			net.JoinStimul(i, TrainSetRGB[i], output[i]);
		}
		/*for (int i = 0; i < N1; i++)
		{
			output[i] = new sbyte[1];

			if (topError.Contains(i))
			{
				output[i][0] = 1;
			}

			net.JoinStimul(i, TrainSetRGB[i], output[i]);
		}*/

		net.Learned2();
		net.Examin(E);


		/*
		List<int> topErrorNew = net.TopError(25000);
		string set = "";
		for (int i = 0; i < topErrorNew.Count; i++)
		{
			set += topErrorNew[i].ToString() + ",";
		}
		File.WriteAllText("HardError.txt", set);
		*/
	}
}

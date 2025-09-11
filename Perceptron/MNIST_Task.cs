using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Tac.Perceptron;

/// <summary>
/// Пример решения задачи распознования рукописных цифр (MNIST) перцептроном Розенблатта
/// </summary>
public class MNIST_Task
{
	public void Run()
	{
		int N1 = 59999;
		//int N1 = 10000;
		int L = 441;

		//NeironNetTree net = new NeironNetTree(L, 20000, 10, N1);
		NeironNetTree net = new NeironNetTree(L, 20000, 10, N1, true);
		//net.ACount = 0;
		//net.LoadSA("SA_1.bin");
		//net.LoadSA("SA_2.bin");

		//NeironNetBT net = new NeironNetBT(L, 20000, 10, N1, 10, 10);
		net.IsAnalyze = false;
		net.SASelectCount = 3;
		net.SinapsXCount = 2;
		net.SinapsYCount = 2;
		net.sinapsType = NeironNetTree.SinapsType.Custom;

		string[] LearningSet = File.ReadAllLines("MNIST\\LearningSet.txt");
		string[] ExaminationSet = File.ReadAllLines("MNIST\\ExaminationSet.txt");


		int E = 9999;
		BitBlock[] inputE = new BitBlock[E];
		BitBlock[] outputE = new BitBlock[E];
		for (int i = 0; i < E; i++)
		{
			inputE[i] = new BitBlock(L);
			outputE[i] = new BitBlock(10);

			for (int j = 0; j < L; j++)
			{
				if (ExaminationSet[i].Substring(j + 2, 1) == "1")
				{
					inputE[i][j] = true;
				}
			}

			int c = int.Parse(ExaminationSet[i].Substring(0, 1));

			for (int j = 0; j < 10; j++)
			{
				if (c == j)
				{
					outputE[i][j] = true;
				}
			}

			net.JoinEStimul(i, inputE[i], outputE[i]);
		}

		BitBlock[] input = new BitBlock[N1];
		BitBlock[] output = new BitBlock[N1];

		for (int i = 0; i < N1; i++)
		{
			input[i] = new BitBlock(L);
			output[i] = new BitBlock(10);

			for (int j = 0; j < L; j++)
			{
				if (LearningSet[i].Substring(j + 2, 1) == "1")
				{
					input[i][j] = true;
				}
			}

			int c = int.Parse(LearningSet[i].Substring(0, 1));

			for (int j = 0; j < 10; j++)
			{
				if (c == j)
				{
					output[i][j] = true;
				}
			}

			net.JoinStimul(i, input[i], output[i]);
		}

		net.Learned();
		net.Examin(E);
	}
}

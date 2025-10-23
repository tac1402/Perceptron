
using Tac.Experiment;
using Tac.Perceptron;
using static System.Runtime.InteropServices.JavaScript.JSType;

/// <summary>
/// Пример решения задачи распознования рукописных цифр (MNIST) перцептроном Розенблатта
/// </summary>
public class MNIST_Task : MNISTLib
{
	public void Run()
	{
		int N1 = 60000; //60000;
		//int N1 = 10000;
		int L = 768;
		int E = 10000;

		//NeironNetTree net = new NeironNetTree(L, 20000, 10, N1);
		PerceptronTLNL net = new PerceptronTLNL(L, 20000, 10, N1, E);
		PerceptronTLNL netB = new PerceptronTLNL(L, 10000, 10, N1, E);
		//Perceptron2TLNL net = new Perceptron2TLNL(L, 5000, 2000, 10, N1, E);

		LoadF();

		//Load();
		//LoadHard(5000);

		//ReSort();
		
		string hardError = File.ReadAllText("HardError.txt");
		string[] hError = hardError.Split(',');
		List<int> topError = new List<int>();
		Dictionary<int, int> tE = new Dictionary<int, int>();


		for (int i = 0; i < hError.Length - 1; i++)
		{
			int k = int.Parse(hError[i]);
			topError.Add(k);
			tE.Add(k, 0);
		}

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

			net.JoinEStimul(i, ExamSet[i], outputE[i]);
			netB.JoinEStimul(i, ExamSet[i], outputE[i]);
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

			net.JoinStimul(i, TrainSet[i], output[i]);
		}

		net.ExceptStimul = topError;
		netB.OnlyStimul = topError;

		//net.Learned();
		//net.Examin(E);

		net.ExaminAB(E, netB);


		/*
		List<int> topErrorNew = net.TopError(100);
		SaveSet(topErrorNew);

		string set = "";
		for (int i = 0; i < topErrorNew.Count; i++)
		{
			set += topErrorNew[i].ToString() + ",";
		}
		File.WriteAllText("HardError7.txt", set);
		*/
		/*
		Dictionary<int, int> newError = LoadErrorLog();
		Dictionary<int, int> oldError = LoadErrorLog("ErrorLog_Old.txt");

		Dictionary<int, int> result1 = oldError.Where(kv => !newError.ContainsKey(kv.Key) && oldError[kv.Key] == 1)
										  .ToDictionary(kv => kv.Key, kv => kv.Value);
		Dictionary<int, int> result2 = oldError.Where(kv => !newError.ContainsKey(kv.Key) && oldError[kv.Key] == 2)
										  .ToDictionary(kv => kv.Key, kv => kv.Value);
		int a = 1;
		*/
	}




	public void Analyze()
	{
		LoadF();

		int[] SField = new int[768];
		int[] SField0 = new int[768];

		for (int j = 0; j < 768; j++)
		{
			for (int i = 1; i < 60000; i++)
			{
				if (TrainSet[i][j] !=0 && TrainSet[i][j] == TrainSet[i - 1][j])
				{
					SField[j]++;
				}
				if (TrainSet[i][j] != 0)
				{
					SField0[j]++;
				}
			}
		}
		int a = 1;
	}

	public Dictionary<int, int> LoadErrorLog(string argFileName = "ErrorLog.txt")
	{ 
		Dictionary<int, int> error = new Dictionary<int, int>();
		string[] e = File.ReadAllLines(argFileName);
		for (int i = 0; i < e.Length; i++)
		{
			string[] line = e[i].Split('\t');
			error.Add(int.Parse(line[0]), int.Parse(line[1]));
		}
		return error;
	}

	public void SaveSet(List<int> argSet)
	{
		int n = argSet.Count;

		float[][] newLearningSet = new float[n][];
		float[] newLearningLabel = new float[n];
		for (int i = 0; i < n; i++)
		{
			newLearningSet[i] = new float[28 * 28];
		}

		for (int i = 0; i < n; i++)
		{
			newLearningSet[i] = TrainSet[argSet[i]];
			newLearningLabel[i] = TrainLabels[argSet[i]];
		}

		WritePicture("MNIST\\train-images#hard", n, newLearningSet);
		WriteLabels("MNIST\\train-labels#hard", n, newLearningLabel);
	}



	/// <summary>
	/// Пересортировать обучающую и экзаменационную выборку
	/// </summary>
	public void ReSort()
	{
		int rndNumber = 100;
		Random rnd = new Random(rndNumber);
		int N = 60000;
		int E = 10000;
		int L = 768;

		int[] set = new int[E + N];
		for (int i = 0; i < N + E; i++)
		{
			set[i] = i;
		}
		Shuffle(set, rnd);


		float[][] newLearningSet = new float[N][];
		float[] newLearningLabel = new float[N];
		for (int i = 0; i < N; i++)
		{
			newLearningSet[i] = new float[28 * 28];
		}

		float[][] newExaminationSet = new float[E][];
		float[] newExaminationLabel = new float[E];
		for (int i = 0; i < E; i++)
		{
			newExaminationSet[i] = new float[28 * 28];
		}

		for (int i = 0; i < N; i++)
		{
			if (set[i] < N)
			{
				newLearningSet[i] = TrainSet[set[i]];
				newLearningLabel[i] = TrainLabels[set[i]];
			}
			else
			{
				newLearningSet[i] = ExamSet[set[i]-N];
				newLearningLabel[i] = ExamLabels[set[i]-N];
			}
		}

		for (int i = N; i < N + E; i++)
		{
			if (set[i] < N)
			{
				newExaminationSet[i - N] = TrainSet[set[i]];
				newExaminationLabel[i - N] = TrainLabels[set[i]];
			}
			else
			{
				newExaminationSet[i - N] = ExamSet[set[i] - N];
				newExaminationLabel[i - N] = ExamLabels[set[i] - N];
			}
		}

		WritePicture("MNIST\\train-images#", 60000, newLearningSet);
		WriteLabels("MNIST\\train-labels#", 60000, newLearningLabel);

		WritePicture("MNIST\\t10k-images#", 10000, newExaminationSet);
		WriteLabels("MNIST\\t10k-labels#", 10000, newExaminationLabel);
	}


}

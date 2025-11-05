
using Tac.Experiment;
using Tac.Perceptron;

/// <summary>
/// Пример решения задачи распознования рукописных цифр (MNIST) перцептроном Розенблатта
/// </summary>
public class MNIST_Task : MNISTLib
{
	public void Run()
	{
		int N1 = 60000;
		//int N1 = 10000;
		int L = 28*28;
		int E = 10000;

		//int Add = 100;

		
		PerceptronDT net = new PerceptronDT(L, 100000, 1, N1, E, 5);
		net.IsAnalyze = true;
		//net.SinapsXCount = 32;
		//net.SinapsYCount = 32;
		net.sinapsType = PerceptronDT.SinapsType.Full;
		

		//PerceptronTLNL net = new PerceptronTLNL(L + Add, 20000, 10, N1, E, "B");
		//PerceptronTLNL net = new PerceptronTLNL(L, 5000, 1, N1, E, "С");
		//PerceptronTLNL netB = new PerceptronTLNL(L, 10000, 10, N1, E, "B");
		//Perceptron2TLNL net = new Perceptron2TLNL(L, 1000, 1000, 1, N1, E);

		LoadF(0);

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
		
		/*
		string outputExam = File.ReadAllText("Output_[60000]784x16265.txt");
		int[] outputEx = new int[E];
		for (int i = 0; i < E; i++)
		{
			outputEx[i] = int.Parse(outputExam.Substring(i, 1));
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
			/*for (int j = 0; j < Add; j++)
			{
				ExamSet[i][L + j] = outputEx[i];
			}*/
			
			net.JoinEStimul(i, ExamSet[i], outputE[i]);
		}

		sbyte[][] output = new sbyte[N1][];
		/*for (int i = 0; i < N1; i++)
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
			//int classC = 0;
			//if (topError.Contains(i))
			//{
			//	classC = 1;
			//}
			//for (int j = 0; j < Add; j++)
			//{
			//	TrainSet[i][L + j] = classC;
			//}


			net.JoinStimul(i, TrainSet[i], output[i]);
		}*/
		for (int i = 0; i < N1; i++)
		{
			output[i] = new sbyte[1];

			if (topError.Contains(i))
			{
				output[i][0] = 1;
			}

			net.JoinStimul(i, TrainSet[i], output[i]);
		}



		//net.ExceptStimul = topError;
		//netB.OnlyStimul = topError;

		net.Learned();
		net.Examin(E);

		//net.LoadWeights();
		//netB.LoadWeights();
		//net.ExaminAB(E, netB);

		/*
		List<int> topErrorNew = net.TopError(10000);
		SaveSet(topErrorNew);

		string set = "";
		for (int i = 0; i < topErrorNew.Count; i++)
		{
			set += topErrorNew[i].ToString() + ",";
		}
		File.WriteAllText("HardError1.txt", set);
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


	public float[] InsertGaps(float[] argSet, int L, int Add, float gapValue)
	{
		float[] dest = new float[L + Add];

		// Вычисляем шаг для равномерного распределения
		float step = (float)L / (Add + 1);

		int srcIndex = 0;
		for (int i = 0; i < L + Add; i++)
		{
			// Определяем, должна ли это быть позиция для дырки
			if (srcIndex < L && i > 0 && Math.Abs(i - (srcIndex * step)) < 0.5f)
			{
				dest[i] = gapValue;
			}
			else
			{
				dest[i] = argSet[srcIndex];
				srcIndex++;
			}
		}

		return dest;
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

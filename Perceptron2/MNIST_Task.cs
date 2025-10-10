
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
		PerceptronTLNL net = new PerceptronTLNL(L, 10000, 10, N1);

		string[] LearningSet = File.ReadAllLines("MNIST\\LearningSet.txt");
		string[] ExaminationSet = File.ReadAllLines("MNIST\\ExaminationSet.txt");
		//string[] LearningSet = File.ReadAllLines("MNIST_Fashion\\LearningSetF.txt");
		//string[] ExaminationSet = File.ReadAllLines("MNIST_Fashion\\ExaminationSetF.txt");


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

	/// <summary>
	/// Пересортировать обучающую и экзаменационную выборку
	/// </summary>
	public void ReSort()
	{
		int rndNumber = 100;
		Random rnd = new Random(rndNumber);
		int N = 59999;
		int E = 9999;
		int L = 441;

		string[] LearningSet = File.ReadAllLines("MNIST\\LearningSet.txt");
		string[] ExaminationSet = File.ReadAllLines("MNIST\\ExaminationSet.txt");

		string[] set = new string[E + N];

		for (int i = 0; i < N; i++)
		{
			set[i] = LearningSet[i];
		}
		for (int i = N; i < N + E; i++)
		{
			set[i] = ExaminationSet[i - N];
		}

		string[] newLearningSet = new string[N];
		string[] newExaminationSet = new string[E];

		List<int> ExamSet = new List<int>();
		for (int i = 0; i < E; i++)
		{
			bool IsReSort = false;
			while (IsReSort == false)
			{
				int index = rnd.Next(0, N + E);
				if (ExamSet.Contains(index) == false)
				{
					ExamSet.Add(index);
					IsReSort = true;
				}
			}
		}

		for (int i = 0; i < ExamSet.Count; i++)
		{
			newExaminationSet[i] = set[ExamSet[i]];
		}
		int k = 0;
		for (int i = 0; i < N + E; i++)
		{
			if (ExamSet.Contains(i) == false)
			{
				newLearningSet[k] = set[i];
				k++;
			}
		}
		File.WriteAllLines("MNIST\\LearningSet#" + rndNumber.ToString() + ".txt", newLearningSet);
		File.WriteAllLines("MNIST\\ExaminationSet#" + rndNumber.ToString() + ".txt", newExaminationSet);

	}


}


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
		int L = 768;

		//NeironNetTree net = new NeironNetTree(L, 20000, 10, N1);
		PerceptronTLNL net = new PerceptronTLNL(L, 5000, 10, N1, false);

		Load();

		//string[] LearningSet = File.ReadAllLines("MNIST\\LearningSet.txt");
		//string[] ExaminationSet = File.ReadAllLines("MNIST\\ExaminationSet.txt");
		//string[] LearningSet = File.ReadAllLines("MNIST_Fashion\\LearningSetF.txt");
		//string[] ExaminationSet = File.ReadAllLines("MNIST_Fashion\\ExaminationSetF.txt");


		int E = 10000;
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

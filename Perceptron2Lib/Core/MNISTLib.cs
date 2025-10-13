// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Experiment
{
	public class MNISTLib
	{
		public float[][] TrainSet = new float[60000][];
		public float[] TrainLabels = new float[60000];

		public float[][] ExamSet = new float[60000][];
		public float[] ExamLabels = new float[60000];

		public void Load()
		{
			for (int i = 0; i < TrainSet.Length; i++)
			{
				TrainSet[i] = new float[28 * 28];
			}
			for (int i = 0; i < ExamSet.Length; i++)
			{
				ExamSet[i] = new float[28 * 28];
			}


			ReadPicture("MNIST\\train-images.idx3-ubyte", 60000, TrainSet);
			ReadLabels("MNIST\\train-labels.idx1-ubyte", 60000, TrainLabels);

			ReadPicture("MNIST\\t10k-images.idx3-ubyte", 10000, ExamSet);
			ReadLabels("MNIST\\t10k-labels.idx1-ubyte", 10000, ExamLabels);
		}

		public void ReadPicture(string argFileName, int argCount, float[][] argSet)
		{
			FileStream fs = new FileStream(argFileName, FileMode.Open, FileAccess.Read);
			BinaryReader r = new BinaryReader(fs);
			for (int i = 0; i < 4; i++)
			{
				r.ReadInt32();
			}
			for (int n = 0; n < argCount; n++)
			{
				for (int i = 0; i < 28 * 28; i++)
				{
					argSet[n][i] = r.ReadByte();
				}
			}
		}

		public void ReadLabels(string argFileName, int argCount, float[] argSet)
		{
			FileStream fs = new FileStream(argFileName, FileMode.Open, FileAccess.Read);
			BinaryReader r = new BinaryReader(fs);
			for (int i = 0; i < 2; i++)
			{
				r.ReadInt32();
			}
			for (int n = 0; n < argCount; n++)
			{
				argSet[n] = r.ReadByte();
			}
		}


	}
}

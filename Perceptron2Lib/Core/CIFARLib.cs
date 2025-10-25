// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


namespace Tac.Experiment
{
	public class CIFARLib
	{

		public float[][] TrainSetR = new float[50000][];
		public float[][] TrainSetG = new float[50000][];
		public float[][] TrainSetB = new float[50000][];
		public float[][] TrainSetRGB = new float[50000][];
		public float[][] TrainSetPack = new float[50000][];
		public float[] TrainLabels = new float[50000];

		public float[][] ExamSetR = new float[10000][];
		public float[][] ExamSetG = new float[10000][];
		public float[][] ExamSetB = new float[10000][];
		public float[][] ExamSetRGB = new float[10000][];
		public float[][] ExamSetPack = new float[10000][];
		public float[] ExamLabels = new float[10000];

		private void Init()
		{
			for (int i = 0; i < TrainSetR.Length; i++)
			{
				TrainSetR[i] = new float[32 * 32];
				TrainSetG[i] = new float[32 * 32];
				TrainSetB[i] = new float[32 * 32];
				TrainSetRGB[i] = new float[32 * 32 * 3];
				TrainSetPack[i] = new float[32 * 32];
			}
			for (int i = 0; i < ExamSetR.Length; i++)
			{
				ExamSetR[i] = new float[32 * 32];
				ExamSetG[i] = new float[32 * 32];
				ExamSetB[i] = new float[32 * 32];
				ExamSetRGB[i] = new float[32 * 32 * 3];
				ExamSetPack[i] = new float[32 * 32];
			}
		}

		public void Load()
		{
			Init();

			ReadPicture("CIFAR10\\data_batch_1.bin", 10000, TrainSetR, TrainSetG, TrainSetB, TrainSetRGB, TrainSetPack, TrainLabels);
			ReadPicture("CIFAR10\\data_batch_2.bin", 10000, TrainSetR, TrainSetG, TrainSetB, TrainSetRGB, TrainSetPack, TrainLabels);
			ReadPicture("CIFAR10\\data_batch_3.bin", 10000, TrainSetR, TrainSetG, TrainSetB, TrainSetRGB, TrainSetPack, TrainLabels);
			ReadPicture("CIFAR10\\data_batch_4.bin", 10000, TrainSetR, TrainSetG, TrainSetB, TrainSetRGB, TrainSetPack, TrainLabels);
			ReadPicture("CIFAR10\\data_batch_5.bin", 10000, TrainSetR, TrainSetG, TrainSetB, TrainSetRGB, TrainSetPack, TrainLabels);

			nn = 0;
			ReadPicture("CIFAR10\\test_batch.bin", 10000, ExamSetR, ExamSetG, ExamSetB, ExamSetRGB, ExamSetPack, ExamLabels);
		}


		int nn = 0;
		public void ReadPicture(string argFileName, int argCount, float[][] argSetR, float[][] argSetG, float[][] argSetB, 
			float[][] argSetRGB, float[][] argSetPack, float[] argSetL)
		{
			FileStream fs = new FileStream(argFileName, FileMode.Open, FileAccess.Read);
			BinaryReader r = new BinaryReader(fs);

			for (int n = 0; n < argCount; n++)
			{
				argSetL[nn] = r.ReadByte();
				byte[] R = new byte[32 * 32];
				byte[] G = new byte[32 * 32];
				byte[] B = new byte[32 * 32];
				for (int i = 0; i < 32 * 32; i++)
				{
					R[i] = r.ReadByte();
				}
				for (int i = 0; i < 32 * 32; i++)
				{
					G[i] = r.ReadByte();
				}
				for (int i = 0; i < 32 * 32; i++)
				{
					B[i] = r.ReadByte();
				}

				for (int i = 0; i < 32 * 32; i++)
				{
					uint packed = (uint)(R[i] << 16) | (uint)(G[i] << 8) | B[i];
					argSetPack[nn][i] = packed / 16777215.0f;
				}

				int ii = 0;
				for (int i = 0; i < 32 * 32; i++)
				{
					argSetR[nn][i] = R[i];
					argSetR[nn][i] /= 255f;
					argSetRGB[nn][ii] = argSetR[nn][i];
					ii++;
				}
				for (int i = 0; i < 32 * 32; i++)
				{
					argSetG[nn][i] = G[i];
					argSetG[nn][i] /= 255f;
					argSetRGB[nn][ii] = argSetG[nn][i];
					ii++;
				}
				for (int i = 0; i < 32 * 32; i++)
				{
					argSetB[nn][i] = B[i];
					argSetB[nn][i] /= 255f;
					argSetRGB[nn][ii] = argSetB[nn][i];
					ii++;
				}

				nn++;
			}
		}

	}
}

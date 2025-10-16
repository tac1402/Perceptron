// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System.Data;

namespace Tac.Experiment
{
	public class MNISTLib
	{
		public float[][] TrainSet = new float[60000][];
		public float[] TrainLabels = new float[60000];

		public float[][] ExamSet = new float[60000][];
		public float[] ExamLabels = new float[60000];

		private void Init()
		{
			for (int i = 0; i < TrainSet.Length; i++)
			{
				TrainSet[i] = new float[28 * 28];
			}
			for (int i = 0; i < ExamSet.Length; i++)
			{
				ExamSet[i] = new float[28 * 28];
			}
		}

		public void Load()
		{
			Init();

			ReadPicture("MNIST\\train-images.idx3-ubyte", 60000, TrainSet);
			ReadLabels("MNIST\\train-labels.idx1-ubyte", 60000, TrainLabels);

			ReadPicture("MNIST\\t10k-images.idx3-ubyte", 10000, ExamSet);
			ReadLabels("MNIST\\t10k-labels.idx1-ubyte", 10000, ExamLabels);
		}

		public void LoadHard(int argCount)
		{
			Init();

			ReadPicture("MNIST\\train-images#hard", argCount, TrainSet);
			ReadLabels("MNIST\\train-labels#hard", argCount, TrainLabels);

			ReadPicture("MNIST\\t10k-images.idx3-ubyte", 10000, ExamSet);
			ReadLabels("MNIST\\t10k-labels.idx1-ubyte", 10000, ExamLabels);
		}

		public void LoadF()
		{
			Init();

			ReadPicture("MNIST_Fashion\\train-images-idx3-ubyte", 60000, TrainSet);
			ReadLabels("MNIST_Fashion\\train-labels-idx1-ubyte", 60000, TrainLabels);

			ReadPicture("MNIST_Fashion\\t10k-images-idx3-ubyte", 10000, ExamSet);
			ReadLabels("MNIST_Fashion\\t10k-labels-idx1-ubyte", 10000, ExamLabels);
		}


		public void Load2()
		{
			Init();

			ReadPicture("MNIST\\train-images#", 60000, TrainSet);
			ReadLabels("MNIST\\train-labels#", 60000, TrainLabels);

			ReadPicture("MNIST\\t10k-images#", 10000, ExamSet);
			ReadLabels("MNIST\\t10k-labels#", 10000, ExamLabels);
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
					argSet[n][i] /= 255f;
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

		public void WritePicture(string argFileName, int argCount, float[][] argSet)
		{
			FileStream fs = new FileStream(argFileName, FileMode.Create, FileAccess.Write);
			BinaryWriter w = new BinaryWriter(fs);

			// Записываем заголовок (4 пустых int32, как в исходном файле)
			for (int i = 0; i < 4; i++)
			{
				w.Write(0); // или нужные значения заголовка, если они известны
			}

			for (int n = 0; n < argCount; n++)
			{
				for (int i = 0; i < 28 * 28; i++)
				{
					// Обратное преобразование: умножаем на 255 и конвертируем в byte
					byte value = (byte)(argSet[n][i] * 255f);
					w.Write(value);
				}
			}

			w.Close();
			fs.Close();
		}

		public void WriteLabels(string argFileName, int argCount, float[] argSet)
		{
			FileStream fs = new FileStream(argFileName, FileMode.Create, FileAccess.Write);
			BinaryWriter w = new BinaryWriter(fs);

			// Записываем заголовок (2 пустых int32)
			for (int i = 0; i < 2; i++)
			{
				w.Write(0); // или нужные значения заголовка
			}

			for (int n = 0; n < argCount; n++)
			{
				// Конвертируем float в byte (метки обычно целочисленные)
				byte label = (byte)argSet[n];
				w.Write(label);
			}

			w.Close();
			fs.Close();
		}

		public int[] Shuffle(int[] list, Random rnd)
		{
			int n = list.Length;
			while (n > 1)
			{
				n--;
				int k = rnd.Next(n + 1);
				int value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
			return list;
		}

	}
}

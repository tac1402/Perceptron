using System;
using System.Collections.Generic;

namespace BackProp
{

	public class ParityDataset
	{
		public NArray[] features;
		public NArray labels;

		public ParityDataset(NArray[] argFeatures, NArray argLabels)
		{
			features = argFeatures;
			labels = argLabels;
		}

		public long Count => features.Length;

		// Создание датасета для задачи четности (XOR)
		public static ParityDataset CreateXORDataset()
		{
			NArray[] features = new NArray[4];
			features[0].Set(0, 0);
			features[1].Set(0, 1);
			features[2].Set(1, 0);
			features[3].Set(1, 1);

			NArray labels = new NArray(4, 0);
			labels.Set(0, 1, 1, 0);

			return new ParityDataset(features, labels);
		}

		// Метод для создания датасета n-битной четности
		public static ParityDataset CreateNParityDataset(int numBits)
		{
			int numSamples = (int)Math.Pow(2, numBits);
			NArray[] features = new NArray[numSamples];
			NArray labels = new NArray(numSamples, 0);

			for (int i = 0; i < numSamples; i++)
			{
				features[i] = new NArray(numBits, 0);
				int sum = 0;

				for (int j = 0; j < numBits; j++)
				{
					long period = (long)Math.Pow(2, j + 1);
					long halfPeriod = period / 2;
					double value = ((i % period) >= halfPeriod) ? 1.0f : 0.0f;

					features[i][j] = value;
					sum += (int)value;
				}

				// Вычисляем четность (XOR всех битов)
				int parity = sum % 2;

				labels[i] = parity;
			}

			return new ParityDataset(features, labels);
		}
	}
}

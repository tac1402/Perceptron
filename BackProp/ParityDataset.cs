using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackProp
{

	public class ParityDataset
	{
		public double[][] features;
		public double[] labels;

		public ParityDataset(double[][] features, double[] labels)
		{
			if (features.Length != labels.Length)
				throw new ArgumentException("Features and labels must have the same length");

			this.features = features;
			this.labels = labels;
		}

		public long Count => features.Length;

		public (double[] features, double labels) GetItem(long index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException($"Index {index} is out of range for dataset with {Count} items");

			return (features[index], labels[index]);
		}

		// Создание датасета для задачи четности (XOR)
		public static ParityDataset CreateXORDataset()
		{
			double[][] features = new double[][]
			{
			new double[] {0, 0},
			new double[] {0, 1},
			new double[] {1, 0},
			new double[] {1, 1}
			};

			double[] labels = new double[]
			{
				0,
				1,
				1,
				0
			};

			return new ParityDataset(features, labels);
		}

		// Метод для создания датасета n-битной четности
		public static ParityDataset CreateNParityDataset(int numBits)
		{
			long numSamples = (long)Math.Pow(2, numBits);
			double[][] features = new double[numSamples][];
			double[] labels = new double[numSamples];

			for (long i = 0; i < numSamples; i++)
			{
				features[i] = new double[numBits];
				int sum = 0;

				// Заполняем features битовым представлением числа i
				for (int j = 0; j < numBits; j++)
				{
					// Используем принцип периодов как в вашем примере
					long period = (long)Math.Pow(2, j + 1);
					long halfPeriod = period / 2;
					double value = ((i % period) >= halfPeriod) ? 1.0 : 0.0;

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

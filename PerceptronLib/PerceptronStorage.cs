using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptronLib
{
	public class PerceptronStorage
	{
		public int ACount;
		public int HCount;

		public Dictionary<int, float[]>[] WeightSAChange; // Веса между S-A элементами

		public PerceptronStorage(int argACount, int argHCount)
		{
			ACount = argACount;
			HCount = argHCount;
			WeightSAChange = new Dictionary<int, float[]>[HCount];
		}

		public void Clear()
		{
			WeightSAChange = new Dictionary<int, float[]>[HCount];
		}

		public void Add(int argStimulNumber, int SNumber, int ANumber, float value)
		{
			if (WeightSAChange[argStimulNumber] == null)
			{
				WeightSAChange[argStimulNumber] = new Dictionary<int, float[]>();
			}
			if (WeightSAChange[argStimulNumber].ContainsKey(SNumber) == false)
			{
				WeightSAChange[argStimulNumber].Add(SNumber, new float[ACount]);
			}
			WeightSAChange[argStimulNumber][SNumber][ANumber] += value;
		}

		public void Load(ref Dictionary<int, float[]> argWeightSA, int argStimulNumber)
		{
			// Проверяем корректность номера стимула и наличие изменений
			if (argStimulNumber < 0 || argStimulNumber >= HCount || WeightSAChange[argStimulNumber] == null)
				return;

			// Проходим по всем записям изменений для данного стимула
			foreach (var change in WeightSAChange[argStimulNumber])
			{
				for (int i = 0; i < ACount; i++)
				{
					argWeightSA[change.Key][i] -= change.Value[i];
				}
			}
		}

	}
}

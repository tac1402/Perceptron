using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tac.Perceptron;

namespace PerceptronLib
{
	public class NeuronSpecialization
	{
		public NeuronSpecializationInfo[] spec;
		//private NeighborhoodAnalyzer neighborhoodAnalyzer;

		protected int SCount; // Количество сенсоров
		public int ACount; // Количество ассоциаций
		protected int RCount; // Количество реакций
		protected int HCount; // Количество примеров

		protected Random rnd;

		private Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		private LRegion purityAnalyzer;

		public NeuronSpecialization(int argSCount, int argACount, int argRCount, Random argRnd, Dictionary<int, BitBlock> argNecessaryReactions)
		{
			SCount = argSCount;
			ACount = argACount;
			RCount = argRCount;
			rnd = argRnd;
			NecessaryReactions = argNecessaryReactions;
			//neighborhoodAnalyzer =new NeighborhoodAnalyzer();

			purityAnalyzer = new LRegion(argNecessaryReactions);

			InitializeSpecializations();
		}

		private void InitializeSpecializations()
		{
			spec = new NeuronSpecializationInfo[ACount];

			int neuronsPerSpecialization = ACount / (RCount * 2);
			int remainingNeurons = ACount % (RCount * 2);

			int neuronIndex = 0;
			for (int r = 0; r < RCount; r++)
			{
				for (int value = 0; value <= 1; value++)
				{
					int count = neuronsPerSpecialization + (remainingNeurons > 0 ? 1 : 0);
					for (int i = 0; i < count && neuronIndex < ACount; i++)
					{
						spec[neuronIndex] = new NeuronSpecializationInfo
						{
							TargetRNeuron = r,
							TargetValue = value,
							Usefulness = 1.0,
							CurrentPurity = 0.5,
							AvgActivation = 0.5
						};
						neuronIndex++;
					}
					if (remainingNeurons > 0) remainingNeurons--;
				}
			}
		}

		public void AnalyzePurity(Dictionary<int, float[]> aActivations)
		{


			// Анализируем чистоту для каждого R-нейрона отдельно
			for (int r = 0; r < RCount; r++)
			{
				// Создаем массив реакций для текущего R-нейрона
				int[] reactions = new int[aActivations.Count];
				for (int i = 0; i < aActivations.Count; i++)
				{
					int v = 0; if (NecessaryReactions[i][r]) v = 1;
					reactions[i] = v;
				}

				// Используем существующий метод анализа чистоты
				purityAnalyzer.reactions = reactions;
				purityAnalyzer.NeighborhoodPurity(aActivations, k: aActivations.Count/2);

				// Обновляем чистоту для соответствующих специализаций
				UpdatePurity(r, purityAnalyzer.avgPurity);
			}
		}

		private void UpdatePurity(int rNeuron, double purity)
		{
			for (int i = 0; i < spec.Length; i++)
			{
				var sp = spec[i];
				if (sp.TargetRNeuron == rNeuron)
				{
					// Плавное обновление чистоты
					sp.CurrentPurity = 0.9 * sp.CurrentPurity + 0.1 * purity;
				}
			}
		}

		public void UpdateSpec(Dictionary<int, float[]> aActivations)
		{
			for (int i = 0; i < spec.Length; i++)
			{
				var sp = spec[i];
				if (sp.TargetRNeuron == -1) continue;

				// Вычисляем среднюю активацию для целевых случаев
				double sumActivation = 0;
				int count = 0;

				for (int sample = 0; sample < aActivations.Count; sample++)
				{
					int v = 0; if (NecessaryReactions[sample][sp.TargetRNeuron]) v = 1;

					if (v == sp.TargetValue)
					{
						int aa = 0;

						if (aActivations[sample][i] > 0)
						{
							aa = 1;
							sumActivation += aa;
							count++;
						}
					}
				}

				if (count > 0)
				{
					sp.AvgActivation = sumActivation / count;

					// Обновляем полезность на основе чистоты и активации
					sp.Usefulness = sp.CurrentPurity * sp.AvgActivation;
				}
			}

			int a = 1;
		}


		public float GetSpecializationFactor(NeuronSpecializationInfo spec, int rNeuron, int targetValue)
		{
			if (spec.TargetRNeuron == rNeuron && spec.TargetValue == targetValue)
				return 1.0f; // Увеличиваем вероятность для целевой специализации
			else
				return 0.0f; // Нейроны, специализированные на других R-нейронах
		}

		public float GetPurityFactor(NeuronSpecializationInfo spec, double correction)
		{
			// Учитываем чистоту окрестности при принятии решения о коррекции
			float ret = 0.001f;
			if (spec.CurrentPurity > 0.5f)
			{
				ret = 1; // (float)spec.CurrentPurity;
			}
			return ret;

			/*if (correction > 0) // Активация
			{
				// Поощряем активацию, если чистота высокая
				return (float)(0.8 + 0.4 * spec.CurrentPurity);
			}
			else // Деактивация
			{
				// Осторожнее с деактивацией при высокой чистоте
				return (float)(1.2 - 0.4 * spec.CurrentPurity);
			}*/
		}

	}

	public class NeuronSpecializationInfo
	{
		public int TargetRNeuron = -1;		// Индекс R-нейрона, на котором специализируется данный A-нейрон
		public int TargetValue;			// Целевое значение (0 или 1), для которого специализируется нейрон
		public double Usefulness;       // Полезность нейрона для своей специализации
		public double CurrentPurity;
		public double AvgActivation;
	}

}

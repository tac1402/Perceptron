// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;

using TorchSharp;
using static TorchSharp.torch;

namespace Tac.Perceptron
{
	/// <summary>
	/// Классическая версия элементарного перцептрона Розенблатта
	/// </summary>
	public class NeironNetR
	{
		public Tensor SensorsField; /* Сенсорное поле */
		public Tensor AssociationsField; /* Ассоциативное поле */
		public Tensor ReactionsField; /* Реагирующие поле */

		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров, запоминается реакция A-элементов на каждый пример из обучающей выборки

		public Tensor AHConnections; // Как реагируют A-элементы на каждый стимул из обучающей выборки

		public Tensor LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Tensor NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Tensor WeightSA; // Веса между S-A элементами
		public Tensor WeightAR; // Веса между A-R элементами

		private Tensor ReactionError;

		private Random rnd = new Random(10);

		public NeironNetR(int argSCount, int argACount, int argRCount, int argHCount)
		{
			ACount = argACount;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;

			SensorsField = zeros(SCount, device: torch.CUDA);
			AssociationsField = zeros(ACount, device: torch.CUDA);
			ReactionsField = zeros(RCount, device: torch.CUDA);

			WeightSA = zeros(SCount, ACount, device: torch.CUDA);
			WeightAR = zeros(ACount, RCount, device: torch.CUDA);

			for (int i = 0; i < ACount; i++)
			{
				InitSA(i);
			}

			LearnedStimuls = zeros(HCount, SCount, device: torch.CUDA);
			NecessaryReactions = zeros(HCount, RCount, device: torch.CUDA);
		}

		private void InitSA(int argAId)
		{
			int SinapsCount = 16;

			int sensorNumber = 0;
			sbyte sensorType = 0;

			for (int j = 0; j < SinapsCount; j++)
			{
				sensorNumber = rnd.Next(SCount);

				if (rnd.Next(2) == 0) sensorType = 1; else sensorType = -1;

				WeightSA[sensorNumber][argAId] = tensor(sensorType);
			}

		}

		/// <summary>
		/// Добавить на обработку новый пример из обучающей выборки
		/// </summary>
		/// <param name="argStimulNumber">Номер примера из обучающей выборки</param>
		/// <param name="argPerception">Стимулы (входы) из примера обучающей выборки</param>
		/// <param name="argReaction">Нужная реакция (выходы) из примера обучающей выборки</param>
		public void JoinStimul(int argStimulNumber, BitBlock argPerception, BitBlock argReaction)
		{
			// Запомним обучающий стимул
			for (int i = 0; i < SCount; i++)
			{
				float v = 0;
				if (argPerception[i] == true) { v = 1; }
				LearnedStimuls[argStimulNumber][i] = tensor(v);
			}

			// Запомним какая реакция должна быть на этот пример
			for (int i = 0; i < RCount; i++)
			{
				NecessaryReactions[argStimulNumber][i] = tensor(argReaction[i]);
			}

		}

		// Глобальный счетчик ошибок (тензор на GPU)
		private Tensor GlobalErrorCount;

		/// <summary>
		/// Когда все примеры добавлены, вызывается чтобы перцептрон их выучил
		/// </summary>
		public void Learned()
		{
			// Делаем очень много итераций
			for (int n = 0; n < 100000; n++)
			{
				long Error = 0;

				GlobalErrorCount = torch.tensor(0L, device: torch.CUDA);

				DateTime begin = DateTime.Now;
				aTime = 0;

				// За каждую итерацию прокручиваем все примеры из обучающей выборки
				for (int i = 0; i < HCount; i++)
				{
					// Активируем S-элементы, т.е. подаем входы и рассчитываем средний слой A-элементы
					SActivation(i);
					// Активируем R-элементы, т.е. рассчитываем выходы
					RActivation(i);
					// Узнаем ошибся перцептрон или нет, если ошибся отправляем на обучение
					GetError(i);
					LearnedStimul(i);
				}

				Error = GlobalErrorCount.item<long>();
				double t = (DateTime.Now - begin).TotalMilliseconds;
				Console.WriteLine(n.ToString() + " - " + Error.ToString() + " - " + t.ToString() + " ms");
				Console.WriteLine("\t" + aTime.ToString() + " ms");
				if (Error == 0) { break; }
			}
		}

		double aTime = 0;

		private void SActivation(int argStimulNumber)
		{
			DateTime begin = DateTime.Now;

			AssociationsField = zeros(ACount, device: torch.CUDA);

			// Кинем на сенсоры обучающий пример
			SensorsField = LearnedStimuls[argStimulNumber];

			// Если есть активные сенсоры
			if (SensorsField.numel() > 0)
			{
				// Умножаем маску на веса (используем расширение размерности)
				Tensor maskedWeights = WeightSA * SensorsField.unsqueeze(1);

				// Суммируем по первому измерению (по сенсорам)
				Tensor sumWeights = maskedWeights.sum(dim: 0);

				// Добавляем к AssociationsField
				AssociationsField.add_(sumWeights);

				maskedWeights.Dispose();
				sumWeights.Dispose();
			}

			// Запомним как на этот пример реагировали A - элементы
			Tensor mask = AssociationsField > 0;
			AHConnections = mask.nonzero().squeeze();

			mask.Dispose();

			//AHConnections_ = AHConnections.cpu().to_type(ScalarType.Int32).data<int>().ToArray<int>();

			double t = (DateTime.Now - begin).TotalMilliseconds;
			aTime += t;
		}


		private void RActivation(int argStimulNumber)
		{
			Tensor Summa = zeros(RCount, device: torch.CUDA);

			if (AHConnections.numel() > 0)
			{
				Tensor selectedRows = WeightAR.index_select(0, AHConnections);
				Summa = selectedRows.sum(dim: 0);
				selectedRows.Dispose();
			}

			ReactionsField = Summa > 0;

			//int[] Summa_a = Summa.cpu().to_type(ScalarType.Int32).data<int>().ToArray<int>();

			Summa.Dispose();
		}

		private void GetError(int argStimulNumber)
		{
			Tensor mask = ReactionsField != NecessaryReactions[argStimulNumber];

			ReactionError = zeros(RCount, device: torch.CUDA);

			// Вычисляем флаг ошибки
			Tensor isErrorTensor = mask.any();
			GlobalErrorCount.add_(isErrorTensor);

			// Преобразуем ReactionsField в числовой формат (1.0 для true, 0.0 для false)
			Tensor reactionsNumeric = NecessaryReactions[argStimulNumber].to_type(ScalarType.Float32);
			// Вычисляем значения для ошибок: 1 для true, -1 для false
			Tensor errorValues = (2 * reactionsNumeric - 1) * mask.to_type(ScalarType.Float32);
			
			// Обновляем ReactionError только при наличии ошибки
			ReactionError = torch.where(isErrorTensor, errorValues, ReactionError);

			reactionsNumeric.Dispose();
			errorValues.Dispose();
			mask.Dispose();
		}

		private void LearnedStimul(int argStimulNumber)
		{
			if (AHConnections.numel() > 0)
			{
				Tensor expandedError = ReactionError.unsqueeze(0);
				Tensor broadcastedError = expandedError.expand((int)AHConnections.size(0), -1);
				Tensor selectedRows = WeightAR.index_select(0, AHConnections);
				Tensor updatedRows = selectedRows + broadcastedError;

				WeightAR.index_copy_(0, AHConnections, updatedRows);

				expandedError.Dispose();
				broadcastedError.Dispose();
				selectedRows.Dispose();
				updatedRows.Dispose();
			}
		}
	}
}

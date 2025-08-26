// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using TorchSharp;
using static TorchSharp.torch;

namespace Tac.Perceptron
{
	/// <summary>
	/// Классическая версия элементарного перцептрона Розенблатта
	/// </summary>
	public class NeironNetR
	{
		public BitBlock SensorsField; /* Сенсорное поле */
		public Tensor AssociationsField; /* Ассоциативное поле */
		//public BitBlock ReactionsField; /* Реагирующие поле */
		public Tensor ReactionsField; /* Реагирующие поле */

		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров, запоминается реакция A-элементов на каждый пример из обучающей выборки

		public Tensor AHConnections; // Как реагируют A-элементы на каждый стимул из обучающей выборки

		public Dictionary<int, BitBlock> LearnedStimuls; // Обучающие стимулы из обучающей выборки
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

			SensorsField = new BitBlock(SCount);

			WeightSA = zeros(SCount, ACount, device: torch.CUDA);
			AssociationsField = zeros(ACount, device: torch.CUDA);

			for (int i = 0; i < ACount; i++)
			{
				InitSA(i);
			}

			LearnedStimuls = new Dictionary<int, BitBlock>();

			NecessaryReactions = zeros(HCount, RCount, device: torch.CUDA);

			WeightAR = zeros(ACount, RCount, device: torch.CUDA);
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
			LearnedStimuls.Add(argStimulNumber, argPerception);

			// Запомним какая реакция должна быть на этот пример
			for (int i = 0; i < RCount; i++)
			{
				NecessaryReactions[argStimulNumber][i] = tensor(argReaction[i]);
			}

		}


		/// <summary>
		/// Когда все примеры добавлены, вызывается чтобы перцептрон их выучил
		/// </summary>
		public void Learned()
		{
			// Делаем очень много итераций
			for (int n = 0; n < 100000; n++)
			{
				int Error = 0;

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
					bool e = GetError(i);
					if (e == true)
					{
						LearnedStimul(i);
						Error++; // Число ошибок, если в конце итерации =0, то выскакиваем из обучения.
					}
				}
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

			for (int i = 0; i < SCount; i++)
			{
				if (SensorsField[i] == true)
				{
					AssociationsField.add_(WeightSA[i]);
				}
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
			Tensor selectedRows = WeightAR.index_select(0, AHConnections);
			Summa = selectedRows.sum();

			ReactionsField = Summa > 0;

			//int[] Summa_a = Summa.cpu().to_type(ScalarType.Int32).data<int>().ToArray<int>();

			Summa.Dispose();
			selectedRows.Dispose();
		}

		private bool GetError(int argStimulNumber)
		{
			Tensor mask = ReactionsField != NecessaryReactions[argStimulNumber];

			bool IsError = mask.any().item<bool>();
			ReactionError = zeros(RCount, device: torch.CUDA);

			if (IsError)
			{
				// Преобразуем ReactionsField в числовой формат (1.0 для true, 0.0 для false)
				Tensor reactionsNumeric = NecessaryReactions[argStimulNumber].to_type(ScalarType.Float32);
				// Вычисляем значения для ошибок: 1 для true, -1 для false
				Tensor errorValues = (2 * reactionsNumeric - 1) * mask.to_type(ScalarType.Float32);
				ReactionError.add_(errorValues);

				reactionsNumeric.Dispose();
				errorValues.Dispose();
			}
			mask.Dispose();

			return IsError;
		}

		private void LearnedStimul(int argStimulNumber)
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

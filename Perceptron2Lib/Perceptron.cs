// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Perceptron
{

	public class StimulSet
	{
		public Dictionary<int, float[]> Stimuls = new Dictionary<int, float[]>(); 
	}

	public class Perceptron
	{
		public int SCount; // Количество сенсоров
		public int ACount; // Количество ассоциаций
		public int RCount; // Количество реакций
		public int HCount; // Количество примеров
		public int ECount; // Количество примеров
		public int TSet = 1;

		public StimulSet[] LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public StimulSet[] ExaminStimuls; // Стимулы для экзамена

		public Dictionary<int, sbyte[]> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки
		public Dictionary<int, sbyte[]> ExaminReactions; // Требуемая реакция на каждый стимул во время экзамена

		public Reaction r;

		protected float[] RField;
		protected Random rnd = new Random(24);

		protected PerceptronAA AB;

		public Perceptron(int argSCount, int argACount, int argRCount, int argHCount, int argECount, int argTSet = 1)
		{
			ACount = argACount;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;
			ECount = argECount;
			TSet = argTSet;

			LearnedStimuls = new StimulSet[TSet];
			ExaminStimuls = new StimulSet[TSet];
			for (int i = 0; i < TSet; i++)
			{
				LearnedStimuls[i] = new StimulSet();
				ExaminStimuls[i] = new StimulSet();
			}

			NecessaryReactions = new Dictionary<int, sbyte[]>();
			ExaminReactions = new Dictionary<int, sbyte[]>();

			r = new Reaction(RCount);
		}

		/// <summary>
		/// Добавить на обработку новый пример из обучающей выборки
		/// </summary>
		/// <param name="argStimulNumber">Номер примера из обучающей выборки</param>
		/// <param name="argPerception">Стимулы (входы) из примера обучающей выборки</param>
		/// <param name="argReaction">Нужная реакция (выходы) из примера обучающей выборки</param>
		public void JoinStimul(int argStimulNumber, float[] argPerception, sbyte[] argReaction, int t = 0)
		{
			// Запомним обучающий стимул
			LearnedStimuls[t].Stimuls.Add(argStimulNumber, argPerception);

			if (t == 0)
			{
				// Запомним какая реакция должна быть на этот пример
				NecessaryReactions.Add(argStimulNumber, argReaction);
			}
		}

		public void JoinEStimul(int argStimulNumber, float[] argPerception, sbyte[] argReaction, int t = 0)
		{
			// Запомним обучающий стимул
			ExaminStimuls[t].Stimuls.Add(argStimulNumber, argPerception);

			if (t == 0)
			{
				// Запомним какая реакция должна быть на этот пример
				ExaminReactions.Add(argStimulNumber, argReaction);
			}
		}


		protected void GetError(int argStimulNumber, int argMode = 0)
		{
			if (argMode == 0)
			{
				GetError(argStimulNumber, NecessaryReactions[argStimulNumber]);
			}
			else if (argMode == 1)
			{
				GetError(argStimulNumber, ExaminReactions[argStimulNumber]);
			}
		}

		private void GetError(int argStimulNumber, sbyte[] need)
		{
			r.Clear();
			r.CalcRMax(RField);

			int a = 0;

			for (int i = 0; i < RCount; i++)
			{
				int output = (RField[i] > 0) ? 1 : -1;
				int n = (need[i] == 1) ? 1 : -1;
				if (output != n)
				{
					r.IsErrorHard = true;
					r.Error[i] = n;
				}
				else
				{
					a++;
				}

				if (need[i] == 1 && i != r.RMax)
				{
					r.IsErrorSoft = true;
				}
			}

			if (a == 1)
			{
				int b = 1;
			}

		}

	}
}

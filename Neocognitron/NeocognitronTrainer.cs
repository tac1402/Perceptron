// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Neocognitron
{
	/// <summary>
	/// Это вспомогательный класс, используемый для обучения неокогнитрона. Используя Lists входных данных, неокогнитрон 
	/// можно обучить и проверить.Он также содержит методы для определения частоты ошибок сети.
	/// </summary>
	public class NeocognitronTrainer
	{

		// Input images used for training and verification
		List<float[][]> inputs;
		List<float[][]> testInputs;

		public NeocognitronTrainer(List<float[][]> argInputs, List<float[][]> argTestInputs)
		{ 
			inputs = argInputs;
			testInputs = argTestInputs;
		}

		/// <summary>
		/// Обучаем неокогнитрон, используя определённое количество циклов. Используя определённый обучающий набор изображений.
		/// </summary>
		/// <param name="loops">Количество раз, сколько каждое изображение должно быть показано сети</param>
		public Neocognitron runTrainingSet(int loops)
		{
			Neocognitron output = new Neocognitron(new NeocognitronStructure());

			for (int n = 0; n < loops; n++)
			{
				for (int n2 = 0; n2 < inputs.Count; n2++)
				{
					output.propagate(inputs[n2], true);
					//Console.Write(".");
				}
				//Console.Write("*");
			}
			return output;
		}

		public Neocognitron runTrainingSet2(int loops, Neocognitron output)
		{
			//Neocognitron output = new Neocognitron(new NeocognitronStructure());

			for (int n = 0; n < loops; n++)
			{
				for (int n2 = 0; n2 < inputs.Count; n2++)
				{
					output.propagate(inputs[n2], true);
					//Console.Write(".");
				}
				//Console.Write("*");
			}
			return output;
		}

		/// <summary>
		/// Определите, прошло ли обучение успешно. Неокогнитрон выдаёт ошибку, если два символа возвращают одинаковый результат 
		/// или если нет выходных данных для какого-либо символа.
		/// </summary>
		public int verifyTraining(Neocognitron n)
		{
			List<int> outLoc = new List<int>();
			int output;
			for (int i = 0; i < inputs.Count; i++)
			{
				output = n.propagate(inputs[i], false);

				// If output is already been used, or there is no output
				if (output == -1)
				{
					return 0;
				}
				if (outLoc.Contains(output))
				{
					return outLoc.Count;
				}
				outLoc.Add(output);
			}
			return -1;
		}

		public Neocognitron getNeocognitron(int trainingLoops)
		{
			Neocognitron output = null;

			int count = 0;
			double errorRate = 100;
			double errorRate2 = 100;
			double bestError = 100;
			do
			{       // While the error rate is not zero
				int isAllOk = 0;
				//output = new Neocognitron(new NeocognitronStructure());
				do
				{   // and while the training is not successful
					//output = runTrainingSet2(trainingLoops, output);
					output = runTrainingSet(trainingLoops);

					isAllOk = verifyTraining(output);
					errorRate2 = verifyNeocognitron(output, testInputs, false);

					count++;
					Console.WriteLine("Loop: " + count.ToString() + "\tBest: " + bestError.ToString("F4") + 
						"\tCurrent: " + errorRate.ToString("F4") + "\t" + errorRate2.ToString("F4") +
						"\tCount: " + isAllOk);
				} while (isAllOk != -1 || errorRate2 > bestError);

				errorRate = verifyNeocognitron(output, testInputs, false);
				if (errorRate < bestError)
				{
					//Neocognitron.SaveNeocognitron(output, neoFile);
					bestError = errorRate;
				}
			} while (errorRate != 0);

			return output;
		}

		public float verifyNeocognitron(Neocognitron n, List<float[][]> t, bool verbose)
		{

			float output = 0;

			int trainingOutput, testOutput;
			if (verbose)
			{
				Console.WriteLine("Training vs Test");
			}

			for (int i = 0; i < t.Count; i++)
			{
				trainingOutput = n.propagate(inputs[i % inputs.Count], false);
				testOutput = n.propagate(t[i], false);

				if (verbose)
				{
					Console.WriteLine(trainingOutput.ToString() + "\t" + testOutput.ToString());
				}

				if (trainingOutput != testOutput)
					output++;
			}
			return output;
		}

	}
}

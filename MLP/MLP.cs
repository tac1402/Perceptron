
using MLP;
using System.Text;
using System.Collections.Generic;

using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.utils.data;


namespace Tac.Perceptron
{
	public class MLP
	{
		private int SCount; // Количество сенсоров
		private int ACount; // Количество ассоциаций
		private int RCount; // Количество реакций
		private int HCount; // Количество примеров

		public Dictionary<int, BitBlock> LearnedStimuls; // Обучающие стимулы из обучающей выборки
		public Dictionary<int, BitBlock> NecessaryReactions; // Требуемая реакция на каждый стимул из обучающей выборки

		public Dictionary<int, BitBlock> ExaminStimuls; // Стимулы для экзамена
		public Dictionary<int, BitBlock> ExaminReactions; // Требуемая реакция на каждый стимул во время экзамена


		public MLP(int argSCount, int argACount, int argRCount, int argHCount)
		{
			ACount = argACount;
			SCount = argSCount;
			RCount = argRCount;
			HCount = argHCount;

			LearnedStimuls = new Dictionary<int, BitBlock>();
			NecessaryReactions = new Dictionary<int, BitBlock>();

			ExaminStimuls = new Dictionary<int, BitBlock>();
			ExaminReactions = new Dictionary<int, BitBlock>();
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
			NecessaryReactions.Add(argStimulNumber, argReaction);
		}

		public void JoinEStimul(int argStimulNumber, BitBlock argPerception, BitBlock argReaction)
		{
			// Запомним обучающий стимул
			ExaminStimuls.Add(argStimulNumber, argPerception);

			// Запомним какая реакция должна быть на этот пример
			ExaminReactions.Add(argStimulNumber, argReaction);
		}

		MNIST_Network model;

		public void Learned()
		{
			// Конвертируем словари в тензоры
			Tensor x_train = ConvertToTensor(LearnedStimuls, SCount);
			Tensor y_train = ConvertToTensor(NecessaryReactions, RCount);

			// Создаем экземпляр набора данных
			var train_dataset = new ParityDataset(x_train, y_train);

			// Создание загрузчика данных
			int batch_size = 32;
			var train_loader = new DataLoader(train_dataset, batch_size, shuffle: false);

			// Создание модели, функции потерь и оптимизатора
			model = new MNIST_Network(SCount, ACount, RCount);

			//var criterion = BCELoss();
			var criterion = MSELoss();
			var optimizer = new Adam(model.parameters(), 0.0001);

			// Обучение модели
			int num_epochs = 100000;
			for (int epoch = 0; epoch < num_epochs; epoch++)
			{
				model.train();

				double train_loss = 0.0;
				long correct = 0;
				long correct10 = 0;

				// Проход по всем батчам
				foreach (var batch in train_loader)
				{
					// Получение признаков и меток из батча
					var batch_X = batch["features"];
					var batch_y = batch["labels"];

					using (var d = torch.NewDisposeScope())
					{
						// Прямой проход
						var outputs = model.forward(batch_X);

						// Вычисление потерь
						var loss = criterion.forward(outputs, batch_y);

						// Обратный проход и оптимизация
						optimizer.zero_grad();
						loss.backward();
						optimizer.step();

						train_loss += loss.item<float>();

						// Вычисление точности
						var predictions = (outputs > 0.5).to(torch.float32);
						//correct = (predictions == batch_y).sum().item<long>();

						// Получаем булев тензор, где каждый элемент указывает на совпадение
						var elementwise_correct = predictions == batch_y;

						// Проверяем, что все элементы в каждой строке совпали (по измерению 1)
						var all_correct_in_row = elementwise_correct.all(dim: 1);

						// Суммируем количество строк, где все элементы совпали
						correct += all_correct_in_row.sum().item<long>();

					}
				}

				// Вывод статистики
				string output = epoch.ToString() + " - " + (HCount - correct).ToString() +  "\tloss = " + train_loss.ToString();
				Console.WriteLine(output);
				File.AppendAllText("Error_" + SCount.ToString() + "x" + ACount.ToString() + ".txt", output + "\n");

				// Прерывание обучения при достижении 100% точности
				if (correct >= HCount - 150)
				{
					break;
				}
			}


		}

		public void Examin(int argECount)
		{
			// Конвертируем экзаменационные данные в тензоры
			Tensor x_exam = ConvertToTensor(ExaminStimuls, SCount);
			Tensor y_exam = ConvertToTensor(ExaminReactions, RCount);

			// Создаем экземпляр набора данных для экзамена
			var exam_dataset = new ParityDataset(x_exam, y_exam);

			// Создание загрузчика данных
			int batch_size = 32;
			var exam_loader = new DataLoader(exam_dataset, batch_size, shuffle: false);

			// Переводим модель в режим оценки
			model.eval();

			long total_correct = 0;
			long total_samples = 0;

			// Отключаем вычисление градиентов для ускорения и экономии памяти
			using (torch.no_grad())
			{
				foreach (var batch in exam_loader)
				{
					var batch_X = batch["features"];
					var batch_y = batch["labels"];

					using (var d = torch.NewDisposeScope())
					{
						// Прямой проход
						var outputs = model.forward(batch_X);

						// Вычисление предсказаний
						var predictions = (outputs > 0.5).to(torch.float32);

						// Подсчет полностью правильных строк в батче
						var elementwise_correct = predictions == batch_y;
						var all_correct_in_row = elementwise_correct.all(dim: 1);
						long batch_correct = all_correct_in_row.sum().item<long>();

						total_correct += batch_correct;
						total_samples += batch_y.shape[0]; // Общее количество примеров в батче
					}
				}
			}

			// Вывод результатов экзамена
			double accuracy = (double)total_correct / total_samples * 100;
			string result = $"Результаты экзамена: {total_correct}/{total_samples} ({accuracy:F2}%)";
			Console.WriteLine(result);
			File.AppendAllText("Exam_Results_" + SCount.ToString() + "x" + ACount.ToString() + ".txt", result + "\n");

			// Дополнительно: можно сохранить детальные предсказания
			SaveDetailedPredictions(x_exam, y_exam, "exam_predictions.txt");

		}

		private void SaveDetailedPredictions(Tensor X_exam, Tensor y_exam, string filename)
		{
			model.eval();

			using (torch.no_grad())
			using (var d = torch.NewDisposeScope())
			{
				var outputs = model.forward(X_exam);
				var predictions = (outputs > 0.5).to(torch.float32);

				// Сравнение предсказаний с истинными значениями
				var correct_mask = (predictions == y_exam).all(dim: 1);

				// Сохраняем детальную информацию
				var sb = new StringBuilder();
				sb.AppendLine("Детальные результаты экзамена:");
				sb.AppendLine("Index\tCorrect\tPredicted\tActual");

				for (int i = 0; i < X_exam.shape[0]; i++)
				{
					bool correct = correct_mask[i].item<bool>();
					string predicted = string.Join("", predictions[i].data<float>().Select(f => f > 0.5 ? "1" : "0"));
					string actual = string.Join("", y_exam[i].data<float>().Select(f => f > 0.5 ? "1" : "0"));

					sb.AppendLine($"{i}\t{correct}\t{predicted}\t{actual}");
				}

				File.WriteAllText(filename, sb.ToString());
			}
		}


		public static torch.Tensor ConvertToTensor(Dictionary<int, BitBlock> dataDict, int argCount)
		{
			float[] dataArray = new float[dataDict.Count * argCount];
			int index = 0;
			foreach (var block in dataDict)
			{
				for (int j = 0; j < argCount; j++)
				{
					dataArray[index++] = block.Value[j] ? 1.0f : 0.0f;
				}
			}

			return torch.tensor(dataArray, torch.float32).reshape(dataDict.Count, argCount);
		}



	}
}

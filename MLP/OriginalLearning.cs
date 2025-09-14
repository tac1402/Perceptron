using System;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.utils.data;

namespace MLP
{
	public class OriginalLearning
	{

		public void Run()
		{
			// Генерация данных
			int num_inputs = 16;
			long num_samples = (long)Math.Pow(2, num_inputs);

			// Создание индексов
			var indices = torch.arange(num_samples, dtype: torch.float32).unsqueeze(1);

			// Создание битовых позиций и периодов
			var bit_positions = torch.arange(num_inputs, dtype: torch.float32);
			var periods = torch.pow(2, bit_positions + 1);

			// Создание входных данных X_train
			var X_train = (indices % periods).ge(periods / 2).to(torch.float32);

			// Создание целевых значений y_train
			var y_train = (torch.sum(X_train, 1) + 1) % 2;
			y_train = y_train.to(torch.float32).unsqueeze(1);

			// Создание экземпляра набора данных
			var train_dataset = new ParityDataset(X_train, y_train);

			// Создание загрузчика данных
			int batch_size = 32;
			var train_loader = new DataLoader(train_dataset, batch_size, shuffle: true);

			// Создание модели, функции потерь и оптимизатора
			var model = new ParityNetwork3(num_inputs);
			var criterion = BCELoss();
			var optimizer = new Adam(model.parameters(), 0.0001);

			// Обучение модели
			int num_epochs = 10000;
			for (int epoch = 0; epoch < num_epochs; epoch++)
			{
				model.train();

				double train_loss = 0.0;
				long correct = 0;

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
						correct += (predictions == batch_y).sum().item<long>();
					}
				}

				// Вывод статистики
				var time = DateTime.Now;
				Console.WriteLine($"{time} epoch={epoch}, train_loss={train_loss}, accuracy={correct}");

				// Прерывание обучения при достижении 100% точности
				if (correct == num_samples)
				{
					break;
				}
			}
		}

	}
}

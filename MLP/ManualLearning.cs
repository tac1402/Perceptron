using System;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.utils.data;


namespace MLP
{
	public class ManualLearning
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
			var train_loader = new DataLoader(train_dataset, batch_size/*, shuffle: true*/);

			// Создание модели, функции потерь и оптимизатора
			double learningRate = 0.01;
			var model = new ParityNetwork(num_inputs);
			var criterion = MSELoss();
			//var criterion = BCELoss();
			//var optimizer = new Adam(model.parameters(), 0.0001);

			// Обучение модели
			int num_epochs = 100000;
			double oldtrain_loss = 0.0;
			double train_loss = 0.0;
			for (int epoch = 0; epoch < num_epochs; epoch++)
			{
				model.train();

				oldtrain_loss = train_loss;
				train_loss = 0.0;

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

						// Используем нашу ручную реализацию BCELoss
						//var loss = ManualLossFunctions.ManualBCELoss(outputs, batch_y);
						


						// Обратный проход и оптимизация
						//optimizer.zero_grad();
						loss.backward();
						//optimizer.step();


						// Ручное обновление весов с SGD
						using (torch.no_grad())
						{
							foreach (var param in model.parameters())
							{
								if (param.grad is not null)
								{
									param.set_(param - learningRate * param.grad);
									param.grad.zero_();
								}
							}
						}

						train_loss += loss.item<float>();

						// Вычисление точности
						var predictions = (outputs > 0.5).to(torch.float32);
						correct += (predictions == batch_y).sum().item<long>();
					}
				}

				// Вывод статистики
				//if (epoch % 100 == 0)
				Console.WriteLine($"epoch={epoch}, train_loss={train_loss}, accuracy={num_samples - correct}");

				/*if (oldtrain_loss < train_loss)
				{
					learningRate -= learningRate * 0.01;
				}*/

				// Прерывание обучения при достижении 100% точности
				if (correct == num_samples)
				{
					break;
				}
			}
		}

	}


	public static class ManualLossFunctions
	{
		/*public static Tensor ManualBCELoss(Tensor outputs, Tensor targets, double epsilon = 1e-12)
		{
			using (var d = torch.NewDisposeScope())
			{
				// Ограничиваем значения для численной стабильности
				var clamped_outputs = outputs.clamp(epsilon, 1.0 - epsilon);

				// Вычисляем потерю для каждого элемента
				var term1 = targets * clamped_outputs.log();
				var term2 = (1 - targets) * (1 - clamped_outputs).log();
				var loss_per_element = -(term1 + term2);

				// Возвращаем среднее значение по батчу
				return loss_per_element.mean().MoveToOuterDisposeScope();
			}
		}*/
		public static Tensor ManualBCELoss(Tensor logits, Tensor targets)
		{
			using (var d = torch.NewDisposeScope())
			{
				// Стабильная реализация BCEWithLogitsLoss
				// Используем формулу: max(logits, 0) - logits * targets + log(1 + exp(-abs(logits)))

				// Вычисляем стабильную версию
				var max_logits = torch.maximum(logits, torch.tensor(0.0));
				var abs_logits = torch.abs(logits);
				var log_exp = torch.log(1 + torch.exp(-abs_logits));

				var loss_per_element = max_logits - logits * targets + log_exp;
				var loss = loss_per_element.mean();

				return loss.MoveToOuterDisposeScope();
			}
		}

	}
}

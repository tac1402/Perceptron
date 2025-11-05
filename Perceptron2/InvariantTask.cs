

using Tac.Perceptron;

public class InvariantTask
{
	public void Run()
	{
		// Подготовка данных
		double[][,] trainingData = new double[4][,]
		{
			new double[,] { // Квадрат 1
                {1,1,0,0},
				{1,1,0,0},
				{0,0,0,0},
				{0,0,0,0}
			},
			new double[,] { // Квадрат 2
                {0,0,0,0},
				{0,0,0,0},
				{0,0,1,1},
				{0,0,1,1}
			},
			new double[,] { // Треугольник 1
                {0,0,1,0},
				{0,0,1,1},
				{0,0,0,0},
				{0,0,0,0}
			},
			new double[,] { // Треугольник 2
                {0,0,0,0},
				{0,0,0,0},
				{1,0,0,0},
				{1,1,0,0}
			}
		};

		int[] labels = { 0, 0, 1, 1 }; // 0 - квадрат, 1 - треугольник

		// Создаем и обучаем сеть
		SimpleCNN cnn = new SimpleCNN();
		cnn.Train(trainingData, labels, epochs: 1000, learningRate: 0.01);

		// Тестируем
		Console.WriteLine("\nTesting:");
		for (int i = 0; i < trainingData.Length; i++)
		{
			int prediction = cnn.Predict(trainingData[i]);
			string predictedClass = prediction == 0 ? "Square" : "Triangle";
			string actualClass = labels[i] == 0 ? "Square" : "Triangle";
			Console.WriteLine($"Sample {i + 1}: Predicted {predictedClass}, Actual {actualClass}");
		}
	}
}

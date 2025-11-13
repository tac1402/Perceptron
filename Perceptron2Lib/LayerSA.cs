// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


namespace Tac.Perceptron
{
	public class LayerSA
	{
		public int SCount; // Количество сенсоров
		public int ACount; // Количество ассоциаций

		public float[] AField;

		private PerceptronAA AB;

		private string Name = "";
		private string Arh
		{
			get { return "SA_" + SCount.ToString() + "x" + ACount.ToString(); }
		}
		private bool isLoaded = false;


		public LayerSA(int argSCount, int argACount, string argName = "")
		{
			ACount = argACount;
			SCount = argSCount;
			Name = argName;

			AB = new PerceptronAA(SCount, ACount, 0);
			LoadWeights();
		}

		public string WeightFileName()
		{
			string weightFileName = "weight" + Arh;
			if (Name != "")
			{
				weightFileName += "_" + Name;
			}
			weightFileName += ".bin";
			return weightFileName;
		}

		public void LoadWeights()
		{
			string weightFileName = WeightFileName();
			if (File.Exists(weightFileName))
			{
				int ret = AB.LoadWeights(weightFileName);
				if (ret != 0)
				{
					isLoaded = true;
				}
			}
		}

		/// <summary>
		/// Активация S-A слоя
		/// </summary>
		public void SActivation(float[] SensorsField, int startA = 0, int endA = -1)
		{
			AField = AB.AActivation(SensorsField, startA, endA);
		}

		public void SaveWeights()
		{
			AB.SaveWeights(WeightFileName());
		}

	}
}

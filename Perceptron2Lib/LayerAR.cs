// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


namespace Tac.Perceptron
{
	public class LayerAR
	{
		public int ACount; // Количество ассоциаций
		public int RCount; // Количество реакций

		public float[] RField;

		private PerceptronAA AB;

		private string Name = "";
		private string Arh
		{
			get { return "AR_" + ACount.ToString() + "x" + RCount.ToString(); }
		}
		private bool isLoaded = false;

		public LayerAR(int argACount, int argRCount, string argName = "")
		{
			ACount = argACount;
			RCount = argRCount;
			Name = argName;

			AB = new PerceptronAA(0, ACount, RCount);
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

		public void RActivation(float[] AField)
		{
			RField = AB.RActivation(AField);
		}

		public void LearnedStimulAR(float[] AField, float[] rError)
		{
			AB.LearnedStimulAR(rError, AField);
		}

		public void SaveWeights()
		{ 
			AB.SaveWeights(WeightFileName());
		}

	}
}

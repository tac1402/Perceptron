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

		public LayerAR(int argACount, int argRCount, string argName = "")
		{
			ACount = argACount;
			RCount = argRCount;
			Name = argName;

			AB = new PerceptronAA(0, ACount, RCount);
		}

		public void RActivation(float[] AField)
		{
			RField = AB.RActivation(AField);
		}

		public void LearnedStimulAR(float[] AField, float[] rError)
		{
			AB.LearnedStimulAR(rError, AField);
		}


	}
}

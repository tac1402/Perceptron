// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


namespace Tac.Perceptron
{
	public class LayerSA
	{
		public int SCount; // Количество сенсоров
		public int ACount; // Количество ассоциаций
		public int TSet = 1;

		public float[] AField;
		public float[][] AFieldT;

		private PerceptronAA AB;

		private string Name = "";
		private string Arh
		{
			get { return "SA_" + SCount.ToString() + "x" + ACount.ToString(); }
		}
		private bool isLoaded = false;


		public LayerSA(int argSCount, int argACount, string argName = "", int argTSet = 1)
		{
			ACount = argACount;
			SCount = argSCount;
			TSet = argTSet;
			Name = argName;

			AFieldT = new float[TSet][];

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
		public void SActivation(float[] SensorsField, int startA = 0, int endA = -1, int t = 0)
		{
			AFieldT[t] = AB.AActivation(SensorsField, startA, endA);

			if (t == 0)
			{
				AField = AFieldT[0];
			}
			else if (t > 0)
			{
				AField = AFieldSum(AField, AFieldT[t]);
			}
		}

		public void SaveWeights()
		{
			AB.SaveWeights(WeightFileName());
		}


		public static float[] AFieldSum(float[] AField1, float[] AField2)
		{
			float[] AField = new float[AField1.Length + AField2.Length];
			Array.Copy(AField1, 0, AField, 0, AField1.Length);
			Array.Copy(AField2, 0, AField, AField1.Length, AField2.Length);
			return AField;
		}

		public static float[] AFieldMax(float[] AField1, float[] AField2)
		{
			for (int i = 0; i < AField1.Length; i++)
			{
				if (AField2[i] > AField1[i])
				{ 
					AField1[i] = AField2[i];
				}
			}
			return AField1;
		}

	}
}

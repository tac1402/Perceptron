using System;
using System.Collections.Generic;
using System.Linq;


namespace BackProp
{

	public class Adam
	{
		private Dictionary<int, NArray> parameters;
		private Dictionary<int, NArray> m; // First moment vector
		private Dictionary<int, NArray> v; // Second moment vector
		private Dictionary<int, NArray> gradients; // Gradients storage

		public double LearningRate;
		public int Precision;

		private double beta1;
		private double beta2;
		private double epsilon;
		public int TimeStep;

		public dynamic this[int i, int j]
		{
			get { return gradients[i][j]; }
			set { gradients[i][j] = value; }
		}

		public dynamic this[int j]
		{
			get { return gradients[0][j]; }
			set { gradients[0][j] = value; }
		}

		public Adam(NArray argParameters, int argPrecision,
						float argLearningRate = 0.001f, float argBeta1 = 0.9f, float argBeta2 = 0.999f, float argEpsilon = 1e-8f)
						: this(argLearningRate, argBeta1, argBeta2, argEpsilon, argPrecision)
		{
			parameters = new Dictionary<int, NArray>();
			parameters.Add(0, argParameters);

			m = new Dictionary<int, NArray>();
			v = new Dictionary<int, NArray>();
			gradients = new Dictionary<int, NArray>();
			m.Add(0, new NArray(argParameters.Count, Precision));
			v.Add(0, new NArray(argParameters.Count, Precision));
			gradients.Add(0, new NArray(argParameters.Count, Precision));
		}

		public Adam(Dictionary<int, NArray> argParameters, int argPrecision,
						float argLearningRate = 0.001f, float argBeta1 = 0.9f, float argBeta2 = 0.999f, float argEpsilon = 1e-8f)
						: this(argLearningRate, argBeta1, argBeta2, argEpsilon, argPrecision)
		{
			parameters = argParameters;

			m = new Dictionary<int, NArray>();
			v = new Dictionary<int, NArray>();
			gradients = new Dictionary<int, NArray>();
			foreach (var parameter in parameters)
			{
				m.Add(parameter.Key, new NArray(parameter.Value.Count, Precision));
				v.Add(parameter.Key, new NArray(parameter.Value.Count, Precision));
				gradients.Add(parameter.Key, new NArray(parameter.Value.Count, Precision));
			}
		}

		private Adam(float argLearningRate, float argBeta1, float argBeta2, float argEpsilon, int argPrecision)
		{
			LearningRate = argLearningRate;
			beta1 = argBeta1;
			beta2 = argBeta2;
			epsilon = argEpsilon;
			Precision = argPrecision;
			TimeStep = 0;
		}

		public bool isAdam = false;

		public void Update()
		{
			if (isAdam)
			{
				TimeStep++;
			}

			foreach (var parameter in parameters)
			{
				int i = parameter.Key;
				for (int j = 0; j < parameter.Value.Count; j++)
				{
					// Update parameters
					if (isAdam == false)
					{
						parameters[i][j] -= LearningRate * gradients[i][j];
					}
					else
					{
						// Update first moment estimate (линейная интерполяция между grad и 1 - m[i][j])
						m[i][j] = beta1 * m[i][j] + (1 - beta1) * gradients[i][j];

						// Update second moment estimate
						v[i][j] = beta2 * v[i][j] + (1 - beta2) * gradients[i][j] * gradients[i][j];

						// Compute bias-corrected first moment estimate
						//double mHat = m[i][j] / (1 - Math.Pow(beta1, TimeStep));

						// Compute bias-corrected second moment estimate
						//double vHat = v[i][j] / (1 - Math.Pow(beta2, TimeStep));

						// Compute bias correction factors first moment estimate
						double biasCorrection1 = 1 - Math.Pow(beta1, TimeStep);
						// Compute bias correction factors second moment estimate
						double biasCorrection2 = 1 - Math.Pow(beta2, TimeStep);

						// Compute step size (learning rate with bias correction)
						double stepSize = LearningRate / biasCorrection1;

						// Compute denominator:
						double denom = (Math.Sqrt(v[i][j]) / Math.Sqrt(biasCorrection2)) + epsilon;

						// Update parameters
						parameters[i][j] -= (float)(stepSize * m[i][j] / denom);


						//parameters[i][j] -= (float)(LearningRate * mHat / (Math.Sqrt(vHat + epsilon) ));
					}


					// Reset gradient
					gradients[i][j] = 0;
				}
			}
		}
	}

}

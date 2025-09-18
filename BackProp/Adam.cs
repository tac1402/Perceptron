using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackProp
{

	public class Adam
	{
		private Dictionary<int, float[]> parameters;
		private Dictionary<int, float[]> m; // First moment vector
		private Dictionary<int, float[]> v; // Second moment vector
		private Dictionary<int, float[]> gradients; // Gradients storage

		public float LearningRate;
		private float beta1;
		private float beta2;
		private float epsilon;
		public int TimeStep;

		public float this[int i, int j]
		{
			get { return gradients[i][j]; }
			set { gradients[i][j] = value; }
		}

		public float this[int j]
		{
			get { return gradients[0][j]; }
			set { gradients[0][j] = value; }
		}

		public Adam(float[] argParameters,
						float argLearningRate = 0.001f, float argBeta1 = 0.9f, float argBeta2 = 0.999f, float argEpsilon = 1e-8f)
						: this(argLearningRate, argBeta1, argBeta2, argEpsilon)
		{
			parameters = new Dictionary<int, float[]>();
			parameters.Add(0, argParameters);

			m = new Dictionary<int, float[]>();
			v = new Dictionary<int, float[]>();
			gradients = new Dictionary<int, float[]>();
			m.Add(0, new float[argParameters.Length]);
			v.Add(0, new float[argParameters.Length]);
			gradients.Add(0, new float[argParameters.Length]);
		}

		public Adam(Dictionary<int, float[]> argParameters,
						float argLearningRate = 0.001f, float argBeta1 = 0.9f, float argBeta2 = 0.999f, float argEpsilon = 1e-8f)
						: this(argLearningRate, argBeta1, argBeta2, argEpsilon)
		{
			parameters = argParameters;

			m = new Dictionary<int, float[]>();
			v = new Dictionary<int, float[]>();
			gradients = new Dictionary<int, float[]>();
			foreach (var parameter in parameters)
			{
				m.Add(parameter.Key, new float[parameter.Value.Length]);
				v.Add(parameter.Key, new float[parameter.Value.Length]);
				gradients.Add(parameter.Key, new float[parameter.Value.Length]);
			}
		}

		private Adam(float argLearningRate, float argBeta1, float argBeta2, float argEpsilon)
		{
			LearningRate = argLearningRate;
			beta1 = argBeta1;
			beta2 = argBeta2;
			epsilon = argEpsilon;
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
				for (int j = 0; j < parameter.Value.Length; j++)
				{
					// Update parameters
					if (isAdam == false)
					{
						parameters[i][j] -= LearningRate * gradients[i][j];
					}
					else
					{
						// Update first moment estimate
						m[i][j] = beta1 * m[i][j] + (1 - beta1) * gradients[i][j];

						// Update second moment estimate
						v[i][j] = beta2 * v[i][j] + (1 - beta2) * (float)Math.Pow(gradients[i][j], 2);

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

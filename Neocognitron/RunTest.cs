using System;
using System.Collections.Generic;

using System.Drawing;

namespace Neocognitron
{
	/// <summary>
	/// 
	/// </summary>
	public class RunTest
	{

		public void TrainerTest()
		{

			string[] files = new string[5];
			files[0] = "data/0_00.bmp";
			files[1] = "data/1_00.bmp";
			files[2] = "data/2_00.bmp";
			files[3] = "data/3_00.bmp";
			files[4] = "data/4_00.bmp";

			List<double[][]> inputs = new List<double[][]>();
			for (int i = 0; i < files.Length; i++)
			{
				inputs.Add(readImage(files[i]));
			}

			files = new string[15];
			files[0] = "data/0_01.bmp";
			files[1] = "data/1_01.bmp";
			files[2] = "data/2_01.bmp";
			files[3] = "data/3_01.bmp";
			files[4] = "data/4_01.bmp";
			files[5] = "data/0_02.bmp";
			files[6] = "data/1_02.bmp";
			files[7] = "data/2_02.bmp";
			files[8] = "data/3_02.bmp";
			files[9] = "data/4_02.bmp";
			files[10] = "data/0_03.bmp";
			files[11] = "data/1_03.bmp";
			files[12] = "data/2_03.bmp";
			files[13] = "data/3_03.bmp";
			files[14] = "data/4_03.bmp";

			List<double[][]> testInputs = new List<double[][]>();
			for (int i = 0; i < files.Length; i++)
			{
				testInputs.Add(readImage(files[i]));
			}

			NeocognitronTrainer trainer = new NeocognitronTrainer(inputs, testInputs);

			//(int)Math.Round(Math.random() * 15 + 5)
			Neocognitron n = trainer.getNeocognitron(5);

			//Neocognitron n = trainer.runTrainingSet(10);
			//trainer.verifyTraining(n);

			trainer.verifyNeocognitron(n, testInputs, false);
		}


		public double[][] readImage(string file) 
		{
			using (var bmp = new Bitmap(file))
			{
				double[][] output = new double[bmp.Height][];

				for (int i = 0; i < bmp.Height; i++)
					output[i] = new double[bmp.Width];

				for (int x = 0; x < bmp.Height; x++)
				{
					for (int y = 0; y < bmp.Width; y++)
					{
						Color c = bmp.GetPixel(y, x);
						if ((c.B + c.R + c.G) == 0)
							output[x][y] = 1;
						else
							output[x][y] = 0;
					}
				}

				return output;
			}
		}

	}
}

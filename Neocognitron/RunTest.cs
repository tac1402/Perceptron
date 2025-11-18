using System.Drawing;

using Tac.Neocognitron;

public class RunTest
{

	string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	public void TrainerTest()
	{

		string[] files = new string[5];
		files[0] = "data/0_00.bmp";
		files[1] = "data/1_00.bmp";
		files[2] = "data/2_00.bmp";
		files[3] = "data/3_00.bmp";
		files[4] = "data/4_00.bmp";

		List<float[][]> inputs = new List<float[][]>();
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

		List<float[][]> testInputs = new List<float[][]>();
		for (int i = 0; i < files.Length; i++)
		{
			testInputs.Add(readImage(files[i]));
		}

		NeocognitronTrainer trainer = new NeocognitronTrainer(inputs, testInputs);

		//(int)Math.Round(Math.random() * 15 + 5)
		Neocognitron n = trainer.getNeocognitron(50, 5);

		//Neocognitron n = trainer.runTrainingSet(10);
		//trainer.verifyTraining(n);

		trainer.verifyNeocognitron(n, testInputs, false);
	}

	public void TrainerTest2()
	{
		List<string> files = new List<string>();

		for (int i = 0; i < 10; i++)
		{
			for (int j = 11; j < 21; j++)
			{
				string file = "data3\\Training\\" + Alphabet.Substring(i, 1) + "\\" + "matrix_" + Alphabet.Substring(i, 1) + j.ToString();
				files.Add(file + ".bmp");
			}
		}

		List<float[][]> inputs = new List<float[][]>();
		for (int i = 0; i < files.Count; i++)
		{
			inputs.Add(readImage(files[i]));
		}


		for (int i = 0; i < 10; i++)
		{
			for (int j = 1; j < 21; j++)
			{
				string file = "data3\\Test\\" + Alphabet.Substring(i, 1) + "\\" + "matrix_" + Alphabet.Substring(i, 1) + j.ToString();
				files.Add(file + ".bmp");
			}
		}

		List<float[][]> testInputs = new List<float[][]>();
		for (int i = 0; i < files.Count; i++)
		{
			testInputs.Add(readImage(files[i]));
		}

		NeocognitronTrainer trainer = new NeocognitronTrainer(inputs, testInputs);

		Neocognitron n = trainer.getNeocognitron(50, 10);

		trainer.verifyNeocognitron(n, testInputs, false);

	}

	public void Save()
	{
		for (int i = 0; i < Alphabet.Length; i++)
		{
			for (int j = 1; j < 21; j++)
			{
				//string file = "data3\\Training\\" + Alphabet.Substring(i, 1) + "\\" + "matrix_" + Alphabet.Substring(i, 1) + j.ToString();
				string file = "data3\\Test\\" + Alphabet.Substring(i, 1) + "\\" + "matrix_" + Alphabet.Substring(i, 1) + j.ToString();

				float[][] image = readTextImage(file + ".txt");
				writeImage(image, file + ".bmp");
			}
		}

	}

	public float[][] readImage(string file) 
	{
		using (var bmp = new Bitmap(file))
		{
			float[][] output = new float[bmp.Height][];

			for (int i = 0; i < bmp.Height; i++)
				output[i] = new float[bmp.Width];

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

	public void writeImage(float[][] image, string file)
	{
		int height = image.Length;
		int width = image[0].Length;

		using (var bmp = new Bitmap(width, height))
		{
			for (int x = 0; x < height; x++)
			{
				for (int y = 0; y < width; y++)
				{
					Color color = image[x][y] == 1 ? Color.Black : Color.White;
					bmp.SetPixel(y, x, color);
				}
			}

			bmp.Save(file, System.Drawing.Imaging.ImageFormat.Bmp);
		}
	}

	public float[][] readTextImage(string file)
	{
		string[] lines = File.ReadAllLines(file);
		int height = lines.Length;
		int width = lines[0].Split(' ').Length;

		float[][] output = new float[height][];

		for (int i = 0; i < height; i++)
		{
			output[i] = new float[width];
			string[] values = lines[i].Split(' ');

			for (int j = 0; j < width; j++)
			{
				output[i][j] = float.Parse(values[j]);
			}
		}

		return output;
	}
}


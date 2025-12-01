// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;

using PerceptronWrapper;

namespace Tac.CNN
{

	public class Tensor
	{
		public float[] Data;
		public int[] Dimension;
		public int Length => Data.Length;

		public Tensor(int[] dimension)
		{
			Dimension = dimension;
			Data = new float[CalculateSize(dimension)];
		}

		public Tensor(float[] data, int[] dimension)
		{
			Data = data;
			Dimension = dimension;
		}

		private int CalculateSize(int[] dimension)
		{
			return dimension.Aggregate(1, (a, b) => a * b);
		}

		public float this[params int[] indices]
		{
			get
			{
				int index = GetIndex(indices);
				return Data[index];
			}
			set
			{
				int index = GetIndex(indices);
				Data[index] = value;
			}
		}

		private int GetIndex(int[] indices)
		{
			int index = 0;
			int multiplier = 1;
			for (int i = indices.Length - 1; i >= 0; i--)
			{
				index += indices[i] * multiplier;
				multiplier *= Dimension[i];
			}
			return index;
		}

		public Tensor Clone()
		{
			return new Tensor((float[])Data.Clone(), (int[])Dimension.Clone());
		}

		public void Fill(float argValue)
		{
			for (int i = 0; i < Data.Length; i++)
			{
				Data[i] = argValue;
			}
		}

	}

	public abstract class Layer
	{
		public abstract Tensor Forward(Tensor input);
	}

	public class Conv2d : Layer
	{
		private int inChannels;
		private int outChannels;
		private int windowSize;

		Random rnd = new Random(24);

		private Window[] outputsW; //Весы

		private MaxPool2d maxPool;
		private ReLU relu;

		public Conv2d(int argInChannels, int argOutChannels, int argWinowsSize)
		{
			inChannels = argInChannels;
			outChannels = argOutChannels;
			windowSize = argWinowsSize;

			maxPool = new MaxPool2d(argWindowSize: 2, argStride: 2);
			relu = new ReLU();

			outputsW = new Window[outChannels];

			// Create K outputs, each (size by size)
			for (int i = 0; i < outChannels; i++)
			{
				outputsW[i] = new Window(windowSize);
			}

			InitWeight();

			CreatePointField(28);
			PrecomputeIndices(inPointField, 28, 24, 5);

			maxPool.outPointField = outPointField;
		}

		public void InitWeight()
		{
			// Инициализация весов (Xavier/Glorot)
			/*float scale = (float)Math.Sqrt(2.0 / (inChannels * windowSize * windowSize));
			for (int i = 0; i < outChannels; i++)
			{
				for (int x = 0; x < windowSize; x++)
				{
					for (int y = 0; y < windowSize; y++)
					{
						outputsW[i].Set(x, y, (float)(rnd.NextDouble() * 2 - 1) * scale);
					}
				}
			}*/

			/*sbyte sensorType = 0;
			for (int i = 0; i < outChannels; i++)
			{
				//for (int j = 0; j < inChannels; j++)
				{
					for (int x = 0; x < windowSize; x++)
					{
						for (int y = 0; y < windowSize; y++)
						{
							if (rnd.Next(2) == 0) sensorType = 1; else sensorType = -1;
							//weights[i, j, x, y] = sensorType;
							outputsW[i].Set(x, y, sensorType);
						}
					}
				}
			}*/

		}

		Point2D[] inPointField;
		Point2D[,] outPointField;
		Point2D[,][] outPointField2;

		private int outSize;

		public void CreatePointField(int size)
		{
			int inHeight = size;
			int inWidth = size;

			int outHeight = (inHeight - windowSize + 1);
			int outWidth = (inWidth - windowSize + 1);

			inPointField = new Point2D[inHeight * inWidth];
			for (int ih = 0; ih < inHeight; ih++)
			{
				for (int iw = 0; iw < inWidth; iw++)
				{
					inPointField[ih * inWidth + iw] = new Point2D(iw, ih);
				}
			}
			outPointField = new Point2D[outHeight * outWidth, windowSize * windowSize];

			for (int oh = 0; oh < outHeight; oh++)
			{
				for (int ow = 0; ow < outWidth; ow++)
				{

					for (int kh = 0; kh < windowSize; kh++)
					{
						int ih = oh + kh;
						for (int kw = 0; kw < windowSize; kw++)
						{
							int iw = ow + kw;

							outPointField[oh * outWidth + ow, kh * windowSize + kw] = new Point2D(iw, ih);
						}
					}
				}
			}

			int windowSize2 = 2;
			int stride2 = 2;
			int outHeight2 = outHeight / stride2;
			int outWidth2 = outWidth / stride2;
			outSize = outHeight2;

			outPointField2 = new Point2D[outHeight2 * outWidth2, windowSize2 * windowSize2][];

			for (int oh = 0; oh < outHeight2; oh++)
			{
				int ihStart = oh * stride2;
				for (int ow = 0; ow < outWidth2; ow++)
				{
					int iwStart = ow * stride2;
					float max = float.MinValue;

					// Обработка окна пулинга
					for (int kh = 0; kh < windowSize2; kh++)
					{
						int ih = ihStart + kh;
						for (int kw = 0; kw < windowSize2; kw++)
						{
							int iw = iwStart + kw;
							//float val = input[c, ih, iw];
							//if (val > max) max = val;
							{
								outPointField2[oh * outWidth2 + ow, kh * windowSize2 + kw] = new Point2D[windowSize * windowSize];
								for (int w = 0; w < windowSize * windowSize; w++)
								{
									outPointField2[oh * outWidth2 + ow, kh * windowSize2 + kw][w] = outPointField[ih * outWidth + iw, w];
								}
							}
						}
					}

					int a2 = 1;

					//output[c, oh, ow] = max;
				}
			}


			int a = 1;
		}


		public Tensor FullForward(Tensor input)
		{
			input = Forward(input);
			input = relu.Forward(input);
			input = maxPool.Forward(input);
			return input;
		}

		int[] indices;

		public void PrecomputeIndices(Point2D[] inPointField, int inSize, int outSize, int winSize)
		{
			int totalPoints = winSize * winSize;
			int totalIndices = outSize * outSize * totalPoints;
			indices = new int[totalIndices];

			// Параллельное заполнение для скорости
			for (int oh= 0; oh < outSize; oh++)
			{
				for (int ow = 0; ow < outSize; ow++)
				{
					int baseIndex = (oh * outSize + ow) * totalPoints;

					for (int w = 0; w < totalPoints; w++)
					{
						int x = w % winSize;
						int y = w / winSize;

						Point2D pointIn = inPointField[(oh + y) * outSize + (ow + x)];

						indices[baseIndex + w] = pointIn.Y * inSize + pointIn.X;
					}
				}
			}
		}


		public override Tensor Forward(Tensor input)
		{
			int inChannels = input.Dimension[0];
			int inHeight = input.Dimension[1];
			int inWidth = input.Dimension[2];

			int outHeight = (inHeight - windowSize + 1) ;
			int outWidth = (inWidth - windowSize + 1) ;

			var output = new Tensor(new[] { outChannels, outWidth, outHeight });

			//int oc = 0;
			// Основной параллельный цикл по выходным каналам
			Parallel.For(0, outChannels, oc =>
			{
				outputsW[oc].To1D();
				WinSumWrapper calculator = new WinSumWrapper(input.Data, indices, outputsW[oc].Full_1D, outWidth, inWidth, windowSize);
				for (int oh = 0; oh < outHeight; oh++)
				{
					for (int ow = 0; ow < outWidth; ow++)
					{
						float sum = calculator.Compute(oh, ow);
						// Используем результат...
						output[oc, ow, oh] = sum;
					}
				}

				/*for (int oh = 0; oh < outHeight; oh++)
				{
					for (int ow = 0; ow < outWidth; ow++)
					{
						float sum = 0;

						for (int w = 0; w < windowSize * windowSize; w++)
						{
							int x = w % windowSize;
							int y = w / windowSize;

							Point2D pointIn = inPointField[(oh + y) * outWidth + (ow + x)];

							float val = input[0, pointIn.X, pointIn.Y];
							float weight = outputsW[oc].Full[x, y];
							sum += val * weight;

						}
						output[oc, ow, oh] = sum;
					}
				}*/
			});

			return output;
		}

		float p3 = 0.000001f;
		float correct3 = 0.00001f;

		//float p3 = 0.0000001f;
		//float correct3 = 0.00001f;

		Point2D[][] repWin;

		public void RandomChange(Tensor input, float d)
		{
			//int size = argOutput.Dimension[1];

			float d3 = p3 * d;
			
			// Получить репрезентативные местоположения ячеек из выходных данных
			repWin = getRepresentativeCells(outChannels, outSize, windowSize);

			int a = 1;
			
			for (int oc = 0; oc < outChannels; oc++)
			{
				// Пока есть репрезентативная ячейка, обновляйте веса плоскости.
				//if (repLoc[oc] != null)
				{
					// Get specific representative location
					Point2D[] point = repWin[oc];
					
					//float[,] win = outputWindow[oc][point.X - 2, point.Y - 2, windowSize];

					//for (int ck = 0; ck < inChannels; ck++)
					//{
						for (int y = 0; y < windowSize; y++)
						{
							for (int x = 0; x < windowSize; x++)
							{
								int w = y * windowSize + x;
								float deltaA = input[point[w].X, point[w].Y] * correct3 + correct3;

								if (deltaA > correct3)
								{
									int b = 1;
								}

								float p = (float)rnd.NextDouble();
								if (p < d3)
								{
									outputsW[oc].Add(x, y, deltaA);
								}
							}
						}
					//}
				}
			}
		}

		public void LearnedStimulSA(float[] rError, Tensor argOutput, float[] arWeights)
		{
			int a = 1;

			float[] AField = argOutput.Data;
			float[] AFieldNorm = Normalize(AField);

			for (int oc = 0; oc < outChannels; oc++)
			{
				Point2D[] point = repWin[oc];


				for (int j = 0; j < argOutput.Dimension[1]; j++)
				{

					float[] w = new float[windowSize * windowSize];

					if (argOutput[oc, j] > 0)
					{
						for (int r = 0; r < 10; r++) // RCount
						{
							if (rError[r] != 0 && Math.Sign(arWeights[r * 144 + j]) != Math.Sign(rError[r]))
							{
								for (int i = 0; i < windowSize * windowSize; i++)
								{
									w[i] -= AFieldNorm[j];
								}
							}
						}
					}
					else
					{
						/*for (int r = 0; r < RCount; r++)
						{
							if (Math.Sign(WeightAR[j][r]) == Math.Sign(ReactionError[r]))
							{
								for (int i = 0; i < SCount; i++)
								{
									w[i] += AFieldNorm[j];
								}
							}
						}*/
					}

					for (int i = 0; i < windowSize * windowSize; i++)
					{
						//WeightSA[i][j] += w[i];

						//outputsW[oc].Add(x, y, deltaA);
					}
				}
			}

		}


		/// <summary>
		/// Отбирается одна точка из каждого фильтра с максимальным выходным значением
		/// K - количество фильтров
		/// </summary>
		public Point2D[][] getRepresentativeCells(int K,int size, int wSize)
		{
			Point2D[][] rp = new Point2D[K][];
			float[] maxV = new float[K];

			for (int k = 0; k < K; k++)
			{
				if (rp[k] == null) 
				{ 
					rp[k] = new Point2D[windowSize * windowSize];
					for (int y = 0; y < windowSize; y++)
					{
						for (int x = 0; x < windowSize; x++)
						{
							int w = y * windowSize + x;
							rp[k][w] = new Point2D(x, y);
						}
					}
				}

				for (int n = 0; n < size; n++)
				{
					for (int m = 0; m < size; m++)
					{
						int maxIndex = (int)maxPool.outMax[k, n, m];
						float value = maxPool.output[k, n, m];
						if (value > maxV[k])
						{
							maxV[k] = value;
							rp[k] = outPointField2[n * size + m, maxIndex];
						}
						//Point2D[] win = outPointField2[n *size + m, maxIndex]
					}
				}

				/*for (int n = 0; n < size - wSize + 1; n++)
				{
					for (int m = 0; m < size - wSize + 1; m++)
					{
						float value = argOutput[k, n, m];

						if (value > maxV[k])
						{
							rp[k].X = n;
							rp[k].Y = m;
							maxV[k] = value;
						}
					}
				}*/
			}

			return rp;
		}

		private float[] Normalize(float[] argAField)
		{
			//sum = 0;
			float maxAbs = 0;
			// Находим максимальное по модулю значение
			for (int i = 0; i < argAField.Length; i++)
			{
				float absValue = Math.Abs(argAField[i]);
				if (absValue > maxAbs) maxAbs = absValue;

				//sum += argAField[i];
			}

			// Если все значения нулевые, возвращаем исходный массив
			if (maxAbs == 0)
				return argAField;

			// Нормализуем значения
			float[] normalized = new float[argAField.Length];

			for (int i = 0; i < argAField.Length; i++)
			{
				normalized[i] = argAField[i] / maxAbs;
			}

			return normalized;
		}

	}

	public class MaxPool2d : Layer
	{
		public Point2D[,] outPointField;

		private int windowSize;
		private int stride;

		public MaxPool2d(int argWindowSize, int argStride)
		{
			windowSize = argWindowSize;
			stride = argStride;
		}

		public Tensor output;
		public Tensor outMax;

		public override Tensor Forward(Tensor input)
		{
			int channels = input.Dimension[0];
			int inHeight = input.Dimension[1];
			int inWidth = input.Dimension[2];

			int outHeight = inHeight / stride;
			int outWidth = inWidth / stride;

			output = new Tensor(new[] { channels, outWidth, outHeight });
			outMax = new Tensor(new[] { channels, outWidth, outHeight });

			Parallel.For(0, channels, c =>
			{
				for (int oh = 0; oh < outHeight; oh++)
				{
					int ihStart = oh * stride;
					for (int ow = 0; ow < outWidth; ow++)
					{
						int iwStart = ow * stride;
						float max = float.MinValue;
						int maxIndex = -1;
						int w = 0;

						// Обработка окна пулинга
						for (int kh = 0; kh < windowSize; kh++)
						{
							int ih = ihStart + kh;
							for (int kw = 0; kw < windowSize; kw++)
							{
								int iw = iwStart + kw;

								float val = input[c, iw, ih];
								if (val > max)
								{
									max = val;
									maxIndex = w;
								}
								w++;
							}
						}
						output[c, ow, oh] = max;
						outMax[c, ow, oh] = maxIndex;
					}
				}
			}); 
			
			return output;
		}
	}

	public class ReLU : Layer
	{
		public override Tensor Forward(Tensor input)
		{
			var output = input.Clone();
			for (int i = 0; i < output.Length; i++)
			{
				if (output.Data[i] < 0)
				{
					output.Data[i] = 0;
				}
			}
			return output;
		}
	}

	public class Flatten : Layer
	{
		public override Tensor Forward(Tensor input)
		{
			int newSize = 1;
			for (int i = 1; i < input.Dimension.Length; i++)
			{
				newSize *= input.Dimension[i];
			}

			return new Tensor(input.Data, new[] { input.Dimension[0], newSize });
		}
	}

	/*public class Point2D
	{
		public int X;
		public int Y;
		public Point2D(int x, int y)
		{
			X = x;
			Y = y;
		}
		public static float Distance(float centerX, float centerY, float x, float y)
		{
			float dx = centerX - x;
			float dy = centerY - y;
			return (float)Math.Sqrt(dx * dx + dy * dy);
		}
	}*/

	public class Window
	{
		public float[,] Full;
		public int size;

		public float[] Full_1D;

		public string key(int x, int y) { return x.ToString() + "-" + y.ToString(); }

		public (int x, int y) xy(string key) { return (int.Parse(key.Substring(0, key.IndexOf('-'))), int.Parse(key.Substring(key.IndexOf('-') + 1))); }

		public float[,] this[int wx, int wy, int wSize]
		{
			get
			{
				float[,] win = new float[wSize, wSize];
				for (int y = 0; y < wSize; y++)
				{
					for (int x = 0; x < wSize; x++)
					{
						if (wx + x >= 0 && wx + x < size && wy + y >= 0 && wy + y < size)
						{
							win[x, y] = Full[wx + x, wy + y];
						}
					}
				}
				return win;
			}
		}

		public float[] this[int wx, int wy, int wSize, bool plane]
		{
			get
			{
				float[] win = new float[wSize * wSize];
				int index = 0;
				for (int x = 0; x < wSize; x++)
				{
					for (int y = 0; y < wSize; y++)
					{
						if (wx + x < size && wy + y < size)
						{
							win[index] = Full[wx + x, wy + y];
						}
						index++;
					}
				}
				return win;
			}
		}



		public Window(int argSize)
		{
			size = argSize;
			Full = new float[size, size];
		}

		public void Set(float[][] argMatrix)
		{
			int size = argMatrix.Length;
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					Full[x, y] = argMatrix[x][y];
				}
			}
		}

		public void Set(int x, int y, float value)
		{
			Full[x, y] = value;
		}

		public void Add(int x, int y, float value)
		{
			Full[x, y] += value;
		}

		public void To1D()
		{
			int height = Full.GetLength(0);
			int width = Full.GetLength(1);
			Full_1D = new float[height * width];

			Buffer.BlockCopy(Full, 0, Full_1D, 0, Full_1D.Length * sizeof(float));
		}

	}


	public class CNN
	{
		public int WinowsSize;

		private Conv2d conv1;
		private Flatten flatten;

		public CNN(int argFCount, int argWinowsSize)
		{
			WinowsSize = argWinowsSize;

			// conv1 = 28 [-4] -> 24 [/2] -> 12 
			// conv2 = 12 [-4] -> 8 [/2] -> 2

			conv1 = new Conv2d(argInChannels: 1, argOutChannels: argFCount, argWinowsSize: WinowsSize);
			flatten = new Flatten();
		}

		//Tensor convOutput;
		Tensor output;

		public Tensor forward(Tensor input)
		{
			input = conv1.FullForward(input);
			//convOutput = input.Clone();
			output = flatten.Forward(input);
			return output;
		}

		public void RandomChange(Tensor input, float d)
		{
			conv1.RandomChange(input, d);
		}

		public void LearnedStimulSA(float[] rError, float[] arWeights)
		{
			conv1.LearnedStimulSA(rError, output, arWeights);
		}

	}

}

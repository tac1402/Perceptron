using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace ViewMNIST
{
	public partial class PictureView : Form
	{

		Bitmap tmpBitmap;

		private int ImageWidth;
		private int ImageHeight;
		bool onlyGray = true;
		bool isMNIST = false;
		bool isCIFAR10 = false;

		public PictureView()
		{
			InitializeComponent();
			//ReadMNIST_E();
			ReadMNIST_L();
			//ReadCIFAR();
		}

		public void ReadMNIST_L()
		{
			ImageWidth = 28;
			ImageHeight = 28;
			onlyGray = true;
			isMNIST = true;
			isCIFAR10 = false;

			int totalPixels = ImageWidth * ImageHeight;
			tmpPicture = new MyColor[60000][];
			for (int i = 0; i < 60000; i++)
			{
				tmpPicture[i] = new MyColor[totalPixels];
			}

			//FashionMNIST
			//ReadPicture("train-images-idx3-ubyte", 60000);
			//ReadLabels("train-labels-idx1-ubyte", 60000);
			//MNIST
			ReadPicture("MNIST\\train-images.idx3-ubyte", 60000, 0);
			ReadLabels("MNIST\\train-labels.idx1-ubyte", 60000);
		}

		public void ReadMNIST_E()
		{
			ImageWidth = 28;
			ImageHeight = 28;
			onlyGray = true;
			isMNIST = true;
			isCIFAR10 = false;

			int totalPixels = ImageWidth * ImageHeight;
			tmpPicture = new MyColor[10000][];
			for (int i = 0; i < 10000; i++)
			{
				tmpPicture[i] = new MyColor[totalPixels];
			}

			//FashionMNIST
			ReadPicture("t10k-images-idx3-ubyte", 10000, 0);
			ReadLabels("t10k-labels-idx1-ubyte", 10000);
			//MNIST
			//ReadPicture("t10k-images.idx3-ubyte", 10000);
			//ReadLabels("t10k-labels.idx1-ubyte", 10000);
		}

		public void ReadCIFAR()
		{
			ImageWidth = 32;
			ImageHeight = 32;
			onlyGray = false;
			isCIFAR10 = true;

			int totalPixels = ImageWidth * ImageHeight;

			tmpPicture = new MyColor[60000][];
			for (int i = 0; i < 60000; i++)
			{
				tmpPicture[i] = new MyColor[totalPixels];
			}
			tmplLabelsCIFAR = new byte[60000];

			ReadPicture("CIFAR10\\data_batch_1.bin", 10000, 0);
			ReadPicture("CIFAR10\\data_batch_2.bin", 10000, 10000);
			ReadPicture("CIFAR10\\data_batch_3.bin", 10000, 20000);
			ReadPicture("CIFAR10\\data_batch_4.bin", 10000, 30000);
			ReadPicture("CIFAR10\\data_batch_5.bin", 10000, 40000);
			ReadPicture("CIFAR10\\test_batch.bin", 10000, 50000);
		}


		MyColor[][] tmpPicture;
		MyColor[][] tmpPictureOut;
		string[] tmpLabels = new string [60000];
		byte[] tmplLabelsCIFAR;

		int PictureNumber=0;

		public void ReadPicture(string argFileName, int argCount, int argAdd)
		{
			int totalPixels = ImageWidth * ImageHeight;

			FileStream fs = new FileStream(argFileName, FileMode.Open, FileAccess.Read);
			BinaryReader r = new BinaryReader(fs);

			if (isMNIST == true)
			{
				for (int i = 0; i < 4; i++)
				{
					r.ReadInt32();
				}
			}

			for (int n = 0; n < argCount; n++)
			{
				if (isCIFAR10 == true)
				{
					tmplLabelsCIFAR[n + argAdd] = r.ReadByte();
				}

				for (int i = 0; i < totalPixels; i++)
				{
					tmpPicture[n + argAdd][i] = new MyColor();
					tmpPicture[n + argAdd][i].R = r.ReadByte();
				}
				if (onlyGray == false)
				{
					for (int i = 0; i < totalPixels; i++)
					{
						tmpPicture[n + argAdd][i].G = r.ReadByte();
					}
					for (int i = 0; i < totalPixels; i++)
					{
						tmpPicture[n + argAdd][i].B = r.ReadByte();
					}
				}
			}
		}

		public byte ToGray(MyColor color)
		{
			return (byte)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
		}
		public byte ToGray2(MyColor color)
		{
			return (byte)((color.R + color.G + color.B)/3);
		}

		public void ReadLabels(string argFileName, int argCount)
		{
			FileStream fs = new FileStream(argFileName, FileMode.Open, FileAccess.Read);

			BinaryReader r = new BinaryReader(fs);
			for (int i = 0; i < 2; i++)
			{
				r.ReadInt32();
			}
			for (int n = 0; n < argCount; n++)
			{
				tmpLabels[n] = r.ReadByte().ToString();
			}
		}

		public void ImageDraw(bool argView = true)
		{
			if (argView)
			{
				PictureNumber = int.Parse(PictureNumberTxt.Text);
			}

			Bitmap newBitmap = new Bitmap(ImageWidth, ImageHeight);

			int g = 0;
			for (int y = 0; y < ImageHeight; y++)
			{
				for (int x = 0; x < ImageWidth; x++)
				{
					MyColor c = tmpPicture[PictureNumber][g];
					newBitmap.SetPixel(x, y, Color.FromArgb(c.R, c.G, c.B));

					g++;
				}
			}

			//Bitmap newBitmap2 = ExposureCorrection.AdjustExposure(newBitmap, 1.0f, -0.3f);

			//MyColor[] image0 = BitmapConverter.FromBitmap(newBitmap2, ImageWidth, ImageHeight);

			//MyColor[] image2 = Shift(tmpPicture[PictureNumber], -1, -1);
			//CheckImage(image0, newBitmap2);

			//Bitmap newBitmap3 = new Bitmap(ImageWidth, ImageHeight);

			//ApplyImportanceMask(image0, mask, ImageWidth, ImageHeight, 0.5f);

			/*g = 0;
			for (int y = 0; y < ImageHeight; y++)
			{
				for (int x = 0; x < ImageWidth; x++)
				{
					MyColor c = image0[g];
					newBitmap3.SetPixel(x, y, Color.FromArgb(c.R, c.G, c.B));
					g++;
				}
			}

			// Автоматическая коррекция
			//Bitmap newBitmap3 = AutoWhiteBalance(newBitmap2, 5);

			MyColor[] image1 = BitmapConverter.FromBitmap(newBitmap3, ImageWidth, ImageHeight);

			if (argView == false)
			{
				tmpPictureOut[PictureNumber] = image1;
			}*/

			//Bitmap newBitmap4 = new Bitmap(ImageWidth, ImageHeight);
			//Bitmap newBitmap5 = new Bitmap(ImageWidth, ImageHeight);

			g = 0;
			for (int y = 0; y < ImageHeight; y++)
			{
				for (int x = 0; x < ImageWidth; x++)
				{
					if (onlyGray)
					{
						byte tmpValue = tmpPicture[PictureNumber][g].R;
						newBitmap.SetPixel(x, y, Color.FromArgb(tmpValue, Color.Black));
					}
					/*else
					{
						//MyColor c = image1[g];
						//newBitmap3.SetPixel(x, y, Color.FromArgb(c.R, c.G, c.B));
						byte tmpValue = ToGray(image1[g]);
						newBitmap4.SetPixel(x, y, Color.FromArgb(tmpValue, Color.Black));

						byte tmpValue2 = ToGray(tmpPicture[PictureNumber][g]);
						newBitmap5.SetPixel(x, y, Color.FromArgb(tmpValue2, Color.Black));
					}*/

					g++;
				}
			}


			//Bitmap newBitmap3 = AutoWhiteBalance(newBitmap, 10);


			/*newBitmap2 = ExposureCorrection.AdjustTones(newBitmap2,
				shadows: 1.5f,    // Осветлить тени
				midtones: 0.5f,   // Затемнить средние тона
				highlights: 1.5f  // Осветлить света
			);*/

			//Bitmap newBitmap3 = AutoWhiteBalance(newBitmap2, 5);


			PictureNumber++;
			if (argView)
			{
				// Обновление на форме
				pictureBox.Image = newBitmap;
				//pictureBox2.Image = newBitmap3;
				//pictureBox3.Image = newBitmap4;
				//pictureBox4.Image = newBitmap5;
				PictureNumberTxt.Text = PictureNumber.ToString();
			}
		}


		private void button1_Click(object sender, EventArgs e)
		{
			ImageDraw();
		}

		private void SaveNext(string argFileName, int argCount)
		{
			string tmp = "";

			int frameX = -1; // 3
			int frameY = 28; // 25

			int g = 0;
			for (int i = 0; i < 28; i++)
			{
				for (int j = 0; j < 28; j++)
				{
					if (i <= frameX || i >= frameY || j <= frameX || j >= frameY)
					{
						g++;
						continue;
					}

					byte tmpValue = 0;
					if (tmpPicture[PictureNumber][g].R > 128) tmpValue = 255;

					if (tmpValue == 0)
					{
						tmp += "0";
					}
					else
					{
						tmp += "1";
					}

					g++;
				}
			}

			if (PictureNumber <= argCount)
			{
				File.AppendAllText(argFileName, tmpLabels[PictureNumber] + ":" + tmp + "\n");
				PictureNumber++;
			}
		}

		private void SaveAllData_Click(object sender, EventArgs e)
		{
			PictureNumber = 0;

			if (isMNIST == true)
			{
				int count = 60000;
				for (int i = 0; i < count; i++)
				{
					//SaveNext("ExaminationSet.txt", count);
					SaveNext("LearningSet.txt", count);
				}
			}

			if (isCIFAR10 == true)
			{
				int totalPixels = ImageWidth * ImageHeight;

				tmpPictureOut = new MyColor[60000][];
				for (int i = 0; i < 60000; i++)
				{
					tmpPictureOut[i] = new MyColor[totalPixels];
				}
				PictureNumber = 0;

				SaveCIFAR("data_batch_1.bin", 10000, 0);
				SaveCIFAR("data_batch_2.bin", 10000, 10000);
				SaveCIFAR("data_batch_3.bin", 10000, 20000);
				SaveCIFAR("data_batch_4.bin", 10000, 30000);
				SaveCIFAR("data_batch_5.bin", 10000, 40000);
				SaveCIFAR("test_batch.bin", 10000, 50000);
			}
		}

		public void SaveCIFAR(string argFileName, int argCount, int argAdd)
		{
			int totalPixels = ImageWidth * ImageHeight;
			FileStream fs = new FileStream("CIFAR10B\\" + argFileName, FileMode.Create, FileAccess.Write);
			BinaryWriter w = new BinaryWriter(fs);


			for (int n = 0; n < argCount; n++)
			{
				ImageDraw(false);

				w.Write(tmplLabelsCIFAR[n]);

				for (int i = 0; i < totalPixels; i++)
				{
					w.Write(tmpPictureOut[n + argAdd][i].R);
				}
				for (int i = 0; i < totalPixels; i++)
				{
					w.Write(tmpPictureOut[n + argAdd][i].G);
				}
				for (int i = 0; i < totalPixels; i++)
				{
					w.Write(tmpPictureOut[n + argAdd][i].B);
				}
			}
			w.Close();
			fs.Close();
		}

		public void ApplyImportanceMask(MyColor[] image, float[] importanceMask, int width, int height, float fadeStrength = 0.3f)
		{
			for (int i = 0; i < image.Length; i++)
			{
				float importance = importanceMask[i];

				// Нелинейное затухание: квадрат для более плавного перехода
				float fadeFactor = importance + (1 - importance) * fadeStrength;

				image[i].R = (byte)(image[i].R * fadeFactor);
				image[i].G = (byte)(image[i].G * fadeFactor);
				image[i].B = (byte)(image[i].B * fadeFactor);
			}
		}


		public Bitmap AutoWhiteBalance(Bitmap original, float percent = 0.5f)
		{
			if (original == null) throw new ArgumentNullException(nameof(original));
			//if (percent < 0 || percent > 10) throw new ArgumentException("Percent should be between 0 and 10");

			// Блокируем биты для быстрой обработки
			BitmapData originalData = original.LockBits(
				new Rectangle(0, 0, original.Width, original.Height),
				ImageLockMode.ReadOnly,
				PixelFormat.Format24bppRgb);

			Bitmap balanced = new Bitmap(original.Width, original.Height);
			BitmapData balancedData = balanced.LockBits(
				new Rectangle(0, 0, balanced.Width, balanced.Height),
				ImageLockMode.WriteOnly,
				PixelFormat.Format24bppRgb);

			unsafe
			{
				byte* origPtr = (byte*)originalData.Scan0;
				byte* balPtr = (byte*)balancedData.Scan0;

				int stride = originalData.Stride;
				int width = original.Width;
				int height = original.Height;
				int bytesPerPixel = 3;

				// Создаем гистограммы для каждого канала
				int[] histR = new int[256];
				int[] histG = new int[256];
				int[] histB = new int[256];

				// Заполняем гистограммы
				for (int y = 0; y < height; y++)
				{
					byte* row = origPtr + (y * stride);
					for (int x = 0; x < width; x++)
					{
						int pos = x * bytesPerPixel;
						histB[row[pos]]++;     // Blue
						histG[row[pos + 1]]++; // Green
						histR[row[pos + 2]]++; // Red
					}
				}

				// Вычисляем пороги отсечения для каждого канала
				int totalPixels = width * height;
				int cutOff = (int)(totalPixels * percent / 200.0f); // Делим на 200 для двух сторон

				// Находим минимальные и максимальные значения для каждого канала
				int minR = FindMinThreshold(histR, cutOff);
				int maxR = FindMaxThreshold(histR, cutOff);
				int minG = FindMinThreshold(histG, cutOff);
				int maxG = FindMaxThreshold(histG, cutOff);
				int minB = FindMinThreshold(histB, cutOff);
				int maxB = FindMaxThreshold(histB, cutOff);

				// Предотвращаем деление на ноль
				if (maxR == minR) maxR = minR + 1;
				if (maxG == minG) maxG = minG + 1;
				if (maxB == minB) maxB = minB + 1;

				// Применяем коррекцию к каждому пикселю
				for (int y = 0; y < height; y++)
				{
					byte* origRow = origPtr + (y * stride);
					byte* balRow = balPtr + (y * stride);

					for (int x = 0; x < width; x++)
					{
						int pos = x * bytesPerPixel;

						byte b = origRow[pos];
						byte g = origRow[pos + 1];
						byte r = origRow[pos + 2];

						// Применяем линейное растяжение для каждого канала
						balRow[pos] = (byte)Math.Max(0, Math.Min(255,
							(b - minB) * 255 / (maxB - minB)));
						balRow[pos + 1] = (byte)Math.Max(0, Math.Min(255,
							(g - minG) * 255 / (maxG - minG)));
						balRow[pos + 2] = (byte)Math.Max(0, Math.Min(255,
							(r - minR) * 255 / (maxR - minR)));
					}
				}
			}

			// Разблокируем биты
			original.UnlockBits(originalData);
			balanced.UnlockBits(balancedData);

			return balanced;
		}

		// Находит минимальный порог, отсекая cutOff пикселей слева
		private int FindMinThreshold(int[] histogram, int cutOff)
		{
			int sum = 0;
			for (int i = 0; i < 256; i++)
			{
				sum += histogram[i];
				if (sum >= cutOff)
					return i;
			}
			return 0;
		}

		// Находит максимальный порог, отсекая cutOff пикселей справа
		private int FindMaxThreshold(int[] histogram, int cutOff)
		{
			int sum = 0;
			for (int i = 255; i >= 0; i--)
			{
				sum += histogram[i];
				if (sum >= cutOff)
					return i;
			}
			return 255;
		}


		public int CellWidth = 2;
		public int CellHeight = 2;

		public int MaxColorDistance = 25 * 5; //(0-255)^2
		public int MinMoveLevel = 0;

		int MoveThresholdG = 1;
		int MoveThresholdB = 2;
		int MoveThresholdR = 3;

		private Dictionary<string, int> PicDiff = new Dictionary<string, int>();


		float[] mask;
		public void CheckImage(MyColor[] image, Bitmap realImage)
		{
			PicDiff.Clear();
			MovementType IsMovement = MovementType.None;
			int MovementCellCountG = 0;
			int MovementCellCountB = 0;
			int MovementCellCountR = 0;

			mask = new float[image.Length];
			for (int i = 0; i < mask.Length; i++)
			{
				mask[i] = 0.3f;
			}

			MyColor[] image2 = Shift(image, 0, -1);

			int cellX = 0;
			int cellY = 0;
			for (int i = 0; i < ImageWidth; i += CellWidth)
			{
				for (int j = 0; j < ImageHeight; j += CellHeight)
				{
					ClearCell(cellX, cellY, image, image2);
					cellY++;
				}
				cellX++;
				cellY = 0;
			}

			List<string> locKeyList = PicDiff.Keys.ToList();
			foreach (string key in locKeyList)
			{
				PicDiff[key] -= MinMoveLevel;
				if (PicDiff[key] < 0) { PicDiff[key] = 0; }

				if (PicDiff[key] > MoveThresholdG) { MovementCellCountG++; }
				if (PicDiff[key] > MoveThresholdB) { MovementCellCountB++; }
				if (PicDiff[key] > MoveThresholdR) { MovementCellCountR++; }
			}

			if (MovementCellCountR > 1)
			{
				IsMovement = MovementType.R;
			}
			else if (MovementCellCountB > 1)
			{
				IsMovement = MovementType.B;
			}
			else if (MovementCellCountG > 1)
			{
				IsMovement = MovementType.G;
			}

			Graphics g;

			if (IsMovement != MovementType.None)
			{

				g = Graphics.FromImage(realImage);

				cellX = 0;
				cellY = 0;
				for (int i = 0; i < ImageWidth; i += CellWidth)
				{
					for (int j = 0; j < ImageHeight; j += CellHeight)
					{
						DrawDiffMap(cellX, cellY, g);
						cellY++;
					}
					cellX++;
					cellY = 0;
				}
				g.Dispose();
			}

		}


		private void DrawDiffMap(int argXIndex, int argYIndex, Graphics g)
		{
			int startX = argXIndex * CellWidth;
			int startY = argYIndex * CellHeight;
			string key = argXIndex.ToString() + "-" + argYIndex.ToString();

			if (startY == 0) return;

			Rectangle rec = new Rectangle(startX, startY, CellWidth, CellHeight);

			if (PicDiff[key] > MoveThresholdR)
			{
				g.DrawRectangle(new Pen(Color.Red, 1), rec);

				for (int i = 0; i < CellWidth; i++)
				{
					for (int j = 0; j < CellHeight; j++)
					{
						mask[(startY + j) * ImageWidth + (startX + i)] = 1;
					}
				}
			}
			/*else if (PicDiff[key] > MoveThresholdB)
			{
				g.DrawRectangle(new Pen(Color.Blue, 1), rec);
			}
			else if (PicDiff[key] > MoveThresholdG)
			{
				g.DrawRectangle(new Pen(Color.Green, 1), rec);
			}*/

		}


		private void ClearCell(int argXIndex, int argYIndex, MyColor[] image1, MyColor[] image2)
		{

			int startX = argXIndex * CellWidth;
			int endX = 0;
			if (startX + CellWidth < ImageWidth) { endX = startX + CellWidth; }
			else { endX = ImageWidth; }

			int startY = argYIndex * CellHeight;
			int endY = 0;
			if (startY + CellHeight < ImageHeight) { endY = startY + CellHeight; }
			else { endY = ImageHeight; }

			int diffProc = 0;
			int diffCount = 0;
			for (int i = startX; i < endX; i++)
			{
				for (int j = startY; j < endY; j++)
				{
					int index = j * ImageWidth + i;
					double d = MyColor.ColorDistance(image1[index], image2[index]);

					if (d > MaxColorDistance)
					{
						diffCount++;
					}
					else
					{
						int a = 1;
					}
				}
			}

			diffProc = (100 * diffCount) / (CellWidth * CellHeight);

			if (diffProc < MinMoveLevel)
			{
				MinMoveLevel = diffProc;
			}

			string key = argXIndex.ToString() + "-" + argYIndex.ToString();
			PicDiff.Add(key, diffProc);
		}

		public MyColor[] Shift(MyColor[] originalImage, int dx, int dy)
		{
			int totalPixels = ImageWidth * ImageHeight;
			MyColor[] shiftedImage = new MyColor[totalPixels];

			// Инициализируем массив белым цветом
			for (int i = 0; i < totalPixels; i++)
			{
				shiftedImage[i] = new MyColor { R = 255, G = 255, B = 255 };
			}

			// Перебираем все пиксели исходного изображения
			for (int y = 0; y < ImageHeight; y++)
			{
				for (int x = 0; x < ImageWidth; x++)
				{
					// Вычисляем новые координаты после сдвига
					int newX = x - dx;
					int newY = y - dy;

					// Проверяем, попадают ли новые координаты в границы изображения
					if (newX >= 0 && newX < ImageWidth && newY >= 0 && newY < ImageHeight)
					{
						// Копируем пиксель из исходного изображения в новую позицию
						int originalIndex = y * ImageWidth + x;
						int newIndex = newY * ImageWidth + newX;
						shiftedImage[newIndex] = originalImage[originalIndex];
					}
				}
			}

			return shiftedImage;
		}

	}

	public enum MovementType
	{
		None = 0,
		G = 1,
		B = 2,
		R = 3
	}


	public class MyColor
	{
		public byte R;
		public byte G;
		public byte B;


		// https://www.compuphase.com/cmetric.htm
		public static double ColorDistance(MyColor e1, MyColor e2)
		{
			long rmean = ((long)e1.R + (long)e2.R) / 2;
			long r = (long)e1.R - (long)e2.R;
			long g = (long)e1.G - (long)e2.G;
			long b = (long)e1.B - (long)e2.B;
			return Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
		}
	}

}
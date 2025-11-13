// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;

namespace Tac.Perceptron
{
	public class Color
	{
		public byte R;
		public byte G;
		public byte B;
	}

	public class ImageShift
	{

		public int ImageWidth = 32;
		public int ImageHeight = 32;
		public int CellWidth = 4;
		public int CellHeight = 4;

		public int MaxColorDistance = 25 * 25; //(0-255)^2
		public int MinMoveLevel = 0;

		


		private Dictionary<string, int> PicDiff = new Dictionary<string, int>();

		public void CheckImage(Color[][] TrainSetRGB)
		{
			PicDiff.Clear();

			int cellX = 0;
			int cellY = 0;
			for (int i = 0; i < ImageWidth; i += CellWidth)
			{
				for (int j = 0; j < ImageHeight; j += CellHeight)
				{
					ClearCell(cellX, cellY, TrainSetRGB[0]);
					cellY++;
				}
				cellX++;
				cellY = 0;
			}
		}


		private void ClearCell(int argXIndex, int argYIndex, Color[] image1)
		{

			Color[] image2 = Shift(image1, 1, 1);

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
					double d = ColorDistance(image1[index], image2[index]); // currentImage1.ColorDistance(i, j, currentImage2);

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


		public Color[] Shift(Color[] originalImage, int dx, int dy)
		{
			int totalPixels = ImageWidth * ImageHeight;
			Color[] shiftedImage = new Color[totalPixels];

			// Инициализируем массив белым цветом
			for (int i = 0; i < totalPixels; i++)
			{
				shiftedImage[i] = new Color { R = 255, G = 255, B = 255 };
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

		// https://www.compuphase.com/cmetric.htm
		public double ColorDistance(Color e1, Color e2)
		{
			long rmean = ((long)e1.R + (long)e2.R) / 2;
			long r = (long)e1.R - (long)e2.R;
			long g = (long)e1.G - (long)e2.G;
			long b = (long)e1.B - (long)e2.B;
			return Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
		}

	}
}

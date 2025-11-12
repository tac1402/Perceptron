using System;
using System.Drawing;
using System.Drawing.Imaging;

public static class ExposureCorrection
{
	public static Bitmap AdjustExposure(Bitmap original, float exposure = 0f, float blackLevel = 0f, float gamma = 1f)
	{
		if (original == null) throw new ArgumentNullException(nameof(original));

		// Создаем lookup table для быстрого преобразования
		byte[] lookupTable = CreateExposureLookupTable(exposure, blackLevel, gamma);

		Bitmap result = new Bitmap(original.Width, original.Height);

		// Блокируем биты для быстрой обработки
		BitmapData originalData = original.LockBits(
			new Rectangle(0, 0, original.Width, original.Height),
			ImageLockMode.ReadOnly,
			PixelFormat.Format24bppRgb);

		BitmapData resultData = result.LockBits(
			new Rectangle(0, 0, result.Width, result.Height),
			ImageLockMode.WriteOnly,
			PixelFormat.Format24bppRgb);

		unsafe
		{
			byte* origPtr = (byte*)originalData.Scan0;
			byte* resPtr = (byte*)resultData.Scan0;

			int stride = originalData.Stride;
			int width = original.Width;
			int height = original.Height;
			int bytesPerPixel = 3;

			for (int y = 0; y < height; y++)
			{
				byte* origRow = origPtr + (y * stride);
				byte* resRow = resPtr + (y * stride);

				for (int x = 0; x < width; x++)
				{
					int pos = x * bytesPerPixel;

					// Применяем преобразование через lookup table
					resRow[pos] = lookupTable[origRow[pos]];     // Blue
					resRow[pos + 1] = lookupTable[origRow[pos + 1]]; // Green
					resRow[pos + 2] = lookupTable[origRow[pos + 2]]; // Red
				}
			}
		}

		original.UnlockBits(originalData);
		result.UnlockBits(resultData);

		return result;
	}

	private static byte[] CreateExposureLookupTable(float exposure, float blackLevel, float gamma)
	{
		byte[] table = new byte[256];

		for (int i = 0; i < 256; i++)
		{
			double value = i / 255.0;

			// 1. Применяем коррекцию черного уровня
			value = value * (1 - blackLevel) + blackLevel;

			// 2. Применяем экспозицию (линейное умножение)
			value = value * Math.Pow(2, exposure);

			// 3. Применяем гамма-коррекцию
			value = Math.Pow(value, 1.0 / gamma);

			// 4. Ограничиваем диапазон 0-1 и конвертируем обратно в byte
			value = Math.Max(0, Math.Min(1, value));
			table[i] = (byte)(value * 255);
		}

		return table;
	}

	// Альтернативная реализация с отдельными методами для каждого параметра
	public static Bitmap AdjustExposureDetailed(Bitmap original, float exposure)
	{
		return AdjustExposure(original, exposure, 0f, 1f);
	}

	public static Bitmap AdjustBlackLevel(Bitmap original, float blackLevel)
	{
		return AdjustExposure(original, 0f, blackLevel, 1f);
	}

	public static Bitmap AdjustGamma(Bitmap original, float gamma)
	{
		return AdjustExposure(original, 0f, 0f, gamma);
	}

	// Автоматическая коррекция экспозиции на основе гистограммы
	public static Bitmap AutoExposure(Bitmap original)
	{
		// Анализируем гистограмму для определения оптимальной экспозиции
		float optimalExposure = CalculateOptimalExposure(original);
		return AdjustExposure(original, optimalExposure, 0f, 1f);
	}

	private static float CalculateOptimalExposure(Bitmap original)
	{
		BitmapData originalData = original.LockBits(
			new Rectangle(0, 0, original.Width, original.Height),
			ImageLockMode.ReadOnly,
			PixelFormat.Format24bppRgb);

		int[] histogram = new int[256];
		int totalPixels = original.Width * original.Height;

		unsafe
		{
			byte* origPtr = (byte*)originalData.Scan0;
			int stride = originalData.Stride;
			int bytesPerPixel = 3;

			for (int y = 0; y < original.Height; y++)
			{
				byte* row = origPtr + (y * stride);
				for (int x = 0; x < original.Width; x++)
				{
					int pos = x * bytesPerPixel;
					// Используем формулу яркости для grayscale
					byte brightness = (byte)(0.299 * row[pos + 2] + 0.587 * row[pos + 1] + 0.114 * row[pos]);
					histogram[brightness]++;
				}
			}
		}

		original.UnlockBits(originalData);

		// Находим медианную яркость
		int median = FindMedian(histogram, totalPixels);

		// Целевая медианная яркость (средне-серая)
		int targetMedian = 128;

		// Вычисляем необходимую коррекцию экспозиции
		float exposure = (float)Math.Log((double)targetMedian / median, 2);

		return Math.Max(-4f, Math.Min(4f, exposure)); // Ограничиваем диапазон
	}

	private static int FindMedian(int[] histogram, int totalPixels)
	{
		int sum = 0;
		for (int i = 0; i < 256; i++)
		{
			sum += histogram[i];
			if (sum >= totalPixels / 2)
				return i;
		}
		return 128;
	}

	// Метод для тонкой настройки теней, средних тонов и светов
	public static Bitmap AdjustTones(Bitmap original, float shadows = 1f, float midtones = 1f, float highlights = 1f)
	{
		byte[] lookupTable = CreateToneCurve(shadows, midtones, highlights);

		Bitmap result = new Bitmap(original.Width, original.Height);

		BitmapData originalData = original.LockBits(
			new Rectangle(0, 0, original.Width, original.Height),
			ImageLockMode.ReadOnly,
			PixelFormat.Format24bppRgb);

		BitmapData resultData = result.LockBits(
			new Rectangle(0, 0, result.Width, result.Height),
			ImageLockMode.WriteOnly,
			PixelFormat.Format24bppRgb);

		unsafe
		{
			byte* origPtr = (byte*)originalData.Scan0;
			byte* resPtr = (byte*)resultData.Scan0;

			int stride = originalData.Stride;
			int width = original.Width;
			int height = original.Height;
			int bytesPerPixel = 3;

			for (int y = 0; y < height; y++)
			{
				byte* origRow = origPtr + (y * stride);
				byte* resRow = resPtr + (y * stride);

				for (int x = 0; x < width; x++)
				{
					int pos = x * bytesPerPixel;

					// Применяем тоновую кривую к каждому каналу
					resRow[pos] = lookupTable[origRow[pos]];     // Blue
					resRow[pos + 1] = lookupTable[origRow[pos + 1]]; // Green
					resRow[pos + 2] = lookupTable[origRow[pos + 2]]; // Red
				}
			}
		}

		original.UnlockBits(originalData);
		result.UnlockBits(resultData);

		return result;
	}

	private static byte[] CreateToneCurve(float shadows, float midtones, float highlights)
	{
		byte[] table = new byte[256];

		for (int i = 0; i < 256; i++)
		{
			double value = i / 255.0;
			double result;

			if (value < 0.5)
			{
				// Тени: применяем разную степень коррекции
				result = Math.Pow(value * 2, shadows) * 0.5;
			}
			else if (value < 0.75)
			{
				// Средние тона
				double normalized = (value - 0.5) * 4; // 0-1 в диапазоне 0.5-0.75
				result = 0.5 + Math.Pow(normalized, midtones) * 0.25;
			}
			else
			{
				// Света
				double normalized = (value - 0.75) * 4; // 0-1 в диапазоне 0.75-1.0
				result = 0.75 + Math.Pow(normalized, highlights) * 0.25;
			}

			table[i] = (byte)(Math.Max(0, Math.Min(1, result)) * 255);
		}

		return table;
	}
}
using System;
using System.Drawing;
using System.Drawing.Imaging;
using ViewMNIST;


public static class BitmapConverter
{
	// Конвертация из Bitmap в MyColor[]
	public static MyColor[] FromBitmap(Bitmap bitmap, int imageWidth, int imageHeight)
	{
		if (bitmap == null)
			throw new ArgumentNullException(nameof(bitmap));

		if (bitmap.Width != imageWidth || bitmap.Height != imageHeight)
			throw new ArgumentException("Bitmap dimensions don't match specified width and height");

		int totalPixels = imageWidth * imageHeight;
		MyColor[] result = new MyColor[totalPixels];

		// Блокируем биты для быстрого доступа
		BitmapData bitmapData = bitmap.LockBits(
			new Rectangle(0, 0, imageWidth, imageHeight),
			ImageLockMode.ReadOnly,
			PixelFormat.Format24bppRgb); // Предполагаем 24bpp RGB

		unsafe
		{
			byte* ptr = (byte*)bitmapData.Scan0;
			int stride = bitmapData.Stride;
			int bytesPerPixel = 3; // Format24bppRgb

			for (int y = 0; y < imageHeight; y++)
			{
				byte* row = ptr + (y * stride);
				for (int x = 0; x < imageWidth; x++)
				{
					int pixelIndex = y * imageWidth + x;
					int byteIndex = x * bytesPerPixel;

					result[pixelIndex] = new MyColor
					{
						B = row[byteIndex],     // Blue
						G = row[byteIndex + 1], // Green
						R = row[byteIndex + 2]  // Red
					};
				}
			}
		}

		bitmap.UnlockBits(bitmapData);
		return result;
	}

	// Конвертация из MyColor[] в Bitmap
	public static Bitmap ToBitmap(MyColor[] pixels, int imageWidth, int imageHeight)
	{
		if (pixels == null)
			throw new ArgumentNullException(nameof(pixels));

		if (pixels.Length != imageWidth * imageHeight)
			throw new ArgumentException("Pixel array length doesn't match image dimensions");

		Bitmap bitmap = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);

		// Блокируем биты для быстрой записи
		BitmapData bitmapData = bitmap.LockBits(
			new Rectangle(0, 0, imageWidth, imageHeight),
			ImageLockMode.WriteOnly,
			PixelFormat.Format24bppRgb);

		unsafe
		{
			byte* ptr = (byte*)bitmapData.Scan0;
			int stride = bitmapData.Stride;
			int bytesPerPixel = 3; // Format24bppRgb

			for (int y = 0; y < imageHeight; y++)
			{
				byte* row = ptr + (y * stride);
				for (int x = 0; x < imageWidth; x++)
				{
					int pixelIndex = y * imageWidth + x;
					int byteIndex = x * bytesPerPixel;

					MyColor color = pixels[pixelIndex];
					row[byteIndex] = color.B;     // Blue
					row[byteIndex + 1] = color.G; // Green
					row[byteIndex + 2] = color.R; // Red
				}
			}
		}

		bitmap.UnlockBits(bitmapData);
		return bitmap;
	}
}
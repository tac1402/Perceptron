// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System.Runtime.Intrinsics.Arm;

namespace Tac.Neocognitron
{
	public class MWeights
	{
		public float[] w;

		public MWeights(int size, double argBase, int planes, bool norm)
		{ 
			w = new float[size * size];
			generateMonotonic(size, argBase, planes, norm);
		}

		/// <summary>
		/// Сгенерировать монотонно убывающую двумерную функцию.
		/// </summary>
		/// <param name="argBase">Базовое значение для функции</param>
		/// <param name="size">Размер используемого окна</param>
		/// <param name="planes">Количество плоскостей, используемых для нормализации</param>
		/// <param name="norm">Нормализовать выход, чтобы получить сумму 1</param>
		/// <returns>Возвращает монотонную двумерную функцию</returns>
		private void generateMonotonic(int size, double argBase, int planes, bool norm)
		{
			// Calculated each value
			int index = 0;
			for (int n = 0; n < size; n++)
			{
				for (int m = 0; m < size; m++)
				{
					w[index] = (float)Math.Pow(argBase, Point2D.Distance(size / 2f, size / 2f, n, m));
					index++;
				}
			}

			// Normalize the entire function
			if (norm)
			{
				int size2 = size * size;
				float sum = 0;
				for (int i = 0; i < size2; i++)
				{
					sum += w[i];
				}
				// Normalize with respect to # of planes
				for (int i = 0; i < size2; i++)
				{
					w[i] /= (planes * sum);
				}
			}
		}
	}
}

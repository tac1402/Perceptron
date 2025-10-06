// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Tac.Perceptron
{
	public class Hamming
	{
		private const float HammingThreshold = 0.0001f; // Ваш порог

		public unsafe int HammingDistance(float[] a, float[] b)
		{
			if (a.Length != b.Length)
				throw new ArgumentException("Vectors must have the same length");

			if (!Avx2.IsSupported)
			{
				// Fallback для систем без AVX2 поддержки
				return HammingDistanceScalar(a, b);
			}

			int distance = 0;
			int i = 0;

			fixed (float* aPtr = a, bPtr = b)
			{
				// Обрабатываем по 8 элементов за раз (Vector256<float> содержит 8 float)
				int vectorSize = Vector256<float>.Count;
				int length = a.Length;

				// Вектор с пороговым значением
				Vector256<float> thresholdVector = Vector256.Create(HammingThreshold);

				// Вектор для подсчета (каждый элемент будет содержать 0 или 1)
				Vector256<int> sumVector = Vector256<int>.Zero;

				for (; i <= length - vectorSize; i += vectorSize)
				{
					// Загружаем 8 элементов из каждого массива
					Vector256<float> aVec = Avx.LoadVector256(aPtr + i);
					Vector256<float> bVec = Avx.LoadVector256(bPtr + i);

					// Вычисляем абсолютную разность |a[i] - b[i]|
					Vector256<float> diff = Avx.Subtract(aVec, bVec);
					Vector256<float> absDiff = Avx.AndNot(Vector256.Create(-0.0f), diff); // Быстрый abs

					// Сравниваем с порогом: если absDiff > threshold, то 0xFFFFFFFF, иначе 0
					Vector256<float> comparison = Avx.Compare(absDiff, thresholdVector,
						FloatComparisonMode.OrderedGreaterThanNonSignaling);

					// Преобразуем результат сравнения в int (0 или -1)
					Vector256<int> intMask = Avx2.ConvertToVector256Int32(comparison.AsSingle());

					// Используем арифметический сдвиг для распространения знакового бита
					Vector256<int> ones = Avx2.ShiftRightArithmetic(intMask, 31); // -1 >> 31 = -1

					// Берем абсолютное значение - преобразуем -1 в 1
					Vector256<uint> absOnes = Avx2.Abs(ones); // Возвращает Vector256<uint>

					Vector256<int> counted = absOnes.AsInt32();
					sumVector = Avx2.Add(sumVector, counted);

				}

				// Горизонтальное сложение 8 значений
				int* temp = stackalloc int[vectorSize];
				Avx.Store(temp, sumVector);

				for (int j = 0; j < vectorSize; j++)
				{
					distance += temp[j];
				}

				// Обрабатываем оставшиеся элементы скалярно
				for (; i < length; i++)
				{
					if (Math.Abs(a[i] - b[i]) > HammingThreshold)
						distance++;
				}
			}

			return distance;
		}

		// Скалярная версия для fallback
		private int HammingDistanceScalar(float[] a, float[] b)
		{
			int distance = 0;
			for (int i = 0; i < a.Length; i++)
			{
				if (Math.Abs(a[i] - b[i]) > HammingThreshold)
					distance++;
			}
			return distance;
		}

	}
}

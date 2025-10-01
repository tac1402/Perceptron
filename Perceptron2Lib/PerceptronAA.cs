// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Tac.Perceptron
{
	public class PerceptronAA : IDisposable
	{
		private float[] _weights;
		private int _sCount;
		private int _aCount;

		public void SetWeights(int SCount, int ACount, Dictionary<int, float[]> weights)
		{
			// Преобразуем 2D массив в 1D для более эффективного доступа
			_sCount = SCount;
			_aCount = ACount;
			_weights = new float[SCount * ACount];

			for (int i = 0; i < SCount; i++)
			{
				for (int j = 0; j < ACount; j++)
				{
					_weights[i * ACount + j] = weights[i][j];
				}
			}
		}

		public float[] AActivation(int SCount, int ACount, int stimulNumber, byte[] SField)
		{
			float[] AField = new float[ACount];
			// Выбираем лучшую доступную реализацию
			if (Avx2.IsSupported)
			{
				AActivationAvx2(AField, SCount, ACount, SField);
			}
			else
			{
				// Скалярная реализация как последний запасной вариант
				AActivationScalar(AField, SCount, ACount, SField);
			}

			return AField;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe void AActivationAvx2(float[] AField, int SCount, int ACount, byte[] SField)
		{
			fixed (float* pAField = AField)
			fixed (byte* pSField = SField)
			fixed (float* pWeights = _weights)
			{
				// Обрабатываем по 8 выходных элементов за раз (Vector256<float> содержит 8 float)
				int j = 0;
				for (; j <= ACount - 8; j += 8)
				{
					// Инициализируем аккумуляторы для 8 выходных значений
					Vector256<float> sum0 = Vector256<float>.Zero;
					Vector256<float> sum1 = Vector256<float>.Zero;
					Vector256<float> sum2 = Vector256<float>.Zero;
					Vector256<float> sum3 = Vector256<float>.Zero;

					float* weightPtr0 = pWeights + j;        // WeightSA[0][j]
					float* weightPtr1 = pWeights + j + 1;    // WeightSA[0][j+1]
					float* weightPtr2 = pWeights + j + 2;    // WeightSA[0][j+2]
					float* weightPtr3 = pWeights + j + 3;    // WeightSA[0][j+3]

					// Внутренний цикл по i (SCount)
					for (int i = 0; i < SCount; i++)
					{
						// Загружаем SField[i] и преобразуем byte в float
						float sVal = pSField[i];
						Vector256<float> sVector = Vector256.Create(sVal);

						// Загружаем веса для текущего i и 8 последовательных j
						// Используем невыровненную загрузку поскольку выравнивание не гарантировано
						Vector256<float> weightsRow = Avx.LoadVector256(pWeights + i * ACount + j);

						// Умножаем веса на SField[i] и добавляем к аккумуляторам
						Vector256<float> weighted = Avx.Multiply(weightsRow, sVector);

						// Обновляем аккумуляторы
						if (i == 0)
						{
							sum0 = weighted;
						}
						else
						{
							sum0 = Avx.Add(sum0, weighted);
						}
					}

					// Сохраняем результат для 8 выходных элементов
					Avx.Store(pAField + j, sum0);
				}

				// Обрабатываем оставшиеся элементы скалярно
				for (; j < ACount; j++)
				{
					float sum = 0f;
					for (int i = 0; i < SCount; i++)
					{
						sum += _weights[i * ACount + j] * pSField[i];
					}
					pAField[j] = sum;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AActivationScalar(float[] AField, int SCount, int ACount, byte[] SField)
		{
			// Простая скалярная реализация как запасной вариант
			for (int j = 0; j < ACount; j++)
			{
				float sum = 0f;
				for (int i = 0; i < SCount; i++)
				{
					sum += _weights[i * ACount + j] * SField[i];
				}
				AField[j] = sum;
			}
		}

		public void Dispose()
		{
			_weights = null;
		}
	}
}

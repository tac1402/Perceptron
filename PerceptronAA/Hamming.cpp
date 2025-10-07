#include "pch.h"

#include "Hamming.h"
#include <immintrin.h>
#include <nmmintrin.h> // Для _mm_popcnt_u32
#include <vector>
#include <algorithm>

class HammingDistance
{
public:
    static int CalculateAvx(const float* a, const float* b, int length, float threshold = 0.000001f)
    {
        int distance = 0;
        int i = 0;

        // Создаем вектор с пороговым значением
        const __m256 threshold_vec = _mm256_set1_ps(threshold);
        const __m256 negative_threshold_vec = _mm256_set1_ps(-threshold);

        // Маска для абсолютного значения (сбрасывает знак)
        const __m256 abs_mask = _mm256_castsi256_ps(_mm256_set1_epi32(0x7FFFFFFF));

        // Обрабатываем по 8 элементов за раз
        for (; i <= length - 8; i += 8) 
        {
            // Загружаем 8 элементов из a и b
            __m256 vec_a = _mm256_loadu_ps(a + i);
            __m256 vec_b = _mm256_loadu_ps(b + i);

            // Вычисляем разность
            __m256 diff = _mm256_sub_ps(vec_a, vec_b);

            // Вычисляем абсолютное значение разности
            __m256 abs_diff = _mm256_and_ps(diff, abs_mask);

            // Сравниваем с порогом: abs(a[i] - b[i]) > threshold
            __m256 cmp_result = _mm256_cmp_ps(abs_diff, threshold_vec, _CMP_GT_OQ);

            // Получаем битовую маску результатов сравнения
            int mask = _mm256_movemask_ps(cmp_result);

            // Подсчитываем количество установленных битов (количество различий)
            distance += _mm_popcnt_u32(mask);
        }

        // Обрабатываем оставшиеся элементы скалярно
        for (; i < length; i++)
        {
            float diff = a[i] - b[i];
            if (diff < 0) diff = -diff; // Абсолютное значение
            if (diff > threshold)
            {
                distance++;
            }
        }

        return distance;
    }
};

extern "C"
{
    HAMMING_API int CalculateHamming(const float* a, const float* b, int length, float threshold)
    {
        return HammingDistance::CalculateAvx(a, b, length, threshold);
    }
}
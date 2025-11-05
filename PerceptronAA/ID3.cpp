// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


#include "pch.h"

#include "ID3.h"
#include <immintrin.h>
#include <iostream>

class ID3
{

public:

    ID3()
    {
    }

    ~ID3()
    {
    }

    double CalcEntropyTotal(const signed char* samplesClass, int total)
    {
        int totalPositives = getTotalPositives_avx2(samplesClass, total);
        return calcEntropy(totalPositives, total - totalPositives);
    }

    signed char AllSamples(const signed char* samplesClass, int lenght, signed char argValue)
    {
        return allSamples_avx2(samplesClass, lenght, argValue);
    }

    double CalcEntropyAdd(const signed char* attributeSet, const signed char* samplesClass, int total, signed char value)
    {
        return calcEntropyAdd_avx2(attributeSet, samplesClass, total, value);
    }


///////////////////////////////////////////////////////////////////////////////////////////

    int getTotalPositives_avx2(const signed char* samplesClass, int length)
    {
        if (length == 0) return 0;

        int result = 0;
        int i = 0;

        // Создаем вектор, заполненный единицами для сравнения
        const __m256i v_ones = _mm256_set1_epi8(1);

        // Обрабатываем данные блоками по 32 байта (256 бит)
        for (; i <= length - 32; i += 32)
        {
            // Загружаем 32 байта (signed char значения)
            __m256i data = _mm256_load_si256((const __m256i*)(samplesClass + i));

            // Сравниваем с единицами (находим элементы равные 1)
            __m256i mask = _mm256_cmpeq_epi8(data, v_ones);

            // Получаем битовую маску (32 бита, где каждый бит соответствует одному байту)
            int bit_mask = _mm256_movemask_epi8(mask);

            // Подсчитываем количество установленных битов (количество единиц)
            result += _mm_popcnt_u32(bit_mask);
        }

        // Обрабатываем оставшиеся элементы обычным способом
        for (; i < length; i++)
        {
            if (samplesClass[i] == 1)  // Считаем только единицы
            {
                result++;
            }
        }

        return result;
    }

    signed char allSamples_avx2(const signed char* samplesClass, int length, signed char argValue)
    {
        if (length == 0) return 1;  // Пустой массив считаем совпадающим

        // Создаем вектор, заполненный значением argValue для сравнения
        const __m256i v_target = _mm256_set1_epi8(argValue);

        int i = 0;

        // AVX2 обработка по 32 элемента
        for (; i + 32 <= length; i += 32)
        {
            __m256i v_data = _mm256_loadu_si256((const __m256i*)(samplesClass + i));
            __m256i v_result = _mm256_cmpeq_epi8(v_data, v_target);

            // Если есть хотя бы один несовпадающий элемент - возвращаем -1 (false)
            if (_mm256_movemask_epi8(v_result) != 0xFFFFFFFF)
            {
                return -1;
            }
        }

        // Скалярная обработка остатка
        for (; i < length; i++)
        {
            if (samplesClass[i] != argValue)
            {
                return -1;
            }
        }

        return 1;  // Все элементы совпадают
    }

    double calcEntropyAdd_avx2(const signed char* attributeSet, const signed char* samplesClass, int total, signed char value)
    {
        if (total == 0) return 0.0;

        int positives = 0;
        int negatives = 0;
        int i = 0;

        // AVX2 векторы для сравнения
        const __m256i v_value = _mm256_set1_epi8(value);
        const __m256i v_one = _mm256_set1_epi8(1);

        // AVX2 обработка по 32 элемента
        for (; i <= total - 32; i += 32) 
        {
            // Загружаем 32 значения атрибута и класса
            __m256i v_attr = _mm256_loadu_si256((const __m256i*)(attributeSet + i));
            __m256i v_class = _mm256_loadu_si256((const __m256i*)(samplesClass + i));

            // Сравниваем атрибуты с целевым значением
            __m256i v_attr_match = _mm256_cmpeq_epi8(v_attr, v_value);

            // Сравниваем классы с 1 (для positives)
            __m256i v_class_match = _mm256_cmpeq_epi8(v_class, v_one);

            // Комбинируем условия: атрибут совпадает И класс равен 1
            __m256i v_pos_mask = _mm256_and_si256(v_attr_match, v_class_match);

            // Для negatives: атрибут совпадает И класс не равен 1
            __m256i v_neg_mask = _mm256_and_si256(v_attr_match,
                _mm256_xor_si256(v_class_match, _mm256_set1_epi8(0xFF)));

            // Получаем битовые маски и подсчитываем количество установленных битов
            int pos_mask = _mm256_movemask_epi8(v_pos_mask);
            int neg_mask = _mm256_movemask_epi8(v_neg_mask);

            positives += _mm_popcnt_u32(pos_mask);
            negatives += _mm_popcnt_u32(neg_mask);
        }

        // Обработка оставшихся элементов
        for (; i < total; i++) 
        {
            if (attributeSet[i] == value) 
            {
                if (samplesClass[i] == 1) { positives++; }
                else { negatives++; }
            }
        }

        // Вычисляем энтропию и взвешенную сумму
        double entropy = calcEntropy(positives, negatives);
        int matchingCount = positives + negatives;

        if (matchingCount == 0) return 0.0;

        double sum = -static_cast<double>(matchingCount) / total * entropy;
        return sum;
    }


    float calcEntropy(int positives, int negatives) 
    {
        int total = positives + negatives;
        if (total == 0) return 0.0f;

        float ratioPositive = static_cast<float>(positives) / total;
        float ratioNegative = static_cast<float>(negatives) / total;

        float result = 0.0f;

        if (ratioPositive != 0.0f) 
        {
            result -= ratioPositive * std::log2(ratioPositive);
        }
        if (ratioNegative != 0.0f) 
        {
            result -= ratioNegative * std::log2(ratioNegative);
        }
        return result;
    }
};


// C wrapper functions
extern "C"
{
    ID3_API ID3Handle CreateID3()
    {
        try
        {
            return new ID3();
        }
        catch (...)
        {
            return nullptr;
        }
    }

    ID3_API void DisposeID3(ID3Handle handle)
    {
        if (handle)
        {
            delete static_cast<ID3*>(handle);
        }
    }

    ID3_API double CalcEntropyTotal(ID3Handle handle, const signed char* samplesClass, int lenght)
    {
        if (handle && samplesClass)
        {
            return static_cast<ID3*>(handle)->CalcEntropyTotal(samplesClass, lenght);
        }
    }

    ID3_API signed char AllSamples(ID3Handle handle, const signed char* samplesClass, int lenght, signed char argValue)
    {
        if (handle && samplesClass)
        {
            return static_cast<ID3*>(handle)->AllSamples(samplesClass, lenght, argValue);
        }
    }

    ID3_API double CalcEntropyAdd(ID3Handle handle, const signed char* attributeSet, const signed char* samplesClass, int total, signed char value)
    {
        if (handle && samplesClass && attributeSet)
        {
            return static_cast<ID3*>(handle)->CalcEntropyAdd(attributeSet, samplesClass, total, value);
        }
    }
}
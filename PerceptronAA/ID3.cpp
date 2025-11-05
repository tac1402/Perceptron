// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


#include "pch.h"

#include "ID3.h"
#include <immintrin.h>
#include <intrin.h>
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

    int GetTotalPositives(const signed char* samplesClass, int lenght)
    {
        return getTotalPositives_avx2(samplesClass, lenght);
    }

    signed char AllSamples(const signed char* samplesClass, int lenght, signed char argValue)
    {
        return allSamples_avx2(samplesClass, lenght, argValue);
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

    ID3_API int GetTotalPositives(ID3Handle handle, const signed char* samplesClass, int lenght)
    {
        if (handle && samplesClass)
        {
            return static_cast<ID3*>(handle)->GetTotalPositives(samplesClass, lenght);
        }
    }

    ID3_API signed char AllSamples(ID3Handle handle, const signed char* samplesClass, int lenght, signed char argValue)
    {
        if (handle && samplesClass)
        {
            return static_cast<ID3*>(handle)->AllSamples(samplesClass, lenght, argValue);
        }
    }
}
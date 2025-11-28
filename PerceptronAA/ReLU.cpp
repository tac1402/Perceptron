// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


#include "pch.h"
#include "ReLu.h"
#include "Tensor.h"

#include <immintrin.h>


Tensor ReLU::Forward(Tensor& input)
{
    Tensor output = input.Clone();
    float* data = output.Data;
    int len = output.DataLength;

    // Обработка элементов с использованием AVX2
    int i = 0;
    for (; i <= len - 8; i += 8) 
    {
        // Загружаем 8 элементов
        __m256 vec = _mm256_loadu_ps(data + i);
        // Создаем нулевой вектор
        __m256 zero = _mm256_setzero_ps();
        // Вычисляем максимум между вектором и нулем
        __m256 result = _mm256_max_ps(vec, zero);
        // Сохраняем результат
        _mm256_storeu_ps(data + i, result);
    }

    // Обработка оставшихся элементов
    for (; i < len; ++i) 
    {
        if (data[i] < 0) 
        {
            data[i] = 0;
        }
    }

    return output;
}


// Реализация C-интерфейса
extern "C" 
{
    CNN_API void* ReLU_Create() 
    {
        return new ReLU();
    }

    CNN_API void ReLU_Delete(void* relu) 
    {
        delete static_cast<ReLU*>(relu);
    }

    CNN_API void* ReLU_Forward(void* relu, void* inputTensor) 
    {
        ReLU* r = static_cast<ReLU*>(relu);
        Tensor* input = static_cast<Tensor*>(inputTensor);
        Tensor* result = new Tensor(r->Forward(*input));
        return result;
    }
}

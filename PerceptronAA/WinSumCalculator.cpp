// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


#include "pch.h"

#include "WinSumCalculator.h"
#include <immintrin.h>

WinSumCalculator::WinSumCalculator(const float* input, const Point2D* inPointField,
    const float* weights, int outWidth, int inputWidth, int windowSize)
    : input_(input), inPointField_(inPointField), weights_(weights),
    outWidth_(outWidth), inputWidth_(inputWidth), windowSize_(windowSize) 
{
}

float WinSumCalculator::Compute(int oh, int ow) 
{
    auto hsum_avx = [](__m256 v) -> float {
        __m128 v1 = _mm256_extractf128_ps(v, 1);
        __m128 v2 = _mm256_extractf128_ps(v, 0);
        v1 = _mm_add_ps(v1, v2);
        v1 = _mm_add_ps(v1, _mm_movehl_ps(v1, v1));
        v1 = _mm_add_ss(v1, _mm_movehdup_ps(v1));
        return _mm_cvtss_f32(v1);
        };

    const int totalPoints = windowSize_ * windowSize_;
    __m256 sum_vec = _mm256_setzero_ps();
    int w = 0;

    for (; w <= totalPoints - 8; w += 8) 
    {
        // Создаем массив индексов
        int indices[8];
        for (int i = 0; i < 8; i++) 
        {
            int current_w = w + i;
            int x = current_w % windowSize_;
            int y = current_w / windowSize_;
            const Point2D& pointIn = inPointField_[(oh + y) * outWidth_ + (ow + x)];
            indices[i] = pointIn.Y * inputWidth_ + pointIn.X;
        }

        // Используем gather для загрузки значений
        __m256i index_vec = _mm256_loadu_si256((__m256i*)indices);
        __m256 val_vec = _mm256_i32gather_ps(input_, index_vec, 4);

        // Загружаем веса
        __m256 weight_vec = _mm256_loadu_ps(&weights_[w]);

        sum_vec = _mm256_fmadd_ps(val_vec, weight_vec, sum_vec);
    }

    // Остаточная обработка (без изменений)
    float sum = hsum_avx(sum_vec);
    for (; w < totalPoints; w++) 
    {
        int x = w % windowSize_;
        int y = w / windowSize_;
        const Point2D& pointIn = inPointField_[(oh + y) * outWidth_ + (ow + x)];
        float val = input_[pointIn.Y * inputWidth_ + pointIn.X];
        float weight = weights_[y * windowSize_ + x];
        sum += val * weight;
    }

    return sum;
}

// C-интерфейс реализации
extern "C" 
{
    CNN_API WinSumCalculator* CreateCalculator(
        const float* input, const Point2D* inPointField, const float* weights,
        int outWidth, int inputWidth, int windowSize) 
    {
        return new WinSumCalculator(input, inPointField, weights, outWidth, inputWidth, windowSize);
    }

    CNN_API float ComputeSum(WinSumCalculator* calculator, int oh, int ow) 
    {
        return calculator->Compute(oh, ow);
    }

    CNN_API void DeleteCalculator(WinSumCalculator* calculator) 
    {
        delete calculator;
    }
}

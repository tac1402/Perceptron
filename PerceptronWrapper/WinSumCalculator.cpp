// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


#include "pch.h"

#include "WinSumCalculator.h"
#include <immintrin.h>
#include <cstdio>

WinSumCalculator::WinSumCalculator(float* input, int* indices, float* weights, int outSize, int inSize, int winSize)
    : input_(input), indices_(indices), weights_(weights), outSize_(outSize), inSize_(inSize), winSize_(winSize)
{ }

float WinSumCalculator::Compute(int oh, int ow) 
{
    const int totalPoints = winSize_ * winSize_;

    // ¬ычисл€ем базовый индекс в одномерном массиве
    int baseIndex = (oh * outSize_ + ow) * totalPoints;

    float sum = 0; 
    for (int w = 0; w < totalPoints; w++)
    {
        sum += input_[indices_[baseIndex + w]] * weights_[w];
    }

    return sum;
}



// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

#include "PerceptronCuda.cuh"
#include <cuda_runtime.h>
#include <iostream>
#include <algorithm>
#include <string>

#include "device_launch_parameters.h"

// Kernel функция - ТОЛЬКО здесь доступны blockIdx, threadIdx и т.д.
__global__ void AActivationCudaKernel(float* AField, const float* SField, const float* SAWeights,
    const int* nonZeroIndices, int nonZeroCount,
    int ACount, int SCount) 
{
    // Теперь эти переменные определены!
    int j = blockIdx.x * blockDim.x + threadIdx.x;

    if (j < ACount) {
        float sum = 0.0f;

        for (int idx = 0; idx < nonZeroCount; ++idx) {
            int i = nonZeroIndices[idx];
            sum += SAWeights[i * ACount + j] * SField[i];
        }

        AField[j] = sum;
    }
}

// Функция-обертка для запуска kernel (вызывается из C++ кода)
void launchActivationKernel(float* AField, const float* SField, const float* SAWeights,
    const int* nonZeroIndices, int nonZeroCount,
    int ACount, int SCount) 
{
    // Настройка запуска kernel
    int blockSize = 256;
    int numBlocks = (ACount + blockSize - 1) / blockSize;

    // Запуск kernel
    AActivationCudaKernel<<<numBlocks, blockSize>>> (
        AField, SField, SAWeights, nonZeroIndices,
        nonZeroCount, ACount, SCount
        );
}

// Реализация методов класса
PerceptronCuda::PerceptronCuda() {}

PerceptronCuda::~PerceptronCuda() {
    cleanup();
}

bool PerceptronCuda::initialize(const float* argSAWeights, int SCount, int ACount) 
{

    if (initialized && currentSCount == SCount && currentACount == ACount) 
    {
        return true;
    }

    cleanup();

    try 
    {
        cudaError_t err;

        err = cudaMalloc(&d_SAWeights, SCount * ACount * sizeof(float));
        if (err != cudaSuccess) return false;

        err = cudaMemcpy(d_SAWeights, argSAWeights, SCount * ACount * sizeof(float), cudaMemcpyHostToDevice);
        if (err != cudaSuccess) return false;

        err = cudaMalloc(&d_SField, SCount * sizeof(float));
        if (err != cudaSuccess) return false;

        err = cudaMalloc(&d_AField, ACount * sizeof(float));
        if (err != cudaSuccess) return false;

        err = cudaMalloc(&d_nonZeroIndices, SCount * sizeof(int));
        if (err != cudaSuccess) return false;

        currentSCount = SCount;
        currentACount = ACount;
        initialized = true;

        return true;
    }
    catch (...) 
    {
        cleanup();
        return false;
    }
}

void PerceptronCuda::AActivationCuda(float* hostAField, const float* hostSField, int ACount, int SCount) 
{
    if (!initialized) 
    {
        throw std::runtime_error("PerceptronCuda not initialized");
    }

    // 1. Собираем ненулевые индексы
    std::vector<int> nonZeroIndices;
    nonZeroIndices.reserve(SCount);
    for (int i = 0; i < SCount; ++i) 
    {
        if (hostSField[i] != 0.0f) 
        {
            nonZeroIndices.push_back(i);
        }
    }
    const int nonZeroCount = static_cast<int>(nonZeroIndices.size());

    // 2. Копируем данные на GPU
    cudaMemcpy(d_SField, hostSField, SCount * sizeof(float), cudaMemcpyHostToDevice);
    cudaMemcpy(d_nonZeroIndices, nonZeroIndices.data(), nonZeroCount * sizeof(int),
        cudaMemcpyHostToDevice);

    // 3. Запускаем kernel через функцию-обертку
    launchActivationKernel(d_AField, d_SField, d_SAWeights, d_nonZeroIndices,
        nonZeroCount, ACount, SCount);

    cudaDeviceSynchronize();

    // 4. Копируем результат обратно
    cudaMemcpy(hostAField, d_AField, ACount * sizeof(float), cudaMemcpyDeviceToHost);
}

void PerceptronCuda::cleanup() 
{
    if (d_SAWeights) cudaFree(d_SAWeights);
    if (d_SField) cudaFree(d_SField);
    if (d_AField) cudaFree(d_AField);
    if (d_nonZeroIndices) cudaFree(d_nonZeroIndices);

    d_SAWeights = nullptr;
    d_SField = nullptr;
    d_AField = nullptr;
    d_nonZeroIndices = nullptr;
    initialized = false;
}
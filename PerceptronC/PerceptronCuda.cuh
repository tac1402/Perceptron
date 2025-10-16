// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

#pragma once

#include <vector>
#include <stdexcept>

class PerceptronCuda
{
    private:
        float* d_SAWeights = nullptr;
        float* d_SField = nullptr;
        float* d_AField = nullptr;
        int* d_nonZeroIndices = nullptr;

        int currentSCount = 0;
        int currentACount = 0;
        bool initialized = false;

    public:
        PerceptronCuda();
        ~PerceptronCuda();

        PerceptronCuda(const PerceptronCuda&) = delete;
        PerceptronCuda& operator=(const PerceptronCuda&) = delete;

        bool initialize(const float* hostSAWeights, int SCount, int ACount);
        void AActivationCuda(float* hostAField, const float* hostSField, int ACount, int SCount);
        void cleanup();

        bool isInitialized() const { return initialized; }
};

// Объявления kernel функций
void launchActivationKernel(float* AField, const float* SField, const float* SAWeights,
    const int* nonZeroIndices, int nonZeroCount,
    int ACount, int SCount);
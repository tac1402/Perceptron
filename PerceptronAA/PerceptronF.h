// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

#pragma once

#ifdef PERCEPTRONF_EXPORTS
#define PERCEPTRONF_API __declspec(dllexport)
#else
#define PERCEPTRONF_API __declspec(dllimport)
#endif

extern "C" 
{
    // Handle types
    typedef void* PerceptronFHandle;

    // Creation and destruction
    PERCEPTRONF_API PerceptronFHandle CreatePerceptronF(int sCount, int aCount, int rCount);
    PERCEPTRONF_API void DisposePerceptronF(PerceptronFHandle handle);

    // Operations
    PERCEPTRONF_API void SAf(PerceptronFHandle handle, int sIndex, int aIndex, float value);
    PERCEPTRONF_API void ARf(PerceptronFHandle handle, int aIndex, int rIndex, float value);
    PERCEPTRONF_API float AR_f(PerceptronFHandle handle, int aIndex, int rIndex);

    PERCEPTRONF_API void AActivation_f(PerceptronFHandle handle, const float* sField, float* aField);

    PERCEPTRONF_API void RActivation_f(PerceptronFHandle handle, const float* aField, float* rField);
    PERCEPTRONF_API void LearnedStimulARf(PerceptronFHandle handle, const float* reactionError, const float* aField);
}


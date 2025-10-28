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
    PERCEPTRONF_API PerceptronFHandle CreatePerceptronF(int sCount, int aCount, int rCount, int a2Count = 0);
    PERCEPTRONF_API void DisposePerceptronF(PerceptronFHandle handle);

    // Operations
    PERCEPTRONF_API void SA(PerceptronFHandle handle, int sIndex, int aIndex, float value);
    PERCEPTRONF_API float SA_(PerceptronFHandle handle, int sIndex, int aIndex);
    PERCEPTRONF_API void AA(PerceptronFHandle handle, int aIndex, int a2Index, float value);
    PERCEPTRONF_API void AR(PerceptronFHandle handle, int aIndex, int rIndex, float value);
    PERCEPTRONF_API float AR_(PerceptronFHandle handle, int aIndex, int rIndex);

    PERCEPTRONF_API void AActivation(PerceptronFHandle handle, const float* sField, float* aField);
    PERCEPTRONF_API void A2Activation(PerceptronFHandle handle, const float* aField, float* a2Field);

    PERCEPTRONF_API void RActivation(PerceptronFHandle handle, const float* aField, float* rField);
    PERCEPTRONF_API void R2Activation(PerceptronFHandle handle, const float* a2Field, float* rField);

    PERCEPTRONF_API void LearnedStimulAR(PerceptronFHandle handle, const float* reactionError, const float* aField);
    PERCEPTRONF_API void LearnedStimulSA(PerceptronFHandle handle, const float* reactionError, const float* aField, const float* aFieldNorm);

    PERCEPTRONF_API void LearnedStimul2SA(PerceptronFHandle handle, const float* reactionError, const float* aField, const float* aFieldNorm);
    PERCEPTRONF_API void LearnedStimul2AA(PerceptronFHandle handle, const float* reactionError, const float* a2Field, const float* a2FieldNorm, float* retUpdates);

    PERCEPTRONF_API void RandomChange(PerceptronFHandle handle, float d, float c3, const float* AField);
    PERCEPTRONF_API void Random2Change(PerceptronFHandle handle, float d, float c3, const float* AField, const float* A2Field);


    PERCEPTRONF_API bool SaveWeights(PerceptronFHandle handle, const char* filename);
    PERCEPTRONF_API bool LoadWeights(PerceptronFHandle handle, const char* filename);
}


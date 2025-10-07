#pragma once

#ifdef PERCEPTRONAA_EXPORTS
#define PERCEPTRONAA_API __declspec(dllexport)
#else
#define PERCEPTRONAA_API __declspec(dllimport)
#endif

extern "C" 
{
    // Handle types
    typedef void* PerceptronAAHandle;

    // Creation and destruction
    PERCEPTRONAA_API PerceptronAAHandle CreatePerceptronAA(int sCount, int aCount, int rCount);
    PERCEPTRONAA_API void DisposePerceptronAA(PerceptronAAHandle handle);

    // Operations
    PERCEPTRONAA_API void SA(PerceptronAAHandle handle, int sIndex, int aIndex, float value);
    PERCEPTRONAA_API void AR(PerceptronAAHandle handle, int aIndex, int rIndex, float value);
    PERCEPTRONAA_API float AR_(PerceptronAAHandle handle, int aIndex, int rIndex);

    PERCEPTRONAA_API void AActivation(PerceptronAAHandle handle, const float* sField, float* aField);

    PERCEPTRONAA_API void RActivation(PerceptronAAHandle handle, const float* aField, float* rField);
    PERCEPTRONAA_API void LearnedStimulAR(PerceptronAAHandle handle, const float* reactionError, const float* aField);
}


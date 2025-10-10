// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


#pragma once

#ifdef PERCEPTROND_EXPORTS
#define PERCEPTROND_API __declspec(dllexport)
#else
#define PERCEPTROND_API __declspec(dllimport)
#endif

extern "C"
{
    // Handle types
    typedef void* PerceptronDHandle;

    // Creation and destruction
    PERCEPTROND_API PerceptronDHandle CreatePerceptronD(int sCount, int aCount, int rCount);
    PERCEPTROND_API void DisposePerceptronD(PerceptronDHandle handle);

    // Operations
    PERCEPTROND_API void SA(PerceptronDHandle handle, int sIndex, int aIndex, double value);
    PERCEPTROND_API void AR(PerceptronDHandle handle, int aIndex, int rIndex, double value);
    PERCEPTROND_API double AR_(PerceptronDHandle handle, int aIndex, int rIndex);

    PERCEPTROND_API void AActivation(PerceptronDHandle handle, const double* sField, double* aField);

    PERCEPTROND_API void RActivation(PerceptronDHandle handle, const double* aField, double* rField);
    PERCEPTROND_API void LearnedStimulAR(PerceptronDHandle handle, const double* reactionError, const double* aField);
}


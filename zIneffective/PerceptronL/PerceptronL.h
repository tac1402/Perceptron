// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

#pragma once

#ifdef PERCEPTRONL_EXPORTS
#define PERCEPTRONL_API __declspec(dllexport)
#else
#define PERCEPTRONL_API __declspec(dllimport)
#endif

extern "C" 
{
    // Handle types
    typedef void* PerceptronLHandle;

    // Creation and destruction
    PERCEPTRONL_API PerceptronLHandle CreatePerceptronL(int sCount, int aCount, int rCount);
    PERCEPTRONL_API void DisposePerceptronL(PerceptronLHandle handle);

    // Operations
    PERCEPTRONL_API void SA(PerceptronLHandle handle, int sIndex, int aIndex, float value);
    PERCEPTRONL_API void AR(PerceptronLHandle handle, int aIndex, int rIndex, float value);
    PERCEPTRONL_API float AR_(PerceptronLHandle handle, int aIndex, int rIndex);


    PERCEPTRONL_API void AActivation(PerceptronLHandle handle, float* sField, float* aField);
}



// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

#include "pch.h"

#include <vector>
//#include <cstring>

#include "PerceptronL.h"


class PerceptronL 
{
private:
    float* SAWeights;
    float* ARWeights;
    int SCount;
    int ACount;
    int RCount;

public:
    PerceptronL(int argSCount, int argACount, int argRCount) 
        : SCount(argSCount), ACount(argACount), RCount(argRCount)
    {
        SAWeights = static_cast<float*>(_mm_malloc(SCount * ACount * sizeof(float), 32));
        std::memset(SAWeights, 0, SCount * ACount * sizeof(float));

        ARWeights = static_cast<float*>(_mm_malloc(RCount * ACount * sizeof(float), 32));
        std::memset(ARWeights, 0, RCount * ACount * sizeof(float));
    }

    ~PerceptronL() 
    {
        _mm_free(SAWeights);
        _mm_free(ARWeights);
    }

    void SA(int SIndex, int AIndex, float value) 
    {
        SAWeights[SIndex * ACount + AIndex] += value;
    }

    void AR(int AIndex, int RIndex, float value)
    {
        ARWeights[RIndex * ACount + AIndex] += value;
    }

    float AR_(int AIndex, int RIndex)
    {
        return ARWeights[RIndex * ACount + AIndex];
    }

    void AActivation(float* SField, float* AField) 
    {
        std::memset(AField, 0, ACount * sizeof(float));
        AActivationLLVM(AField, SField, SAWeights);
    }

private:

    
    void AActivationLLVM(float* AField, float* SField, float* SAWeights2)
    {
        if (!AField || !SField || !SAWeights) return;

        int aCount = ACount;
        int sCount = SCount;

        // Обнуляем выходной массив
#pragma clang loop vectorize(enable)
        for (int j = 0; j < aCount; j++) 
        {
            AField[j] = 0.0f;
        }

        // Основной расчет без косвенной адресации
#pragma clang loop vectorize(enable) interleave(enable)
        for (int i = 0; i < sCount; i++) 
        {
            float s_val = SField[i];
            if (s_val == 0.0f) continue;  // Пропускаем нули

#pragma clang loop vectorize(enable)
            for (int j = 0; j < aCount; ++j) 
            {
                AField[j] += SAWeights2[i * aCount + j] * s_val;
            }
        }
    }

};

// C wrapper functions
extern "C" 
{
    PERCEPTRONL_API PerceptronLHandle CreatePerceptronL(int sCount, int aCount, int rCount)
    {
        try 
        {
            return new PerceptronL(sCount, aCount, rCount);
        }
        catch (...) 
        {
            return nullptr;
        }
    }

    PERCEPTRONL_API void DisposePerceptronL(PerceptronLHandle handle) 
    {
        if (handle) 
        {
            delete static_cast<PerceptronL*>(handle);
        }
    }

    PERCEPTRONL_API void SA(PerceptronLHandle handle, int sIndex, int aIndex, float value) 
    {
        if (handle) 
        {
            static_cast<PerceptronL*>(handle)->SA(sIndex, aIndex, value);
        }
    }

    PERCEPTRONL_API void AR(PerceptronLHandle handle, int aIndex, int rIndex, float value)
    {
        if (handle)
        {
            static_cast<PerceptronL*>(handle)->AR(aIndex, rIndex, value);
        }
    }

    PERCEPTRONL_API float AR_(PerceptronLHandle handle, int aIndex, int rIndex)
    {
        if (handle)
        {
            return static_cast<PerceptronL*>(handle)->AR_(aIndex, rIndex);
        }
        else
        {
            return 0;
        }
    }

    PERCEPTRONL_API void AActivation(PerceptronLHandle handle, float* sField, float* aField) 
    {
        if (handle && sField && aField) 
        {
            static_cast<PerceptronL*>(handle)->AActivation(sField, aField);
        }
    }
}
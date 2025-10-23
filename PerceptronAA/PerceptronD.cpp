// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


#include "pch.h"

#include "PerceptronD.h"
#include <immintrin.h>
#include <vector>
#include <cstring>
#include <memory>

class PerceptronD
{
private:
    double* SAWeights;
    double* ARWeights;
    int SCount;
    int ACount;
    int RCount;

public:
    PerceptronD(int argSCount, int argACount, int argRCount) : SCount(argSCount), ACount(argACount), RCount(argRCount)
    {
        SAWeights = static_cast<double*>(_mm_malloc(SCount * ACount * sizeof(double), 32));
        std::memset(SAWeights, 0, SCount * ACount * sizeof(double));

        ARWeights = static_cast<double*>(_mm_malloc(RCount * ACount * sizeof(double), 32));
        std::memset(ARWeights, 0, RCount * ACount * sizeof(double));
    }

    ~PerceptronD()
    {
        _mm_free(SAWeights);
        _mm_free(ARWeights);
    }

    void SA(int SIndex, int AIndex, double value)
    {
        SAWeights[SIndex * ACount + AIndex] += value;
    }

    void AR(int AIndex, int RIndex, double value)
    {
        ARWeights[RIndex * ACount + AIndex] += value;
    }

    double AR_(int AIndex, int RIndex)
    {
        return ARWeights[RIndex * ACount + AIndex];
    }

    void AActivation(const double* SField, double* AField)
    {
        std::memset(AField, 0, ACount * sizeof(double));
        AActivationAvx2(AField, SField);
    }

    void RActivation(const double* AField, double* RField)
    {
        std::memset(RField, 0, RCount * sizeof(double));
        RActivationAvx2(RField, AField);
    }

    void LearnedStimulAR(const double* ReactionError, const double* AField)
    {
        LearnedStimulARAvx2(ReactionError, AField);
    }

private:
    inline void AActivationAvx2(double* AField, const double* SField)
    {
        int j = 0;
        for (; j <= ACount - 4; j += 4)
        {
            __m256d sum0 = _mm256_setzero_pd();
            __m256d sum1 = _mm256_setzero_pd();

            int i = 0;
            for (; i <= SCount - 2; i += 2)
            {
                __m256d w0 = _mm256_load_pd(SAWeights + (i + 0) * ACount + j);
                __m256d w1 = _mm256_load_pd(SAWeights + (i + 1) * ACount + j);

                __m256d s0 = _mm256_broadcast_sd(SField + i + 0);
                __m256d s1 = _mm256_broadcast_sd(SField + i + 1);

                sum0 = _mm256_fmadd_pd(w0, s0, sum0);
                sum1 = _mm256_fmadd_pd(w1, s1, sum1);
            }

            __m256d finalSum = _mm256_add_pd(sum0, sum1);

            for (; i < SCount; i++)
            {
                __m256d sVector = _mm256_broadcast_sd(SField + i);
                __m256d weightsRow = _mm256_load_pd(SAWeights + i * ACount + j);
                finalSum = _mm256_fmadd_pd(weightsRow, sVector, finalSum);
            }

            if (j + 4 <= ACount)
            {
                _mm256_store_pd(AField + j, finalSum);
            }
            else
            {
                StoreVector(AField + j, finalSum, ACount - j);
            }
        }

        for (; j < ACount; j++)
        {
            double sum = 0.0f;
            for (int i = 0; i < SCount; i++)
            {
                sum += SAWeights[i * ACount + j] * SField[i];
            }
            AField[j] = sum;
        }
    }

    inline void RActivationAvx2(double* RField, const double* AField)
    {
        int j = 0;

        // Обрабатываем по 2 столбца R за раз
        for (; j <= RCount - 2; j += 2)
        {
            __m256d sum0 = _mm256_setzero_pd(); // Для R[j]
            __m256d sum1 = _mm256_setzero_pd(); // Для R[j+1]

            int i = 0;

            // Указатели на начала строк весов для j и j+1
            double* weightsRowJ = ARWeights + j * ACount;
            double* weightsRowJ1 = ARWeights + (j + 1) * ACount;

            // Обрабатываем по 8 элементов AField за раз
            for (; i <= ACount - 4; i += 4)
            {
                // Загружаем 8 элементов AField
                __m256d aVector = _mm256_load_pd(AField + i);

                // Создаем маску для AField > 0
                __m256d mask = _mm256_cmp_pd(aVector, _mm256_setzero_pd(), _CMP_GT_OQ);

                // Загружаем веса для текущего j и j+1
                __m256d w0 = _mm256_load_pd(weightsRowJ + i);
                __m256d w1 = _mm256_load_pd(weightsRowJ1 + i);

                // Применяем маску к весам (AND с маской)
                __m256d maskedW0 = _mm256_and_pd(w0, mask);
                __m256d maskedW1 = _mm256_and_pd(w1, mask);

                // Суммируем
                sum0 = _mm256_add_pd(sum0, maskedW0);
                sum1 = _mm256_add_pd(sum1, maskedW1);
            }

            // Горизонтальное суммирование для sum0 (R[j])
            __m128d sum0_128 = _mm_add_pd(_mm256_extractf128_pd(sum0, 1), _mm256_castpd256_pd128(sum0));
            double result0 = _mm_cvtsd_f64(sum0_128) + _mm_cvtsd_f64(_mm_unpackhi_pd(sum0_128, sum0_128));

            // Горизонтальное суммирование для sum1 (R[j+1])
            __m128d sum1_128 = _mm_add_pd(_mm256_extractf128_pd(sum1, 1), _mm256_castpd256_pd128(sum1));
            double result1 = _mm_cvtsd_f64(sum1_128) + _mm_cvtsd_f64(_mm_unpackhi_pd(sum1_128, sum1_128));

            // Скалярная обработка оставшихся элементов AField
            for (; i < ACount; i++)
            {
                if (AField[i] > 0)
                {
                    result0 += weightsRowJ[i];
                    result1 += weightsRowJ1[i];
                }
            }

            // Сохраняем результаты
            RField[j] = result0;
            RField[j + 1] = result1;
        }

        // Обрабатываем оставшийся столбец R (если RCount нечетное)
        for (; j < RCount; j++)
        {
            double sum = 0.0f;
            double* weightsRow = ARWeights + j * ACount;

            for (int i = 0; i < ACount; i++)
            {
                if (AField[i] > 0)
                {
                    sum += weightsRow[i];
                }
            }
            RField[j] = sum;
        }
    }

    // РЕАЛИЗАЦИЯ LearnedStimulARAvx2
    inline void LearnedStimulARAvx2(const double* ReactionError, const double* AField)
    {
        int j = 0;

        // Обрабатываем по 2 столбца R за раз
        for (; j <= RCount - 2; j += 2)
        {
            double error0 = ReactionError[j];
            double error1 = ReactionError[j + 1];

            __m256d errorVec0 = _mm256_set1_pd(error0); // Вектор из 8 одинаковых error0
            __m256d errorVec1 = _mm256_set1_pd(error1); // Вектор из 8 одинаковых error1

            // Указатели на начала строк весов для j и j+1
            double* weightsRow0 = ARWeights + j * ACount;
            double* weightsRow1 = ARWeights + (j + 1) * ACount;

            int i = 0;

            // Обрабатываем по 8 элементов AField за раз
            for (; i <= ACount - 4; i += 4)
            {
                // Загружаем 8 элементов AField
                __m256d aVector = _mm256_load_pd(AField + i);

                // Создаем маску для AField > 0
                __m256d mask = _mm256_cmp_pd(aVector, _mm256_setzero_pd(), _CMP_GT_OQ);

                // Загружаем текущие веса для j и j+1
                __m256d w0 = _mm256_load_pd(weightsRow0 + i);
                __m256d w1 = _mm256_load_pd(weightsRow1 + i);

                // Добавляем error к весам, где AField > 0
                __m256d newW0 = _mm256_add_pd(w0, errorVec0);
                __m256d newW1 = _mm256_add_pd(w1, errorVec1);

                // Blend: где mask=true - берем newW, иначе оставляем старый w
                __m256d result0 = _mm256_blendv_pd(w0, newW0, mask);
                __m256d result1 = _mm256_blendv_pd(w1, newW1, mask);

                // Сохраняем обновленные веса
                _mm256_store_pd(weightsRow0 + i, result0);
                _mm256_store_pd(weightsRow1 + i, result1);
            }

            // Скалярная обработка оставшихся элементов
            for (; i < ACount; i++)
            {
                if (AField[i] > 0)
                {
                    weightsRow0[i] += error0;
                    weightsRow1[i] += error1;
                }
            }
        }

        // Обрабатываем оставшийся столбец R (если RCount нечетное)
        for (; j < RCount; j++)
        {
            double error = ReactionError[j];
            __m256d errorVec = _mm256_set1_pd(error);
            double* weightsRow = ARWeights + j * ACount;

            int i = 0;
            for (; i <= ACount - 4; i += 4)
            {
                __m256d aVector = _mm256_load_pd(AField + i);
                __m256d mask = _mm256_cmp_pd(aVector, _mm256_setzero_pd(), _CMP_GT_OQ);
                __m256d w = _mm256_load_pd(weightsRow + i);
                __m256d newW = _mm256_add_pd(w, errorVec);
                __m256d result = _mm256_blendv_pd(w, newW, mask);
                _mm256_store_pd(weightsRow + i, result);
            }

            for (; i < ACount; i++)
            {
                if (AField[i] > 0)
                {
                    weightsRow[i] += error;
                }
            }
        }
    }



    inline void StoreVector(double* dest, __m256d data, int count)
    {
        alignas(32) double temp[4];
        _mm256_store_pd(temp, data);
        std::memcpy(dest, temp, count * sizeof(double));
    }
};

// C wrapper functions
extern "C"
{
    PERCEPTROND_API PerceptronDHandle CreatePerceptronD(int sCount, int aCount, int rCount)
    {
        try
        {
            return new PerceptronD(sCount, aCount, rCount);
        }
        catch (...)
        {
            return nullptr;
        }
    }

    PERCEPTROND_API void DisposePerceptronD(PerceptronDHandle handle)
    {
        if (handle)
        {
            delete static_cast<PerceptronD*>(handle);
        }
    }

    PERCEPTROND_API void SAd(PerceptronDHandle handle, int sIndex, int aIndex, double value)
    {
        if (handle)
        {
            static_cast<PerceptronD*>(handle)->SA(sIndex, aIndex, value);
        }
    }

    PERCEPTROND_API void ARd(PerceptronDHandle handle, int aIndex, int rIndex, double value)
    {
        if (handle)
        {
            static_cast<PerceptronD*>(handle)->AR(aIndex, rIndex, value);
        }
    }

    PERCEPTROND_API double AR_d(PerceptronDHandle handle, int aIndex, int rIndex)
    {
        if (handle)
        {
            return static_cast<PerceptronD*>(handle)->AR_(aIndex, rIndex);
        }
    }


    PERCEPTROND_API void AActivation_d(PerceptronDHandle handle, const double* sField, double* aField)
    {
        if (handle && sField && aField)
        {
            static_cast<PerceptronD*>(handle)->AActivation(sField, aField);
        }
    }

    PERCEPTROND_API void RActivation_d(PerceptronDHandle handle, const double* aField, double* rField)
    {
        if (handle && aField && rField)
        {
            static_cast<PerceptronD*>(handle)->RActivation(aField, rField);
        }
    }

    PERCEPTROND_API void LearnedStimulAR_d(PerceptronDHandle handle, const double* reactionError, const double* aField)
    {
        if (handle && reactionError && aField)
        {
            static_cast<PerceptronD*>(handle)->LearnedStimulAR(reactionError, aField);
        }
    }
}
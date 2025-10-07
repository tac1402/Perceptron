#include "pch.h"

#include "PerceptronAA.h"
#include <immintrin.h>
#include <vector>
#include <cstring>
#include <memory>

class PerceptronAAImpl 
{
private:
    float* SAWeights;
    float* ARWeights;
    int SCount;
    int ACount;
    int RCount;

public:
    PerceptronAAImpl(int argSCount, int argACount, int argRCount) : SCount(argSCount), ACount(argACount), RCount(argRCount) 
    {
        SAWeights = static_cast<float*>(_mm_malloc(SCount * ACount * sizeof(float), 32));
        std::memset(SAWeights, 0, SCount * ACount * sizeof(float));

        ARWeights = static_cast<float*>(_mm_malloc(RCount * ACount * sizeof(float), 32));
        std::memset(ARWeights, 0, RCount * ACount * sizeof(float));
    }

    ~PerceptronAAImpl() 
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

    void AActivation(const float* SField, float* AField) 
    {
        std::memset(AField, 0, ACount * sizeof(float));
        AActivationAvx2(AField, SField);
    }

    void RActivation(const float* AField, float* RField) 
    {
        std::memset(RField, 0, RCount * sizeof(float));
        RActivationAvx2(RField, AField);
    }

    void LearnedStimulAR(const float* ReactionError, const float* AField) 
    {
        LearnedStimulARAvx2(ReactionError, AField);
    }

private:
    inline void AActivationAvx2(float* AField, const float* SField) 
    {
        int j = 0;
        for (; j <= ACount - 8; j += 8) 
        {
            __m256 sum0 = _mm256_setzero_ps();
            __m256 sum1 = _mm256_setzero_ps();

            int i = 0;
            for (; i <= SCount - 2; i += 2) 
            {
                __m256 w0 = _mm256_load_ps(SAWeights + (i + 0) * ACount + j);
                __m256 w1 = _mm256_load_ps(SAWeights + (i + 1) * ACount + j);

                __m256 s0 = _mm256_broadcast_ss(SField + i + 0);
                __m256 s1 = _mm256_broadcast_ss(SField + i + 1);

                sum0 = _mm256_fmadd_ps(w0, s0, sum0);
                sum1 = _mm256_fmadd_ps(w1, s1, sum1);
            }

            __m256 finalSum = _mm256_add_ps(sum0, sum1);

            for (; i < SCount; i++) 
            {
                __m256 sVector = _mm256_broadcast_ss(SField + i);
                __m256 weightsRow = _mm256_load_ps(SAWeights + i * ACount + j);
                finalSum = _mm256_fmadd_ps(weightsRow, sVector, finalSum);
            }

            if (j + 8 <= ACount) 
            {
                _mm256_store_ps(AField + j, finalSum);
            }
            else 
            {
                StoreVector(AField + j, finalSum, ACount - j);
            }
        }

        for (; j < ACount; j++) 
        {
            float sum = 0.0f;
            for (int i = 0; i < SCount; i++) 
            {
                sum += SAWeights[i * ACount + j] * SField[i];
            }
            AField[j] = sum;
        }
    }

    inline void RActivationAvx2(float* RField, const float* AField) 
    {
        int j = 0;

        // Обрабатываем по 2 столбца R за раз
        for (; j <= RCount - 2; j += 2) 
        {
            __m256 sum0 = _mm256_setzero_ps(); // Для R[j]
            __m256 sum1 = _mm256_setzero_ps(); // Для R[j+1]

            int i = 0;

            // Указатели на начала строк весов для j и j+1
            float* weightsRowJ = ARWeights + j * ACount;
            float* weightsRowJ1 = ARWeights + (j + 1) * ACount;

            // Обрабатываем по 8 элементов AField за раз
            for (; i <= ACount - 8; i += 8) 
            {
                // Загружаем 8 элементов AField
                __m256 aVector = _mm256_load_ps(AField + i);

                // Создаем маску для AField > 0
                __m256 mask = _mm256_cmp_ps(aVector, _mm256_setzero_ps(), _CMP_GT_OQ);

                // Загружаем веса для текущего j и j+1
                __m256 w0 = _mm256_load_ps(weightsRowJ + i);
                __m256 w1 = _mm256_load_ps(weightsRowJ1 + i);

                // Применяем маску к весам (AND с маской)
                __m256 maskedW0 = _mm256_and_ps(w0, mask);
                __m256 maskedW1 = _mm256_and_ps(w1, mask);

                // Суммируем
                sum0 = _mm256_add_ps(sum0, maskedW0);
                sum1 = _mm256_add_ps(sum1, maskedW1);
            }

            // Горизонтальное суммирование для sum0 (R[j])
            __m128 sum0_128 = _mm_add_ps(_mm256_extractf128_ps(sum0, 1), _mm256_castps256_ps128(sum0));
            sum0_128 = _mm_add_ps(sum0_128, _mm_movehl_ps(sum0_128, sum0_128));
            sum0_128 = _mm_add_ss(sum0_128, _mm_movehdup_ps(sum0_128));
            float result0 = _mm_cvtss_f32(sum0_128);

            // Горизонтальное суммирование для sum1 (R[j+1])
            __m128 sum1_128 = _mm_add_ps(_mm256_extractf128_ps(sum1, 1), _mm256_castps256_ps128(sum1));
            sum1_128 = _mm_add_ps(sum1_128, _mm_movehl_ps(sum1_128, sum1_128));
            sum1_128 = _mm_add_ss(sum1_128, _mm_movehdup_ps(sum1_128));
            float result1 = _mm_cvtss_f32(sum1_128);

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
            float sum = 0.0f;
            float* weightsRow = ARWeights + j * ACount;

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
    inline void LearnedStimulARAvx2(const float* ReactionError, const float* AField) 
    {
        int j = 0;

        // Обрабатываем по 2 столбца R за раз
        for (; j <= RCount - 2; j += 2) 
        {
            float error0 = ReactionError[j];
            float error1 = ReactionError[j + 1];

            __m256 errorVec0 = _mm256_set1_ps(error0); // Вектор из 8 одинаковых error0
            __m256 errorVec1 = _mm256_set1_ps(error1); // Вектор из 8 одинаковых error1

            // Указатели на начала строк весов для j и j+1
            float* weightsRow0 = ARWeights + j * ACount;
            float* weightsRow1 = ARWeights + (j + 1) * ACount;

            int i = 0;

            // Обрабатываем по 8 элементов AField за раз
            for (; i <= ACount - 8; i += 8) 
            {
                // Загружаем 8 элементов AField
                __m256 aVector = _mm256_load_ps(AField + i);

                // Создаем маску для AField > 0
                __m256 mask = _mm256_cmp_ps(aVector, _mm256_setzero_ps(), _CMP_GT_OQ);

                // Загружаем текущие веса для j и j+1
                __m256 w0 = _mm256_load_ps(weightsRow0 + i);
                __m256 w1 = _mm256_load_ps(weightsRow1 + i);

                // Добавляем error к весам, где AField > 0
                __m256 newW0 = _mm256_add_ps(w0, errorVec0);
                __m256 newW1 = _mm256_add_ps(w1, errorVec1);

                // Blend: где mask=true - берем newW, иначе оставляем старый w
                __m256 result0 = _mm256_blendv_ps(w0, newW0, mask);
                __m256 result1 = _mm256_blendv_ps(w1, newW1, mask);

                // Сохраняем обновленные веса
                _mm256_store_ps(weightsRow0 + i, result0);
                _mm256_store_ps(weightsRow1 + i, result1);
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
            float error = ReactionError[j];
            __m256 errorVec = _mm256_set1_ps(error);
            float* weightsRow = ARWeights + j * ACount;

            int i = 0;
            for (; i <= ACount - 8; i += 8) 
            {
                __m256 aVector = _mm256_load_ps(AField + i);
                __m256 mask = _mm256_cmp_ps(aVector, _mm256_setzero_ps(), _CMP_GT_OQ);
                __m256 w = _mm256_load_ps(weightsRow + i);
                __m256 newW = _mm256_add_ps(w, errorVec);
                __m256 result = _mm256_blendv_ps(w, newW, mask);
                _mm256_store_ps(weightsRow + i, result);
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



    inline void StoreVector(float* dest, __m256 data, int count) 
    {
        alignas(32) float temp[8];
        _mm256_store_ps(temp, data);
        std::memcpy(dest, temp, count * sizeof(float));
    }
};

// C wrapper functions
extern "C" 
{
    PERCEPTRONAA_API PerceptronAAHandle CreatePerceptronAA(int sCount, int aCount, int rCount) 
    {
        try 
        {
            return new PerceptronAAImpl(sCount, aCount, rCount);
        }
        catch (...) 
        {
            return nullptr;
        }
    }

    PERCEPTRONAA_API void DisposePerceptronAA(PerceptronAAHandle handle) 
    {
        if (handle) 
        {
            delete static_cast<PerceptronAAImpl*>(handle);
        }
    }

    PERCEPTRONAA_API void SA(PerceptronAAHandle handle, int sIndex, int aIndex, float value) 
    {
        if (handle) 
        {
            static_cast<PerceptronAAImpl*>(handle)->SA(sIndex, aIndex, value);
        }
    }

    PERCEPTRONAA_API void AR(PerceptronAAHandle handle, int aIndex, int rIndex, float value)
    {
        if (handle)
        {
            static_cast<PerceptronAAImpl*>(handle)->AR(aIndex, rIndex, value);
        }
    }

    PERCEPTRONAA_API float AR_(PerceptronAAHandle handle, int aIndex, int rIndex)
    {
        if (handle)
        {
            return static_cast<PerceptronAAImpl*>(handle)->AR_(aIndex, rIndex);
        }
    }


    PERCEPTRONAA_API void AActivation(PerceptronAAHandle handle, const float* sField, float* aField) 
    {
        if (handle && sField && aField) 
        {
            static_cast<PerceptronAAImpl*>(handle)->AActivation(sField, aField);
        }
    }

    PERCEPTRONAA_API void RActivation(PerceptronAAHandle handle, const float* aField, float* rField) 
    {
        if (handle && aField && rField) 
        {
            static_cast<PerceptronAAImpl*>(handle)->RActivation(aField, rField);
        }
    }

    PERCEPTRONAA_API void LearnedStimulAR(PerceptronAAHandle handle, const float* reactionError, const float* aField) 
    {
        if (handle && reactionError && aField) 
        {
            static_cast<PerceptronAAImpl*>(handle)->LearnedStimulAR(reactionError, aField);
        }
    }
}
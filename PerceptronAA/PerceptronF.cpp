// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


#include "pch.h"

#include "PerceptronF.h"
#include <immintrin.h>
#include <cstring>
#include <memory>
#include <cmath>
#include <random>
#include <fstream>

class PerceptronF 
{
private:
    float* SAWeights;
    float* AAWeights;
    float* ARWeights;
    int SCount;
    int ACount;
    int A2Count;
    int RCount;

    // Random number generator
    std::mt19937 rnd;
    std::uniform_real_distribution<float> dist;

public:
    PerceptronF(int argSCount, int argACount, int argRCount, int argA2Count)
        : SCount(argSCount), ACount(argACount), RCount(argRCount), A2Count(argA2Count), rnd(24), dist(0.0f, 1.0f)
    {
        SAWeights = static_cast<float*>(_mm_malloc(SCount * ACount * sizeof(float), 32));
        std::memset(SAWeights, 0, SCount * ACount * sizeof(float));

        if (A2Count == 0)
        {
            ARWeights = static_cast<float*>(_mm_malloc(RCount * ACount * sizeof(float), 32));
            std::memset(ARWeights, 0, RCount * ACount * sizeof(float));
        }
        else
        {
            AAWeights = static_cast<float*>(_mm_malloc(ACount * A2Count * sizeof(float), 32));
            std::memset(AAWeights, 0, ACount * A2Count * sizeof(float));

            ARWeights = static_cast<float*>(_mm_malloc(RCount * A2Count * sizeof(float), 32));
            std::memset(ARWeights, 0, RCount * A2Count * sizeof(float));
        }

    }

    ~PerceptronF() 
    {
        _mm_free(SAWeights);
        _mm_free(AAWeights);
        _mm_free(ARWeights);
    }

    void SA(int SIndex, int AIndex, float value) 
    {
        SAWeights[SIndex * ACount + AIndex] += value;
    }

    void SetSA(int SIndex, int AIndex, float value)
    {
        SAWeights[SIndex * ACount + AIndex] = value;
    }

    float SA_(int SIndex, int AIndex)
    {
        return SAWeights[SIndex * ACount + AIndex];
    }


    void AA(int AIndex, int A2Index, float value)
    {
        AAWeights[AIndex * A2Count + A2Index] += value;
    }

    void AR(int AIndex, int RIndex, float value)
    {
        ARWeights[RIndex * ACount + AIndex] += value;
    }

    float AR_(int AIndex, int RIndex)
    {
        return ARWeights[RIndex * ACount + AIndex];
    }

    void AActivation(const float* SField, float* AField, int startA, int endA)
    {
        if (endA == -1)
        {
            std::memset(AField, 0, ACount * sizeof(float));
        }
        else
        {
            std::memset(AField, 0, (endA - startA) * sizeof(float));
        }

        AActivationAvx2(AField, SField, SCount, ACount, SAWeights, startA, endA);

        //AActivationAvx2_New(AField, SField, SCount, ACount, SAWeights);
    }

    // Активация второго слоя
    void A2Activation(const float* AField, float* A2Field)
    {
        std::memset(A2Field, 0, A2Count * sizeof(float));
        AActivationAvx2(A2Field, AField, ACount, A2Count, AAWeights, 0, A2Count);
    }

    // Активация R слоя при одном А-слое
    void RActivation(const float* AField, float* RField)
    {
        std::memset(RField, 0, RCount * sizeof(float));
        RActivationAvx2(RField, AField, ACount, RCount, ARWeights);
    }

    // Активация R слоя при двух А-слоев
    void R2Activation(const float* A2Field, float* RField)
    {
        std::memset(RField, 0, RCount * sizeof(float));
        RActivationAvx2(RField, A2Field, A2Count, RCount, ARWeights);
    }

    void LearnedStimulAR(const float* ReactionError, const float* AField) 
    {
        if (A2Count == 0)
        {
            LearnedStimulARAvx2(ReactionError, AField, ACount, RCount, ARWeights);
        }
        else
        {
            LearnedStimulARAvx2(ReactionError, AField, A2Count, RCount, ARWeights);
        }
    }

    void LearnedStimulSA(const float* ReactionError, const float* AField, const float* AFieldNorm)
    {
        LearnedStimulSAAvx2(ReactionError, AField, AFieldNorm, SCount, ACount, RCount, SAWeights, ARWeights, NULL);
    }

    void LearnedStimul2SA(const float* ReactionError, const float* AField, const float* AFieldNorm)
    {
        LearnedStimulSAAvx2(ReactionError, AField, AFieldNorm, SCount, ACount, A2Count, SAWeights, AAWeights, NULL);
    }
    void LearnedStimul2AA(const float* ReactionError, const float* A2Field, const float* A2FieldNorm, float* retUpdates)
    {
        LearnedStimulSAAvx2(ReactionError, A2Field, A2FieldNorm, ACount, A2Count, RCount, AAWeights, ARWeights, retUpdates);
    }


    void RandomChange(float d, float c3, const float* AField)
    {
        RandomChangeAVX2(d, c3, AField, SCount, ACount, RCount, SAWeights);
    }

    void Random2Change(float d, float c3, const float* AField, const float* A2Field)
    {
        RandomChangeAVX2(d, c3, AField, SCount, ACount, A2Count, SAWeights);
        RandomChangeAVX2(d, c3, A2Field, ACount, A2Count, RCount, AAWeights);
    }

    void Normalize(const float* AField, float* retAFieldNorm)
    {
        NormalizeAvx2(AField, ACount, retAFieldNorm);
    }


    // Функция для сохранения весов в бинарный файл
    bool SaveWeights(const char* filename)
    {
        std::ofstream file(filename, std::ios::binary);
        if (!file.is_open())
        {
            return false;
        }

        // Записываем размерности массивов
        file.write(reinterpret_cast<const char*>(&SCount), sizeof(SCount));
        file.write(reinterpret_cast<const char*>(&ACount), sizeof(ACount));
        file.write(reinterpret_cast<const char*>(&RCount), sizeof(RCount));

        // Записываем данные массивов
        size_t sa_size = SCount * ACount;
        size_t ar_size = RCount * ACount;

        file.write(reinterpret_cast<const char*>(SAWeights), sa_size * sizeof(float));
        file.write(reinterpret_cast<const char*>(ARWeights), ar_size * sizeof(float));

        if (!file.good())
        {
            file.close();
            return false;
        }

        file.close();
        return true;
    }

    int LoadWeights(const char* filename)
    {
        std::ifstream file(filename, std::ios::binary);
        if (!file.is_open()) { return 0; }

        // Читаем размерности массивов из файла
        int file_SCount, file_ACount, file_RCount;
        file.read(reinterpret_cast<char*>(&file_SCount), sizeof(file_SCount));
        file.read(reinterpret_cast<char*>(&file_ACount), sizeof(file_ACount));
        file.read(reinterpret_cast<char*>(&file_RCount), sizeof(file_RCount));

        // Проверяем базовые условия для первого массива
        if (file_SCount != SCount || file_ACount != ACount)
        {
            file.close();
            return 0;
        }

        // Определяем необходимость загрузки второго массива
        bool loadBoth = (file_RCount == RCount);
        size_t sa_size = SCount * ACount;
        size_t ar_size = RCount * ACount;

        // Читаем первый массив
        file.read(reinterpret_cast<char*>(SAWeights), sa_size * sizeof(float));
        if (!file.good())
        {
            file.close();
            return 0;
        }

        int loadedCount = 1; // Первый массив загружен успешно

        // Читаем второй массив если нужно
        if (loadBoth)
        {
            file.read(reinterpret_cast<char*>(ARWeights), ar_size * sizeof(float));
            if (!file.good())
            {
                file.close();
                return 1; // Возвращаем 1, так как первый массив уже загружен
            }
            loadedCount = 2; // Оба массива загружены
        }

        file.close();
        return loadedCount;
    }


private:

    inline void AActivationAvx2_New(float* argAField, const float* argSField,
        int argSCount, int argACount, float* argWeight)
    {
        // Предварительное вычисление ненулевых элементов
        std::vector<int> nonZeroS;
        nonZeroS.reserve(argSCount);
        for (int i = 0; i < argSCount; ++i)
        {
            if (argSField[i] != 0.0f)
            {
                nonZeroS.push_back(i);
            }
        }
        const int nonZeroCount = static_cast<int>(nonZeroS.size());

        // Оптимизация: предвычисление указателей на строки весов
        std::vector<const float*> weightPtrs;
        std::vector<float> sValues;
        weightPtrs.reserve(nonZeroCount);
        sValues.reserve(nonZeroCount);

        for (int idx = 0; idx < nonZeroCount; ++idx)
        {
            const int i = nonZeroS[idx];
            weightPtrs.push_back(argWeight + i * argACount);
            sValues.push_back(argSField[i]);
        }

        int j = 0;
        for (; j <= argACount - 8; j += 8)
        {
            __m256 sum = _mm256_setzero_ps();

            // Обрабатываем по 4 элемента за итерацию
            int idx = 0;
            for (; idx <= nonZeroCount - 4; idx += 4)
            {
                // Загрузка 4 весовых строк
                __m256 w0 = _mm256_load_ps(weightPtrs[idx] + j);
                __m256 w1 = _mm256_load_ps(weightPtrs[idx + 1] + j);
                __m256 w2 = _mm256_load_ps(weightPtrs[idx + 2] + j);
                __m256 w3 = _mm256_load_ps(weightPtrs[idx + 3] + j);

                // Broadcast значений S
                __m256 s0 = _mm256_set1_ps(sValues[idx]);
                __m256 s1 = _mm256_set1_ps(sValues[idx + 1]);
                __m256 s2 = _mm256_set1_ps(sValues[idx + 2]);
                __m256 s3 = _mm256_set1_ps(sValues[idx + 3]);

                // FMA операции
                sum = _mm256_fmadd_ps(w0, s0, sum);
                sum = _mm256_fmadd_ps(w1, s1, sum);
                sum = _mm256_fmadd_ps(w2, s2, sum);
                sum = _mm256_fmadd_ps(w3, s3, sum);
            }

            // Оставшиеся элементы
            for (; idx < nonZeroCount; ++idx)
            {
                __m256 w = _mm256_load_ps(weightPtrs[idx] + j);
                __m256 s = _mm256_set1_ps(sValues[idx]);
                sum = _mm256_fmadd_ps(w, s, sum);
            }

            _mm256_store_ps(argAField + j, sum);
        }

        // Скалярная обработка хвоста
        for (; j < argACount; j++)
        {
            float sum = 0.0f;
            for (int idx = 0; idx < nonZeroCount; idx++)
            {
                sum += weightPtrs[idx][j] * sValues[idx];
            }
            argAField[j] = sum;
        }
    }


    inline void AActivationAvx2(float* argAField, const float* argSField, int argSCount, int argACount, float* argWeight,
        int startA, int endA)
    { 
        if (endA == -1) endA = argACount;

        // Собираем индексы ненулевых элементов SField
        std::vector<int> nonZeroS;
        nonZeroS.reserve(argSCount);
        for (int i = 0; i < argSCount; ++i)
        {
            if (argSField[i] != 0.0f)
            {
                nonZeroS.push_back(i);
            }
        }
        const int nonZeroCount = static_cast<int>(nonZeroS.size());

        int j = startA;
        for (; j <= endA - 8; j += 8)
        {
            __m256 sum0 = _mm256_setzero_ps();
            __m256 sum1 = _mm256_setzero_ps();

            int idx = 0;
            // Обрабатываем по два ненулевых элемента за итерацию
            for (; idx <= nonZeroCount - 2; idx += 2)
            {
                const int i0 = nonZeroS[idx];
                const int i1 = nonZeroS[idx + 1];

                __m256 w0 = _mm256_load_ps(argWeight + i0 * argACount + j);
                __m256 w1 = _mm256_load_ps(argWeight + i1 * argACount + j);

                __m256 s0 = _mm256_broadcast_ss(argSField + i0);
                __m256 s1 = _mm256_broadcast_ss(argSField + i1);

                sum0 = _mm256_fmadd_ps(w0, s0, sum0);
                sum1 = _mm256_fmadd_ps(w1, s1, sum1);
            }

            __m256 finalSum = _mm256_add_ps(sum0, sum1);

            // Обрабатываем оставшиеся ненулевые элементы
            for (; idx < nonZeroCount; ++idx)
            {
                const int i = nonZeroS[idx];
                __m256 sVector = _mm256_broadcast_ss(argSField + i);
                __m256 weightsRow = _mm256_load_ps(argWeight + i * argACount + j);
                finalSum = _mm256_fmadd_ps(weightsRow, sVector, finalSum);
            }

            // Сохраняем результат для векторизованной части
            if (j + 8 <= endA)
            {
                _mm256_store_ps(argAField + (j - startA), finalSum);
            }
            else
            {
                StoreVector(argAField + (j - startA), finalSum, endA - j);
            }
        }

        // Скалярная обработка оставшихся j
        for (; j < endA; j++)
        {
            float sum = 0.0f;
            for (int idx = 0; idx < nonZeroCount; idx++)
            {
                const int i = nonZeroS[idx];
                sum += argWeight[i * argACount + j] * argSField[i];
            }

            argAField[j - startA] = sum;
        }
    }

    inline void RActivationAvx2(float* argRField, const float* argAField, int argACount, int argRCount, float* argWeight, float threshold = 0.0f)
    {
        int j = 0;

        // Создаем AVX вектор с пороговым значением
        const __m256 threshold_vec = _mm256_set1_ps(threshold);

        // Обрабатываем по 2 столбца R за раз
        for (; j <= argRCount - 2; j += 2)
        {
            __m256 sum0 = _mm256_setzero_ps(); // Для R[j]
            __m256 sum1 = _mm256_setzero_ps(); // Для R[j+1]

            int i = 0;

            // Указатели на начала строк весов для j и j+1
            float* weightsRowJ = argWeight + j * argACount;
            float* weightsRowJ1 = argWeight + (j + 1) * argACount;

            // Обрабатываем по 8 элементов AField за раз
            for (; i <= argACount - 8; i += 8)
            {
                // Загружаем 8 элементов AField
                __m256 aVector = _mm256_load_ps(argAField + i);

                // Создаем маску для AField > 0
                __m256 mask = _mm256_cmp_ps(aVector, threshold_vec, _CMP_GT_OQ);

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
            for (; i < argACount; i++)
            {
                if (argAField[i] > threshold)
                {
                    result0 += weightsRowJ[i];
                    result1 += weightsRowJ1[i];
                }
            }

            // Сохраняем результаты
            argRField[j] = result0;
            argRField[j + 1] = result1;
        }

        // Обрабатываем оставшийся столбец R (если RCount нечетное)
        for (; j < argRCount; j++)
        {
            float sum = 0.0f;
            float* weightsRow = argWeight + j * argACount;

            for (int i = 0; i < argACount; i++)
            {
                if (argAField[i] > threshold)
                {
                    sum += weightsRow[i];
                }
            }
            argRField[j] = sum;
        }
    }

    inline void LearnedStimulARAvx2(const float* argReactionError, const float* argAField, int argACount, int argRCount, float* argWeight)
    {
        int j = 0;

        // Обрабатываем по 2 столбца R за раз
        for (; j <= argRCount - 2; j += 2)
        {
            float error0 = argReactionError[j];
            float error1 = argReactionError[j + 1];

            __m256 errorVec0 = _mm256_set1_ps(error0); // Вектор из 8 одинаковых error0
            __m256 errorVec1 = _mm256_set1_ps(error1); // Вектор из 8 одинаковых error1

            // Указатели на начала строк весов для j и j+1
            float* weightsRow0 = argWeight + j * argACount;
            float* weightsRow1 = argWeight + (j + 1) * argACount;

            int i = 0;

            // Обрабатываем по 8 элементов AField за раз
            for (; i <= argACount - 8; i += 8)
            {
                // Загружаем 8 элементов AField
                __m256 aVector = _mm256_load_ps(argAField + i);

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
            for (; i < argACount; i++)
            {
                if (argAField[i] > 0) 
                {
                    weightsRow0[i] += error0;
                    weightsRow1[i] += error1;
                }
            }
        }

        // Обрабатываем оставшийся столбец R (если RCount нечетное)
        for (; j < argRCount; j++)
        {
            float error = argReactionError[j];
            __m256 errorVec = _mm256_set1_ps(error);
            float* weightsRow = argWeight + j * argACount;

            int i = 0;
            for (; i <= argACount - 8; i += 8)
            {
                __m256 aVector = _mm256_load_ps(argAField + i);
                __m256 mask = _mm256_cmp_ps(aVector, _mm256_setzero_ps(), _CMP_GT_OQ);
                __m256 w = _mm256_load_ps(weightsRow + i);
                __m256 newW = _mm256_add_ps(w, errorVec);
                __m256 result = _mm256_blendv_ps(w, newW, mask);
                _mm256_store_ps(weightsRow + i, result);
            }

            for (; i < argACount; i++)
            {
                if (argAField[i] > 0) 
                {
                    weightsRow[i] += error;
                }
            }
        }
    }


    inline void RandomChangeAVX2(float d, float c3, const float* argAField, int argSCount, int argACount, int argRCount, float* argWeight)
    {
        // Создаем маску для условий AField[j] <= th
        std::vector<float> conditionMask(argACount, 0.0f);
        for (int j = 0; j < argACount; j++)
        {
            conditionMask[j] = (argAField[j] <= 0) ? 1.0f : 0.0f;
        }

        // AVX2 константы
        const __m256 dVec = _mm256_set1_ps(d);
        const __m256 correct3Vec = _mm256_set1_ps(c3);

        //for (int r = 0; r < argRCount; r++)
        {
            for (int i = 0; i < argSCount; i++)
            {
                int j = 0;

                // Векторизованная обработка по 8 элементов j
                for (; j <= argACount - 8; j += 8)
                {
                    // Загружаем маску условий для 8 элементов j
                    __m256 conditionMaskVec = _mm256_loadu_ps(&conditionMask[j]);

                    // Генерируем 8 случайных чисел
                    float randoms[8];
                    for (int k = 0; k < 8; k++) 
                    {
                        randoms[k] = dist(rnd);
                    }
                    __m256 randVec = _mm256_loadu_ps(randoms);

                    // Создаем маску: случайное число < d И условие AField[j] <= th
                    __m256 probMask = _mm256_cmp_ps(randVec, dVec, _CMP_LT_OQ);
                    __m256 finalMask = _mm256_and_ps(probMask, conditionMaskVec);

                    // Загружаем веса для текущего i и 8 элементов j
                    // Веса расположены последовательно: SAWeights[i * ACount + j] до SAWeights[i * ACount + j+7]
                    __m256 weights = _mm256_loadu_ps(&argWeight[i * argACount + j]);

                    // Добавляем коррекцию только где finalMask истинен
                    __m256 correction = _mm256_and_ps(finalMask, correct3Vec);
                    weights = _mm256_add_ps(weights, correction);

                    // Сохраняем обратно
                    _mm256_storeu_ps(&argWeight[i * argACount + j], weights);
                }

                // Скалярная обработка хвоста
                for (; j < argACount; j++)
                {
                    if (conditionMask[j] != 0.0f && dist(rnd) < d) 
                    {
                        argWeight[i * argACount + j] += c3;
                    }
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

    inline int sign(float x) {
        return (x > 0.0f) ? 1 : (x < 0.0f) ? -1 : 0;
    }

    inline void LearnedStimulSAAvx2(const float* ReactionError, const float* AField, const float* AFieldNorm, 
        int argSCount, int argACount, int argRCount, float* argWeight, float* argWeight2, float* retUpdates)
    {
        // Вычисляем обновления для каждого A-нейрона
        std::vector<float> a_updates(argACount, 0.0f);

        for (int j = 0; j < argACount; j++)
        {
            float multiplier = (AField[j] > 0) ? -AFieldNorm[j] : AFieldNorm[j];
            int condition_count = 0;

            if (AField[j] > 0) 
            {
                for (int r = 0; r < argRCount; r++)
                {
                    if (ReactionError[r] != 0 && sign(argWeight2[r * argACount + j]) != sign(ReactionError[r]))
                    {
                        condition_count++;
                    }
                }
            }
            else 
            {
                for (int r = 0; r < argRCount; r++)
                {
                    if (sign(argWeight2[r * argACount + j]) == sign(ReactionError[r]))
                    {
                        condition_count++;
                    }
                }
            }

            a_updates[j] = multiplier * condition_count;
        }

        // Применяем обновления - ВЕКТОРИЗАЦИЯ ПО AIndex
        for (int i = 0; i < argSCount; i++)
        {
            int j = 0;
            for (; j <= argACount - 8; j += 8)
            {
                // Загружаем 8 последовательных весов для фиксированного i
                float* base_ptr = &argWeight[i * argACount + j];
                __m256 weights_vec = _mm256_loadu_ps(base_ptr);

                // Загружаем 8 обновлений для A-нейронов
                __m256 updates_vec = _mm256_loadu_ps(&a_updates[j]);

                // Применяем обновления
                weights_vec = _mm256_add_ps(weights_vec, updates_vec);

                // Сохраняем обратно
                _mm256_storeu_ps(base_ptr, weights_vec);
            }

            // Скалярный хвост по AIndex
            for (; j < argACount; j++)
            {
                argWeight[i * argACount + j] += a_updates[j];
            }
        }
        if (retUpdates != NULL)
        {
            std::copy(a_updates.begin(), a_updates.end(), retUpdates);
        }
    }


    inline void NormalizeAvx2(const float* AField, int length, float* retAFieldNorm)
    {

        //retAFieldNorm = (float*)malloc(length * sizeof(float));

        // Инициализация
        float maxAbs = 0.0f;

        // Векторные переменные для AVX2
        __m256 v_max_abs = _mm256_setzero_ps();

        int i = 0;

        // Обработка основных блоков по 8 элементов
        for (; i <= length - 8; i += 8) 
        {
            __m256 v_data = _mm256_loadu_ps(&AField[i]);

            // Вычисление абсолютных значений
            __m256 v_abs = _mm256_and_ps(v_data, _mm256_castsi256_ps(_mm256_set1_epi32(0x7FFFFFFF)));

            // Обновление максимума
            v_max_abs = _mm256_max_ps(v_max_abs, v_abs);
        }

        // Редукция вектора максимумов
        __m128 v_max_abs_high = _mm256_extractf128_ps(v_max_abs, 1);
        __m128 v_max_abs_low = _mm256_castps256_ps128(v_max_abs);
        __m128 v_max_abs_128 = _mm_max_ps(v_max_abs_high, v_max_abs_low);

        v_max_abs_128 = _mm_max_ps(v_max_abs_128, _mm_shuffle_ps(v_max_abs_128, v_max_abs_128, _MM_SHUFFLE(1, 0, 3, 2)));
        v_max_abs_128 = _mm_max_ps(v_max_abs_128, _mm_shuffle_ps(v_max_abs_128, v_max_abs_128, _MM_SHUFFLE(2, 3, 0, 1)));

        _mm_store_ss(&maxAbs, v_max_abs_128);


        // Обработка оставшихся элементов
        for (; i < length; i++) 
        {
            float absValue = fabsf(AField[i]);
            if (absValue > maxAbs) maxAbs = absValue;
        }

        // Если все значения нулевые, копируем исходный массив
        if (maxAbs == 0.0f) 
        {
            memcpy(retAFieldNorm, AField, length * sizeof(float));
            return;
        }

        // Нормализация
        __m256 v_inv_max = _mm256_set1_ps(1.0f / maxAbs);

        i = 0;
        for (; i <= length - 8; i += 8) 
        {
            __m256 v_data = _mm256_loadu_ps(&AField[i]);
            __m256 v_normalized = _mm256_mul_ps(v_data, v_inv_max);
            _mm256_storeu_ps(&retAFieldNorm[i], v_normalized);
        }

        // Обработка оставшихся элементов
        for (; i < length; i++) 
        {
            retAFieldNorm[i] = AField[i] / maxAbs;
        }
    }
};

// C wrapper functions
extern "C" 
{
    PERCEPTRONF_API PerceptronFHandle CreatePerceptronF(int sCount, int aCount, int rCount, int a2Count)
    {
        try 
        {
            return new PerceptronF(sCount, aCount, rCount, a2Count);
        }
        catch (...) 
        {
            return nullptr;
        }
    }

    PERCEPTRONF_API void DisposePerceptronF(PerceptronFHandle handle) 
    {
        if (handle) 
        {
            delete static_cast<PerceptronF*>(handle);
        }
    }

    PERCEPTRONF_API void SA(PerceptronFHandle handle, int sIndex, int aIndex, float value) 
    {
        if (handle) 
        {
            static_cast<PerceptronF*>(handle)->SA(sIndex, aIndex, value);
        }
    }
    PERCEPTRONF_API void SetSA(PerceptronFHandle handle, int sIndex, int aIndex, float value)
    {
        if (handle)
        {
            static_cast<PerceptronF*>(handle)->SetSA(sIndex, aIndex, value);
        }
    }

    PERCEPTRONF_API float SA_(PerceptronFHandle handle, int sIndex, int aIndex)
    {
        if (handle)
        {
            return static_cast<PerceptronF*>(handle)->SA_(sIndex, aIndex);
        }
    }

    PERCEPTRONF_API void AA(PerceptronFHandle handle, int aIndex, int a2Index, float value)
    {
        if (handle)
        {
            static_cast<PerceptronF*>(handle)->AA(aIndex, a2Index, value);
        }
    }

    PERCEPTRONF_API void AR(PerceptronFHandle handle, int aIndex, int rIndex, float value)
    {
        if (handle)
        {
            static_cast<PerceptronF*>(handle)->AR(aIndex, rIndex, value);
        }
    }

    PERCEPTRONF_API float AR_(PerceptronFHandle handle, int aIndex, int rIndex)
    {
        if (handle)
        {
            return static_cast<PerceptronF*>(handle)->AR_(aIndex, rIndex);
        }
    }


    PERCEPTRONF_API void AActivation(PerceptronFHandle handle, const float* sField, float* aField, int startA, int endA)
    {
        if (handle && sField && aField) 
        {
            static_cast<PerceptronF*>(handle)->AActivation(sField, aField, startA, endA);
        }
    }
    PERCEPTRONF_API void A2Activation(PerceptronFHandle handle, const float* aField, float* a2Field)
    {
        if (handle && aField && a2Field)
        {
            static_cast<PerceptronF*>(handle)->A2Activation(aField, a2Field);
        }
    }

    PERCEPTRONF_API void RActivation(PerceptronFHandle handle, const float* aField, float* rField) 
    {
        if (handle && aField && rField) 
        {
            static_cast<PerceptronF*>(handle)->RActivation(aField, rField);
        }
    }
    PERCEPTRONF_API void R2Activation(PerceptronFHandle handle, const float* a2Field, float* rField)
    {
        if (handle && a2Field && rField)
        {
            static_cast<PerceptronF*>(handle)->R2Activation(a2Field, rField);
        }
    }

    PERCEPTRONF_API void LearnedStimulAR(PerceptronFHandle handle, const float* reactionError, const float* aField) 
    {
        if (handle && reactionError && aField) 
        {
            static_cast<PerceptronF*>(handle)->LearnedStimulAR(reactionError, aField);
        }
    }

    PERCEPTRONF_API void LearnedStimulSA(PerceptronFHandle handle, const float* reactionError, const float* aField, const float* aFieldNorm)
    {
        if (handle && reactionError && aField && aFieldNorm)
        {
            static_cast<PerceptronF*>(handle)->LearnedStimulSA(reactionError, aField, aFieldNorm);
        }
    }

    PERCEPTRONF_API void LearnedStimul2SA(PerceptronFHandle handle, const float* reactionError, const float* aField, const float* aFieldNorm)
    {
        if (handle && reactionError && aField && aFieldNorm)
        {
            static_cast<PerceptronF*>(handle)->LearnedStimul2SA(reactionError, aField, aFieldNorm);
        }
    }

    PERCEPTRONF_API void LearnedStimul2AA(PerceptronFHandle handle, const float* reactionError, const float* a2Field, const float* a2FieldNorm, float* retUpdates)
    {
        if (handle && reactionError && a2Field && a2FieldNorm && retUpdates)
        {
            static_cast<PerceptronF*>(handle)->LearnedStimul2AA(reactionError, a2Field, a2FieldNorm, retUpdates);
        }
    }

    PERCEPTRONF_API void RandomChange(PerceptronFHandle handle, float d, float c3, const float* aField)
    {
        if (handle && aField)
        {
            static_cast<PerceptronF*>(handle)->RandomChange(d, c3, aField);
        }
    }

    PERCEPTRONF_API void Random2Change(PerceptronFHandle handle, float d, float c3, const float* aField, const float* a2Field)
    {
        if (handle && aField && a2Field)
        {
            static_cast<PerceptronF*>(handle)->Random2Change(d, c3, aField, a2Field);
        }
    }

    PERCEPTRONF_API void Normalize(PerceptronFHandle handle, const float* aField, float* retAFieldNorm)
    {
        if (handle && aField && retAFieldNorm)
        {
            static_cast<PerceptronF*>(handle)->Normalize(aField, retAFieldNorm);
        }
    }

    PERCEPTRONF_API bool SaveWeights(PerceptronFHandle handle, const char* filename)
    {
        return static_cast<PerceptronF*>(handle)->SaveWeights(filename);
    }
    PERCEPTRONF_API int LoadWeights(PerceptronFHandle handle, const char* filename)
    {
        return static_cast<PerceptronF*>(handle)->LoadWeights(filename);
    }


}
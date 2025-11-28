// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


#include "pch.h"
#include "Tensor.h"

#include <memory>


int Tensor::CalculateSize(int* dims, int count)
{
    int size = 1;
    for (int i = 0; i < count; i++) 
    {
        size *= dims[i];
    }
    return size;
}

int Tensor::GetIndex(int* indices, int indicesCount)
{
    if (indicesCount != DimCount) 
    {
        // Ошибка: количество индексов не совпадает с размерностью
        return -1;
    }

    int index = 0;
    int multiplier = 1;
    for (int i = indicesCount - 1; i >= 0; i--) 
    {
        if (indices[i] < 0 || indices[i] >= Dimension[i]) 
        {
            // Ошибка: индекс вне границ
            return -1;
        }
        index += indices[i] * multiplier;
        multiplier *= Dimension[i];
    }
    return index;
}



// Конструктор по размерности
Tensor::Tensor(int* dimension, int dimCount)
{
    DimCount = dimCount;
    Dimension = new int[dimCount];
    for (int i = 0; i < dimCount; i++) 
    {
        Dimension[i] = dimension[i];
    }
    DataLength = CalculateSize(dimension, dimCount);
    Data = new float[DataLength](); // Инициализируем нулями
}

// Конструктор по данным и размерности
Tensor::Tensor(float* data, int* dimension, int dimCount, int dataLength)
{
    DimCount = dimCount;
    Dimension = new int[dimCount];
    for (int i = 0; i < dimCount; i++) 
    {
        Dimension[i] = dimension[i];
    }
    DataLength = dataLength;
    Data = new float[DataLength];
    std::memcpy(Data, data, DataLength * sizeof(float)); // Копируем данные
}

// Деструктор
Tensor::~Tensor()
{
    delete[] Data;
    delete[] Dimension;
}

// Конструктор копирования
Tensor::Tensor(const Tensor& other)
{
    DimCount = other.DimCount;
    DataLength = other.DataLength;

    Dimension = new int[DimCount];
    for (int i = 0; i < DimCount; i++) 
    {
        Dimension[i] = other.Dimension[i];
    }

    Data = new float[DataLength];
    std::memcpy(Data, other.Data, DataLength * sizeof(float)); // Копируем данные
}

// Оператор присваивания
Tensor& Tensor::operator=(const Tensor& other)
{
    if (this == &other) return *this;

    // Освобождаем старую память
    delete[] Data;
    delete[] Dimension;

    // Копируем новые данные
    DimCount = other.DimCount;
    DataLength = other.DataLength;

    Dimension = new int[DimCount];
    for (int i = 0; i < DimCount; i++) 
    {
        Dimension[i] = other.Dimension[i];
    }

    Data = new float[DataLength];
    std::memcpy(Data, other.Data, DataLength * sizeof(float)); // Копируем данные

    return *this;
}

// Получение значения по индексам
float Tensor::Get(int* indices, int indicesCount)
{
    int index = GetIndex(indices, indicesCount);
    if (index >= 0 && index < DataLength) 
    {
        return Data[index];
    }
    return 0.0f; // или выбросить исключение
}

// Установка значения по индексам
void Tensor::Set(int* indices, int indicesCount, float value)
{
    int index = GetIndex(indices, indicesCount);
    if (index >= 0 && index < DataLength) 
    {
        Data[index] = value;
    }
}

// Клонирование
Tensor Tensor::Clone() const
{
    return Tensor(Data, Dimension, DimCount, DataLength);
}

// Реализация C-интерфейса
extern "C" 
{
    CNN_API void* Tensor_Create(int* dimensions, int dimCount) 
    {
        return new Tensor(dimensions, dimCount);
    }

    CNN_API void* Tensor_CreateFromData(float* data, int* dimensions, int dimCount, int dataLength) 
    {
        return new Tensor(data, dimensions, dimCount, dataLength);
    }

    CNN_API void Tensor_Delete(void* tensor) 
    {
        delete static_cast<Tensor*>(tensor);
    }

    CNN_API float* Tensor_GetData(void* tensor) 
    {
        return static_cast<Tensor*>(tensor)->Data;
    }

    CNN_API int Tensor_GetDataLength(void* tensor) 
    {
        return static_cast<Tensor*>(tensor)->DataLength;
    }

    CNN_API int Tensor_GetDimCount(void* tensor) 
    {
        return static_cast<Tensor*>(tensor)->DimCount;
    }

    CNN_API void Tensor_GetDimensions(void* tensor, int* dimensions) 
    {
        Tensor* t = static_cast<Tensor*>(tensor);
        for (int i = 0; i < t->DimCount; i++) {
            dimensions[i] = t->Dimension[i];
        }
    }

    CNN_API void* Tensor_Clone(void* tensor) 
    {
        Tensor* t = static_cast<Tensor*>(tensor);
        return new Tensor(*t);
    }

    CNN_API float Tensor_GetValue(void* tensor, int* indices, int indicesCount) 
    {
        return static_cast<Tensor*>(tensor)->Get(indices, indicesCount);
    }

    CNN_API void Tensor_SetValue(void* tensor, int* indices, int indicesCount, float value) 
    {
        static_cast<Tensor*>(tensor)->Set(indices, indicesCount, value);
    }
}

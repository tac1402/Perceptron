// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

#pragma once

#ifndef TENSOR_H
#define TENSOR_H

#include <cstring>

class Tensor {
private:
    int CalculateSize(int* dims, int count);
    int GetIndex(int* indices, int indicesCount);

public:
    float* Data;
    int* Dimension;
    int DimCount;
    int DataLength;

    // Конструктор по размерности
    Tensor(int* dimension, int dimCount);

    // Конструктор по данным и размерности
    Tensor(float* data, int* dimension, int dimCount, int dataLength);

    // Деструктор
    ~Tensor();

    // Конструктор копирования
    Tensor(const Tensor& other);

    // Оператор присваивания
    Tensor& operator=(const Tensor& other);

    // Получение значения по индексам
    float Get(int* indices, int indicesCount);

    // Установка значения по индексам
    void Set(int* indices, int indicesCount, float value);

    // Клонирование
    Tensor Clone() const;
};

#ifdef CNN_EXPORTS
#define CNN_API __declspec(dllexport)
#else
#define CNN_API __declspec(dllimport)
#endif

extern "C" 
{
    // Создание и удаление тензора
    CNN_API void* Tensor_Create(int* dimensions, int dimCount);
    CNN_API void* Tensor_CreateFromData(float* data, int* dimensions, int dimCount, int dataLength);
    CNN_API void Tensor_Delete(void* tensor);

    // Работа с данными
    CNN_API float* Tensor_GetData(void* tensor);
    CNN_API int Tensor_GetDataLength(void* tensor);
    CNN_API int Tensor_GetDimCount(void* tensor);
    CNN_API void Tensor_GetDimensions(void* tensor, int* dimensions);

    // Операции
    CNN_API void* Tensor_Clone(void* tensor);
    CNN_API float Tensor_GetValue(void* tensor, int* indices, int indicesCount);
    CNN_API void Tensor_SetValue(void* tensor, int* indices, int indicesCount, float value);
}

#endif // TENSOR_H
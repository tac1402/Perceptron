// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev


#include "pch.h"
#include "Flatten.h"

// Реализация метода Forward
Tensor Flatten::Forward(Tensor& input) 
{
    // Вычисляем новую размерность
    int newSize = 1;
    for (int i = 1; i < input.DimCount; i++) 
    {
        newSize *= input.Dimension[i];
    }

    // Создаем массив для новой размерности
    int* newDims = new int[2];
    newDims[0] = input.Dimension[0];  // batch size
    newDims[1] = newSize;             // flattened features

    // Создаем новый тензор с теми же данными, но новой формой
    Tensor result(input.Data, newDims, 2, input.DataLength);

    delete[] newDims;
    return result;
}

// Реализация C-интерфейса
extern "C" 
{
    CNN_API void* Flatten_Create() 
    {
        return new Flatten();
    }

    CNN_API void Flatten_Delete(void* flatten) 
    {
        delete static_cast<Flatten*>(flatten);
    }

    CNN_API void* Flatten_Forward(void* flatten, void* inputTensor) 
    {
        Flatten* f = static_cast<Flatten*>(flatten);
        Tensor* input = static_cast<Tensor*>(inputTensor);
        Tensor* result = new Tensor(f->Forward(*input));
        return result;
    }
}

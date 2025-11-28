// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

#pragma once

#ifndef FLATTEN_H
#define FLATTEN_H

#include "Tensor.h"

class Flatten 
{
public:
    Tensor Forward(Tensor& input);
};


// C-интерфейс для работы из C#
extern "C" 
{
    CNN_API void* Flatten_Create();
    CNN_API void Flatten_Delete(void* flatten);
    CNN_API void* Flatten_Forward(void* flatten, void* inputTensor);
}

#endif // FLATTEN_H
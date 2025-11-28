// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

#pragma once


#ifndef RELU_H
#define RELU_H

#include "Tensor.h"

class ReLU 
{
public:
    Tensor Forward(Tensor& input);
};



// C-интерфейс для работы из C#
extern "C" 
{
    CNN_API void* ReLU_Create();
    CNN_API void ReLU_Delete(void* relu);
    CNN_API void* ReLU_Forward(void* relu, void* inputTensor);
}


#endif // RELU_H

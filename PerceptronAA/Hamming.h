// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

#pragma once

#ifdef HAMMING_EXPORTS
#define HAMMING_API __declspec(dllexport)
#else
#define HAMMING_API __declspec(dllimport)
#endif

extern "C" 
{
    HAMMING_API int CalculateHamming(const float* a, const float* b, int length, float threshold);
}


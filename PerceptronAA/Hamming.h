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


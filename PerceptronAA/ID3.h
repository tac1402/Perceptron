// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

#pragma once

#ifdef ID3_EXPORTS
#define ID3_API __declspec(dllexport)
#else
#define ID3_API __declspec(dllimport)
#endif

extern "C"
{
    // Handle types
    typedef void* ID3Handle;

    // Creation and destruction
    ID3_API ID3Handle CreateID3();
    ID3_API void DisposeID3(ID3Handle handle);

    // Operations
    ID3_API double CalcEntropyTotal(ID3Handle handle, const signed char* samplesClass, int total);
    ID3_API signed char AllSamples(ID3Handle handle, const signed char* samplesClass, int lenght, signed char argValue);

    ID3_API double CalcEntropyAdd(ID3Handle handle, const signed char* attributeSet, const signed char* samplesClass, int total, signed char value);

}


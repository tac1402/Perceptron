#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

namespace PerceptronWrapper 
{
    public ref class Point2D
    {
    private:
        Point2D_* point;

    public:
        Point2D(int x, int y)
        {
            point = new Point2D_();
            point->X = x;
            point->Y = y;
        }

        ~Point2D() 
        {
            this->!Point2D();
        }

        !Point2D() {
            if (point != nullptr)
            {
                delete point;
                point = nullptr;
            }
        }
        property int X
        {
            int get() { return point->X; }
            void set(int value) { point->X = value; }
        }
        property int Y
        {
            int get() { return point->Y; }
            void set(int value) { point->Y = value; }
        }
    };

    public ref class WinSumWrapper 
    {
    private:
        WinSumCalculator* calculator;
        GCHandle inputHandle;
        GCHandle weightsHandle;
        GCHandle indicesHandle;

        int outSize;
        int inSize;
        int winSize;

    public:
        WinSumWrapper(array<float>^ input, array<int>^ indices, array<float>^ weights, int argOutSize, int argInSize, int argWinSize)
        {
            outSize = argOutSize;
            inSize = argInSize;
            winSize = argWinSize;

            // Закрепляем массивы примитивных типов
            inputHandle = GCHandle::Alloc(input, GCHandleType::Pinned);
            weightsHandle = GCHandle::Alloc(weights, GCHandleType::Pinned);
            indicesHandle = GCHandle::Alloc(indices, GCHandleType::Pinned);

            // Получаем указатели на закрепленные данные
            float* inputPtr = static_cast<float*>(inputHandle.AddrOfPinnedObject().ToPointer());
            float* weightsPtr = static_cast<float*>(weightsHandle.AddrOfPinnedObject().ToPointer());
            int* indicesPtr = static_cast<int*>(indicesHandle.AddrOfPinnedObject().ToPointer());

            calculator = new WinSumCalculator(inputPtr, indicesPtr, weightsPtr, argOutSize, argInSize, argWinSize);
        }

        // Финализатор (вызывается GC)
        ~WinSumWrapper() 
        {
            this->!WinSumWrapper();
        }

        // Деструктор (детерминированное удаление)
        !WinSumWrapper()
        {
            if (calculator != nullptr) 
            {
                delete calculator;
                calculator = nullptr;
            }


            if (inputHandle.IsAllocated) inputHandle.Free();
            if (weightsHandle.IsAllocated) weightsHandle.Free();
            if (indicesHandle.IsAllocated) indicesHandle.Free();
        }

        // Обертки методов
        float Compute(int oh, int ow) { return calculator->Compute(oh, ow); }
    };
}

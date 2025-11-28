#pragma once

#ifdef CNN_EXPORTS
#define CNN_API __declspec(dllexport)
#else
#define CNN_API __declspec(dllimport)
#endif

// —труктура Point2D должна быть одинаковой в C++ и C#
struct Point2D 
{
    int X;
    int Y;
};

// C++ класс дл€ вычислений
class WinSumCalculator 
{
private:
    const float* input_;
    const Point2D* inPointField_;
    const float* weights_;
    int outWidth_;
    int inputWidth_;
    int windowSize_;

public:
    WinSumCalculator(const float* input, const Point2D* inPointField, const float* weights,
        int outWidth, int inputWidth, int windowSize);

    float Compute(int oh, int ow);
};

// C-интерфейс дл€ P/Invoke
extern "C" 
{
    CNN_API WinSumCalculator* CreateCalculator(
        const float* input, const Point2D* inPointField, const float* weights,
        int outWidth, int inputWidth, int windowSize);

    CNN_API float ComputeSum(WinSumCalculator* calculator, int oh, int ow);

    CNN_API void DeleteCalculator(WinSumCalculator* calculator);
}

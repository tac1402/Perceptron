
struct Point2D_
{
    int X;
    int Y;
};


// C++ класс для вычислений
class WinSumCalculator 
{
private:
    float* input_;
    float* weights_;
    int* indices_;

    int outSize_;
    int inSize_;
    int winSize_;

public:
    WinSumCalculator(float* input, int* indices, float* weights, int outSize, int inSize, int winSize);

    float Compute(int oh, int ow);
};


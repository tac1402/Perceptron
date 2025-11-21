using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

public class MNIST_Network : Module<Tensor, Tensor>
{
	private readonly Linear fc1;
	private readonly Linear fc2;
	private readonly Linear fc3;
	private readonly ReLU relu;
	private readonly Sigmoid sigmoid;

	public MNIST_Network(int SCount, int ACount, int RCount, int A2Count) : base("ParityNetwork")
	{
		fc1 = Linear(SCount, ACount);
		fc2 = Linear(ACount, A2Count);
		fc3 = Linear(A2Count, RCount);
		relu = ReLU();
		sigmoid = Sigmoid();

		// Регистрируем модули для управления параметрами
		RegisterComponents();
	}

	public override Tensor forward(Tensor x)
	{
		using (var d = torch.NewDisposeScope())
		{
			x = relu.forward(fc1.forward(x));
			x = relu.forward(fc2.forward(x));
			x = sigmoid.forward(fc3.forward(x));
			return x.MoveToOuterDisposeScope();
		}
	}
}
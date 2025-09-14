using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

public class ParityNetwork3 : Module<Tensor, Tensor>
{
	private readonly Linear fc1;
	private readonly Linear fc2;
	private readonly Linear fc3;
	private readonly ReLU relu;
	private readonly Sigmoid sigmoid;

	public ParityNetwork3(int input_size) : base("ParityNetwork")
	{
		fc1 = Linear(input_size, 50);
		fc2 = Linear(50, 50);
		fc3 = Linear(50, 1);
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
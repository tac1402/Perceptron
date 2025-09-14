using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

public class ParityNetwork : Module<Tensor, Tensor>
{
	private readonly Linear fc1;
	private readonly Linear fc2;
	private readonly ReLU relu;
	//private readonly Sigmoid sigmoid;

	public ParityNetwork(int input_size) : base("ParityNetwork")
	{
		fc1 = Linear(input_size, 50);
		fc2 = Linear(50, 1);
		relu = ReLU();
		//sigmoid = Sigmoid();

		// Регистрируем модули для управления параметрами
		RegisterComponents();
	}

	public override Tensor forward(Tensor x)
	{
		using (var d = torch.NewDisposeScope())
		{
			x = relu.forward(fc1.forward(x));
			//x = sigmoid.forward(fc2.forward(x));
			x = fc2.forward(x);
			return x.MoveToOuterDisposeScope();
		}
	}
}
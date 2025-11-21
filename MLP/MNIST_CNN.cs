using Google.Protobuf.WellKnownTypes;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

public class MNIST_CNN : Module<Tensor, Tensor>
{
	private readonly Conv2d conv1;
	private readonly MaxPool2d max1;
	private readonly Linear fc1;

	private readonly ReLU relu;
	private readonly Flatten flatten;

	public MNIST_CNN() : base("MNIST_CNN")
	{
		conv1 = Conv2d(in_channels: 1, out_channels: 32, kernel_size: 5);
		max1 = MaxPool2d(kernel_size: 2, stride: 2);
		fc1 = Linear(inputSize: 32 * 12 * 12, outputSize: 10);

		relu = ReLU();
		flatten = Flatten();

		// Регистрируем модули для управления параметрами
		RegisterComponents();
	}

	public override Tensor forward(Tensor x)
	{
		using (var d = torch.NewDisposeScope())
		{
			x = conv1.forward(x);
			x = relu.forward(x);
			x = max1.forward(x);
			x = flatten.forward(x);
			x = fc1.forward(x);
			return x.MoveToOuterDisposeScope();
		}
	}

}


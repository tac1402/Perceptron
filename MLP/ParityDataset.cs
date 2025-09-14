using System;
using static TorchSharp.torch.utils.data;
using static TorchSharp.torch;

namespace MLP
{
	// Создание пользовательского набора данных
	public class ParityDataset : Dataset
	{
		private Tensor features;
		private Tensor labels;

		public ParityDataset(Tensor features, Tensor labels)
		{
			this.features = features;
			this.labels = labels;
		}

		public override long Count => features.shape[0];

		public override Dictionary<string, Tensor> GetTensor(long index)
		{
			return new Dictionary<string, Tensor>
		{
			{ "features", features[index] },
			{ "labels", labels[index] }
		};
		}
	}
}

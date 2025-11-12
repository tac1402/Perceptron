using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Neocognitron
{
	public class Neocognitron
	{

		// Contains all physical structure information and constants
		private NeocognitronStructure s;

		// An array of each c and s layers
		private CLayer[] cLayers;
		private SLayer[] sLayers;

		public Neocognitron(NeocognitronStructure argS)
		{ 
			s =argS;

			sLayers = new SLayer[s.numLayers];
			cLayers = new CLayer[s.numLayers];

			// Initialize all the needed neural net layers
			for (int l = 0; l < s.numLayers; l++)
			{
				sLayers[l] = new SLayer(l, s); 
				cLayers[l] = new CLayer(l, s);
			}
		}

		/**
		 * Given an input matrix (character image), the neural network determined
		 * which character the image represents. This method is also used to train
		 * the Neocognitron.
		 * 
		 * @param input	A square image containing the character to be recognized.
		 * @param train	A boolean value which determines if the network should
		 * 				be trained or not
		 * @return		The integer representation of the recognized character
		 */
		public int propagate(double[][] input, bool argTrain)
		{

			// Initialize output class with the input matrix
			OutputConnections output = new OutputConnections(1, s.inputLayerSize);
			output.setPlaneOutput(0, input);

			// Propagate the input through the matrix, layer by layer
			for (int l = 0; l < s.numLayers; l++)
			{
				output = sLayers[l].propagate(output, argTrain);

				output = cLayers[l].propagate(output);
			}

			// Determine the output from the final layer
			return determineOutput(output.getPointsOnPlanes(0, 0));
		}

		/**
		 * Given the output from the final layer, determine the output of
		 * the network. The output is an integer which ranges across all
		 * possible outputs.
		 * 
		 * @param out	Output from the last layer in the neocognitron.
		 * @return		The index of the maximum output in the last layer.
		 */
		public int determineOutput(double[] out_)
		{
			double maxValue = 0;
			int index = -1;
			for (int i = 0; i < out_.Length; i++)
			{
				if (out_[i] > maxValue) 
				{
					maxValue = out_[i] ;
					index = i;
				}
			}
			return index;
		}

	}
}

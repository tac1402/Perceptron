// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Collections.Generic;
using System.Collections.Specialized;


namespace Tac.Perceptron
{
	/// <summary>
	/// S-элемент (сенсорный элемент)
	/// </summary>
	public class SElement
	{
		// Синапсы соединенные с этим S-элементом 
		public Dictionary<int, List<Sinaps>> Sinaps = new Dictionary<int, List<Sinaps>>();

		private BitBlock state; 
		public BitBlock State
		{
			get { return state; }
			set
			{
				state = value;

				for (int i = 0; i < state.Count; i++)
				{
					if (state[i] == true && Sinaps.ContainsKey(i))
					{
						foreach (Sinaps sinaps in Sinaps[i])
						{
							sinaps.Activate();
						}
					}
				}
			}
		}

		public SElement(int argSCount)
		{
			state = new BitBlock(argSCount);
		}

		public void AddSinaps(int argNumber, Sinaps argSinaps)
		{
			if (Sinaps.ContainsKey(argNumber) == false)
			{ 
				Sinaps[argNumber] = new List<Sinaps>();
			}
			Sinaps[argNumber].Add(argSinaps);
		}

	}
}

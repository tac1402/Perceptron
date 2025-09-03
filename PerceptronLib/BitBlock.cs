// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Collections.Specialized;

namespace Tac.Perceptron
{
	public class BitBlock
	{
		public BitVector32[] Data;

		public int Count { get { return Data.Length * 32; } }

		public BitBlock(int argCount)
		{
			int block = argCount / 32 + 1;
			Data = new BitVector32[block];
			for (int i = 0; i < block; i++)
			{
				Data[i] = new BitVector32();
			}
		}
		public BitBlock(int argCount, int[] argData)
		{
			int block = argCount / 32 + 1;
			Data = new BitVector32[block];
			for (int i = 0; i < block; i++)
			{
				Data[i] = new BitVector32(argData[i]);
			}
		}

		public bool this[int number]
		{
			get 
			{
				int block = number / 32;
				int bit = number % 32;
				int mask = 1 << bit;
				return Data[block][mask];
			}
			set
			{
				int block = number / 32;
				int bit = number % 32;
				int mask = 1 << bit;
				Data[block][mask] = value;
			}
		}

		public override string ToString() => ToString(Count);

		public string ToString(int argCount)
		{
			string s = "";
			for (int i = 0; i < argCount; i++)
			{
				if (this[i] == true)
					s += "1";
				else 
					s += "0";
			}
			return s;
		}

	}
}

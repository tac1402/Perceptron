// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Runtime.InteropServices;

namespace Tac.Perceptron
{
	public static class Hamming
	{
		[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int CalculateHamming(float[] a, float[] b, int length, float threshold);


		// Удобный метод с порогом по умолчанию
		public static int Calculate(float[] a, float[] b, float threshold = 0.000001f)
		{
			if (a.Length != b.Length)
				throw new ArgumentException("Arrays must have same length");

			return CalculateHamming(a, b, a.Length, threshold);
		}
	}
}


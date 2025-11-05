// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Runtime.InteropServices;

namespace Tac.Perceptron
{
	public class ID3 : IDisposable
	{
		private IntPtr _handle;

		[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr CreateID3();

		[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void DisposeID3(IntPtr handle);

		[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern double CalcEntropyTotal(IntPtr handle, sbyte[] samplesClass, int length);

		[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern sbyte AllSamples(IntPtr handle, sbyte[] samplesClass, int length, sbyte argValue);

		[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern double CalcEntropyAdd(IntPtr handle, sbyte[] attributeSet, sbyte[] samplesClass, int total, sbyte argValue);

		public ID3()
		{
			_handle = CreateID3();
			if (_handle == IntPtr.Zero)
				throw new Exception("Failed to create ID3 instance");
		}

		public void Dispose()
		{
			if (_handle != IntPtr.Zero)
			{
				DisposeID3(_handle);
				_handle = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}


		public double CalcEntropyTotal(sbyte[] samplesClass)
		{
			return CalcEntropyTotal(_handle, samplesClass, samplesClass.Length);
		}

		public sbyte AllSamples(sbyte[] samplesClass, sbyte argValue)
		{
			return AllSamples(_handle, samplesClass, samplesClass.Length, argValue);
		}

		public double CalcEntropyAdd(sbyte[] attributeSet, sbyte[] samplesClass, sbyte argValue)
		{
			return CalcEntropyAdd(_handle, attributeSet, samplesClass, attributeSet.Length, argValue);
		}

		

		~ID3()
		{
			Dispose();
		}
	}
}

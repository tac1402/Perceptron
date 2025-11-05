// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

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
		public static extern int GetTotalPositives(IntPtr handle, sbyte[] aField, int length);

		[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern sbyte AllSamples(IntPtr handle, sbyte[] aField, int length, sbyte argValue);


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


		public int GetTotalPositives(sbyte[] aField)
		{
			return GetTotalPositives(_handle, aField, aField.Length);
		}

		public sbyte AllSamples(sbyte[] aField, sbyte argValue)
		{
			return AllSamples(_handle, aField, aField.Length, argValue);
		}


		~ID3()
		{
			Dispose();
		}
	}
}

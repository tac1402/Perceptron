// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Runtime.InteropServices;

public class PerceptronL : IDisposable
{
	private IntPtr _handle;

	[DllImport("PerceptronL.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr CreatePerceptronL(int sCount, int aCount, int rCount);

	[DllImport("PerceptronL.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void DisposePerceptronL(IntPtr handle);

	[DllImport("PerceptronL.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void SA(IntPtr handle, int sIndex, int aIndex, float value);

	[DllImport("PerceptronL.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void AR(IntPtr handle, int aIndex, int rIndex, float value);
	[DllImport("PerceptronL.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern float AR_(IntPtr handle, int aIndex, int rIndex);

	[DllImport("PerceptronL.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void AActivation(IntPtr handle, float[] sField, float[] aField);

	private int SCount;
	private int ACount;
	private int RCount;

	public PerceptronL(int argSCount, int argACount, int argRCount)
	{
		SCount = argSCount;
		ACount = argACount;
		RCount = argRCount;

		_handle = CreatePerceptronL(SCount, ACount, RCount);
		if (_handle == IntPtr.Zero)
			throw new Exception("Failed to create PerceptronAA instance");
	}

	public void Dispose()
	{
		if (_handle != IntPtr.Zero)
		{
			DisposePerceptronL(_handle);
			_handle = IntPtr.Zero;
		}
		GC.SuppressFinalize(this);
	}

	public void SA(int sIndex, int aIndex, float value)
	{
		SA(_handle, sIndex, aIndex, value);
	}
	public void AR(int aIndex, int rIndex, float value)
	{
		AR(_handle, aIndex, rIndex, value);
	}
	public float AR_(int aIndex, int rIndex)
	{
		return AR_(_handle, aIndex, rIndex);
	}

	public float[] AActivation(float[] sField)
	{
		float[] aField = new float[ACount];
		AActivation(_handle, sField, aField);
		return aField;
	}


	~PerceptronL()
	{
		Dispose();
	}
}
// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Runtime.InteropServices;

public class PerceptronAA : IDisposable
{
	private IntPtr _handle;

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr CreatePerceptronF(int sCount, int aCount, int rCount, float th);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void DisposePerceptronF(IntPtr handle);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void SAf(IntPtr handle, int sIndex, int aIndex, float value);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ARf(IntPtr handle, int aIndex, int rIndex, float value);
	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern float AR_f(IntPtr handle, int aIndex, int rIndex);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void AActivation_f(IntPtr handle, float[] sField, float[] aField);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void RActivation_f(IntPtr handle, float[] aField, float[] rField, float threshold = 0.0f);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void LearnedStimulARf(IntPtr handle, float[] reactionError, float[] aField);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void LearnedStimulSAf(IntPtr handle, float[] reactionError, float[] aField, float[] aFieldNorm);

	private int SCount;
	private int ACount;
	private int RCount;
	private float th;

	public PerceptronAA(int argSCount, int argACount, int argRCount, float argTh)
	{
		SCount = argSCount;
		ACount = argACount;
		RCount = argRCount;
		th = argTh;

		_handle = CreatePerceptronF(SCount, ACount, RCount, argTh);
		if (_handle == IntPtr.Zero)
			throw new Exception("Failed to create PerceptronAA instance");
	}

	public void Dispose()
	{
		if (_handle != IntPtr.Zero)
		{
			DisposePerceptronF(_handle);
			_handle = IntPtr.Zero;
		}
		GC.SuppressFinalize(this);
	}

	public void SA(int sIndex, int aIndex, float value)
	{
		SAf(_handle, sIndex, aIndex, value);
	}

	public void AR(int aIndex, int rIndex, float value)
	{
		ARf(_handle, aIndex, rIndex, value);
	}
	public float AR_(int aIndex, int rIndex)
	{
		return AR_f(_handle, aIndex, rIndex);
	}


	public float[] AActivation(float[] sField)
	{
		float[] aField = new float[ACount];
		AActivation_f(_handle, sField, aField);
		return aField;
	}

	public float[] RActivation(float[] aField, float threshold = 0.0f)
	{
		float[] rField = new float[RCount];
		RActivation_f(_handle, aField, rField, threshold);
		return rField;
	}

	public void LearnedStimulAR(float[] reactionError, float[] aField)
	{
		LearnedStimulARf(_handle, reactionError, aField);
	}

	public void LearnedStimulSA(float[] reactionError, float[] aField, float[] aFieldNorm)
	{
		LearnedStimulSAf(_handle, reactionError, aField, aFieldNorm);
	}

	~PerceptronAA()
	{
		Dispose();
	}
}
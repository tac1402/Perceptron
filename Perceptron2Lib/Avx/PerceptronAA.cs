// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Runtime.InteropServices;

public class PerceptronAA : IDisposable
{
	private IntPtr _handle;

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr CreatePerceptronF(int sCount, int aCount, int rCount, int a2Count);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void DisposePerceptronF(IntPtr handle);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void SA(IntPtr handle, int sIndex, int aIndex, float value);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void SetSA(IntPtr handle, int sIndex, int aIndex, float value);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern float SA_(IntPtr handle, int sIndex, int aIndex);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void AA(IntPtr handle, int aIndex, int a2Index, float value);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void AR(IntPtr handle, int aIndex, int rIndex, float value);
	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern float AR_(IntPtr handle, int aIndex, int rIndex);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void AActivation(IntPtr handle, float[] sField, float[] aField, int startA, int endA);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void A2Activation(IntPtr handle, float[] aField, float[] a2Field);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void RActivation(IntPtr handle, float[] aField, float[] rField);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void R2Activation(IntPtr handle, float[] a2Field, float[] rField);


	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void LearnedStimulAR(IntPtr handle, float[] reactionError, float[] aField);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void LearnedStimulSA(IntPtr handle, float[] reactionError, float[] aField, float[] aFieldNorm);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void LearnedStimul2SA(IntPtr handle, float[] reactionError, float[] aField, float[] aFieldNorm);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void LearnedStimul2AA(IntPtr handle, float[] reactionError, float[] a2Field, float[] a2FieldNorm, float[] retUpdates);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void RandomChange(IntPtr handle, float d, float c3, float[] aField);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern void Random2Change(IntPtr handle, float d, float c3, float[] aField, float[] a2Field);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern bool SaveWeights(IntPtr handle, string filename);

	[DllImport("PerceptronAA.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern bool LoadWeights(IntPtr handle, string filename);


	private int SCount;
	private int ACount;
	private int A2Count;
	private int RCount;

	public PerceptronAA(int argSCount, int argACount, int argRCount, int argA2Count = 0)
	{
		SCount = argSCount;
		ACount = argACount;
		A2Count = argA2Count;
		RCount = argRCount;

		_handle = CreatePerceptronF(SCount, ACount, RCount, argA2Count);
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
		SA(_handle, sIndex, aIndex, value);
	}
	public void SetSA(int sIndex, int aIndex, float value)
	{
		SetSA(_handle, sIndex, aIndex, value);
	}

	public float SA_(int sIndex, int aIndex)
	{
		return SA_(_handle, sIndex, aIndex);
	}
	public void AA(int aIndex, int a2Index, float value)
	{
		AA(_handle, aIndex, a2Index, value);
	}

	public void AR(int aIndex, int rIndex, float value)
	{
		AR(_handle, aIndex, rIndex, value);
	}
	public float AR_(int aIndex, int rIndex)
	{
		return AR_(_handle, aIndex, rIndex);
	}


	public float[] AActivation(float[] sField, int startA = 0, int endA = -1)
	{
		float[] aField;
		if (endA != -1)
		{
			aField = new float[endA - startA];
		}
		else { aField = new float[ACount]; }

		AActivation(_handle, sField, aField, startA, endA);
		return aField;
	}
	public float[] A2Activation(float[] aField)
	{
		float[] a2Field = new float[A2Count];
		A2Activation(_handle, aField, a2Field);
		return a2Field;
	}


	public float[] RActivation(float[] aField)
	{
		float[] rField = new float[RCount];
		RActivation(_handle, aField, rField);
		return rField;
	}
	public float[] R2Activation(float[] a2Field)
	{
		float[] rField = new float[RCount];
		R2Activation(_handle, a2Field, rField);
		return rField;
	}


	public void LearnedStimulAR(float[] reactionError, float[] aField)
	{
		LearnedStimulAR(_handle, reactionError, aField);
	}

	public void LearnedStimulSA(float[] reactionError, float[] aField, float[] aFieldNorm)
	{
		LearnedStimulSA(_handle, reactionError, aField, aFieldNorm);
	}
	public void LearnedStimul2SA(float[] reactionError, float[] aField, float[] aFieldNorm)
	{
		LearnedStimul2SA(_handle, reactionError, aField, aFieldNorm);
	}
	public void LearnedStimul2AA(float[] reactionError, float[] a2Field, float[] a2FieldNorm, float[] retUpdates)
	{
		LearnedStimul2AA(_handle, reactionError, a2Field, a2FieldNorm, retUpdates);
	}

	public void RandomChange(float d, float c3, float[] aField)
	{
		RandomChange(_handle, d, c3, aField);
	}
	public void Random2Change(float d, float c3, float[] aField, float[] a2Field)
	{
		Random2Change(_handle, d, c3, aField, a2Field);
	}

	public bool SaveWeights(string filename)
	{
		return SaveWeights(_handle, filename);
	}

	public bool LoadWeights(string filename)
	{
		return LoadWeights(_handle, filename);
	}


	~PerceptronAA()
	{
		Dispose();
	}
}
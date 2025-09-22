using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackProp
{

	public class NArray
	{
		public int Precision;
		private Number[] numbers;

		public int Count { get { return numbers.Length; } }

		public NArray(int argCount, int argPrecision)
		{
			Precision = argPrecision;
			numbers = new Number[argCount];
			for (int i = 0; i < argCount; i++)
			{
				numbers[i] = new Number(argPrecision);
			}
		}

		public dynamic this[int argIndex]
		{ 
			get { return numbers[argIndex].Value; } 
			set { numbers[argIndex].Value = value; } 
		}

		public void Set(params dynamic[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				this[i] = array[i];
			}
		}

	}


	public class Number
	{
		public int Precision;
		private NumberType numberType;

		private dynamic value;
		public dynamic Value
		{
			get 
			{
				dynamic ret = 0f;
				switch (numberType)
				{
					case NumberType.Float:
						ret = Round((float)value, Precision);
						break;
					case NumberType.Double:
						ret = Round((double)value, Precision);
						break;
					case NumberType.Decimal:
						ret = Round((decimal)value, Precision);
						break;

				}
				return ret; 
			}
			set 
			{
				switch (numberType)
				{
					case NumberType.Float:
						this.value = Round((float)value, Precision);
						break;
					case NumberType.Double:
						this.value = Round((double)value, Precision);
						break;
					case NumberType.Decimal:
						this.value = Round((decimal)value, Precision);
						break;

				}
			}
		}

		public enum NumberType
		{
			Float,
			Double,
			Decimal
		}

		public Number(dynamic argValue, int argPrecision)
		{
			Precision = argPrecision;

			if (Precision <= 7)
			{
				numberType = NumberType.Float;
			}
			else if (Precision <= 15)
			{
				numberType = NumberType.Double;
			}
			else
			{
				numberType = NumberType.Decimal;
			}
			Value = argValue; // Важно, чтобы присваивалось после определения типа
		}


		public Number(int argPrecision)
		{
			Precision = argPrecision;

			if (Precision <= 7)
			{
				numberType = NumberType.Float;
				Value = default(float); // Важно, чтобы присваивалось после определения типа
			}
			else if (Precision <= 15)
			{
				numberType = NumberType.Double;
				Value = default(double); // Важно, чтобы присваивалось после определения типа
			}
			else
			{
				numberType = NumberType.Decimal;
				Value = default(decimal); // Важно, чтобы присваивалось после определения типа
			}
		}

		public T To<T>() => (T)Value;


		// Методы для округления до значащих цифр
		private float Round(float d, int digits)
		{
			return (float)Math.Round(d, digits);
		}
		private double Round(double d, int digits)
		{
			return Math.Round(d, digits);
		}
		private decimal Round(decimal d, int digits)
		{
			return Math.Round(d, digits);
		}

		// Арифметические операции
		public static Number operator +(Number a, Number b)
		{
			Number ret = new Number(Math.Max(a.Precision, b.Precision));
			switch (a.numberType)
			{
				case NumberType.Float:
					ret.Value = a.To<float>() + b.To<float>();
					break;
				case NumberType.Double:
					ret.Value = a.To<double>() + b.To<double>();
					break;
				case NumberType.Decimal:
					ret.Value = a.To<decimal>() + b.To<decimal>();
					break;

			}
			return ret;
		}

		public static Number operator -(Number a, Number b)
		{
			Number ret = new Number(Math.Max(a.Precision, b.Precision));
			switch (a.numberType)
			{
				case NumberType.Float:
					ret.Value = a.To<float>() - b.To<float>();
					break;
				case NumberType.Double:
					ret.Value = a.To<double>() - b.To<double>();
					break;
				case NumberType.Decimal:
					ret.Value = a.To<decimal>() - b.To<decimal>();
					break;
			}
			return ret;
		}

		
		public static Number operator *(Number a, Number b)
		{
			Number ret = new Number(Math.Max(a.Precision, b.Precision));
			switch (a.numberType)
			{
				case NumberType.Float:
					ret.Value = a.To<float>() * b.To<float>();
					break;
				case NumberType.Double:
					ret.Value = a.To<double>() * b.To<double>();
					break;
				case NumberType.Decimal:
					ret.Value = a.To<decimal>() * b.To<decimal>();
					break;
			}
			return ret;
		}

		public static Number operator /(Number a, Number b)
		{
			Number ret = new Number(Math.Max(a.Precision, b.Precision));
			switch (a.numberType)
			{
				case NumberType.Float:
					ret.Value = a.To<float>() / b.To<float>();
					break;
				case NumberType.Double:
					ret.Value = a.To<double>() / b.To<double>();
					break;
				case NumberType.Decimal:
					ret.Value = a.To<decimal>() / b.To<decimal>();
					break;
			}
			return ret;
		}
	}


}

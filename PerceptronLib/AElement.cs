// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;
using System.Collections.Generic;

namespace Tac.Perceptron
{
	/// <summary>
	/// A - элемент (ассоциация в скрытом слое между S- и R- элементами)
	/// </summary>
	public class AElement
	{
		// Идентификатор А-элемента
		public int Id;
		// Уровень активации А-элемента
		public int ActivationLevel = 0;
		// Синапсы соединенные с этим А-элементом 
		public Dictionary<int, Sinaps> Sinaps  = new Dictionary<int, Sinaps>();

		public AElement(int argId, SElement sensorsField, int argSCount, Random argRnd)
		{
			Id = argId;
			int SinapsCount = 10;

			int sensorNumber = 0;
			int sensorBlock = 0;
			int sensorNumber32 = 0;
			sbyte sensorType = 0;

			for (int j = 0; j < SinapsCount; j++)
			{
				sensorNumber = argRnd.Next(argSCount);

				if (Sinaps.ContainsKey(sensorNumber) == false)
				{
					sensorBlock = sensorNumber / 32;
					sensorNumber32 = sensorNumber % 32;
					if (argRnd.Next(2) == 0) sensorType = 1; else sensorType = -1;

					Sinaps sinaps = new Sinaps(Id, sensorNumber, sensorType, AssumeSinapsSignal);
					Sinaps.Add(sensorNumber, sinaps);
					sensorsField.AddSinaps(sensorNumber, sinaps);
				}
			}
		}

		void AssumeSinapsSignal(sbyte Type)
		{
			ActivationLevel += Type;
		}
	}

}

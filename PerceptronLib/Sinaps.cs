// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

using System;

namespace Tac.Perceptron
{
	public delegate void ActivateSinaps(sbyte Type);
	public class Sinaps
	{
		public int FromId;
		public int ToId;

		public sbyte type;
		private ActivateSinaps ActivateSinaps;
		public Sinaps(int argFromId, int argToId, sbyte argType, ActivateSinaps argActivateSinaps)
		{
			FromId = argFromId;
			ToId = argToId;
			type = argType;
			ActivateSinaps = argActivateSinaps;
		}
		public void Activate()
		{
			ActivateSinaps(type);
		}
	}
}

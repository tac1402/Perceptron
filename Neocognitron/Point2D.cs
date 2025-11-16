// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Neocognitron
{
	public class Point2D
	{
		public float X;
		public float Y;

		public Point2D(float x, float y)
		{
			X = x;
			Y = y;
		}

		public float Distance(float x, float y)
		{
			float dx = X - x;
			float dy = Y - y;
			return (float)Math.Sqrt(dx * dx + dy * dy);
		}

	}
}

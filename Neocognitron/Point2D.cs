// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Neocognitron
{
	public class Point2D
	{
		public int X;
		public int Y;

		public Point2D(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static float Distance(float centerX, float centerY, float x, float y)
		{
			float dx = centerX - x;
			float dy = centerY - y;
			return (float)Math.Sqrt(dx * dx + dy * dy);
		}

	}
}

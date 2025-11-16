// Author: Sergej Jakovlev <tac1402@gmail.com>
// Copyright (C) 2025 Sergej Jakovlev

namespace Tac.Neocognitron
{
	public class Location
	{
		// The specific plane within a layer
		public int Plane;
		// The location of a cell within a specific plane.
		public Point2D Point;

		public Location(int argK, int argX, int argY)
		{ 
			Plane = argK;
			Point = new Point2D(argX, argY);
		}
	}
}

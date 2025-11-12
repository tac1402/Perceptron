using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neocognitron
{
	public class Point2D
	{
		public double X;
		public double Y;

		public Point2D(double x, double y)
		{
			X = x;
			Y = y;
		}

		public double Distance(double x, double y)
		{
			double dx = X - x;
			double dy = Y - y;
			return Math.Sqrt(dx * dx + dy * dy);
		}

	}
}

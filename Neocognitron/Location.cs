using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neocognitron
{
	public class Location
	{
		// The specific plane within a layer
		private int k;
		// The location of a cell within a specific plane.
		private Point2D n;

		public Location(int argK, int argX, int argY)
		{ 
			k = argK;
			n = new Point2D(argX, argY);
		}

		/**
		 * Get which plane the specific cell is located within.
		 * 
		 * @return	The plane number.
		 */
		public int getPlane()
		{
			return k;
		}

		/**
		 * Get where the cell is within the plane
		 * 
		 * @return	The point location within the plane.
		 */
		public Point2D getPoint()
		{
			return n;
		}

		/**
		 * Set where the cell is within a specific plane
		 * 
		 * @param p		New point within the plane value.
		 */
		public void setPoint(Point2D p)
		{
			n = p;
		}
	}
}

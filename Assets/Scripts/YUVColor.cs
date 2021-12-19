using System;
using UnityEngine;

namespace OokiiTsuki.ColorPalette
{
    public class YUVColor : ICloneable
    {
		public float Y { get; set; }
		public float U { get; set; }
		public float V { get; set; }

		internal Color ToRGBColor()
		{
			float r = Y + (1.4075f * V);
			float g = Y - (0.3455f * U) - (0.581f * V);
			float b = Y + (1.7790f * U);

			return new Color(r, g, b);
		}

		// Distance between two colors in Y'UV plane
		public static double DistanceTo(YUVColor color1, YUVColor color2)
		{
			var o1 = Mathf.Pow(color1.U - color2.U, 2);
			var o2 = Mathf.Pow(color1.V - color2.V, 2);

			return Mathf.Sqrt(o1 + o2);
		}

		// Returns color that is at specified distance from specified color in Y'UV plane
		public static YUVColor ColorAtDistanceFrom(YUVColor color, YUVColor referenceColor, float distance)
		{
			YUVColor result = new YUVColor() { Y = color.Y, U = color.U, V = color.V };

			if (color.Y > referenceColor.Y)
				// Need to increase color.Y value
				result.Y = referenceColor.Y + distance;
			else
				// Need to decrease color.Y value
				result.Y = referenceColor.Y - distance;

			return result;
		}

		// Lighttens color by specified distance
		public static YUVColor LighttenByDistane(YUVColor color, float distance)
		{
			YUVColor result = (YUVColor)color.Clone();

			result.Y += distance;

			if (result.Y > 1)
				// Y value can't be > 1
				result.Y = 0.85f;

			return result;
		}

		// Darkens color by specified distance
		public static YUVColor DarkenByDistane(YUVColor color, float distance)
		{
			YUVColor result = (YUVColor)color.Clone();

			result.Y -= distance;

			if (result.Y < 0)
				// Y value can't be < 0
				result.Y = 0.15f;

			return result;
		}

		public override string ToString()
		{
			return $"(Y={Y}, U={U}, V={V})";
		}

		public object Clone()
		{
			return new YUVColor() { Y = this.Y, U = this.U, V = this.V };
		}
	}
}
using UnityEngine;

namespace OokiiTsuki.ColorPalette
{
    public static class ColorExtentions
    {
		public static YUVColor ToYUV(this Color color)
		{
			// Calculate Y'UV color:
			float y = 0.299f * color.r + 0.587f * color.g + 0.114f * color.b;
			//float u = 0.492f * (color.b - y);
			//float v = 0.877f * (color.a - y);
			float u = -0.147f * color.r - 0.289f * color.g + 0.436f * color.b;
			float v = 0.615f * color.r - 0.515f * color.g - 0.1f * color.b;
			return new YUVColor() { Y = y, U = u, V = v };
		}
	}
}
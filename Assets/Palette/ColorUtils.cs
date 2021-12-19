using System;
using UnityEngine;

namespace OokiiTsuki.Palette
{
    public static class ColorUtils
    {
        private const int MIN_ALPHA_SEARCH_MAX_ITERATIONS = 10;
        private const int MIN_ALPHA_SEARCH_PRECISION = 1;
        private const float MIN_CONTRAST_TITLE_TEXT = 3.0f;
        private const float MIN_CONTRAST_BODY_TEXT = 4.5f;
        public static float CalculateXyzLuma(this int color)
        {
            Color32 c = color.ToColor();
            return (0.2126f * c.r +
                    0.7152f * c.g +
                    0.0722f * c.b / 255f);
        }
        public static float CalculateContrast(this int color1, int color2)
        {
            return Mathf.Abs(color1.CalculateXyzLuma() - color2.CalculateXyzLuma());
        }
        public static float[] RGBtoHSL(int r, int g, int b)
        {
            float rf = r / 255f;
            float gf = g / 255f;
            float bf = b / 255f;
            float max = Mathf.Max(rf, Mathf.Max(gf, bf));
            float min = Mathf.Min(rf, Mathf.Min(gf, bf));
            float deltaMaxMin = max - min;
            float h, s;
            float l = (max + min) / 2f;
            float[] hsl = new float[3];
            if (max == min)
            {
                // Monochromatic
                h = s = 0f;
            }
            else
            {
                if (max == rf)
                {
                    h = ((gf - bf) / deltaMaxMin) % 6f;
                }
                else if (max == gf)
                {
                    h = ((bf - rf) / deltaMaxMin) + 2f;
                }
                else
                {
                    h = ((rf - gf) / deltaMaxMin) + 4f;
                }
                s = deltaMaxMin / (1f - Mathf.Abs(2f * l - 1f));
            }
            hsl[0] = (h * 60f) % 360f;
            hsl[1] = s;
            hsl[2] = l;
            return hsl;
        }
        public static int HSLtoRGB(float[] hsl)
        {
            float h = hsl[0];
            float s = hsl[1];
            float l = hsl[2];
            float c = (1f - Mathf.Abs(2 * l - 1f)) * s;
            float m = l - 0.5f * c;
            float x = c * (1f - Mathf.Abs((h / 60f % 2f) - 1f));
            int hueSegment = (int)h / 60;
            int r = 0, g = 0, b = 0;
            switch (hueSegment)
            {
                case 0:
                    r = (int)Mathf.Round(255 * (c + m));
                    g = (int)Mathf.Round(255 * (x + m));
                    b = (int)Mathf.Round(255 * m);
                    break;
                case 1:
                    r = (int)Mathf.Round(255 * (x + m));
                    g = (int)Mathf.Round(255 * (c + m));
                    b = (int)Mathf.Round(255 * m);
                    break;
                case 2:
                    r = (int)Mathf.Round(255 * m);
                    g = (int)Mathf.Round(255 * (c + m));
                    b = (int)Mathf.Round(255 * (x + m));
                    break;
                case 3:
                    r = (int)Mathf.Round(255 * m);
                    g = (int)Mathf.Round(255 * (x + m));
                    b = (int)Mathf.Round(255 * (c + m));
                    break;
                case 4:
                    r = (int)Mathf.Round(255 * (x + m));
                    g = (int)Mathf.Round(255 * m);
                    b = (int)Mathf.Round(255 * (c + m));
                    break;
                case 5:
                case 6:
                    r = (int)Mathf.Round(255 * (c + m));
                    g = (int)Mathf.Round(255 * m);
                    b = (int)Mathf.Round(255 * (x + m));
                    break;
            }
            r = Mathf.Max(0, Mathf.Min(255, r));
            g = Mathf.Max(0, Mathf.Min(255, g));
            b = Mathf.Max(0, Mathf.Min(255, b));
            return new Color32((byte)r, (byte)g, (byte)b, 255).ToInt();
        }
        public static float[] RGBToXYZ(byte r, byte g, byte b)
        {
            float[] outXyz = new float[3];
            float sr = r / 255.0f;
            sr = sr < 0.04045f ? sr / 12.92f : Mathf.Pow((sr + 0.055f) / 1.055f, 2.4f);
            float sg = g / 255.0f;
            sg = sg < 0.04045f ? sg / 12.92f : Mathf.Pow((sg + 0.055f) / 1.055f, 2.4f);
            float sb = b / 255.0f;
            sb = sb < 0.04045f ? sb / 12.92f : Mathf.Pow((sb + 0.055f) / 1.055f, 2.4f);

            outXyz[0] = 100 * (sr * 0.4124f + sg * 0.3576f + sb * 0.1805f);
            outXyz[1] = 100 * (sr * 0.2126f + sg * 0.7152f + sb * 0.0722f);
            outXyz[2] = 100 * (sr * 0.0193f + sg * 0.1192f + sb * 0.9505f);
            return outXyz;
        }
        public static Color32 ToColor(this int color)
        {
            Color32 c = new Color32();
            c.b = (byte)(color & 0xFF);
            c.g = (byte)((color >> 8) & 0xFF);
            c.r = (byte)((color >> 16) & 0xFF);
            c.a = (byte)((color >> 24) & 0xFF);
            return c;
        }
        public static int ToInt(this Color32 color)
        {
            return (color.a & 0xFF) << 24 | (color.r & 0xFF) << 16 | (color.g & 0xFF) << 8 | (color.b & 0xFF);
        }
        public static int Red(this int color) => (color >> 16) & 0xFF;
        public static int Green(this int color) => (color >> 8) & 0xFF;
        public static int Blue(this int color) => color & 0xFF;

        ///<summary>Returns an appropriate color to use for any 'title' text which is displayed over this <c>color</c>.
        ///This color is guaranteed to have sufficient contrast.</summary>
        ///<returns>An appropriate color</returns>
        public static Color GetTitleTextColor(this Color color) => GetTitleTextColor((Color32)color);
        
        ///<summary>Returns an appropriate color to use for any 'body' text which is displayed over this <c>color</c>.
        ///This color is guaranteed to have sufficient contrast.</summary>
        ///<returns>An appropriate color</returns>
        public static Color GetBodyTextColor(this Color color) => GetBodyTextColor((Color32)color);

        ///<summary>Returns an appropriate color to use for any 'title' text which is displayed over this <c>color</c>.
        ///This color is guaranteed to have sufficient contrast.</summary>
        ///<returns>An appropriate color</returns>
        public static Color32 GetTitleTextColor(this Color32 color)
        {
            int lightTitleAlpha = CalculateMinimumAlpha(Color.white, color, MIN_CONTRAST_TITLE_TEXT);
            if (lightTitleAlpha != -1)
            {
                // If we found valid light values, use them and return
                return new Color32(255, 255, 255, (byte)lightTitleAlpha);
            }
            int darkTitleAlpha = CalculateMinimumAlpha(Color.black, color, MIN_CONTRAST_TITLE_TEXT);
            if (darkTitleAlpha != -1)
            {
                // If we found valid dark values, use them and return
                return new Color32(0, 0, 0, (byte)darkTitleAlpha);
            }
            

            return lightTitleAlpha != -1
                ? new Color32(255, 255, 255, (byte)lightTitleAlpha)
                : new Color32(0, 0, 0, (byte)darkTitleAlpha);
        }

        ///<summary>Returns an appropriate color to use for any 'body' text which is displayed over this <c>color</c>.
        ///This color is guaranteed to have sufficient contrast.</summary>
        ///<returns>An appropriate color</returns>
        public static Color32 GetBodyTextColor(this Color32 color)
        {
            int lightBodyAlpha = CalculateMinimumAlpha(Color.white, color, MIN_CONTRAST_BODY_TEXT);
            if (lightBodyAlpha != -1)
            {
                // If we found valid light values, use them and return
                return new Color32(255, 255, 255, (byte)lightBodyAlpha);
            }
            int darkBodyAlpha = CalculateMinimumAlpha(Color.black, color, MIN_CONTRAST_BODY_TEXT);
            if (darkBodyAlpha != -1)
            {
                // If we found valid dark values, use them and return
                return new Color32(0, 0, 0, (byte)darkBodyAlpha);
            }
            return lightBodyAlpha != -1
                ? new Color32(255, 255, 255, (byte)lightBodyAlpha)
                : new Color32(0, 0, 0, (byte)darkBodyAlpha);
        }
        public static int CalculateMinimumAlpha(this Color32 foreground, Color32 background, float minContrastRatio)
        {
            if (background.a != 255)
                throw new ArgumentException("background can not be translucent: #" + background.ToInt().ToString("X"));

            // First lets check that a fully opaque foreground has sufficient contrast
            Color32 testForeground = new Color32(foreground.r, foreground.g, foreground.b, 255);
            float testRatio = CalculateContrast(testForeground, background);
            if (testRatio < minContrastRatio)
            {
                // Fully opaque foreground does not have sufficient contrast, return error
                return -1;
            }
            // Binary search to find a value with the minimum value which provides sufficient contrast
            int numIterations = 0;
            int minAlpha = 0;
            int maxAlpha = 255;

            while (numIterations <= MIN_ALPHA_SEARCH_MAX_ITERATIONS
                && (maxAlpha - minAlpha) > MIN_ALPHA_SEARCH_PRECISION)
            {
                int testAlpha = (minAlpha + maxAlpha) / 2;

                testForeground = new Color32(foreground.r, foreground.g, foreground.b, (byte)testAlpha);
                testRatio = CalculateContrast(testForeground, background);

                if (testRatio < minContrastRatio)
                {
                    minAlpha = testAlpha;
                }
                else
                {
                    maxAlpha = testAlpha;
                }

                numIterations++;
            }

            // Conservatively return the max of the range of possible alphas, which is known to pass.
            return maxAlpha;
        }
        public static float CalculateContrast(this Color32 foreground, Color32 background)
        {
            if (background.a != 255)
                throw new ArgumentException("background can not be translucent: #" + background.ToInt().ToString("X"));
            if (foreground.a != 255)
            {
                // If the foreground is translucent, composite the foreground over the background
                foreground = CompositeColors(foreground, background);
            }

            float luminance1 = CalculateLuminance(foreground) + 0.05f;
            float luminance2 = CalculateLuminance(background) + 0.05f;

            // Now return the lighter luminance divided by the darker luminance
            return Mathf.Max((float)luminance1, (float)luminance2) / Mathf.Min((float)luminance1, (float)luminance2);
        }
        public static Color32 CompositeColors(this Color32 foreground, Color32 background)
        {
            byte bgAlpha = background.a;
            byte fgAlpha = foreground.a;
            byte a = CompositeAlpha(fgAlpha, bgAlpha);

            byte r = CompositeComponent(foreground.r, fgAlpha, background.r, bgAlpha, a);
            byte g = CompositeComponent(foreground.g, fgAlpha, background.g, bgAlpha, a);
            byte b = CompositeComponent(foreground.b, fgAlpha, background.b, bgAlpha, a);

            return new Color32(r, g, b, a);
        }
        private static byte CompositeAlpha(this byte foregroundAlpha, byte backgroundAlpha)
        {
            return (byte)(0xFF - (((0xFF - backgroundAlpha) * (0xFF - foregroundAlpha)) / 0xFF));
        }
        private static byte CompositeComponent(int fgC, int fgA, int bgC, int bgA, int a)
        {
            if (a == 0)
                return 0;
            return (byte)(((0xFF * fgC * fgA) + (bgC * bgA * (0xFF - fgA))) / (a * 0xFF));
        }
        public static float CalculateLuminance(Color32 color)
        {
            // Luminance is the Y component
            return RGBToXYZ(color.r, color.g, color.b)[1] / 100;
        }
    }
}
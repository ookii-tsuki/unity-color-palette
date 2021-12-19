using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OokiiTsuki.Palette.Utils
{
    public static class ColorUtils
    {
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
    }
}
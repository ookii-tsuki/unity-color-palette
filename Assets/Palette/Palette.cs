using OokiiTsuki.Palette.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OokiiTsuki.Palette
{
    public class Palette
    {
        private const int CALCULATE_TEXTURE_MIN_DIMENSION = 100;
        private const int DEFAULT_CALCULATE_NUMBER_COLORS = 16;
        private const float TARGET_DARK_LUMA = 0.26f;
        private const float MAX_DARK_LUMA = 0.45f;
        private const float MIN_LIGHT_LUMA = 0.55f;
        private const float TARGET_LIGHT_LUMA = 0.74f;
        private const float MIN_NORMAL_LUMA = 0.3f;
        private const float TARGET_NORMAL_LUMA = 0.5f;
        private const float MAX_NORMAL_LUMA = 0.7f;
        private const float TARGET_MUTED_SATURATION = 0.3f;
        private const float MAX_MUTED_SATURATION = 0.4f;
        private const float TARGET_VIBRANT_SATURATION = 1f;
        private const float MIN_VIBRANT_SATURATION = 0.35f;

        private List<Swatch> mSwatches;
        private int mHighestPopulation;

        public Swatch VibrantSwatch { get; private set; }
        public Swatch MutedSwatch { get; private set; }
        public Swatch DarkVibrantSwatch { get; private set; }
        public Swatch DarkMutedSwatch { get; private set; }
        public Swatch LightVibrantSwatch { get; private set; }
        public Swatch LightMutedSwatch { get; private set; }

        public static Palette Generate(Texture2D texture, int numColors = DEFAULT_CALCULATE_NUMBER_COLORS)
        {
            if (numColors < 1)
                throw new ArgumentException("numColors must be 1 or greater");
            // First we'll scale down the bitmap so it's shortest dimension is 100px
            Texture2D scaledTexture = ScaleTextureDown(texture);
            // Now generate a quantizer from the Bitmap
            ColorCutQuantizer quantizer = ColorCutQuantizer.FromTexture2D(scaledTexture, numColors);

            // Now return a ColorExtractor instance
            return new Palette(quantizer.QuantizedColors);
        }
        private Palette(List<Swatch> swatches)
        {
            mSwatches = swatches;
            mHighestPopulation = FindMaxPopulation();
            VibrantSwatch = FindColor(TARGET_NORMAL_LUMA, MIN_NORMAL_LUMA, MAX_NORMAL_LUMA,
                    TARGET_VIBRANT_SATURATION, MIN_VIBRANT_SATURATION, 1f);
            LightVibrantSwatch = FindColor(TARGET_LIGHT_LUMA, MIN_LIGHT_LUMA, 1f,
                    TARGET_VIBRANT_SATURATION, MIN_VIBRANT_SATURATION, 1f);
            DarkVibrantSwatch = FindColor(TARGET_DARK_LUMA, 0f, MAX_DARK_LUMA,
                    TARGET_VIBRANT_SATURATION, MIN_VIBRANT_SATURATION, 1f);
            MutedSwatch = FindColor(TARGET_NORMAL_LUMA, MIN_NORMAL_LUMA, MAX_NORMAL_LUMA,
                    TARGET_MUTED_SATURATION, 0f, MAX_MUTED_SATURATION);
            LightMutedSwatch = FindColor(TARGET_LIGHT_LUMA, MIN_LIGHT_LUMA, 1f,
                    TARGET_MUTED_SATURATION, 0f, MAX_MUTED_SATURATION);
            DarkMutedSwatch = FindColor(TARGET_DARK_LUMA, 0f, MAX_DARK_LUMA,
                    TARGET_MUTED_SATURATION, 0f, MAX_MUTED_SATURATION);
            // Now try and generate any missing colors
            GenerateEmptySwatches();
        }
        
        ///<returns>True if we have already selected <c>swatch</c></returns>
        private bool IsAlreadySelected(Swatch swatch)
        {
            return VibrantSwatch == swatch || DarkVibrantSwatch == swatch ||
                    LightVibrantSwatch == swatch || MutedSwatch == swatch ||
                    DarkMutedSwatch == swatch || LightMutedSwatch == swatch;
        }
        private Swatch FindColor(float targetLuma, float minLuma, float maxLuma,
                             float targetSaturation, float minSaturation, float maxSaturation)
        {
            Swatch max = null;
            float maxValue = 0f;
            foreach (Swatch swatch in mSwatches)
            {
                float sat = swatch.Hsl[1];
                float luma = swatch.Hsl[2];
                if (sat >= minSaturation && sat <= maxSaturation &&
                        luma >= minLuma && luma <= maxLuma &&
                        !IsAlreadySelected(swatch))
                {
                    float thisValue = CreateComparisonValue(sat, targetSaturation, luma, targetLuma,
                            swatch.Population, mHighestPopulation);
                    if (max == null || thisValue > maxValue)
                    {
                        max = swatch;
                        maxValue = thisValue;
                    }
                }
            }
            return max;
        }

        ///<summary>Try and generate any missing swatches from the swatches we did find.</summary>
        private void GenerateEmptySwatches()
        {
            if (VibrantSwatch == null)
            {
                // If we do not have a vibrant color...
                if (DarkVibrantSwatch != null)
                {
                    // ...but we do have a dark vibrant, generate the value by modifying the luma
                    float[] newHsl = CopyHslValues(DarkVibrantSwatch);
                    newHsl[2] = TARGET_NORMAL_LUMA;
                    VibrantSwatch = new Swatch(ColorUtils.HSLtoRGB(newHsl), 0);
                }
            }
            if (DarkVibrantSwatch == null)
            {
                // If we do not have a dark vibrant color...
                if (VibrantSwatch != null)
                {
                    // ...but we do have a vibrant, generate the value by modifying the luma
                    float[] newHsl = CopyHslValues(VibrantSwatch);
                    newHsl[2] = TARGET_DARK_LUMA;
                    DarkVibrantSwatch = new Swatch(ColorUtils.HSLtoRGB(newHsl), 0);
                }
            }
        }
        
        ///<summary>Find the <see cref="Swatch"/> with the highest population value and return the population.</summary>
        private int FindMaxPopulation()
        {
            int population = 0;
            foreach (Swatch swatch in mSwatches)
            {
                population = Mathf.Max(population, swatch.Population);
            }
            return population;
        }

        private static Texture2D ScaleTextureDown(Texture2D texture)
        {
            // Scale texture to fit max size preserving aspect ratio

            var maxResizeFactor = Mathf.Min(CALCULATE_TEXTURE_MIN_DIMENSION / (float)texture.width, CALCULATE_TEXTURE_MIN_DIMENSION / (float)texture.height);

            if (maxResizeFactor > 1)
                return texture;

            var width = (int)(maxResizeFactor * texture.width);
            var height = (int)(maxResizeFactor * texture.height);

            RenderTexture rt = new RenderTexture(width, height, 24);
            RenderTexture.active = rt;
            Graphics.Blit(texture, rt);
            Texture2D result = new Texture2D(width, height);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            return result;
        }
        private static float CreateComparisonValue(float saturation, float targetSaturation,
            float luma, float targetLuma,
            int population, int highestPopulation)
        {
            return WeightedMean(
                    InvertDiff(saturation, targetSaturation), 3f,
                    InvertDiff(luma, targetLuma), 6.5f,
                    population / (float)highestPopulation, 0.5f
            );
        }
        /**
     * Copy a {@link Swatch}'s HSL values into a new float[].
     */
        ///<summary>Copy a <see cref="Swatch"/>'s  HSL values into a new float[].</summary>
        private static float[] CopyHslValues(Swatch color)
        {
            float[] newHsl = new float[3];
            Array.Copy(color.Hsl, 0, newHsl, 0, 3);
            return newHsl;
        }

        ///<returns>
        ///Returns a value in the range 0-1. 1 is returned when <c>value</c> equals the
        ///<c>targetValue</c> and then decreases as the absolute difference between <c>value</c> and
        ///<c>targetValue</c> increases.
        ///</returns>
        ///<param name="value">the item's value</param>
        ///<param name="targetValue">targetValue the value which we desire</param>
        private static float InvertDiff(float value, float targetValue)
        {
            return 1f - Mathf.Abs(value - targetValue);
        }
        private static float WeightedMean(params float[] values)
        {
            float sum = 0f;
            float sumWeight = 0f;
            for (int i = 0; i < values.Length; i += 2)
            {
                float value = values[i];
                float weight = values[i + 1];
                sum += (value * weight);
                sumWeight += weight;
            }
            return sum / sumWeight;
        }
        public Color GetVibrantColor(Color defaultColor = default)
            => VibrantSwatch != null ? VibrantSwatch.ToColor() : defaultColor;
        public Color GetMutedColor(Color defaultColor = default)
            => MutedSwatch != null ? MutedSwatch.ToColor() : defaultColor;
        public Color GetDarkVibrantColor(Color defaultColor = default)
            => DarkVibrantSwatch != null ? DarkVibrantSwatch.ToColor() : defaultColor;
        public Color GetDarkMutedColor(Color defaultColor = default)
            => DarkMutedSwatch != null ? DarkMutedSwatch.ToColor() : defaultColor;
        public Color GetLightVibrantColor(Color defaultColor = default)
            => LightVibrantSwatch != null ? LightVibrantSwatch.ToColor() : defaultColor;
        public Color GetLightMutedColor(Color defaultColor = default)
            => LightMutedSwatch != null ? LightMutedSwatch.ToColor() : defaultColor;
        public class Swatch
        {
            public int Red { get; private set; }
            public int Green { get; private set; }
            public int Blue { get; private set; }
            public int Rgb { get; private set; }
            public int Population { get; private set; }
            private float[] mHsl;
            public Swatch(int rgbColor, int population)
            {
                Red = rgbColor.Red();
                Green = rgbColor.Green();
                Blue = rgbColor.Blue();
                Rgb = rgbColor;
                Population = population;
            }
            public Swatch(int red, int green, int blue, int population)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Rgb = new Color32((byte)red, (byte)green, (byte)blue, 255).ToInt();
                Population = population;
            }
            public Color ToColor()
            {
                return Rgb.ToColor();
            }

            ///<summary>
            ///This swatch's HSL values.
            ///<para>hsv[0] is Hue [0 .. 360)</para>
            ///<para>hsv[1] is Saturation [0...1]</para>
            ///<para>hsv[2] is Lightness [0...1]</para>
            ///</summary>
            public float[] Hsl
            {
                get
                {
                    if (mHsl == null)
                        // Lazily generate HSL values from RGB
                        mHsl = ColorUtils.RGBtoHSL(Red, Green, Blue);
                    return mHsl;
                }
            }
            public override string ToString()
            {
                return new StringBuilder(typeof(Swatch).Name).Append(" ")
                        .Append("[").Append(Rgb.ToString("X")).Append(']')
                        .Append("[HSL: ").Append(string.Join(", ", Hsl)).Append(']')
                        .Append("[Population: ").Append(Population).Append(']').ToString();
            }
        }
    }
}
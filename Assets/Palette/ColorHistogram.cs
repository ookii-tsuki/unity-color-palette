using System;

namespace OokiiTsuki.Palette
{
    public class ColorHistogram
    {
        public int[] Colors { get; private set; }
        public int[] ColorCounts { get; private set; }
        public int NumberOfColors { get; private set; }
        public ColorHistogram(int[] pixels)
        {
            // Sort the pixels to enable counting below
            Array.Sort(pixels);
            // Count number of distinct colors
            NumberOfColors = CountDistinctColors(pixels);
            // Create arrays
            Colors = new int[NumberOfColors];
            ColorCounts = new int[NumberOfColors];
            // Finally count the frequency of each color
            CountFrequencies(pixels);
        }
        private static int CountDistinctColors(int[] pixels)
        {
            if (pixels.Length < 2)
            {
                // If we have less than 2 pixels we can stop here
                return pixels.Length;
            }
            // If we have at least 2 pixels, we have a minimum of 1 color...
            int colorCount = 1;
            int currentColor = pixels[0];
            // Now iterate from the second pixel to the end, counting distinct colors
            for (int i = 1; i < pixels.Length; i++)
            {
                // If we encounter a new color, increase the population
                if (pixels[i] != currentColor)
                {
                    currentColor = pixels[i];
                    colorCount++;
                }
            }
            return colorCount;
        }
        private void CountFrequencies(int[] pixels)
        {
            if (pixels.Length == 0)
                return;

            int currentColorIndex = 0;
            int currentColor = pixels[0];
            Colors[currentColorIndex] = currentColor;
            ColorCounts[currentColorIndex] = 1;
            if (pixels.Length == 1)
                // If we only have one pixel, we can stop here
                return;

            // Now iterate from the second pixel to the end, population distinct colors
            for (int i = 1; i < pixels.Length; i++)
            {
                if (pixels[i] == currentColor)
                {
                    // We've hit the same color as before, increase population
                    ColorCounts[currentColorIndex]++;
                }
                else
                {
                    // We've hit a new color, increase index
                    currentColor = pixels[i];
                    currentColorIndex++;
                    Colors[currentColorIndex] = currentColor;
                    ColorCounts[currentColorIndex] = 1;
                }
            }
        }
    }
}
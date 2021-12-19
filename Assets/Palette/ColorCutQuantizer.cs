using OokiiTsuki.Palette.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static OokiiTsuki.Palette.Palette;

namespace OokiiTsuki.Palette
{
    public class ColorCutQuantizer
    {
        private static float[] mTempHsl = new float[3];
        private const float BLACK_MAX_LIGHTNESS = 0.05f;
        private const float WHITE_MIN_LIGHTNESS = 0.95f;
        private const int COMPONENT_RED = -3;
        private const int COMPONENT_GREEN = -2;
        private const int COMPONENT_BLUE = -1;
        private static int[] colors;
        private static Dictionary<int, int> mColorPopulations;
        public List<Swatch> QuantizedColors { get; private set; }

        
        ///<summary>Factory-method to generate a <see cref="ColorCutQuantizer"/> from a <see cref="Texture2D"/> object.</summary>
        ///<param name="texture">Texture to extract the pixel data from</param>
        ///<param name="maxColors">The maximum number of colors that should be in the result palette.</param>
        public static ColorCutQuantizer FromTexture2D(Texture2D texture, int maxColors)
        {
            Color[] pixels = texture.GetPixels();
            int[] intPixels = new int[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
                intPixels[i] = ((Color32)pixels[i]).ToInt();

            return new ColorCutQuantizer(new ColorHistogram(intPixels), maxColors);
        }

        /// <summary>Private constructor.</summary>
        /// <param name="colorHistogram">histogram representing an image's pixel data</param>
        /// <param name="maxColors">The maximum number of colors that should be in the result palette.</param>
        private ColorCutQuantizer(ColorHistogram colorHistogram, int maxColors)
        {
            if (colorHistogram == null)
            {
                throw new ArgumentException("colorHistogram can not be null");
            }
            if (maxColors < 1)
            {
                throw new ArgumentException("maxColors must be 1 or greater");
            }
            int rawColorCount = colorHistogram.NumberOfColors;
            int[] rawColors = colorHistogram.Colors;
            int[] rawColorCounts = colorHistogram.ColorCounts;
            // First, lets pack the populations into a Dictionary so that they can be easily
            // retrieved without knowing a color's index
            mColorPopulations = new Dictionary<int, int>();
            for (int i = 0; i < rawColors.Length; i++)
            {
                mColorPopulations.Add(rawColors[i], rawColorCounts[i]);
            }
            // Now go through all of the colors and keep those which we do not want to ignore
            colors = new int[rawColorCount];
            int validColorCount = 0;
            foreach (int color in rawColors)
            {
                if (!ShouldIgnoreColor(color))
                {
                    colors[validColorCount++] = color;
                }
            }
            if (validColorCount <= maxColors)
            {
                // The image has fewer colors than the maximum requested, so just return the colors
                QuantizedColors = new List<Swatch>();
                foreach (int color in colors)
                {
                    QuantizedColors.Add(new Swatch(color, mColorPopulations[color]));
                }
            }
            else
            {
                // We need use quantization to reduce the number of colors
                QuantizedColors = QuantizePixels(validColorCount - 1, maxColors);
            }
        }
        private List<Swatch> QuantizePixels(int maxColorIndex, int maxColors)
        {
            // Create the sorted set which is sorted by volume descending. This means we always
            // split the largest box in the queue
            SortedSet<Vbox> vboxes = new SortedSet<Vbox>();
            // To start, add a box which contains all of the colors
            vboxes.Add(new Vbox(0, maxColorIndex));
            // Now go through the boxes, splitting them until we have reached maxColors or there are no
            // more boxes to split
            vboxes = SplitBoxes(vboxes, maxColors);
            
            // Finally, return the average colors of the color boxes
            return GenerateAverageColors(vboxes);
        }
        private SortedSet<Vbox> SplitBoxes(SortedSet<Vbox> vboxes, int maxSize)
        {

            while (vboxes.Count < maxSize)
            {
                Vbox vbox = vboxes.Max();
                vboxes.Remove(vbox);
                if (vbox != null && vbox.CanSplit())
                {
                    // First split the box, and add the result
                    vboxes.Add(vbox.SplitBox());
                    // Then add the box back
                    vboxes.Add(vbox);
                }
                else
                {
                    // If we get here then there are no more boxes to split, so return
                    return vboxes;
                }
            }
            return vboxes;
        }

        private List<Swatch> GenerateAverageColors(SortedSet<Vbox> vboxes)
        {
            List<Swatch> colors = new List<Swatch>(vboxes.Count);
            
            while (vboxes.Count > 0)
            {
                var vbox = vboxes.Max();
                vboxes.Remove(vbox);
                Swatch color = vbox.GetAverageColor();
                if (!ShouldIgnoreColor(color))
                {                    
                    // As we're averaging a color box, we can still get colors which we do not want, so
                    // we check again here
                    colors.Add(color);
                }
            }
            return colors;
        }

        /// <summary>Represents a tightly fitting box around a color space.</summary>
        private class Vbox : IComparable<Vbox>
        {
            private int lowerIndex;
            private int upperIndex;
            private int minRed, maxRed;
            private int minGreen, maxGreen;
            private int minBlue, maxBlue;
            public Vbox(int lowerIndex, int upperIndex)
            {
                this.lowerIndex = lowerIndex;
                this.upperIndex = upperIndex;
                FitBox();
            }
            int IComparable<Vbox>.CompareTo(Vbox other)
            {
                return this.GetVolume() - other.GetVolume();
            }
            int GetVolume()
            {
                return (maxRed - minRed + 1) * (maxGreen - minGreen + 1) * (maxBlue - minBlue + 1);
            }
            public bool CanSplit()
            {
                return GetColorCount() > 1;
            }
            int GetColorCount()
            {
                return upperIndex - lowerIndex;
            }

            ///<summary>Recomputes the boundaries of this box to tightly fit the colors within the box.</summary>
            void FitBox()
            {
                // Reset the min and max to opposite values
                minRed = minGreen = minBlue = 0xFF;
                maxRed = maxGreen = maxBlue = 0x0;
                for (int i = lowerIndex; i <= upperIndex; i++)
                {
                    int color = colors[i];
                    int r = color.Red();
                    int g = color.Green();
                    int b = color.Blue();
                    if (r > maxRed)
                    {
                        maxRed = r;
                    }
                    if (r < minRed)
                    {
                        minRed = r;
                    }
                    if (g > maxGreen)
                    {
                        maxGreen = g;
                    }
                    if (g < minGreen)
                    {
                        minGreen = g;
                    }
                    if (b > maxBlue)
                    {
                        maxBlue = b;
                    }
                    if (b < minBlue)
                    {
                        minBlue = b;
                    }
                }
            }

            ///<summary>Split this color box at the mid-point along it's longest dimension</summary>
            ///<returns>the new ColorBox</returns>
            public Vbox SplitBox()
            {
                if (!CanSplit())
                {
                    throw new InvalidOperationException("Can not split a box with only 1 color");
                }
                // find median along the longest dimension
                int splitPoint = FindSplitPoint();
                Vbox newBox = new Vbox(splitPoint + 1, upperIndex);
                // Now change this box's upperIndex and recompute the color boundaries
                upperIndex = splitPoint;
                FitBox();
                return newBox;
            }
            ///<returns>The dimension which this box is largest in</returns>
            int GetLongestColorDimension()
            {
                int redLength = maxRed - minRed;
                int greenLength = maxGreen - minGreen;
                int blueLength = maxBlue - minBlue;
                if (redLength >= greenLength && redLength >= blueLength)
                {
                    return COMPONENT_RED;
                }
                else if (greenLength >= redLength && greenLength >= blueLength)
                {
                    return COMPONENT_GREEN;
                }
                else
                {
                    return COMPONENT_BLUE;
                }
            }

            ///<summary>
            ///Finds the point within this box's lowerIndex and upperIndex index of where to split.
            ///This is calculated by finding the longest color dimension, and then sorting the
            ///sub-array based on that dimension value in each color. The colors are then iterated over
            ///until a color is found with at least the midpoint of the whole box's dimension midpoint.
            ///</summary>
            ///<returns>The index of the colors array to split from</returns>
            int FindSplitPoint()
            {
                int longestDimension = GetLongestColorDimension();
                // We need to sort the colors in this box based on the longest color dimension.
                // As we can't use a Comparator to define the sort logic, we modify each color so that
                // it's most significant is the desired dimension
                ModifySignificantOctet(longestDimension, lowerIndex, upperIndex);
                // Now sort...
                Array.Sort(colors, lowerIndex, upperIndex + 1 - lowerIndex);
                // Now revert all of the colors so that they are packed as RGB again
                ModifySignificantOctet(longestDimension, lowerIndex, upperIndex);
                int dimensionMidPoint = MidPoint(longestDimension);
                for (int i = lowerIndex; i < upperIndex; i++)
                {
                    int color = colors[i];
                    switch (longestDimension)
                    {
                        case COMPONENT_RED:
                            if (color.Red() >= dimensionMidPoint)
                            {
                                return i;
                            }
                            break;
                        case COMPONENT_GREEN:
                            if (color.Green() >= dimensionMidPoint)
                            {
                                return i;
                            }
                            break;
                        case COMPONENT_BLUE:
                            if (color.Blue() > dimensionMidPoint)
                            {
                                return i;
                            }
                            break;
                    }
                }
                return lowerIndex;
            }

            ///<returns>The average color of this box.</returns>
            public Swatch GetAverageColor()
            {
                int redSum = 0;
                int greenSum = 0;
                int blueSum = 0;
                int totalPopulation = 0;
                for (int i = lowerIndex; i <= upperIndex; i++)
                {
                    int color = colors[i];
                    int colorPopulation = mColorPopulations[color];
                    totalPopulation += colorPopulation;
                    redSum += colorPopulation * color.Red();
                    greenSum += colorPopulation * color.Green();
                    blueSum += colorPopulation * color.Blue();
                }
                int redAverage = (int)Mathf.Round(redSum / (float)totalPopulation);
                int greenAverage = (int)Mathf.Round(greenSum / (float)totalPopulation);
                int blueAverage = (int)Mathf.Round(blueSum / (float)totalPopulation);
                return new Swatch(redAverage, greenAverage, blueAverage, totalPopulation);
            }

            ///<returns>the midpoint of this box in the given <c>dimension</c></returns>
            int MidPoint(int dimension)
            {
                switch (dimension)
                {
                    case COMPONENT_RED:
                    default:
                        return (minRed + maxRed) / 2;
                    case COMPONENT_GREEN:
                        return (minGreen + maxGreen) / 2;
                    case COMPONENT_BLUE:
                        return (minBlue + maxBlue) / 2;
                }
            }
        }

        ///<summary>
        ///Modify the significant octet in a packed color int. Allows sorting based on the value of a
        ///single color component.
        ///See <see cref="Vbox.FindSplitPoint()"/>
        /// </summary>
        private static void ModifySignificantOctet(int dimension, int lowIndex, int highIndex)
        {
            switch (dimension)
            {
                case COMPONENT_RED:
                    // Already in RGB, no need to do anything
                    break;
                case COMPONENT_GREEN:
                    // We need to do a RGB to GRB swap, or vice-versa
                    for (int i = lowIndex; i <= highIndex; i++)
                    {
                        int color = colors[i];
                        //colors[i] = Color.rgb((color >> 8) & 0xFF, (color >> 16) & 0xFF, color & 0xFF);
                        colors[i] = new Color32((byte)color.Green(), (byte)color.Red(), (byte)color.Blue(), 255).ToInt();
                    }
                    break;
                case COMPONENT_BLUE:
                    // We need to do a RGB to BGR swap, or vice-versa
                    for (int i = lowIndex; i <= highIndex; i++)
                    {
                        int color = colors[i];
                        //colors[i] = Color.rgb(color & 0xFF, (color >> 8) & 0xFF, (color >> 16) & 0xFF);
                        colors[i] = new Color32((byte)color.Blue(), (byte)color.Green(), (byte)color.Red(), 255).ToInt();
                    }
                    break;
            }
        }
        private static bool ShouldIgnoreColor(int color)
        {
            mTempHsl = ColorUtils.RGBtoHSL(color.Red(), color.Green(), color.Blue());
            return ShouldIgnoreColor(mTempHsl);
        }
        private static bool ShouldIgnoreColor(Swatch color)
        {
            return ShouldIgnoreColor(color.Hsl);
        }
        private static bool ShouldIgnoreColor(float[] hslColor)
        {
            return IsWhite(hslColor) || IsBlack(hslColor) || IsNearRedILine(hslColor);
        }
        
        ///<returns>True if the color represents a color which is close to black.</returns>
        private static bool IsBlack(float[] hslColor)
        {
            return hslColor[2] <= BLACK_MAX_LIGHTNESS;
        }

        ///<returns>True if the color represents a color which is close to white.</returns>
        private static bool IsWhite(float[] hslColor)
        {
            return hslColor[2] >= WHITE_MIN_LIGHTNESS;
        }
 
        ///<returns>True if the color lies close to the red side of the I line.</returns>
        private static bool IsNearRedILine(float[] hslColor)
        {
            return hslColor[0] >= 10f && hslColor[0] <= 37f && hslColor[1] <= 0.82f;
        }
    }
}
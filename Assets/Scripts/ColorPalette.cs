using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OokiiTsuki.ColorPalette
{
    public class ColorPalette
    {
        // =========================== Constants ============================ //

        // 0 - 2√2. Colors within this range will be treated as equal
        private const double RANGE = 0.075;

        // 0 - 1. Mininum distance of color's Y component for colors not to blend in with each other
        private const float MIN_CONTRAST_DISTANCE = 0.5f;

        // 0 - 1. By this distance color's Y component will be increased/decreased when generating lighter and darker version of colors
        private const float VARIANT_COLOR_DISTANCE = 0.3f;

        // Maximum image width and height. If image is bigger, it will be scaled down to fit
        private const int MAX_IMAGE_SIZE = 40;

        // ======================== Public properties ======================= //

        /// <summary>
        /// Dominant muted color
        /// </summary>
        public Color MutedColor { get; private set; }

        /// <summary>
        /// Dominant vibrant color
        /// </summary>
        public Color VibrantColor { get; private set; }

        /// <summary>
        /// Dominant light muted color
        /// </summary>
        public Color LightMutedColor { get; private set; }

        /// <summary>
        /// Dominant light vibrant color
        /// </summary>
        public Color LightVibrantColor { get; private set; }

        /// <summary>
        /// Dominant dark muted color
        /// </summary>
        public Color DarkMutedColor { get; private set; }

        /// <summary>
        /// Dominant light vibrant color
        /// </summary>
        public Color DarkVibrantColor { get; private set; }

        // ========================= private fields ========================= //

        private List<PixelColor> swatch = new List<PixelColor>();

        // ================================================================= //
        public ColorPalette(Texture2D image)
        {
            // Scale image to fit max size preserving aspect ratio
            image = ScaleImageDown(image);

            for (int y = 1; y < image.height - 1; y++)
            {
                for (int x = 1; x < image.width - 1; x++)
                {
                    var color = image.GetPixel(x, y);
                    var cl = swatch.Find(c => YUVColor.DistanceTo(c.Color, color.ToYUV()) <= RANGE);

                    if (cl == null)
                        // Add new color to the list
                        swatch.Add(new PixelColor() { Color = color.ToYUV(), NumberOfOccurrences = 0 });
                    else
                        // This color already exists. Just increase NumberOfOccurrences
                        cl.NumberOfOccurrences++;
                }
            }
            // Find the color palette
            FindColorPalette();
        }

        private void FindColorPalette()
        {
            YUVColor dominantColor;
            YUVColor accentColor;
            YUVColor mutedColor;
            YUVColor vibrantColor;
            YUVColor lightMutedColor;
            YUVColor lightVibrantColor;
            YUVColor darkMutedColor;
            YUVColor darkVibrantColor;


            // Sort list
            swatch = swatch.OrderByDescending(c => c.NumberOfOccurrences).ToList();

            // Dominant color is the most frequent one
            dominantColor = swatch[0].Color;

            // Find accent color
            double biggestDifference = 0;
            int accentColorIndex = 1;

            if (swatch.Count >= 2)
            {
                for (int i = 1; i < swatch.Count; i++)
                {
                    // Accent color is the most distant color from dominant color
                    var distance = YUVColor.DistanceTo(swatch[i].Color, dominantColor);
                    if (distance > biggestDifference)
                    {
                        biggestDifference = distance;
                        accentColorIndex = i;
                    }
                }

                accentColor = swatch[accentColorIndex].Color;
            }
            else
            {
                // Only one color in the image. Bruh
                accentColor = swatch[0].Color;
            }

            if (YUVColor.DistanceTo(accentColor, dominantColor) < MIN_CONTRAST_DISTANCE)
            {
                // Colors are to close and might blend with each other. We need to seperate them
                accentColor = YUVColor.ColorAtDistanceFrom(accentColor, dominantColor, MIN_CONTRAST_DISTANCE);
            }

            // Determine which color is darker
            if (dominantColor.Y < accentColor.Y)
            {
                // dominant color is darker
                mutedColor = dominantColor;
                vibrantColor = accentColor;
            }
            else
            {
                // accent color is darker
                mutedColor = accentColor;
                vibrantColor = dominantColor;
            }

            // Create lighter version of muted and vibrant colors
            lightMutedColor = YUVColor.LighttenByDistane(mutedColor, VARIANT_COLOR_DISTANCE);
            lightVibrantColor = YUVColor.LighttenByDistane(vibrantColor, VARIANT_COLOR_DISTANCE);

            // Create darker version of muted and vibrant colors
            darkMutedColor = YUVColor.DarkenByDistane(mutedColor, VARIANT_COLOR_DISTANCE);
            darkVibrantColor = YUVColor.DarkenByDistane(vibrantColor, VARIANT_COLOR_DISTANCE);

            // Assign
            MutedColor = mutedColor.ToRGBColor();
            VibrantColor = vibrantColor.ToRGBColor();

            LightMutedColor = lightMutedColor.ToRGBColor();
            LightVibrantColor = lightVibrantColor.ToRGBColor();

            DarkMutedColor = darkMutedColor.ToRGBColor();
            DarkVibrantColor = darkVibrantColor.ToRGBColor();
        }

        private Texture2D ScaleImageDown(Texture2D sourceImage)
        {
            // Scale image to fit max size preserving aspect ratio

            var maxResizeFactor = Mathf.Min(MAX_IMAGE_SIZE / (float)sourceImage.width, MAX_IMAGE_SIZE / (float)sourceImage.height);

            if (maxResizeFactor > 1)
                return sourceImage;

            var width = (int)(maxResizeFactor * sourceImage.width);
            var height = (int)(maxResizeFactor * sourceImage.height);

            RenderTexture rt = new RenderTexture(width, height, 24);
            RenderTexture.active = rt;
            Graphics.Blit(sourceImage, rt);
            Texture2D result = new Texture2D(width, height);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            return result;
        }
        /*public override string ToString()
        {
            return $"MutedColor: {MutedColor}\nVibrantColor: {VibrantColor}\n"
        }*/
    }
}

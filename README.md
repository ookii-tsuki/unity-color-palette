## About The Project
 This is a unity library for generating a color palette from an image ported from [Android's Palette API](https://developer.android.com/training/material/palette-colors)
 
## Installation
 Download the [Latest release](https://github.com/ookii-tsuki/unity-color-palette/releases) of the unity package and open it while the Unity Editor is open.

## Create a palette
 A `Palette` object gives you access to the primary colors in an image, as well as the corresponding colors for overlaid text. Use palettes to design your games's style and to dynamically change your games's color scheme based on a given source image.
 
 ### Generate a Palette instance
 Generate a `Palette` instance using `Palette`'s `Generate(Texture2D texture)` function
 ```csharp
 Palette palette = Palette.Generate(image.sprite.texture);
 ```
 Based on the standards of material design, the palette library extracts commonly used color profiles from an image. Each profile is defined by a Target, and colors extracted from the texture image are scored against each profile based on saturation, luminance, and population (number of pixels in the texture represented by the color). For each profile, the color with the best score defines that color profile for the given image.
 
The palette library attempts to extract the following six color profiles:
* Light Vibrant
* Vibrant
* Dark Vibrant
* Light Muted
* Muted
* Dark Muted

Each of `Palette`'s `Get<Profile>Color()` methods returns the color in the palette associated with that particular profile, where `<Profile>` is replaced by the name of one of the six color profiles. For example, the method to get the Dark Vibrant color profile is `GetDarkVibrantColor()`. Since not all images will contain all color profiles, you can also provide a default color to return.

This figure displays a photo and its corresponding color profiles from the `Get<Profile>Color()` methods.
<p align="center">
<img src="https://developer.android.com/training/material/images/palette-library-color-profiles_2-1_2x.png" width="500" title="Figure 1">
</p>

```csharp
MutedColor.color = palette.GetMutedColor();
VibrantColor.color = palette.GetVibrantColor();
LightMutedColor.color = palette.GetLightMutedColor();
LightVibrantColor.color = palette.GetLightVibrantColor();
DarkMutedColor.color = palette.GetDarkMutedColor();
DarkVibrantColor.color = palette.GetDarkVibrantColor();
```
You can aso create more comprehensive color schemes using the `GetBodyTextColor()` and `GetTitleTextColor()` extension methods of Unity's `Color`. These methods return colors appropriate for use over the swatch’s color.
```csharp
Color vibrantColor = palette.GetVibrantColor();
backgroud.color = vibrantColor;
text.color = vibrantColor.GetTitleTextColor();
```
An example image with its vibrant-colored toolbar and corresponding title text color.
<p align="center">
<img src="https://developer.android.com/training/material/images/palette-library-title-text-color_2-1_2x.png" width="300" title="Figure 2">
</p>

## Preview
 These are some preview images from the Unity Editor
 <p align="center">
<img src="https://developer.android.com/training/material/images/palette-library-color-profiles_2-1_2x.png" width="600" title="Preview 1">
</p>
<p align="center">
<img src="https://developer.android.com/training/material/images/palette-library-color-profiles_2-1_2x.png" width="600" title="Preview 2">
</p>
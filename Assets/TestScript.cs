using System.Collections;
using System.Diagnostics;
using OokiiTsuki.Palette;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    public Image Main;

    public Image MutedColor;
    public Image VibrantColor;
    public Image LightMutedColor;
    public Image LightVibrantColor;
    public Image DarkMutedColor;
    public Image DarkVibrantColor;
    // Start is called before the first frame update
    void Start()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Palette palette = Palette.Generate(Main.sprite.texture);
        sw.Stop();
        print(sw.ElapsedMilliseconds);
        MutedColor.color = palette.GetMutedColor();
        VibrantColor.color = palette.GetVibrantColor();
        LightMutedColor.color = palette.GetLightMutedColor();
        LightVibrantColor.color = palette.GetLightVibrantColor();
        DarkMutedColor.color = palette.GetDarkMutedColor();
        DarkVibrantColor.color = palette.GetDarkVibrantColor();
        print(palette.MutedSwatch.ToString());
    }

}

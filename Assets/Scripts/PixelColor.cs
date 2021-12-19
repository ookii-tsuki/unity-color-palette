namespace OokiiTsuki.ColorPalette
{
    public class PixelColor
    {
		public YUVColor Color { get; set; }

		// Number of its occurrences in the image
		public int NumberOfOccurrences { get; set; }

		public override string ToString()
		{
			return $"[PixelColor: Color={Color}, NumberOfOccurrences={NumberOfOccurrences}]";
		}
	}
}

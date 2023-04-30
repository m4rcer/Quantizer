using Quantizer.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Quantizer.Classes
{
    public class PaletteQuantizer : IColorQuantizer, IPaletteQuantizer
    {
        private readonly Palette palette;
        private readonly Dictionary<Color, byte> cache;
        private readonly Dictionary<int, int> colorMap;

        public PaletteQuantizer()
        {
            palette = new Palette();
            cache = new Dictionary<Color, byte>();
            colorMap = new Dictionary<int, int>();
        }

        public void AddColor(Color color)
        {
            int argb = color.ToArgb();

            if (colorMap.ContainsKey(argb))
            {
                colorMap[argb]++;
            }
            else
            {
                colorMap.Add(argb, 1);
            }
        }

        public Palette GetPalette(int colorCount)
        {
            palette.Clear();

            Random random = new Random(13);

            IEnumerable<Color> colors = colorMap.
                OrderBy(entry => random.NextDouble()).
                Select(entry => Color.FromArgb(entry.Key));

            if (colorMap.Count > colorCount)
            {
                colors = SolveRootLevel(colorCount, colors);

                if (colors.Count() > colorCount)
                {
                    colors.OrderBy(color => colorMap[color.ToArgb()]);
                    colors = colors.Take(colorCount);
                }
            }

            cache.Clear();

            palette.AddColors(colors);

            return palette;
        }

        public byte GetPaletteIndex(Color color)
        {
            byte result;

            if (cache.ContainsKey(color))
            {
                result = cache[color];
            }
            else
            {
                result = (byte)GetNearestColor(color, palette);
                cache[color] = result;
            }

            return result;
        }

        public Dictionary<int,int> GetColors(Bitmap source)
        {
            Rectangle bounds = Rectangle.FromLTRB(0, 0, source.Width, source.Height);
            BitmapData sourceData = source.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                int[] sourceBuffer = new int[source.Width];
                long sourceOffset = sourceData.Scan0.ToInt64();

                for (int row = 0; row < source.Height; row++)
                {
                    Marshal.Copy(new IntPtr(sourceOffset), sourceBuffer, 0, source.Width);

                    foreach (Color color in sourceBuffer.Select(argb => Color.FromArgb(argb)))
                    {
                        AddColor(color);
                    }

                    sourceOffset += sourceData.Stride;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                source.UnlockBits(sourceData);
            }


            return colorMap;
        }


        private static IEnumerable<Color> SolveRootLevel(int colorCount, IEnumerable<Color> colors)
        {
            ColorHueComparer hueComparer = new ColorHueComparer();
            ColorSaturationComparer saturationComparer = new ColorSaturationComparer();
            ColorBrightnessComparer brightnessComparer = new ColorBrightnessComparer();

            IEnumerable<Color> hueColors = colors.Distinct(hueComparer);
            IEnumerable<Color> saturationColors = colors.Distinct(saturationComparer);
            IEnumerable<Color> brightnessColors = colors.Distinct(brightnessComparer);

            int hueColorCount = hueColors.Count();
            int saturationColorCount = saturationColors.Count();
            int brightnessColorCount = brightnessColors.Count();

            if (hueColorCount > saturationColorCount && hueColorCount > brightnessColorCount)
            {
                colors = Solve2ndLevel(hueColors, saturationComparer, brightnessComparer, colorCount);
            }
            else if (saturationColorCount < hueColorCount && saturationColorCount < brightnessColorCount)
            {
                colors = Solve2ndLevel(saturationColors, hueComparer, brightnessComparer, colorCount);
            }
            else
            {
                colors = Solve2ndLevel(brightnessColors, hueComparer, saturationComparer, colorCount);
            }

            return colors;
        }

        private static IEnumerable<Color> Solve2ndLevel(IEnumerable<Color> defaultColors, IEqualityComparer<Color> firstComparer, IEqualityComparer<Color> secondComparer, int colorCountLimit)
        {
            IEnumerable<Color> result = defaultColors;

            if (result.Count() > colorCountLimit)
            {
                IEnumerable<Color> firstColors = result.Distinct(firstComparer);
                IEnumerable<Color> secondColors = result.Distinct(secondComparer);

                if (firstColors.Count() > secondColors.Count())
                {
                    result = Solve3rdLevel(firstColors, secondComparer, colorCountLimit);
                }
                else
                {
                    result = Solve3rdLevel(secondColors, firstComparer, colorCountLimit);
                }
            }

            return result;
        }

        private static IEnumerable<Color> Solve3rdLevel(IEnumerable<Color> defaultColors, IEqualityComparer<Color> secondComparer, int colorCountLimit)
        {
            IEnumerable<Color> result = defaultColors;

            if (result.Count() > colorCountLimit)
            {
                IEnumerable<Color> colors = result.Distinct(secondComparer);

                if (colors.Count() >= colorCountLimit)
                {
                    result = colors;
                }
            }

            return result;
        }

        private static int GetNearestColor(Color color, Palette palette)
        {
            int bestIndex = 0;
            int bestFactor = int.MaxValue;

            for (int index = 0; index < palette.Count; index++)
            {
                Color targetColor = palette[index];

                int deltaA = color.A - targetColor.A;
                int deltaR = color.R - targetColor.R;
                int deltaG = color.G - targetColor.G;
                int deltaB = color.B - targetColor.B;

                int factorA = deltaA * deltaA;
                int factorR = deltaR * deltaR;
                int factorG = deltaG * deltaG;
                int factorB = deltaB * deltaB;

                int factor = factorA + factorR + factorG + factorB;

                if (factor == 0) return index;

                if (factor < bestFactor)
                {
                    bestFactor = factor;
                    bestIndex = index;
                }
            }

            return bestIndex;
        }

        public Bitmap Quantize(Bitmap source, int colorsCount)
        {
            if (source == null)
            {
                const string message = "Cannot quantize a null image.";
                throw new ArgumentNullException(message);
            }

            Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format8bppIndexed);
            Rectangle bounds = Rectangle.FromLTRB(0, 0, source.Width, source.Height);
            BitmapData targetData = result.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            GetColors(source);

            BitmapData sourceData = source.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Palette palette = GetPalette(colorsCount);
            ColorPalette imagePalette = result.Palette;

            for (int index = 0; index < palette.Count; index++)
            {
                imagePalette.Entries[index] = palette[index];
            }

            result.Palette = imagePalette;

            
            try
            {
                byte[] targetBuffer = new byte[result.Width];
                int[] sourceBuffer = new int[source.Width];
                long sourceOffset = sourceData.Scan0.ToInt64();
                long targetOffset = targetData.Scan0.ToInt64();

                for (int row = 0; row < source.Height; row++)
                {
                    Marshal.Copy(new IntPtr(sourceOffset), sourceBuffer, 0, source.Width);

                    for (int index = 0; index < source.Width; index++)
                    {
                        Color color = Color.FromArgb(sourceBuffer[index]);
                        targetBuffer[index] = GetPaletteIndex(color);
                    }

                    Marshal.Copy(targetBuffer, 0, new IntPtr(targetOffset), result.Width);

                    sourceOffset += sourceData.Stride;
                    targetOffset += targetData.Stride;
                }
            }
            finally
            {
                source.UnlockBits(sourceData);
                result.UnlockBits(targetData);
            }

            return result;
        }
    }

}

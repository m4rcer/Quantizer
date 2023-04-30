using Quantizer.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantizer.Extensions
{
    public static class BitmapExtensions
    {
        public static Bitmap Quantize(this Bitmap source, int colorsCount)
        {
            var quantizer = new PaletteQuantizer();

            var result = quantizer.Quantize(source, colorsCount);

            return result;
        }

        public static int GetColorCount(this Bitmap source)
        {
            var quantizer = new PaletteQuantizer();

            var colors = quantizer.GetColors(source);

            return colors.Count;
        }
    }
}

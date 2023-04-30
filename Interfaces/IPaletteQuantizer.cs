using Quantizer.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantizer.Interfaces
{
    public interface IPaletteQuantizer
    {
        void AddColor(Color color);

        Palette GetPalette(int colorCount);

        byte GetPaletteIndex(Color color);

        Dictionary<int, int> GetColors(Bitmap source);
    }

}

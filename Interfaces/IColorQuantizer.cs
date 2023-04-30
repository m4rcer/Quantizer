using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantizer.Interfaces
{
    internal interface IColorQuantizer
    {
        Bitmap Quantize(Bitmap source, int colorsCount);
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantizer.Classes
{
    public class ColorBrightnessComparer : IEqualityComparer<Color>
    {
        public Boolean Equals(Color x, Color y)
        {
            return x.GetBrightness() == y.GetBrightness();
        }

        public Int32 GetHashCode(Color color)
        {
            return color.GetBrightness().GetHashCode();
        }
    }

}

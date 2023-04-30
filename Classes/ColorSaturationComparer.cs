using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantizer.Classes
{
    public class ColorSaturationComparer : IEqualityComparer<Color>
    {
        public Boolean Equals(Color x, Color y)
        {
            return x.GetSaturation() == y.GetSaturation();
        }

        public Int32 GetHashCode(Color color)
        {
            return color.GetSaturation().GetHashCode();
        }
    }

}

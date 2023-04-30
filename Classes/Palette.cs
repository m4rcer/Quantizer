using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Quantizer.Classes
{
    public class Palette
    {
        private readonly List<Color> palette;
        public int Count { get
            {
                return palette.Count; 
            } }


        public Color this [int index]
        {
            get => palette[index];
            set => palette[index] = value;
        }

        public Palette()
        {
            palette = new List<Color>();
        }

        public void AddColors(IEnumerable<Color> colors)
        {
            palette.AddRange(colors);
        }

        public void Clear()
        {
            palette.Clear();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Piccent
{
    public static class ColorConverter
    {
        #region hex converter
        public static string ToHex(this Color c)
        {
            //string s = c.ToString(); #FFbada55
            return String.Format("#{0}{1}{2}", c.R.ToString("X2"), c.G.ToString("X2"), c.B.ToString("X2"));
        }

        public static Color FromHex(this string hex)
        {
            return Color.FromArgb(
                    Convert.ToByte(hex.Substring(1, 2), 16),
                    Convert.ToByte(hex.Substring(3, 2), 16),
                    Convert.ToByte(hex.Substring(5, 2), 16),
                    Convert.ToByte(hex.Substring(7, 2), 16));
        }
#endregion

        #region rgb converter
        public static string ToRGB(this Color c)
        {
            return String.Format("{0} {1} {2}", c.R.ToString(), c.G.ToString(), c.B.ToString());
        }
        #endregion

        #region int converter
        // http://stackoverflow.com/a/9674793/1248177
        public static int ToInt(Color c)
        {
            var argb32 = c.A << 24 | c.R << 16 | c.G << 8 | c.B;
            return argb32;
        }

        public static Color FromInt(int argb32)
        {
            const int mask = 0x000000FF;
            byte a, r, g, b;
            a = (byte)((argb32 >> 24) & mask);
            r = (byte)((argb32 >> 16) & mask);
            g = (byte)((argb32 >> 8) & mask);
            b = (byte)(argb32 & mask);
            return Color.FromArgb(a, r, g, b);
        }
        #endregion
    }
}

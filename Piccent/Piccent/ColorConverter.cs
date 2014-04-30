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

        public static string ToRGB(this Color c)
        {
            //Debug.WriteLine(String.Format("{0} {1} {2}", c.R.AddPadding(), c.G.AddPadding(), c.B.AddPadding()));
            return String.Format("{0} {1} {2}", c.R.ToString(), c.G.ToString(), c.B.ToString());
        }

        //private static string AddPadding(this byte b)
        //{
        //    string s = b.ToString();

        //    //s = s.PadLeft(3 - s.Length, ' ');

        //    for (int i = 0; i < 4 - s.Length; i++)
        //    {
        //        s = ' ' + s;
        //        Debug.WriteLine(s);
        //    }
        //    return s;
        //}
    }
}

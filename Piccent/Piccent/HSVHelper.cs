using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Piccent
{
    public static class HSVHelper
    {
        // http://stackoverflow.com/questions/21972303/colors-harmony-theory-and-algorithm-compute-complementary-triad-tetratic-etc
        public class HSVData
        {
            public double h { get; set; }
            public double s { get; set; }
            public double v { get; set; }
        }

        public static HSVData ConvertColorToHSV(Color RGB)
        {
            double r = Convert.ToDouble(RGB.R) / 255;
            double g = Convert.ToDouble(RGB.G) / 255;
            double b = Convert.ToDouble(RGB.B) / 255;

            double h;
            double s;
            double v;

            double min = Math.Min(Math.Min(r, g), b);
            double max = Math.Max(Math.Max(r, g), b);
            v = max;
            double delta = max - min;
            if (max == 0 || delta == 0)
            {
                s = 0;
                h = 0;
            }
            else
            {
                s = delta / max;
                if (r == max)
                    h = (g - b) / delta; // Between Yellow and Magenta
                else if (g == max)
                    h = 2 + (b - r) / delta; // Between Cyan and Yellow
                else
                    h = 4 + (r - g) / delta; // Between Magenta and Cyan
            }

            h *= 60;
            if (h < 0)
                h += 360;

            return new HSVData() { h = h, s = s, v = v };
        }

        public static Color ConvertHsvToColor(HSVData hsvdata)
        {
            double h = hsvdata.h;
            double s = hsvdata.s;
            double v = hsvdata.v;

            //Debug.Assert(0.0 <= s && s <= 1.0);
            //Debug.Assert(0.0 <= v && v <= 1.0);

            // normalize the hue:
            while (h < 0)
                h += 360;
            while (h > 360)
                h -= 360;

            h = h / 360;

            byte MAX = 255;

            if (s > 0)
            {
                if (h >= 1)
                    h = 0;
                h = 6 * h;
                int hueFloor = Convert.ToInt32(Math.Floor(h));
                byte a = (byte)Math.Round(MAX * v * (1.0 - s));
                byte b = (byte)Math.Round(MAX * v * (1.0 - (s * (h - hueFloor))));
                byte c = (byte)Math.Round(MAX * v * (1.0 - (s * (1.0 - (h - hueFloor)))));
                byte d = (byte)Math.Round(MAX * v);

                switch (hueFloor)
                {
                    case 0: return Color.FromArgb(MAX, d, c, a);
                    case 1: return Color.FromArgb(MAX, b, d, a);
                    case 2: return Color.FromArgb(MAX, a, d, c);
                    case 3: return Color.FromArgb(MAX, a, b, d);
                    case 4: return Color.FromArgb(MAX, c, a, d);
                    case 5: return Color.FromArgb(MAX, d, a, b);
                    default: return Color.FromArgb(0, 0, 0, 0);
                }
            }
            else
            {
                byte d = (byte)(v * MAX);
                return Color.FromArgb(255, d, d, d);
            }
        }
    }
}

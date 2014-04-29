using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Piccent.Resources;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Shapes;

namespace Piccent
{
    // http://imgur.com/a/LuO2G
    // http://www.reddit.com/r/windowsphone/comments/2450vo/one_simple_yet_neat_thing_the_wp_dev_team_could/

    public partial class MainPage : PhoneApplicationPage
    {
        PhotoChooserTask photoChooserTask;
        BitmapImage mainImage;

        public MainPage()
        {
            InitializeComponent();

            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
        }

        #region get picture
        private void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                //MessageBox.Show(e.ChosenPhoto.Length.ToString());

                mainImage = new BitmapImage();
                mainImage.SetSource(e.ChosenPhoto);
                MainImage.Source = mainImage;

                WriteableBitmap wb = new WriteableBitmap(mainImage);
                //SearchColors(wb);
                SolidColorBrush scb = new SolidColorBrush(getDominantColor(wb));
                Src.Background = scb;
                SrcText.Text = HexConverter(scb.Color);
                Res.Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
                ResText.Text = GetCurrentMetroString();
            }
        }

        public static string HexConverter(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            photoChooserTask.Show();
        }
        #endregion

        private void SearchColors(WriteableBitmap wb)
        {
            List<Color> l = new List<Color>();

            for (int y = 0; y < wb.PixelHeight; y++)
            {
                for (int x = 0; x < wb.PixelWidth; x++)
                {
                    Color color = wb.GetPixel(x, y);
                    l.Add(color);
                }
            }
            CountOccurence<Color>(l);
        }
        private void CountOccurence<T>(List<T> collection)
        {
            var g = collection.GroupBy(item => item).OrderByDescending(group => group.Count()).Take(6);

            var l = new List<ColorItem>();

            foreach (var grp in g)
            {
                Debug.WriteLine("{0} {1}", grp.Key, grp.Count());
                l.Add(new ColorItem() { Background = grp.Key.ToString() });
            }
            //ColorsList.ItemsSource = l;
        }

        // http://stackoverflow.com/questions/1068373/how-to-calculate-the-average-rgb-color-values-of-a-bitmap
        /*private Color CalculateAverageColor(WriteableBitmap bm)
        {
            int width = bm.PixelWidth;
            int height = bm.PixelHeight;
            int red = 0;
            int green = 0;
            int blue = 0;
            int minDiversion = 15; // drop pixels that do not differ by at least minDiversion between color values (white, gray or black)
            int dropped = 0; // keep track of dropped pixels
            long[] totals = new long[] { 0, 0, 0 };
            int bppModifier = bm.PixelFormat == System.Windows.Media.Imaging.PixelFormat.Format24bppRgb ? 3 : 4; // cutting corners, will fail on anything else but 32 and 24 bit images

            WriteableBitmap srcData = bm.LockBits(new Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);
            int stride = srcData.Stride;
            IntPtr Scan0 = srcData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (y * stride) + x * bppModifier;
                        red = p[idx + 2];
                        green = p[idx + 1];
                        blue = p[idx];
                        if (Math.Abs(red - green) > minDiversion || Math.Abs(red - blue) > minDiversion || Math.Abs(green - blue) > minDiversion)
                        {
                            totals[2] += red;
                            totals[1] += green;
                            totals[0] += blue;
                        }
                        else
                        {
                            dropped++;
                        }
                    }
                }
            }

            int count = width * height - dropped;
            int avgR = (int)(totals[2] / count);
            int avgG = (int)(totals[1] / count);
            int avgB = (int)(totals[0] / count);

            return Color.FromArgb((byte)255, (byte)avgR, (byte)avgG, (byte)avgB);
        }*/

        public static Color getDominantColor(WriteableBitmap bmp)
        {
            if (bmp.Pixels.Length == 0)
                return Colors.White;

            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            double avg = 0;
            int tot = 0;

            for (int x = 0; x < bmp.PixelWidth; x++)
            {
                for (int y = 0; y < bmp.PixelHeight; y++)
                {
                    Color clr = bmp.GetPixel(x, y);

                    HSVHelper.HSVData hsv = HSVHelper.ConvertColorToHSV(clr);

                    avg += hsv.v;

                    if (hsv.v > 0.42 && hsv.s > 0.42)
                    {
                        r += clr.R;
                        g += clr.G;
                        b += clr.B;
                        tot++;

                    }
                }
            }


            if (tot == 0)
                tot++;

            //r /= bmp.Pixels.Length;
            //g /= bmp.Pixels.Length;
            //b /= bmp.Pixels.Length;

            r /= tot;
            g /= tot;
            b /= tot;

            Debug.WriteLine(r);
            Debug.WriteLine(g);
            Debug.WriteLine(b);
            Debug.WriteLine(avg / bmp.Pixels.Length);

            return Color.FromArgb((byte)255, (byte)r, (byte)g, (byte)b);
        }

        private void Love_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private string GetCurrentMetroString()
        {
            // Determine the accent color.
            Color currentAccentColorHex = (Color)Application.Current.Resources["PhoneAccentColor"];

            string currentAccentColor = "";

            switch (currentAccentColorHex.ToString())
            {
                case "#FF1BA1E2": currentAccentColor = "blue"; break;
                case "#FFA05000": currentAccentColor = "brown"; break;
                case "#FF339933": currentAccentColor = "green"; break;
                case "#FFE671B8": currentAccentColor = "pink"; break;
                case "#FFA200FF": currentAccentColor = "purple"; break;
                case "#FFE51400": currentAccentColor = "red"; break;
                case "#FF00ABA9": currentAccentColor = "teal (viridian)"; break;
                // Lime changed to #FFA2C139 in Windows Phone OS 7.1.
                case "#FF8CBF26":
                case "#FFA2C139": currentAccentColor = "lime"; break;
                // Magenta changed to # FFD80073 in Windows Phone OS 7.1.
                case "#FFFF0097":
                case "#FFD80073": currentAccentColor = "magenta"; break;
                // #FFF9609 (previously orange) is named mango in Windows Phone OS 7.1.
                case "#FFF09609": currentAccentColor = "mango (orange)"; break;
                // Mobile operator or hardware manufacturer color
                default: currentAccentColor = "custom eleventh color"; break; // got amber
            }

            return currentAccentColor;
        }
    }
}
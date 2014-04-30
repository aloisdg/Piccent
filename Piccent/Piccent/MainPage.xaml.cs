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
        #region global
        PhotoChooserTask _photoChooserTask;
        BitmapImage _mainImage;
        #endregion

        public MainPage()
        {
            InitializeComponent();

            _photoChooserTask = new PhotoChooserTask();
            _photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
        }

        #region get picture
        private void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                _mainImage = new BitmapImage();
                _mainImage.SetSource(e.ChosenPhoto);
                MainImage.Source = _mainImage;

                WriteableBitmap wb = new WriteableBitmap(_mainImage);
                //SearchColors(wb);

                DisplayColor(new SolidColorBrush(getDominantColor(wb)));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _photoChooserTask.Show();
        }
        #endregion

        private void DisplayColor(SolidColorBrush scb)
        {
            Src.Background = scb;
            SrcText.Text = ColorConverter.ToHex(scb.Color);

            HSVHelper.HSVData hsv = HSVHelper.ConvertColorToHSV(scb.Color);
            AccentManager am = new AccentManager();

            double diff = 360;
            string nearestColor = "";
            foreach (var hex in am.AccentDictio.Keys)
            {
                double abs = Math.Abs(HSVHelper.ConvertColorToHSV(ColorConverter.FromHex(hex)).h - hsv.h);
                if (diff > abs)
                {
                    nearestColor = hex;
                    diff = abs;
                }
            }

            Color color = ColorConverter.FromHex(nearestColor);
            Res.Background = new SolidColorBrush(color);
            string txt;
            ResText.Text = ColorConverter.ToHex(color);
            ResTextName.Text = (am.AccentDictio.TryGetValue(color.ToString(), out txt)) ? txt : "error";
        }

        #region search most numerous color
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
                //Debug.WriteLine("{0} {1}", grp.Key, grp.Count());
                l.Add(new ColorItem() { Background = grp.Key.ToString() });
            }
            //ColorsList.ItemsSource = l;
        }
        #endregion

        #region search avg color
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

            // limit white and black
            // fixed by myself
            const double limit = 0.265;

            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int tot = 0;

            for (int x = 0; x < bmp.PixelWidth; x++)
            {
                for (int y = 0; y < bmp.PixelHeight; y++)
                {
                    Color clr = bmp.GetPixel(x, y);
                    HSVHelper.HSVData hsv = HSVHelper.ConvertColorToHSV(clr);

                    if (hsv.v > limit && hsv.s > limit)
                    {
                        r += clr.R;
                        g += clr.G;
                        b += clr.B;
                        tot++;
                    }
                }
            }

            // be sure to not divide by 0
            if (tot == 0)
                tot = 1;

            return Color.FromArgb((byte)255, (byte)(r / tot), (byte)(g / tot), (byte)(b / tot));
        }
        #endregion

        #region tap message
        private void Src_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ShowMessage(SrcText.Text);
        }
        private void Res_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ShowMessage(ResText.Text);
        }
        private void ShowMessage(string hexa)
        {
            MessageBoxResult result = MessageBox.Show(String.Format("Your color is {0}.{1}Save it to Clipboard ?", hexa, Environment.NewLine), "Color", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                Clipboard.SetText(hexa);
            }
            else if (result == MessageBoxResult.Cancel)
            {
            }
        }
        #endregion

        private void Love_Click(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void ShowRes_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (ResTextName.Opacity == 1)
                ResTextName.Opacity = 0;
            else
                ResTextName.Opacity = 1;
        }
    }
}
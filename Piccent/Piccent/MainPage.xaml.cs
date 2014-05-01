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
        static bool isFirstTime = true;
        #endregion

        public MainPage()
        {
            InitializeComponent();

            _photoChooserTask = new PhotoChooserTask();
            _photoChooserTask.Completed += new EventHandler<PhotoResult>(photoChooserTask_Completed);
        }

        #region get picture
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _photoChooserTask.Show();
        }

        private void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                _mainImage = new BitmapImage();
                _mainImage.SetSource(e.ChosenPhoto);
                MainImage.Source = _mainImage;

                WriteableBitmap wb = new WriteableBitmap(_mainImage);
                //SearchColors(wb);

                if (isFirstTime)
                {
                    HideTitle();
                    isFirstTime = false;
                }

                //GetPalette(wb);
                DisplayColor(new SolidColorBrush(GetDominantColor(wb)));
            }
        }

        private void HideTitle()
        {
            SrcTitle.Visibility = System.Windows.Visibility.Collapsed;
            ResTitle.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion



        private void GetPalette(WriteableBitmap bmp)
        {

            if (bmp.Pixels.Length == 0)
                return;

            // limit white and black
            // fixed by myself
            const double limit = 0.265;


            Dictionary<string, int> occulot = new Dictionary<string, int>();

            var s2 = Stopwatch.StartNew();
            for (int i = 0; i < bmp.Pixels.Length; i++)
            {
                Color clr = ColorConverter.FromInt(bmp.Pixels[i]);
                HSVHelper.HSVData hsv = HSVHelper.ConvertColorToHSV(clr);

                if (hsv.v > limit && hsv.s > limit)
                {
                    string accentColor = GetAccentColor(clr).ToString();
                    if (occulot.ContainsKey(accentColor))
                        occulot[accentColor]++;
                    else
                        occulot.Add(accentColor, 1);
                }
            }
            s2.Stop();

            Debug.WriteLine(((double)(s2.Elapsed.TotalSeconds)).ToString("0.00 s"));

            var items = from pair in occulot orderby pair.Value descending select pair;

            List<ColorItem> l = new List<ColorItem>();
            foreach (var item in items)
            {
                l.Add(new ColorItem() { Background = item.Key.ToString() });
            }

            //ColorsList.ItemsSource = l;
        }

        private static Color FindNearestColor(Color current, List<Color> colors)
        {
            HSVHelper.HSVData hsv = HSVHelper.ConvertColorToHSV(current);
            double diff = 360;

            Color nearestColor = new Color();
            foreach (var color in colors)
            {
                var tmp = color;
                double abs = Math.Abs(HSVHelper.ConvertColorToHSV(tmp).h - hsv.h);
                if (diff > abs)
                {
                    nearestColor = tmp;
                    diff = abs;
                }
            }

            return nearestColor;
        }

        private static Color GetAccentColor(Color src)
        {
            List<Color> colors = new List<Color>();
            foreach (var hex in AccentManager.GetKeys())
                colors.Add(ColorConverter.FromHex(hex));

            return FindNearestColor(src, colors);
        }

        private void DisplayColor(SolidColorBrush scb)
        {
            Src.Background = scb;
            SrcTextHex.Text = ColorConverter.ToHex(scb.Color);
            SrcTextRGB.Text = ColorConverter.ToRGB(scb.Color);

            Color nearestColor = GetAccentColor(scb.Color);

            Res.Background = new SolidColorBrush(nearestColor);
            ResTextHex.Text = ColorConverter.ToHex(nearestColor);
            ResTextRGB.Text = ColorConverter.ToRGB(nearestColor);
            string name = AccentManager.GetName(nearestColor.ToString());
            ResTextName.Text = !String.IsNullOrWhiteSpace(name) ? name.ToUpper() : "ERROR";
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
        #region fast code cant use
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
        #endregion

        #region algos
        private static Color RunOldAlgo(WriteableBitmap bmp)
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

            var s1 = Stopwatch.StartNew();
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
            s1.Stop();
            Debug.WriteLine((s1.Elapsed.TotalSeconds).ToString("0.00 s"));

            if (tot == 0)
                tot = 1;

            return Color.FromArgb((byte)255, (byte)(r / tot), (byte)(g / tot), (byte)(b / tot));
        }
        private static Color RunNewAlgo(WriteableBitmap bmp)
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

            var s2 = Stopwatch.StartNew();
            for (int i = 0; i < bmp.Pixels.Length; i++)
            {
                Color clr = ColorConverter.FromInt(bmp.Pixels[i]);
                HSVHelper.HSVData hsv = HSVHelper.ConvertColorToHSV(clr);

                if (hsv.v > limit && hsv.s > limit)
                {
                    r += clr.R;
                    g += clr.G;
                    b += clr.B;
                    tot++;
                }
            }
            s2.Stop();

            Debug.WriteLine(((double)(s2.Elapsed.TotalSeconds)).ToString("0.00 s"));

            // be sure to not divide by 0
            if (tot == 0)
                tot = 1;

            return Color.FromArgb((byte)255, (byte)(r / tot), (byte)(g / tot), (byte)(b / tot));
        }
        #endregion

        public static Color GetDominantColor(WriteableBitmap bmp)
        {
            //var color1 = RunOldAlgo(bmp);
            var color2 = RunNewAlgo(bmp);
            return color2;
        }
        #endregion

        #region tap message
        private void Src_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MainImage.Source == null)
                return;
            MessageBoxResult result = MessageBox.Show(
                String.Format("Your color is {0}. This color does not exist in the basic palette.{1}You can ask for it on uservoice.{1}I am going to redirect you to it. Continue?",
                SrcTextHex.Text, Environment.NewLine), "Nice color !", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
                CallUserVoice();
        }

        private void CallUserVoice()
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("http://windowsphone.uservoice.com/forums/101801-feature-suggestions/suggestions/2286613-set-tile-color-free-by-rgb", UriKind.Absolute);
            webBrowserTask.Show();
        }

        private void Res_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MainImage.Source == null)
                return;
            MessageBox.Show(
                String.Format("Your color is {0}.{1}If Microsoft add a way to navigate to the setting \"start+theme\", I will use it here !",
                ResTextHex.Text, Environment.NewLine), "Nice color !", MessageBoxButton.OK);
        }
        #endregion

        private void Love_Click(object sender, RoutedEventArgs e)
        {
            //debug
            DesignGrid.Opacity = DesignGrid.Opacity == 0 ? 1 : 0;

            //MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            //marketplaceReviewTask.Show();
        }
    }
}
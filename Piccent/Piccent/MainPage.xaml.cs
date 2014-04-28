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

namespace Piccent
{
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

        private void photoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                MessageBox.Show(e.ChosenPhoto.Length.ToString());

                //Code to display the photo on the page in an image control named myImage.
                mainImage = new BitmapImage();
                mainImage.SetSource(e.ChosenPhoto);
                MainImage.Source = mainImage;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            photoChooserTask.Show();
        }
    }
}
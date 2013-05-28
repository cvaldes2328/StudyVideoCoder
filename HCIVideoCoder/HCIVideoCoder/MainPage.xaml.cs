using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HCIVideoCoder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        #region variables

        DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        public List<String> _loggingData = new List<String>();
        public StorageFile _sampleFile = null;
        public String _currentVideoString = "";

        #endregion

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void DurationSlider_ValueChanged_1(object sender, RangeBaseValueChangedEventArgs e)
        {
            //int slidervalue = (int)DurationSlider.Value;
            //TimeSpan ts = new TimeSpan(0, 0, 0, 0, slidervalue);
            //VideoMediaElement.Position = ts; 
            int slidervalue = (int)DurationSlider.Value;

            // Overloaded constructor takes the arguments days, hours, minutes, seconds, miniseconds. 
            // Create a TimeSpan with miliseconds equal to the slider value.
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, slidervalue);
            VideoMediaElement.Position = ts;
        }

        private void PlayButton_Click_1(object sender, RoutedEventArgs e)
        {           
            if (VideoMediaElement.CurrentState.Equals(MediaElementState.Paused))
                VideoMediaElement.Play();
            else
                VideoMediaElement.Pause();
        }

        private void RewindButton_Click_1(object sender, RoutedEventArgs e)
        {
            VideoMediaElement.PlaybackRate = VideoMediaElement.PlaybackRate-1;
        }

        private void FastForwardButton_Click_1(object sender, RoutedEventArgs e)
        {
            VideoMediaElement.PlaybackRate = VideoMediaElement.PlaybackRate+1;
        }

       

        private async void LoadFileButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (!ApplicationView.TryUnsnap())
            {
                VisualStateManager.GoToState(this, string.Format("Filled{0}", DisplayProperties.ResolutionScale), true);
            }

            await PickFileAsync();
        }

        private async Task PickFileAsync()
        {
            FileOpenPicker fileopenpicker = new FileOpenPicker();

            fileopenpicker.FileTypeFilter.Add(".wmv");
            fileopenpicker.FileTypeFilter.Add(".mp4");
            fileopenpicker.FileTypeFilter.Add(".mp3");

            fileopenpicker.SuggestedStartLocation = PickerLocationId.Desktop;

            var singlefileoperation = await fileopenpicker.PickSingleFileAsync();
            if (singlefileoperation != null)
            {
                await SetMediaElementSourceAsync(singlefileoperation);
            }
        }

        private async Task SetMediaElementSourceAsync(StorageFile file)
        {
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

            VideoMediaElement.SetSource(stream, file.ContentType);
            
            CurrentVideoTextBlock.Text = file.DisplayName;
        }

        private void Page_KeyDown_1(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key.ToString().Equals("Space"))
            {
                 if (VideoMediaElement.CurrentState.Equals(MediaElementState.Paused))
                    VideoMediaElement.Play();
                 else
                    VideoMediaElement.Pause();
            }else{

                 string time = VideoMediaElement.Position.ToString().Replace(":", ",");
                 
                _loggingData.Add(CurrentVideoTextBlock.Text +";"+ e.Key.ToString() + ";=Time(" + time+")"); 

            }
        }

        private void SaveLogButton_Click_1(object sender, RoutedEventArgs e)
        {
            WriteCSVFile(CurrentVideoTextBlock.Text + ".csv");
        }

        private async void WriteCSVFile(string file)
        {
            try
            {                
                _sampleFile = await DownloadsFolder.CreateFileAsync(file, CreationCollisionOption.GenerateUniqueName);

                if (_sampleFile != null)
                {
                    string loggingstring = "";
                    foreach (string s in _loggingData)
                    {
                        loggingstring = loggingstring + s + "\n";
                    }

                    string userContent = loggingstring;
                    if (!String.IsNullOrEmpty(userContent))
                    {
                        await FileIO.WriteTextAsync(_sampleFile, userContent);                        
                    }
                    else
                    {
                        
                    }
                }
            }
            catch (FileNotFoundException)
            {
                this.NotifyUserFileNotExist();
            }
        }

        private void NotifyUserFileNotExist()
        {
            //Write up error message...
        }

        private void VideoMediaElement_MediaOpened_1(object sender, RoutedEventArgs e)
        {
            VideoLengthTextBlock.Text = VideoMediaElement.NaturalDuration.ToString().Substring(0, 8);
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            //Test if this will work with just duration.tostring
            VideoCurrentLocationTextBlock.Text = VideoMediaElement.Position.Hours
                                        + ":" + VideoMediaElement.Position.Minutes
                                        + ":" + VideoMediaElement.Position.Seconds;
        }
    }
}

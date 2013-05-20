// Houssem Dellai
// Studying Software Engineering 
// houssem.dellai@gmail.com
// +216 95 325 964

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.IO;

namespace VideoReaderMetroStyleV1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public List<String> _loggingData = new List<String>();
        public StorageFile sampleFile = null;
        public String CurrentVideo = "";

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// To play media.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Play();
        }

        /// <summary>
        /// To pause media.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            mediaElement.Pause();
        }

        /// <summary>
        /// To choose a media file to open. 
        /// The file must be of extension .mp4, .wmv or .mp3.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (!ApplicationView.TryUnsnap())
            {
                VisualStateManager.GoToState(this, string.Format("Filled{0}", DisplayProperties.ResolutionScale), true);
            }

            await PickFileAsync();
            //MediaDuration.Text = mediaElement.NaturalDuration.ToString();
        }



        #region Related Methods to Open Media File
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

            mediaElement.SetSource(stream, file.ContentType);
            

            if (_loggingData.Count != 0)
            {
                WriteCSVFile(file.DisplayName+".csv");
            }
            
            CurrentVideo = file.DisplayName;
        }


        private async void WriteCSVFile(string file)
        {
            try
            {
                //StorageFolder storageFolder = KnownFolders.DocumentsLibrary;
                //sampleFile = await storageFolder.CreateFileAsync(file, CreationCollisionOption.GenerateUniqueName);
                sampleFile = await DownloadsFolder.CreateFileAsync(file, CreationCollisionOption.GenerateUniqueName);

                if (sampleFile != null)
                {
                    string loggingstring = "";
                    foreach(string s in _loggingData)
                    {
                        loggingstring = loggingstring + s +"\n";
                    }

                    string userContent = loggingstring;
                    if (!String.IsNullOrEmpty(userContent))
                    {
                        await FileIO.WriteTextAsync(sampleFile, userContent);
                       
                        //OutputTextBlock.Text = "The following text was written to '" + file.Name + "':" + Environment.NewLine + Environment.NewLine + userContent;
                    }
                    else
                    {
                        //OutputTextBlock.Text = "The text box is empty, please write something and then click 'Write' again.";
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
            
        }
        #endregion

        /// <summary>
        /// To change the volume of the media.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VulumeChangedEvent(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            mediaElement.Volume = sliderVolume.Value/100;
        }

        /// <summary>
        /// To be invocked when the media is opened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaOpenedEvent(object sender, RoutedEventArgs e)
        {
            MediaDuration.Text = mediaElement.NaturalDuration.ToString().Substring(0, 8);
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, object e)
        {
            MediaCurrentPosition.Text = mediaElement.Position.Hours
                                        + ":" + mediaElement.Position.Minutes
                                        + ":" + mediaElement.Position.Seconds;
        }

        private void OpenFileOnline_Click(object sender, RoutedEventArgs e)
        {
            popUp.IsOpen = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            popUp.IsOpen = false;
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mediaElement.Source = new Uri(UrlTextBox.Text);
                mediaElement.Play();
            }
            catch (Exception exc)
            {
                MessageDialog message = new MessageDialog("Verify the Url or your internet connexion", exc.Message);
            }
        }

        private void Page_KeyDown_1(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            _loggingData.Add(CurrentVideo +","+ e.Key.ToString() + "," + MediaCurrentPosition.Text);
            
        }
    }
}
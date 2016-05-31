using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;

using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace dashBud
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    
public sealed partial class MainPage : Page
    {
        Windows.Media.Capture.MediaCapture captureManager;
        bool isRecording;
        int recordingNum = 0;
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
        
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.

            startCameraView();
        }
       
        public async void startCameraView()
        {
            captureManager = new MediaCapture();
            await captureManager.InitializeAsync();
            viewFinder.Source = captureManager;
            await captureManager.StartPreviewAsync();
        }

        private async void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            //recordings auto increment up to 5, then being saving over previous recordings
            if (recordingNum < 6)
            {
                recordingNum++;
            }
            else
            {
                recordingNum = 1;
            }
            //record video
            if (isRecording == false)
            {
                isRecording = true;
                //.replaceexisting allows for overwriting files with same name. only a max of 5 videos will exist at any given time
                var videoFile = await KnownFolders.SavedPictures.CreateFileAsync("dashVideo" + recordingNum +".mp4", CreationCollisionOption.ReplaceExisting);
                txtSpeedometer.Text = KnownFolders.SavedPictures.ToString();
                MediaEncodingProfile fileFormat = new MediaEncodingProfile();
                fileFormat = MediaEncodingProfile.CreateMp4(Windows.Media.MediaProperties.VideoEncodingQuality.Auto);

                await captureManager.StartRecordToStorageFileAsync(fileFormat, videoFile);
            }
            //stop recording
            else
            {
                isRecording = false;
                await captureManager.StopRecordAsync();
            }
        }
    }
}

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
using Windows.Media.Editing;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Devices.Sensors;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Devices.Geolocation;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.System.Display;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace dashBud
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class MainPage : Page
    {
        Geolocator geolocator = new Geolocator
        {
            DesiredAccuracy = PositionAccuracy.High,
            MovementThreshold = 1

        };

        Windows.Media.Capture.MediaCapture captureManager;
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        MediaCapture mediaCap = null;
        DisplayRequest screenActive = new DisplayRequest();

        bool isRecording;
        int recordingNum;
        int x1, x2;
        
        public MainPage() 
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            ManipulationStarted += (s, e) => x1 = (int)e.Position.X;
            ManipulationCompleted += (s, e) =>
            {
                x2 = (int)e.Position.X;
                if (x1 > x2)
                {
                    x1 = 0;
                    x2 = 0;
                    Frame.Navigate(typeof(settings));
                    
                }
            };
          

            txtMilesorKph.Text = App.milesOrKph;
            recordingNum = Convert.ToInt32(localSettings.Values["vidNumber"]);
            startCameraView();
            geolocator.PositionChanged += geolocator_PositionChanged;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler<object>(timer_Tick);
            timer.Start();
            
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
            
            txtMilesorKph.Text = App.milesOrKph;


        }
        public void onReturnedTo()
        {

        }

       public void pageNavigation()
        {

        }

        
       //initialize captureManager
        public async void startCameraView()
        {
            if (captureManager == null)
            {
                captureManager = new MediaCapture();
                await captureManager.InitializeAsync();

            }
            if (viewFinder.Source == null)
            {
                viewFinder.Source = captureManager;
                await captureManager.StartPreviewAsync();
            }
           
                        
        }
        
       
        public void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            var speed = args.Position.Coordinate.Speed;
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (App.milesOrKph == "mph")
                {
                    //convert m/s to mph and set to textblock
                    double? mphSpeed = speed * 2.24;
                    txtSpeedometer.Text = mphSpeed.ToString();
                }
                else
                {
                    //convert m/s to kph and set to textblock
                    double? kphSpeed = speed * 3.6;
                    txtSpeedometer.Text = kphSpeed.ToString();
                }
                txtSpeedometer.Text = speed.ToString();
            });

        }

        //display date/time 
        public void timer_Tick(object sender, object e)
        {
            DateTime dt = DateTime.Now;

            txtDateTime.Text = dt.ToString("MM/dd HH:mm:ss");
        }

        //start and stop recording.
        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            
            incrementVideoFile();
            //record video
            if (isRecording == false)
            {
                startRecord();
                isRecording = true;
            }
            //stop recording
            else
            {
                isRecording = false;
                stopRecord();
            }
        }
        //recordings auto increment up to 5, then being saving over previous recordings
        public void incrementVideoFile()
        {
            if (recordingNum < 6)
            {
                recordingNum++;
                localSettings.Values["vidNumber"] = recordingNum;
            }
            else
            {
                recordingNum = 1;
                localSettings.Values["vidNumber"] = recordingNum;

            }
        }
        //start recording
        public async void startRecord()
        {
            isRecording = true;
            btnRecord.Content = "recording";
            incrementVideoFile();
            //.replaceexisting allows for overwriting files with same name. only a max of 5 videos will exist at any given time
            //creates file witin videos lbrary
            var videoFile = await KnownFolders.VideosLibrary.CreateFileAsync("dashVideo" + recordingNum + ".mp4", CreationCollisionOption.ReplaceExisting);

            MediaEncodingProfile fileFormat = new MediaEncodingProfile();
            fileFormat = MediaEncodingProfile.CreateMp4(Windows.Media.MediaProperties.VideoEncodingQuality.Auto);
            //await captureManager.StartRecordToStorageFileAsync(fileFormat, videoFile);

            //initialize screencapture
            var screenCapture = Windows.Media.Capture.ScreenCapture.GetForCurrentView();
            var mCISettings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
            mCISettings.VideoSource = screenCapture.VideoSource;
            mCISettings.AudioSource = screenCapture.AudioSource;
            mCISettings.StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.AudioAndVideo;
            
            //initialize the local media capture object
            mediaCap = new MediaCapture();
            await mediaCap.InitializeAsync(mCISettings);
            (App.Current as App).myMediaCapture = mediaCap;
            await mediaCap.StartRecordToStorageFileAsync(fileFormat, videoFile);
            screenActive.RequestActive();
            mediaCap.RecordLimitationExceeded += MediaCap_RecordLimitationExceeded;
            //stop recording after 5 or 10 minutes, depending on vidLength value
            await Task.Delay(TimeSpan.FromMinutes(App.vidLength));
            stopRecord();
        }
        public void MediaCap_RecordLimitationExceeded(MediaCapture sender)
        {
            // This is a notification that recording has to stop, and the app is expected to finalize the recording

            stopRecord();

        }

        //stop recording
        public async void stopRecord()
        {
            btnRecord.Content = "record";
            screenActive.RequestRelease();
            //await captureManager.StopRecordAsync();
            await mediaCap.StopRecordAsync();
            //if stop record was not pressed, continue on to record another clip
            if (isRecording == true)
            {
                startRecord();
            }

        }
    }
}

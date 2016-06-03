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
        MediaCapture mediaCap = null;
        private MediaComposition composition = new MediaComposition();
        bool isRecording;
        int recordingNum = 0;
        int x1, x2;
        

        public MainPage() 
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            //ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            //ManipulationStarted += (s, e) => x1 = (int)e.Position.X;
            //ManipulationCompleted += (s, e) =>
            //{
            //    x2 = (int)e.Position.X;
            //    if (x1 > x2)
            //    {
            //        Frame.Navigate(typeof(settings));
            //        x1 = 0;
            //        x2 = 0;
            //    }
            //};

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

        }

       public void pageNavigation()
        {

        }

       //initialize captureManager
        public async void startCameraView()
        {
            captureManager = new MediaCapture();
            await captureManager.InitializeAsync();
            viewFinder.Source = captureManager;
            await captureManager.StartPreviewAsync();
                        
        }
        
       
        public void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            var speed = args.Position.Coordinate.Speed.ToString();
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
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
            }
            //stop recording
            else
            {
                stopRecord();
            }
        }
        //recordings auto increment up to 5, then being saving over previous recordings
        public void incrementVideoFile()
        {
            if (recordingNum < 6)
            {
                recordingNum++;
            }
            else
            {
                recordingNum = 1;
            }
        }
        //start recording
        public async void startRecord()
        {
            isRecording = true;
            incrementVideoFile();
            //.replaceexisting allows for overwriting files with same name. only a max of 5 videos will exist at any given time
            var videoFile = await KnownFolders.SavedPictures.CreateFileAsync("dashVideo" + recordingNum + ".mp4", CreationCollisionOption.ReplaceExisting);

            MediaEncodingProfile fileFormat = new MediaEncodingProfile();
            fileFormat = MediaEncodingProfile.CreateMp4(Windows.Media.MediaProperties.VideoEncodingQuality.Auto);
            //await captureManager.StartRecordToStorageFileAsync(fileFormat, videoFile);


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


        }
        //stop recording
        public async void stopRecord()
        {
            isRecording = false;
            //await captureManager.StopRecordAsync();
            await mediaCap.StopRecordAsync();
        }
    }
}

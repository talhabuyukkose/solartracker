using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media;
using Windows.System.Threading;
using Windows.ApplicationModel.Background;
using System.Diagnostics;


namespace SolarTracker
{
    
    public sealed partial class MainPage : Page
    {
        public double pulse_x = 2;
        public double pulse_y = 2;
        MediaCapture mc;
        servomotor_x _servomotor_x;
        servomotor_y _servomotor_y;
        ThreadPoolTimer timer;
        bool _isPreviewing;
        private BackgroundTaskDeferral deferral;
        public MainPage()
        {
            this.InitializeComponent();
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 2);
            timer.Tick += Timer_Tick;
            timer.Start();
            _servomotor_x = new servomotor_x();
            _servomotor_y = new servomotor_y();
            _servomotor_x.Run();
            _servomotor_y.Run();
        }
        private async void Timer_Tick(object sender, object e)
        {
            var previewProperties = mc.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
            var videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);
            var frame = await mc.GetPreviewFrameAsync(videoFrame);

            var bitmap = new WriteableBitmap(frame.SoftwareBitmap.PixelWidth, frame.SoftwareBitmap.PixelHeight);
            frame.SoftwareBitmap.CopyToBuffer(bitmap.PixelBuffer);
            var resized = bitmap.Resize(frame.SoftwareBitmap.PixelWidth / 8, frame.SoftwareBitmap.PixelHeight / 8, WriteableBitmapExtensions.Interpolation.Bilinear);

            var result_sol = "";
            var result_sag = "";
            var result_üst = "";
            var result_alt = "";
            var beyaz = 250;
            var sol = 0;
            var sag = 0;
            var ust = 0;
            var alt = 0;
            var hataPayi = 20;
            var yatayOrt = resized.PixelWidth / 2;
            var dikeyOrt = resized.PixelHeight / 2;
            for (int x = 0; x < resized.PixelWidth; x += 1)
            {
                for (int y = 0; y < resized.PixelHeight; y += 1)
                {
                    var color = resized.GetPixel(x, y);

                    byte c = (byte)((color.R + color.B + color.G) / 3);

                    if (c >= beyaz)
                    {
                        if (x < yatayOrt) sol++;
                        else sag++;

                        if (y < dikeyOrt) ust++;
                        else alt++;
                    }

                }
            }

            if (sol > sag + hataPayi)
            {
                result_sol = "sol";
                if (sol != sag)
                {
                    pulse_x = pulse_x + 0.2; if (pulse_x >= 4) pulse_x = 4;
                    _servomotor_x.SetPulse(pulse_x);
                }
                else { }
            }
            if (sag > sol + hataPayi)
            {
                result_sag = "sağ";
                if (sag != sol)
                {
                    pulse_x = pulse_x - 0.2; if (pulse_x <= 0.01) pulse_x = 0.01;
                    _servomotor_x.SetPulse(pulse_x);
                }
                else { }
            }
            if (ust > alt + hataPayi)
            {
                result_üst = "üst";
                if (ust != alt)
                {
                    pulse_y = pulse_y + 0.2; if (pulse_y >= 4) pulse_y = 4;
                    _servomotor_y.SetPulse(pulse_y);
                }
                else { }
            }
            if (alt > ust + hataPayi)
            {
                result_alt = "alt";
                if (alt != ust)
                {
                    pulse_y = pulse_y - 0.2; if (pulse_y <= 0.01) pulse_y = 0.01;
                    _servomotor_y.SetPulse(pulse_y);
                }
                else { }
            }
            lblResult_sag.Text = result_sag + sag;
            lblResult_sol.Text = result_sol + sol;
            lblResult_üst.Text = result_üst + ust;
            lblResult_alt.Text = result_alt + alt;
        }
        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            mc = new MediaCapture();
            await mc.InitializeAsync();

            PreviewControl.Source = mc;
            await mc.StartPreviewAsync();
        }
    }
}

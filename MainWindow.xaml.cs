using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace rlvid2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool fLoaded = false;
        private int iCurrent = 0;
        private DispatcherTimer _timer;
        private bool isDragging = false;

        public MainWindow()
        {
            InitializeComponent();
            SetupTimer();
        }

        string FormatSeconds(double seconds)
        {
            // get hours
            int hours = (int)(seconds / (60.0 * 60.0));
            int minutes = (int)((seconds - hours * 60.0 * 60.0) / 60.0);
            int secs = (int)(seconds - hours * 60.0 * 60.0 - minutes * 60.0);

            if (hours > 0)
            {
                return $"{hours}:{minutes:D2}:{secs:D2}";
            }
            else
            {
                return $"{minutes}:{secs:D2}";
            }
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500); // Update the slider every 500ms
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Only update the slider if the video is playing
            if (videoPlayer.NaturalDuration.HasTimeSpan && !isDragging)
            {
                double currentPosition = videoPlayer.Position.TotalSeconds;
                videoSlider.Value = currentPosition;

                videoTime.Text = FormatSeconds(videoSlider.Value);
            }
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Set the maximum value for the slider based on the video's duration
            videoSlider.Maximum = videoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            videoTime.Text = videoPlayer.NaturalDuration.TimeSpan.TotalSeconds.ToString();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Optionally handle video end, e.g., reset slider to 0
            videoSlider.Value = 0;
            videoTime.Text = FormatSeconds(videoPlayer.NaturalDuration.TimeSpan.TotalSeconds - videoSlider.Value);
        }

        void syncSliderPosition(double newLocation)
        {
            if (videoPlayer.NaturalDuration.HasTimeSpan)
            {
                videoPlayer.Position = TimeSpan.FromSeconds(newLocation);
                videoTime.Text = FormatSeconds(videoPlayer.NaturalDuration.TimeSpan.TotalSeconds - newLocation);
            }
        }

        private void VideoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Skip to the new position when the user interacts with the slider
            if (!isDragging)
            {
                syncSliderPosition(videoSlider.Value);
            }
        }

        string? GetCurrentVideo()
        {
            if (iCurrent < 0)
            {
                iCurrent = 0;
            }

            if (iCurrent >= videoList.Items.Count)
            {
                iCurrent = videoList.Items.Count - 1;
            }

            if (iCurrent >= videoList.Items.Count)
            {
                return null;
            }

            return videoList.Items[iCurrent] as String;
        }

        void LoadCurrentVideo()
        {
            if (fLoaded)
                UnloadCurrentVideo();

            SyncPlayingIndex();
            videoPlayer.Source = new Uri(GetCurrentVideo());
            fLoaded = true;
        }

        void EnsureVideoLoaded()
        {
            if (fLoaded)
                return;

            LoadCurrentVideo();
        }

        void UnloadCurrentVideo()
        {
            videoPlayer.Stop();
            videoPlayer.Source = null;
            fLoaded = false;
        }

        private void doPlayClick(object sender, RoutedEventArgs e)
        {
            EnsureVideoLoaded();
            videoPlayer.Play();
        }

        private void doPauseClick(object sender, RoutedEventArgs e)
        {
            videoPlayer.Pause();
        }

        private void doStopClick(object sender, RoutedEventArgs e)
        {
            videoPlayer.Stop();
        }

        private void doSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void DoFileListDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    videoList.Items.Add(file);
                }
            }
        }

        private void DoFileListDrop_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DoFileListDrop_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        void SyncPlayingIndex()
        {
            videoList.SelectedIndex = iCurrent;
        }

        int GetPreviousItem()
        {
            iCurrent--;

            if (iCurrent <= 0)
                iCurrent = 0;

            SyncPlayingIndex();
            return iCurrent;
        }

        int GetNextItem()
        {
            iCurrent++;
            if (iCurrent >= videoList.Items.Count - 1)
                iCurrent = videoList.Items.Count - 1;

            SyncPlayingIndex();
            return iCurrent;
        }

        private void doPreviousClick(object sender, RoutedEventArgs e)
        {
            GetPreviousItem();
            LoadCurrentVideo();
            videoPlayer.Play();
        }

        private void doReverseClick(object sender, RoutedEventArgs e)
        {
            TimeSpan position = videoPlayer.Position;

            position -= TimeSpan.FromSeconds(10);
            if (position.TotalSeconds < 0)
                position = TimeSpan.Zero;

            videoPlayer.Position = position;
        }

        private void doForwardClick(object sender, RoutedEventArgs e)
        {
            TimeSpan position = videoPlayer.Position;

            position += TimeSpan.FromSeconds(30);

            videoPlayer.Position = position;
        }

        private void doNextClick(object sender, RoutedEventArgs e)
        {
            GetNextItem();
            LoadCurrentVideo();
            videoPlayer.Play();
        }

        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true; // Start dragging
        }

        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false; // End dragging

            // Update the video position when drag ends
            syncSliderPosition(videoSlider.Value);
        }

        private void Slider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get the position of the click relative to the slider
            double clickPosition = e.GetPosition(videoSlider).X;

            // Calculate the value of the slider based on the click position
            double newValue = clickPosition / videoSlider.ActualWidth * videoSlider.Maximum;

            // Set the new value to the slider and update the video position
            videoSlider.Value = newValue;

            syncSliderPosition(newValue);
        }
    }
}

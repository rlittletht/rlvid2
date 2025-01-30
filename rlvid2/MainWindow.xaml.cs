using System.Globalization;
using System.IO;
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
using Path = System.IO.Path;

namespace rlvid2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool fLoaded = false;
        private int iCurrent = 0;
        private DispatcherTimer? _timer = null;
        private bool isDragging = false;

        public MainWindow()
        {
            InitializeComponent();
            SetupTimer();
            Closed += MainWindow_Closed;
            ((App)Application.Current).WindowPlace.Register(this);
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (mover != null)
            {
                mover.Close();
            }
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

        private void Timer_Tick(object? sender, EventArgs e)
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
            videoTime.Text = FormatSeconds(videoPlayer.NaturalDuration.TimeSpan.TotalSeconds);
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

        int GetCurrentIndex()
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
                return -1;
            }

            return iCurrent;
        }

        string? GetCurrentVideo()
        {
            int current = GetCurrentIndex();

            if (iCurrent == -1)
            {
                return null;
            }

            return videoList.Items[current] as String;
        }

        void LoadCurrentVideo()
        {
            if (fLoaded)
                UnloadCurrentVideo();

            SyncPlayingIndex();
            string? current = GetCurrentVideo();
            if (current != null)
            {
                videoPlayer.Source = new Uri(current);
                fLoaded = true;
            }
        }

        void EnsureVideoLoaded()
        {
            if (fLoaded)
                return;

            LoadCurrentVideo();
        }

        void Stop()
        {
            videoPlayer.Stop();
            videoPlayer.Source = null;
            fLoaded = false;
        }

        void UnloadCurrentVideo()
        {
            Stop();
        }

        void Play()
        {
            EnsureVideoLoaded();
            videoPlayer.Play();
        }

        private void doPlayClick(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void doPauseClick(object sender, RoutedEventArgs e)
        {
            videoPlayer.Pause();
        }

        private void doStopClick(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void doSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private Mover? mover = null;

        void processDroppedFiles(string[] files)
        {
            foreach (string file in files)
            {
                if (file.EndsWith("moves.txt", true, CultureInfo.CurrentCulture))
                {
                    if (mover == null)
                        mover = Mover.ShowMover(MoveItem);

                    mover.LoadMover(file);
                }
                else
                {
                    videoList.Items.Add(file);
                }
            }
        }

        private void DoFileListDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                processDroppedFiles(files);
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
            videoList.SelectedIndex = GetCurrentIndex();
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

        void Previous()
        {
            GetPreviousItem();
            LoadCurrentVideo();
            videoPlayer.Play();
        }

        private void doPreviousClick(object sender, RoutedEventArgs e)
        {
            Previous();
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

        void Next()
        {
            GetNextItem();
            LoadCurrentVideo();
            videoPlayer.Play();
        }

        private void doNextClick(object sender, RoutedEventArgs e)
        {
            Next();
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


        public void MoveItem(MoverItem item)
        {
            string? sourceFile = GetCurrentVideo();

            if (sourceFile == null)
                return;

            string destPath = MoverGuts.MakeDestinationPath(sourceFile, item.Destination);

            // move to the next video to release the current one
            Stop();
            int itemToRemove = GetCurrentIndex();
            int nextItem = GetNextItem();

            // remove the current item from the list
            videoList.Items.RemoveAt(itemToRemove);
            nextItem--;

            // make sure we have the directories created
            string? destDir = Path.GetDirectoryName(destPath);
            if (destDir != null && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            try
            {
                FileInfo fi = new FileInfo(sourceFile);
                fi.MoveTo(destPath);
                // File.Move(sourceFile, destPath);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Could not move: {e.Message}");
            }

            iCurrent = nextItem;
            Play();
        }

        private void LoadTestData(object sender, RoutedEventArgs e)
        {
            processDroppedFiles(
            [
                "c:\\temp\\ACR.mp4", "c:\\temp\\ACR - Copy.mp4", "c:\\temp\\ACR - Copy - Copy - Copy.mp4", "c:\\temp\\ACR - Copy - Copy - Copy - Copy.mp4",
                "c:\\temp\\ACR - Copy - Copy.mp4", "c:\\temp\\moves.txt"
            ]);
        }
    }
}

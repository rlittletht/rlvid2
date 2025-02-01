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
        private DispatcherTimer? _timer = null;
        private bool isDragging = false;
        private bool paused = false;
        private Mover? mover = null;
        private Playlist? playlist = null;

        /*----------------------------------------------------------------------------
            %%Function: MainWindow
            %%Qualified: rlvid2.MainWindow.MainWindow
        ----------------------------------------------------------------------------*/
        public MainWindow()
        {
            InitializeComponent();
            SetupTimer();
            Closed += MainWindow_Closed;
            ((App)Application.Current).WindowPlace.Register(this);
            this.KeyDown += DoKeyDown;
        }

        /*----------------------------------------------------------------------------
            %%Function: DoKeyDown
            %%Qualified: rlvid2.MainWindow.DoKeyDown
        ----------------------------------------------------------------------------*/
        private void DoKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Right)
                Forward();
            else if (e.Key == Key.Left)
                Reverse();
            else if (e.Key == Key.Space)
            {
                if (paused)
                {
                    Play();
                }
                else
                {
                    Pause();
                }
            }
            else if (e.Key == Key.N)
            {
                Next();
            }
            else if (e.Key == Key.P)
            {
                Previous();
            }
        }

        /*----------------------------------------------------------------------------
            %%Function: MainWindow_Closed
            %%Qualified: rlvid2.MainWindow.MainWindow_Closed
        ----------------------------------------------------------------------------*/
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (mover != null)
            {
                mover.Close();
            }

            if (playlist != null)
            {
                playlist.Close();
            }
        }

        /*----------------------------------------------------------------------------
            %%Function: FormatSeconds
            %%Qualified: rlvid2.MainWindow.FormatSeconds
        ----------------------------------------------------------------------------*/
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

        /*----------------------------------------------------------------------------
            %%Function: SetupTimer
            %%Qualified: rlvid2.MainWindow.SetupTimer
        ----------------------------------------------------------------------------*/
        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500); // Update the slider every 500ms
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        /*----------------------------------------------------------------------------
            %%Function: Timer_Tick
            %%Qualified: rlvid2.MainWindow.Timer_Tick
        ----------------------------------------------------------------------------*/
        private void Timer_Tick(object? sender, EventArgs e)
        {
            // Only update the slider if the video is playing
            if (videoPlayer.NaturalDuration.HasTimeSpan && !isDragging)
            {
                double currentPosition = videoPlayer.Position.TotalSeconds;
                videoSlider.Value = currentPosition;

                videoTime.Text = FormatSeconds(videoPlayer.NaturalDuration.TimeSpan.TotalSeconds - videoSlider.Value);
            }
        }

        /*----------------------------------------------------------------------------
            %%Function: MediaPlayer_MediaOpened
            %%Qualified: rlvid2.MainWindow.MediaPlayer_MediaOpened
        ----------------------------------------------------------------------------*/
        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Set the maximum value for the slider based on the video's duration
            videoSlider.Maximum = videoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            videoTime.Text = FormatSeconds(videoPlayer.NaturalDuration.TimeSpan.TotalSeconds);
        }

        /*----------------------------------------------------------------------------
            %%Function: MediaElement_MediaEnded
            %%Qualified: rlvid2.MainWindow.MediaElement_MediaEnded
        ----------------------------------------------------------------------------*/
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Optionally handle video end, e.g., reset slider to 0
            videoSlider.Value = 0;
            videoTime.Text = FormatSeconds(videoPlayer.NaturalDuration.TimeSpan.TotalSeconds - videoSlider.Value);
        }

        /*----------------------------------------------------------------------------
            %%Function: syncSliderPosition
            %%Qualified: rlvid2.MainWindow.syncSliderPosition
        ----------------------------------------------------------------------------*/
        void syncSliderPosition(double newLocation)
        {
            if (videoPlayer.NaturalDuration.HasTimeSpan)
            {
                videoPlayer.Position = TimeSpan.FromSeconds(newLocation);
                videoTime.Text = FormatSeconds(videoPlayer.NaturalDuration.TimeSpan.TotalSeconds - newLocation);
            }
        }

        /*----------------------------------------------------------------------------
            %%Function: VideoSlider_ValueChanged
            %%Qualified: rlvid2.MainWindow.VideoSlider_ValueChanged
        ----------------------------------------------------------------------------*/
        private void VideoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Skip to the new position when the user interacts with the slider
            if (!isDragging)
            {
                syncSliderPosition(videoSlider.Value);
            }
        }

        /*----------------------------------------------------------------------------
            %%Function: GetCurrentIndex
            %%Qualified: rlvid2.MainWindow.GetCurrentIndex
        ----------------------------------------------------------------------------*/
        int GetCurrentIndex()
        {
            return playlist?.CurrentIndex ?? 0;
        }

        /*----------------------------------------------------------------------------
            %%Function: GetCurrentVideoPath
            %%Qualified: rlvid2.MainWindow.GetCurrentVideoPath
        ----------------------------------------------------------------------------*/
        string? GetCurrentVideoPath()
        {
            return playlist?.CurrentItem?.Path;
        }

        /*----------------------------------------------------------------------------
            %%Function: LoadCurrentVideo
            %%Qualified: rlvid2.MainWindow.LoadCurrentVideo
        ----------------------------------------------------------------------------*/
        void LoadCurrentVideo()
        {
            if (fLoaded)
                UnloadCurrentVideo();

            SyncPlayingIndex();
            string? current = GetCurrentVideoPath();
            if (current != null)
            {
                videoPlayer.Source = new Uri(current);
                fLoaded = true;
            }
        }

        /*----------------------------------------------------------------------------
            %%Function: EnsureVideoLoaded
            %%Qualified: rlvid2.MainWindow.EnsureVideoLoaded
        ----------------------------------------------------------------------------*/
        void EnsureVideoLoaded()
        {
            if (fLoaded)
                return;

            LoadCurrentVideo();
        }

        /*----------------------------------------------------------------------------
            %%Function: Stop
            %%Qualified: rlvid2.MainWindow.Stop
        ----------------------------------------------------------------------------*/
        void Stop()
        {
            videoPlayer.Stop();
            videoPlayer.Source = null;
            fLoaded = false;
        }

        /*----------------------------------------------------------------------------
            %%Function: UnloadCurrentVideo
            %%Qualified: rlvid2.MainWindow.UnloadCurrentVideo
        ----------------------------------------------------------------------------*/
        void UnloadCurrentVideo()
        {
            Stop();
        }

        /*----------------------------------------------------------------------------
            %%Function: Play
            %%Qualified: rlvid2.MainWindow.Play
        ----------------------------------------------------------------------------*/
        void Play()
        {
            EnsureVideoLoaded();
            videoPlayer.Play();
            paused = false;
        }

        /*----------------------------------------------------------------------------
            %%Function: doPlayClick
            %%Qualified: rlvid2.MainWindow.doPlayClick
        ----------------------------------------------------------------------------*/
        private void doPlayClick(object sender, RoutedEventArgs e)
        {
            Play();
        }

        /*----------------------------------------------------------------------------
            %%Function: Pause
            %%Qualified: rlvid2.MainWindow.Pause
        ----------------------------------------------------------------------------*/
        void Pause()
        {
            videoPlayer.Pause();
            paused = true;
        }

        /*----------------------------------------------------------------------------
            %%Function: doPauseClick
            %%Qualified: rlvid2.MainWindow.doPauseClick
        ----------------------------------------------------------------------------*/
        private void doPauseClick(object sender, RoutedEventArgs e)
        {
            Pause();
        }

        /*----------------------------------------------------------------------------
            %%Function: doStopClick
            %%Qualified: rlvid2.MainWindow.doStopClick
        ----------------------------------------------------------------------------*/
        private void doStopClick(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        /*----------------------------------------------------------------------------
            %%Function: doSelectionChanged
            %%Qualified: rlvid2.MainWindow.doSelectionChanged
        ----------------------------------------------------------------------------*/
        private void doSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        /*----------------------------------------------------------------------------
            %%Function: processDroppedFiles
            %%Qualified: rlvid2.MainWindow.processDroppedFiles
        ----------------------------------------------------------------------------*/
        void processDroppedFiles(string[] files)
        {
            List<string> newFiles = new List<string>();

            foreach (string file in files)
            {
                if (file.EndsWith("moves.txt", true, CultureInfo.CurrentCulture))
                {
                    if (mover == null)
                        mover = Mover.ShowMover(MoveItem, this);

                    mover.LoadMover(file);
                }
                else if (file.EndsWith("cleaner.txt", true, CultureInfo.CurrentCulture))
                {
                    if (mover == null)
                        mover = Mover.ShowMover(MoveItem, this);

                    mover.LoadCleaner(file);
                }
                else
                {
                    newFiles.Add(file);
                }
            }

            if (playlist == null)
            {
                playlist = Playlist.Show(newFiles, this);
            }
            else
            {
                playlist.AddFiles(newFiles);
            }
        }

        /*----------------------------------------------------------------------------
            %%Function: DoFileListDrop
            %%Qualified: rlvid2.MainWindow.DoFileListDrop
        ----------------------------------------------------------------------------*/
        private void DoFileListDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                processDroppedFiles(files);
            }
        }

        /*----------------------------------------------------------------------------
            %%Function: DoFileListDrop_DragOver
            %%Qualified: rlvid2.MainWindow.DoFileListDrop_DragOver
        ----------------------------------------------------------------------------*/
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

        /*----------------------------------------------------------------------------
            %%Function: DoFileListDrop_DragEnter
            %%Qualified: rlvid2.MainWindow.DoFileListDrop_DragEnter
        ----------------------------------------------------------------------------*/
        private void DoFileListDrop_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        /*----------------------------------------------------------------------------
            %%Function: SyncPlayingIndex
            %%Qualified: rlvid2.MainWindow.SyncPlayingIndex
        ----------------------------------------------------------------------------*/
        void SyncPlayingIndex()
        {
//            videoList.SelectedIndex = GetCurrentIndex();
            if (mover != null)
                mover.UpdateSource(GetCurrentVideoPath() ?? "");
        }

        /*----------------------------------------------------------------------------
            %%Function: GetPreviousItem
            %%Qualified: rlvid2.MainWindow.GetPreviousItem
        ----------------------------------------------------------------------------*/
        int GetPreviousItem()
        {
            playlist?.Previous();
            SyncPlayingIndex();
            return playlist?.CurrentIndex ?? 0;
        }

        /*----------------------------------------------------------------------------
            %%Function: GetNextItem
            %%Qualified: rlvid2.MainWindow.GetNextItem
        ----------------------------------------------------------------------------*/
        int GetNextItem()
        {
            playlist?.Next();
            SyncPlayingIndex();
            return playlist?.CurrentIndex ?? 0;
        }

        /*----------------------------------------------------------------------------
            %%Function: Previous
            %%Qualified: rlvid2.MainWindow.Previous
        ----------------------------------------------------------------------------*/
        void Previous()
        {
            GetPreviousItem();
            LoadCurrentVideo();
            videoPlayer.Play();
        }

        /*----------------------------------------------------------------------------
            %%Function: doPreviousClick
            %%Qualified: rlvid2.MainWindow.doPreviousClick
        ----------------------------------------------------------------------------*/
        private void doPreviousClick(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        /*----------------------------------------------------------------------------
            %%Function: Reverse
            %%Qualified: rlvid2.MainWindow.Reverse
        ----------------------------------------------------------------------------*/
        void Reverse()
        {
            TimeSpan position = videoPlayer.Position;

            position -= TimeSpan.FromSeconds(10);
            if (position.TotalSeconds < 0)
                position = TimeSpan.Zero;

            videoPlayer.Position = position;
        }

        /*----------------------------------------------------------------------------
            %%Function: doReverseClick
            %%Qualified: rlvid2.MainWindow.doReverseClick
        ----------------------------------------------------------------------------*/
        private void doReverseClick(object sender, RoutedEventArgs e)
        {
            Reverse();
        }

        /*----------------------------------------------------------------------------
            %%Function: Forward
            %%Qualified: rlvid2.MainWindow.Forward
        ----------------------------------------------------------------------------*/
        void Forward()
        {
            TimeSpan position = videoPlayer.Position;

            position += TimeSpan.FromSeconds(30);

            videoPlayer.Position = position;
        }

        /*----------------------------------------------------------------------------
            %%Function: doForwardClick
            %%Qualified: rlvid2.MainWindow.doForwardClick
        ----------------------------------------------------------------------------*/
        private void doForwardClick(object sender, RoutedEventArgs e)
        {
            Forward();
        }

        /*----------------------------------------------------------------------------
            %%Function: Next
            %%Qualified: rlvid2.MainWindow.Next
        ----------------------------------------------------------------------------*/
        void Next()
        {
            GetNextItem();
            LoadCurrentVideo();
            videoPlayer.Play();
        }

        /*----------------------------------------------------------------------------
            %%Function: doNextClick
            %%Qualified: rlvid2.MainWindow.doNextClick
        ----------------------------------------------------------------------------*/
        private void doNextClick(object sender, RoutedEventArgs e)
        {
            Next();
        }

        /*----------------------------------------------------------------------------
            %%Function: Slider_PreviewMouseDown
            %%Qualified: rlvid2.MainWindow.Slider_PreviewMouseDown
        ----------------------------------------------------------------------------*/
        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true; // Start dragging
        }

        /*----------------------------------------------------------------------------
            %%Function: Slider_PreviewMouseUp
            %%Qualified: rlvid2.MainWindow.Slider_PreviewMouseUp
        ----------------------------------------------------------------------------*/
        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false; // End dragging

            // Update the video position when drag ends
            syncSliderPosition(videoSlider.Value);
        }

        /*----------------------------------------------------------------------------
            %%Function: Slider_PreviewMouseLeftButtonDown
            %%Qualified: rlvid2.MainWindow.Slider_PreviewMouseLeftButtonDown
        ----------------------------------------------------------------------------*/
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

        /*----------------------------------------------------------------------------
            %%Function: MoveItem
            %%Qualified: rlvid2.MainWindow.MoveItem
        ----------------------------------------------------------------------------*/
        public void MoveItem(MoverItem item, string newName)
        {
            if (playlist == null)
                return;

            string? sourceFile = GetCurrentVideoPath();

            if (sourceFile == null)
                return;

            string destPath = MoverGuts.MakeDestinationPath(sourceFile, item.Destination, newName);

            // move to the next video to release the current one
            Stop();
            int itemToRemove = GetCurrentIndex();
            int nextItem = GetNextItem();

            // remove the current item from the list
            playlist.RemoveItemAt(itemToRemove);

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

            Play();
        }

        /*----------------------------------------------------------------------------
            %%Function: LoadTestData
            %%Qualified: rlvid2.MainWindow.LoadTestData
        ----------------------------------------------------------------------------*/
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

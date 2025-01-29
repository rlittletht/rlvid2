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

namespace rlvid2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool fLoaded = false;
        private int iCurrent = 0;

        public MainWindow()
        {
            InitializeComponent();
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
    }
}

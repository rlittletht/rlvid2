using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace rlvid2
{
    /// <summary>
    /// Interaction logic for Playlist.xaml
    /// </summary>
    public partial class Playlist : Window
    {
        PlaylistModel _model = new PlaylistModel();

        public Playlist()
        {
            DataContext= _model;
            InitializeComponent();
            ((App)Application.Current).WindowPlace.Register(this);
        }

        public ObservableCollection<PlaylistItem> PlaylistItems => _model.PlaylistItems;

        public PlaylistItem? CurrentItem
        {
            get => _model.CurrentItem;
            set => _model.CurrentItem = value;
        }

        public int CurrentIndex
        {
            get => _model.CurrentIndex;
            set => _model.CurrentIndex = value;
        }

        public static Playlist Show(IEnumerable<string> sources, Window? parent)
        {
            Playlist playlist = new Playlist();

            foreach (string source in sources)
            {
                PlaylistItem item = new PlaylistItem(source);

                playlist._model.PlaylistItems.Add(item);
            }

            playlist._model.CurrentItem = playlist._model.PlaylistItems[0];
            playlist.Owner = parent;
            playlist.Show();

            return playlist;
        }

        public void AddFiles(IEnumerable<string> sources)
        {
            foreach (string source in sources)
            {
                PlaylistItem item = new PlaylistItem(source);
                _model.PlaylistItems.Add(item);
            }
        }

        public int Next() => _model.Next();

        public int Previous() => _model.Previous();

        public void RemoveItemAt(int index)
        {
            _model.PlaylistItems.RemoveAt(index);
        }
    }
}

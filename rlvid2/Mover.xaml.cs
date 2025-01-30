using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace rlvid2
{
    public delegate void MoveItemDelegate(MoverItem item, string newName);

    /// <summary>
    /// Interaction logic for Mover.xaml
    /// </summary>
    public partial class Mover : Window
    {
        private MoverModel _moverModel { get; set; }= new MoverModel();
        private MoveItemDelegate? _moveItemDelegate = null;

        public Mover()
        {
            DataContext = _moverModel;
            InitializeComponent();
            ((App)Application.Current).WindowPlace.Register(this);
        }

        public void LoadMover(string sFile)
        {
            _moverModel.MoverItems.Clear();
            using StreamReader reader = new StreamReader(sFile);

            while (!reader.EndOfStream)
            {
                string sLine = reader.ReadLine();
                string[] sParts = sLine.Split('\t');
                _moverModel.MoverItems.Add(new MoverItem() { Label = sParts[0], Destination = sParts[1] });
            }
        }

        public static Mover ShowMover(MoveItemDelegate del)
        {
            Mover mover = new Mover();
            mover.Show();
            mover._moveItemDelegate = del;
            return mover;
        }

        public void UpdateSource(string source)
        {
            _moverModel.SourceName = Path.GetFileName(source);
            _moverModel.NewName = _moverModel.SourceName;
        }

        private void MoverListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void RemoveMover_Click(object sender, RoutedEventArgs e)
        {
            MoverItem? item = (sender as Button)?.DataContext as MoverItem;

            if (item != null)
            {
                string newName;

                if (_moverModel.NewName == "")
                    newName = _moverModel.SourceName;
                else
                    newName = _moverModel.NewName;

                _moveItemDelegate?.Invoke(item, newName);
            }
        }

        private void CleanSourceName(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in _moverModel.SourceName)
            {
                Regex rex = new Regex(@"[\(\)\[\]{};.<>!@#$%^-_=~]");

                if (char.IsAsciiLetterOrDigit(c) || rex.IsMatch(c.ToString()))
                    sb.Append(c);
            }

            _moverModel.NewName = sb.ToString();
        }
    }
}

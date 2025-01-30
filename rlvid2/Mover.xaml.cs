using System;
using System.Collections.Generic;
using System.IO;
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
    public delegate void MoveItemDelegate(MoverItem item);

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
        }

        public void LoadMover(string sFile)
        {
            _moverModel.MoverItems.Clear();
            StreamReader reader = new StreamReader(sFile);

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

        private void MoverListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
//            if (_moverModel.CurrentMover != null)
//                _moveItemDelegate?.Invoke(_moverModel.CurrentMover);

            e.Handled = true;
        }

        private void RemoveMover_Click(object sender, RoutedEventArgs e)
        {
            MoverItem? item = (sender as Button)?.DataContext as MoverItem;

            if (item != null)
                _moveItemDelegate?.Invoke(item);
        }
    }
}

namespace rlvid2;
using System.Collections.ObjectModel;

public class MoverModel
{
    public ObservableCollection<MoverItem> MoverItems { get; set; } = new ObservableCollection<MoverItem>();
    public MoverItem? CurrentMover { get; set; }

}

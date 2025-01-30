using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace rlvid2;
using System.Collections.ObjectModel;

public class MoverModel: INotifyPropertyChanged
{
    private string m_sourceName = string.Empty;
    private string m_newName = string.Empty;
    private MoverItem? m_currentMover;
    public ObservableCollection<MoverItem> MoverItems { get; set; } = new ObservableCollection<MoverItem>();

    public MoverItem? CurrentMover
    {
        get => m_currentMover;
        set => SetField(ref m_currentMover, value);
    }

    public string SourceName
    {
        get => m_sourceName;
        set => SetField(ref m_sourceName, value);
    }

    public string NewName
    {
        get => m_newName;
        set => SetField(ref m_newName, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

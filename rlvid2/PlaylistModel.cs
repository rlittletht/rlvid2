using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace rlvid2;

public class PlaylistModel: INotifyPropertyChanged
{
    private ObservableCollection<PlaylistItem> m_playlistItems = new ObservableCollection<PlaylistItem>();
    private PlaylistItem? m_currentItem;
    private int m_currentIndex = 0;

    public ObservableCollection<PlaylistItem> PlaylistItems
    {
        get => m_playlistItems;
        set => SetField(ref m_playlistItems, value);
    }

    public PlaylistItem? CurrentItem
    {
        get => m_currentItem;
        set => SetField(ref m_currentItem, value);
    }

    public int CurrentIndex
    {
        get => m_currentIndex;
        set => SetField(ref m_currentIndex, value);
    }

    public int Next()
    {
        if (CurrentIndex >= m_playlistItems.Count)
            return CurrentIndex;

        return ++CurrentIndex;
    }

    public int Previous()
    {
        if (CurrentIndex <= 0)
            return CurrentIndex;

        return --CurrentIndex;
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

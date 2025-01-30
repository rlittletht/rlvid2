namespace rlvid2;

public class PlaylistItem
{
    public string Display { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;

    public PlaylistItem(string path)
    {
        Path = path;
        Display = path;
    }
}

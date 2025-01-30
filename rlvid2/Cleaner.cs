using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace rlvid2;

public class Cleaner
{
    private List<Regex> patterns = new List<Regex>();

    public void LoadFile(string file)
    {
        patterns.Clear();

        using (StreamReader sr = new StreamReader(file))
        {
            string? line;

            while ((line = sr.ReadLine()) != null)
            {
                patterns.Add(new Regex(line));
            }
        }
    }

    public void LoadLines(string[] lines)
    {
        patterns.Clear();
        foreach (string line in lines)
        {
            patterns.Add(new Regex(line));
        }
    }

    public string Clean(string s)
    {
        foreach (Regex r in patterns)
        {
            s = r.Replace(s, "");
        }

        StringBuilder sb = new StringBuilder();

        foreach (char c in s)
        {
            Regex rex = new Regex(@"[\(\)\[\]{};.<>!@#$%^\-_=~ ]");

            if (char.IsAsciiLetterOrDigit(c) || rex.IsMatch(c.ToString()))
                sb.Append(c);
        }

        return sb.ToString();
    }
}

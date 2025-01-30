using System.IO;

namespace rlvid2;

public class MoverGuts
{
    public MoverGuts()
    {

    }

    public static string MakeDestinationPath(string source, string destPath)
    {
        string sourceFilename = Path.GetFileName(source);
        string sourcePath = Path.GetDirectoryName(source) ?? "";
        string dest;

        if (Path.IsPathRooted(destPath))
        {
            if (destPath.StartsWith("\\") && !destPath.StartsWith("\\\\"))
            {
                // we need to carry over the root path from the source
                if (!Path.IsPathRooted(source))
                {
                    throw new ArgumentException("Source path is not rooted, cannot carry over root path");
                }

                string sourceRoot = Path.GetPathRoot(source);
                dest = Path.Combine(sourceRoot, destPath.Substring(1), sourceFilename);
            }
            else
            {
                // need to remove the whole path from source
                dest = Path.Combine(destPath, sourceFilename);
            }
        }
        else
        {
            dest = Path.Combine(sourcePath, destPath, sourceFilename);
        }

        return dest;
    }

}

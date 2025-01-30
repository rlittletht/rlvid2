using NUnit.Framework;
using rlvid2;

namespace Tests;

public class CleanerTests
{
    [TestCase(new string[] { "foo", "bar" }, "foo bar", " ")]
    [TestCase(new string[] { @"foo(?=\.mp4)", "bar" }, "foo bar", "foo ")]
    [TestCase(new string[] { @"foo(?=\.mp4)", "bar(?=\\.mp4)" }, "bar foo.mp4", "bar .mp4")]
    [TestCase(new string[] { @"foo(?=\.mp4)", "bar(?=\\.mp4)" }, "bar-foo.mp4", "bar-.mp4")]
    [Test]
    public static void TestCleaner(string[] lines, string input, string expected)
    {
        Cleaner cleaner = new Cleaner();
        cleaner.LoadLines(lines);

        string actual = cleaner.Clean(input);
        Assert.That(actual, Is.EqualTo(expected));
    }
}

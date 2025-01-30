using NUnit.Framework;
using rlvid2;


namespace Tests
{
    public class MoverGutsTests
    {
        [TestCase("C:\\temp\\test.mp4", "C:\\temp\\dest", "C:\\temp\\dest\\test.mp4")]
        [TestCase("C:\\temp\\test.mp4", "dest", "C:\\temp\\dest\\test.mp4")]
        [TestCase("C:\\temp\\test.mp4", "\\temp\\dest", "C:\\temp\\dest\\test.mp4")]
        [TestCase("C:\\temp\\test.mp4", "\\dest", "C:\\dest\\test.mp4")]
        [TestCase("\\\\foo\\bar\\temp\\test.mp4", "dest", "\\\\foo\\bar\\temp\\dest\\test.mp4")]
        [TestCase("\\\\foo\\bar\\temp\\test.mp4", "\\dest", "\\\\foo\\bar\\dest\\test.mp4")]
        [TestCase("\\\\foo\\bar\\temp\\test.mp4", "c:\\temp\\dest", "c:\\temp\\dest\\test.mp4")]
        [Test]
        public static void TestMakeDestinationPath(string source, string destPath, string expected)
        {
            string actual = MoverGuts.MakeDestinationPath(source, destPath);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}

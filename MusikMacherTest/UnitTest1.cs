
using System.Drawing.Printing;
using MusikMacher;

namespace MusikMacherTest;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        // TODO: push test audio and generated points to repo.
        var path = "some/path/with/generated/waveform/as/ground/truth";
        var cached = WaveformCache.FromCache(path);
        var points = Track.LoadWaveformGeometry(path, false);

        for (int i = 0; i < points.Length; i++)
        {
            Assert.AreEqual(cached[0][i].Y, points[0][i].Y);
        }
    }
    
    [TestMethod]
    public void TestMethod2()
    {
        var path = "some/path/with/generated/waveform/as/ground/truth";
        var cached = WaveformCache.FromCache(path);
        var points = Track.LoadWaveformGeometry(path, false);

        for (int i = 0; i < points.Length; i++)
        {
            Assert.AreEqual(cached[0][i].Y, points[0][i].Y);
        }
    }
}
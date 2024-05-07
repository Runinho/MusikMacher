using NAudio.WaveFormRenderer;

namespace MusikMacher;

public class MyAveragePeakProvider : PeakProvider
{
  private readonly float scale;

  public MyAveragePeakProvider(float scale)
  {
    this.scale = scale;
  }

  public override PeakInfo GetNextPeak()
  {
    throw new NotImplementedException();
  }

  public float MyGetNextPeak()
  {
    int count = this.Provider.Read(this.ReadBuffer, 0, this.ReadBuffer.Length);
    float sum = 0;
    for (int i = 0; i < count; i++)
    {
      sum += Math.Abs(this.ReadBuffer[i]);
    }

    sum = sum / count;
    return sum * scale;
  }
}
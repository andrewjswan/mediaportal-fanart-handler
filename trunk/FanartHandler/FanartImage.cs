// Type: FanartHandler.FanartImage
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

namespace FanartHandler
{
  internal class FanartImage
  {
    public string Id { get; set; }
    public string Artist { get; set; }
    public string DiskImage { get; set; }
    public string SourceImage { get; set; }
    public string Type { get; set; }
    public string Source { get; set; }

    public FanartImage(string id, string artist, string diskImage, string sourceImage, string type, string source)
    {
      Id = id;
      Artist = artist;
      DiskImage = diskImage;
      SourceImage = sourceImage;
      Type = type;
      Source = source;
    }
  }
}

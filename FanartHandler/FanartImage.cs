// Type: FanartHandler.FanartImage
// Assembly: FanartHandler, Version=4.0.2.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

namespace FanartHandler
{

  internal class FanartClass
  {
    public string Id { get; set; }

    /// <summary>
    /// Initializes a new instance of the FanartClass class.
    /// </summary>
    public FanartClass()
    {
      Id = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartClass class.
    /// </summary>
    /// <param name="id">Identifier number</param>
    public FanartClass(string id)
    {
      Id = id;
    }

    public bool IsEmpty
    {
      get { return string.IsNullOrEmpty(Id); }
    }
  }

  internal class FanartImage : FanartClass
  {
    public string Key { get; set; }
    public string DiskImage { get; set; }
    public string SourceImage { get; set; }
    public string Type { get; set; }
    public string Source { get; set; }

    /// <summary>
    /// Initializes a new instance of the FanartImage class.
    /// </summary>
    public FanartImage() : base ()
    {
      Key = string.Empty;
      DiskImage = string.Empty;
      SourceImage = string.Empty;
      Type = string.Empty;
      Source = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartImage class.
    /// </summary>
    /// <param name="id">Identifier number</param>
    /// <param name="key">DB Key name</param>
    /// <param name="diskImage">Filename on disk</param>
    /// <param name="sourceImage">Filename at source</param>
    /// <param name="type">Type of the file</param>
    /// <param name="source">Source name (like http://...)</param>
    public FanartImage(string id, string key, string diskImage, string sourceImage, string type, string source) : base (id)
    {
      Key = key;
      DiskImage = diskImage;
      SourceImage = sourceImage;
      Type = type;
      Source = source;
    }
  }

  internal class FanartArtist : FanartClass
  {
    public string Artist { get; set; }

    public new bool IsEmpty
    {
      get { return base.IsEmpty && string.IsNullOrEmpty(Artist); }
    }

    /// <summary>
    /// Initializes a new instance of the FanartArtist class.
    /// </summary>
    public FanartArtist() : base ()
    {
      Artist = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartArtist class.
    /// </summary>
    /// <param name="id">MusicBrainz ID - Identifier number</param>
    /// <param name="artist">Artist name</param>
    public FanartArtist(string id, string artist) : base (id)
    {
      Artist = artist;
    }
  } 

  internal class FanartAlbum : FanartClass
  {
    public string Artist { get; set; }
    public string Album { get; set; }
    public int CDs { get; set; }

    public new bool IsEmpty
    {
      get { return base.IsEmpty && string.IsNullOrEmpty(Artist) && string.IsNullOrEmpty(Album); }
    }

    /// <summary>
    /// Initializes a new instance of the FanartAlbum class.
    /// </summary>
    public FanartAlbum() : base ()
    {
      Artist = string.Empty;
      Album = string.Empty;
      CDs = 0;
    }

    /// <summary>
    /// Initializes a new instance of the FanartAlbum class.
    /// </summary>
    /// <param name="id">MusicBrainz ID - Identifier number</param>
    /// <param name="artist">Artist name</param>
    /// <param name="album">Album name</param>
    public FanartAlbum(string id, string artist, string album) : base (id)
    {
      Artist = artist;
      Album = album;
    }

    /// <summary>
    /// Initializes a new instance of the FanartAlbum class.
    /// </summary>
    /// <param name="id">MusicBrainz ID - Identifier number</param>
    /// <param name="artist">Artist name</param>
    /// <param name="album">Album name</param>
    /// <param name="num">Number of Album CD</param>
    public FanartAlbum(string id, string artist, string album, int num) : this (id, artist, album)
    {
      CDs = num;
    }
  } 

  internal class FanartMovie : FanartClass
  {
    public string Title { get; set; }

    /// <summary>
    /// Initializes a new instance of the FanartMovie class.
    /// </summary>
    public FanartMovie() : base ()
    {
      Title = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartMovie class.
    /// </summary>
    /// <param name="id">IMDB ID - Identifier number</param>
    /// <param name="title">Movie Title</param>
    public FanartMovie(string id, string title) : base (id)
    {
      Title = title;
    }
  } 

  internal class FanartTVSeries : FanartClass
  {
    public string Name { get; set; }
    public string LocalName { get; set; }
    public string Seasons { get; set; }

    /// <summary>
    /// Initializes a new instance of the FanartTVSeries class.
    /// </summary>
    public FanartTVSeries() : base ()
    {
      Name = string.Empty;
      LocalName = string.Empty;
      Seasons = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartTVSeries class.
    /// </summary>
    /// <param name="id">TVDB ID - Identifier number</param>
    /// <param name="title">TVSeries Title</param>
    public FanartTVSeries(string id, string title) : base (id)
    {
      Name = title;
    }

    /// <summary>
    /// Initializes a new instance of the FanartTVSeries class.
    /// </summary>
    /// <param name="id">TVDB ID - Identifier number</param>
    /// <param name="title">TVSeries Title</param>
    /// <param name="localname">TVSeries Localized Title</param>
    public FanartTVSeries(string id, string title, string localname) : this (id, title)
    {
      LocalName = localname;
    }

    /// <summary>
    /// Initializes a new instance of the FanartTVSeries class.
    /// </summary>
    /// <param name="id">TVDB ID - Identifier number</param>
    /// <param name="title">TVSeries Title</param>
    /// <param name="localname">TVSeries Localized Title</param>
    /// <param name="seasons">Seasons list</param>
    public FanartTVSeries(string id, string title, string localname, string seasons) : this (id, title, localname)
    {
      Seasons = seasons;
    }
  } 
}

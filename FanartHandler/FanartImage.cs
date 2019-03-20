// Type: FanartHandler.FanartImage
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

namespace FanartHandler
{

  // *** Fanart Class
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

  // *** Fanart Image
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
    public FanartImage() 
      : base ()
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
    public FanartImage(string id, string key, string diskImage, string sourceImage, string type, string source) 
      : base (id)
    {
      Key = key;
      DiskImage = diskImage;
      SourceImage = sourceImage;
      Type = type;
      Source = source;
    }
  }

  // *** Fanart Music
  internal class FanartMusic : FanartClass
  {
    public new string Id 
    {
      get { return base.Id; }
      set 
      { 
        if (string.IsNullOrEmpty(value))
          base.Id = string.Empty;
        else if (value.Length != 36)
          base.Id = string.Empty; 
        else
          base.Id = value;
      }
    }

    public bool HasMBID
    {
      get { return !string.IsNullOrEmpty(Id); }
    }

    /// <summary>
    /// Initializes a new instance of the FanartMusic class.
    /// </summary>
    public FanartMusic() 
      : base ()
    {
    }

    /// <summary>
    /// Initializes a new instance of the FanartMusic class.
    /// </summary>
    /// <param name="mbid">MusicBrainz ID - Identifier number</param>
    public FanartMusic(string mbid) 
      : this ()
    {
      Id = mbid;
    }
  }

  // *** Fanart Artist
  internal class FanartArtist : FanartMusic
  {
    private string _artist;
    public string Artist 
    {
      get
      {
        return _artist;
      }
      set
      {
        if (!string.IsNullOrEmpty(value))
        {
          _artist = value.Trim();
          DBArtist = Utils.GetArtist(_artist);

          if (!HasMBID && !string.IsNullOrEmpty(DBArtist))
          {
            Id = Utils.DBm.GetDBMusicBrainzID(DBArtist, null);
          }
        }
        else
        {
          _artist = string.Empty;
          DBArtist = string.Empty;
        }
      }
    }
    public string DBArtist { get; set; }

    public new bool IsEmpty
    {
      get { return string.IsNullOrEmpty(Artist); }
    }

    public override string ToString()
    {
      if (!IsEmpty)
      {
        return Artist;
      }
      return string.Empty;
    }

    public string GetFileName()
    {
      return MediaPortal.Util.Utils.MakeFileName(Artist).Trim();
    }

    /// <summary>
    /// Initializes a new instance of the FanartArtist class.
    /// </summary>
    public FanartArtist() 
      : base ()
    {
      Artist = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartArtist class.
    /// </summary>
    /// <param name="artist">Artist name</param>
    public FanartArtist(string artist) 
      : this ()
    {
      Artist = artist;
    }

    /// <summary>
    /// Initializes a new instance of the FanartArtist class.
    /// </summary>
    /// <param name="id">MusicBrainz ID - Identifier number</param>
    /// <param name="artist">Artist name</param>
    public FanartArtist(string id, string artist)
      : this ()
    {
      Id = id;
      Artist = artist;
    }
  }

  // *** Fanart Artist Info
  internal class FanartArtistInfo : FanartArtist
  {
    public string Alternate { get; set; }
    public string Bio { get; set; }
    public string BioEN { get; set; }
    public string Style { get; set; }
    public string Genre { get; set; }
    public string Gender { get; set; }
    public string Country { get; set; }
    public string Born { get; set; }
    public string Thumb { get; set; }

    public string GetBio()
    {
      if (string.IsNullOrEmpty(Bio))
      {
        return BioEN;
      }
      return Bio;
    }

    /// <summary>
    /// Initializes a new instance of the FanartArtistInfo class.
    /// </summary>
    public FanartArtistInfo() 
      : base ()
    {
      Alternate = string.Empty;
      Bio = string.Empty;
      BioEN = string.Empty;
      Style = string.Empty;
      Genre = string.Empty;
      Gender = string.Empty;
      Country = string.Empty;
      Born = string.Empty;
      Thumb = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartArtistInfo class.
    /// </summary>
    /// <param name="artist">Artist class</param>
    public FanartArtistInfo(FanartArtist artist)
       : this()
    {
      Id = artist.Id;
      Artist = artist.Artist;
    }
  }
  
  // *** Fanart Album
  internal class FanartAlbum : FanartMusic
  {
    private string _artist;
    public string Artist
    {
      get
      {
        return _artist;
      }
      set
      {
        if (!string.IsNullOrEmpty(value))
        {
          _artist = value.Trim();
          DBArtist = Utils.GetArtist(_artist);

          if (!HasMBID && !string.IsNullOrEmpty(DBArtist) && !string.IsNullOrEmpty(DBAlbum))
          {
            Id = Utils.DBm.GetDBMusicBrainzID(DBArtist, DBAlbum);
          }
          if (HasMBID)
          {
            RecordLabel.SetRecordLabelFromDB(Utils.DBm.GetLabelIdNameForAlbum(Id));
          }
        }
        else
        {
          _artist = string.Empty;
          DBArtist = string.Empty;
        }
      }
    }
    public string DBArtist { get; set; }

    private string _album;
    public string Album
    {
      get
      {
        return _album;
      }
      set
      {
        if (!string.IsNullOrEmpty(value))
        {
          _album = value.Trim();
          DBAlbum = Utils.GetAlbum(_album);

          if (!HasMBID && !string.IsNullOrEmpty(DBArtist) && !string.IsNullOrEmpty(DBAlbum))
          {
            Id = Utils.DBm.GetDBMusicBrainzID(DBArtist, DBAlbum);
          }
          if (HasMBID)
          {
            RecordLabel.SetRecordLabelFromDB(Utils.DBm.GetLabelIdNameForAlbum(Id));
          }
        }
        else
        {
          _album = string.Empty;
          DBAlbum = string.Empty;
        }
      }
    }
    public string DBAlbum { get; set; }

    public int CDs { get; set; }
    public string Year { get; set; }

    public FanartRecordLabel RecordLabel { get; set; }

    public new bool IsEmpty
    {
      get { return string.IsNullOrEmpty(Artist) || string.IsNullOrEmpty(Album); }
    }

    public override string ToString()
    {
      if (!IsEmpty)
      {
        return string.Format(Utils.MusicMask, Artist, Album);
      }
      return string.Empty;
    }

    public string GetFileName()
    {
      return string.Format(Utils.MusicMask, MediaPortal.Util.Utils.MakeFileName(Artist).Trim(), MediaPortal.Util.Utils.MakeFileName(Album).Trim());
    }

    public string GetFileName(string CD)
    {
      if (string.IsNullOrWhiteSpace(CD))
      {
        return GetFileName();
      }
      return GetFileName() + ".CD" + CD.Trim();
    }

    /// <summary>
    /// Initializes a new instance of the FanartAlbum class.
    /// </summary>
    public FanartAlbum() 
      : base ()
    {
      Artist = string.Empty;
      Album = string.Empty;
      CDs = 0;
      Year = string.Empty;
      RecordLabel = new FanartRecordLabel();
    }

    /// <summary>
    /// Initializes a new instance of the FanartAlbum class.
    /// </summary>
    /// <param name="artist">Artist name</param>
    /// <param name="album">Album name</param>
    public FanartAlbum(string artist, string album)
      : this()
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
    public FanartAlbum(string id, string artist, string album)
      : this()
    {
      Id = id;
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
    public FanartAlbum(string artist, string album, int num)
      : this(artist, album)
    {
      CDs = num;
    }

    /// <summary>
    /// Initializes a new instance of the FanartAlbum class.
    /// </summary>
    /// <param name="id">MusicBrainz ID - Identifier number</param>
    /// <param name="artist">Artist name</param>
    /// <param name="album">Album name</param>
    /// <param name="num">Number of Album CD</param>
    public FanartAlbum(string id, string artist, string album, int num) 
      : this (id, artist, album)
    {
      CDs = num;
    }
  }

  // *** Fanart Album Info
  internal class FanartAlbumInfo : FanartAlbum
  {
    public string Description { get; set; }
    public string DescriptionEN { get; set; }
    public string Genre { get; set; }
    public string Style { get; set; }
    public string Thumb { get; set; }

    public string GetDescription()
    {
      if (string.IsNullOrEmpty(Description))
      {
        return DescriptionEN;
      }
      return Description;
    }

    /// <summary>
    /// Initializes a new instance of the FanartAlbumInfo class.
    /// </summary>
    public FanartAlbumInfo() 
      : base ()
    {
      Description = string.Empty;
      DescriptionEN = string.Empty;
      Genre = string.Empty;
      Style = string.Empty;
      Thumb = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartAlbumInfo class.
    /// </summary>
    /// <param name="album">Albumm class</param>
    public FanartAlbumInfo(FanartAlbum album)
       : this()
    {
      Id = album.Id;
      Artist = album.Artist;
      Album = album.Album;
      CDs = album.CDs;
      Year = album.Year;
      RecordLabel = album.RecordLabel;
    }
  }

  // *** Fanart Track
  internal class FanartTrack : FanartMusic
  {
    public string Track { get; set; }

    public new bool IsEmpty
    {
      get { return string.IsNullOrEmpty(Track); }
    }

    public override string ToString()
    {
      if (!IsEmpty)
      {
        return Track;
      }
      return string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartTrack class.
    /// </summary>
    public FanartTrack() 
      : base ()
    {
      Track = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartTrack class.
    /// </summary>
    /// <param name="track">Track name</param>
    public FanartTrack(string track) 
      : this ()
    {
      Track = track;
    }

    /// <summary>
    /// Initializes a new instance of the FanartTrack class.
    /// </summary>
    /// <param name="id">MusicBrainz ID - Identifier number</param>
    /// <param name="track">Track name</param>
    public FanartTrack(string id, string track)
      : this ()
    {
      Id = id;
      Track = track;
    }
  }

  // *** Fanart Music Track
  internal class FanartMusicTrack
  {
    public FanartArtist Artist { get; set; }
    public FanartArtist AlbumArtist { get; set; }
    public FanartAlbum Album { get; set; }
    public FanartTrack Track { get; set; }
    public string Genre { get; set; }

    public string TrackArtist
    {
      get
      {
        if (Artist.IsEmpty)
        {
          return Album.Artist;
        }
        return Artist.Artist;
      }
      set
      {
        Artist.Artist = value;
        Album.Artist = value;
      }
    }

    public string TrackAlbumArtist
    {
      get
      {
        return AlbumArtist.Artist;
      }
      set
      {
        AlbumArtist.Artist = value;
      }
    }

    public string TrackAlbum
    {
      get
      {
        return Album.Album;
      }
      set
      {
        Album.Album = value;
      }
    }

    public string TrackName
    {
      get
      {
        return Track.Track;
      }
      set
      {
        Track.Track = value;
      }
    }

    public string GetArtists
    {
      get
      {
        string artist = string.Empty;
        if (!Artist.IsEmpty)
        {
          artist = TrackArtist ;
        }
        if (!AlbumArtist.IsEmpty && !TrackAlbumArtist.Equals(TrackArtist))
        {
          artist = artist + (string.IsNullOrEmpty(artist) ? string.Empty : "|") + TrackAlbumArtist;
        }
        return artist;
      }
    }

    /// <summary>
    /// Initializes a new instance of the FanartMusicTrack class.
    /// </summary>
    public FanartMusicTrack()
    {
      Artist = new FanartArtist();
      AlbumArtist = new FanartArtist();
      Album = new FanartAlbum();
      Track = new FanartTrack();
      Genre = string.Empty;
    }

  }

  // *** Fanart Video Music Track
  internal class FanartVideoTrack : FanartMusicTrack
  {
    public FanartArtist VideoArtist { get; set; }

    public string TrackVideoArtist
    {
      get
      {
        return VideoArtist.Artist;
      }
      set
      {
        VideoArtist.Artist = value;
      }
    }

    public new string GetArtists
    {
      get
      {
        string artist = base.GetArtists;
        if (!string.IsNullOrEmpty(TrackVideoArtist) && !TrackVideoArtist.Equals(TrackArtist) && !TrackVideoArtist.Equals(TrackAlbumArtist))
        {
          artist = artist + (string.IsNullOrEmpty(artist) ? string.Empty : "|") + TrackVideoArtist;
        }
        return artist;
      }
    }

    /// <summary>
    /// Initializes a new instance of the FanartVideoTrack class.
    /// </summary>
    public FanartVideoTrack()
      : base()
    {
      VideoArtist = new FanartArtist();
    }
  }

  // *** Fanart RecordLabel
  internal class FanartRecordLabel : FanartMusic
  {
    public string RecordLabel { get; set; }

    public new bool IsEmpty
    {
      get { return string.IsNullOrEmpty(RecordLabel); }
    }

    public override string ToString()
    {
      if (!IsEmpty)
      {
        return RecordLabel;
      }
      return string.Empty;
    }

    public string GetFileName()
    {
      return MediaPortal.Util.Utils.MakeFileName(RecordLabel).Trim();
    }

    public void SetRecordLabelFromDB(string idName)
    {
      if (string.IsNullOrEmpty(idName) || (idName.IndexOf("|") <= 0))
      {
        Id = string.Empty;
        RecordLabel = string.Empty;
      }
      else
      {
        Id = idName.Substring(0, idName.IndexOf("|"));
        RecordLabel = idName.Substring(checked(idName.IndexOf("|") + 1));
      }
    }

    /// <summary>
    /// Initializes a new instance of the FanartRecordLabel class.
    /// </summary>
    public FanartRecordLabel()
      : base()
    {
      RecordLabel = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartRecordLabel class.
    /// </summary>
    /// <param name="artist">RecordLabel name</param>
    public FanartRecordLabel(string name)
      : this()
    {
      RecordLabel = name;
    }

    /// <summary>
    /// Initializes a new instance of the FanartRecordLabel class.
    /// </summary>
    /// <param name="id">MusicBrainz ID - Identifier number</param>
    /// <param name="artist">RecordLabel name</param>
    public FanartRecordLabel(string id, string name)
      : this()
    {
      Id = id;
      RecordLabel = name;
    }
  }

  // *** Fanart Movie
  internal class FanartMovie : FanartClass
  {
    public string Title { get; set; }

    private string _imdbid = string.Empty;
    public string IMDBId 
    {
      get { return _imdbid; }
      set 
      { 
        if (string.IsNullOrEmpty(value))
          _imdbid = string.Empty;
        else if (!value.ToUpperInvariant().StartsWith("TT"))
          _imdbid = string.Empty; 
        else
          _imdbid = value.ToLowerInvariant();
      }
    }

    public bool HasIMDBID
    {
      get { return !string.IsNullOrEmpty(IMDBId); }
    }

    /// <summary>
    /// Initializes a new instance of the FanartMovie class.
    /// </summary>
    public FanartMovie() : base ()
    {
      Title = string.Empty;
      IMDBId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartMovie class.
    /// </summary>
    /// <param name="id">MP DB ID - Identifier number</param>
    /// <param name="title">Movie Title</param>
    public FanartMovie(string id, string title) : base (id)
    {
      Title = title;
      IMDBId = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the FanartMovie class.
    /// </summary>
    /// <param name="id">MP DB ID - Identifier number</param>
    /// <param name="title">Movie Title</param>
    /// <param name="imdbid">Movie IMDBID</param>
    public FanartMovie(string id, string title, string imdbid)
      : base(id)
    {
      Title = title;
      IMDBId = imdbid;
    }
  }

  // *** Fanart Movie Collection
  internal class FanartMovieCollection : FanartMovie
  {
    public new string Title 
    {
      get
      {
        return base.Title;
      }
      set
      {
        if (!string.IsNullOrEmpty(value))
        {
          base.Title = value.Trim();
          DBTitle = Utils.GetArtist(value, Utils.Category.Movie, Utils.SubCategory.MovieScraped);
        }
        else
        {
          base.Title = string.Empty;
          DBTitle = string.Empty;
        }
      }
    }
    public string DBTitle { get; set; }

    public bool HasTitle
    {
      get { return !string.IsNullOrEmpty(Title); }
    }

    /// <summary>
    /// Initializes a new instance of the FanartMovieCollection class.
    /// </summary>
    public FanartMovieCollection() 
      : base ()
    {
      Title = string.Empty;
    }

    public FanartMovieCollection(string title)
      : base()
    {
      Title = title;
    }
  }

  // *** Fanart TVSeries
  internal class FanartTVSeries : FanartClass
  {
    public string Name { get; set; }
    public string LocalName { get; set; }
    public string Seasons { get; set; }

    public bool HasTVDBID
    {
      get { return !string.IsNullOrEmpty(Id); }
    }

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

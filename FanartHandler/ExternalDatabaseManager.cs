// Type: FanartHandler.ExternalDatabaseManager
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

extern alias FHNLog;

using MediaPortal.Configuration;

using FHNLog.NLog;

using SQLite.NET;

using System;
using System.IO;

namespace FanartHandler
{
  internal class ExternalDatabaseManager
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private SQLiteClient dbClient;

    static ExternalDatabaseManager()
    {
    }

    public bool InitDB(string dbFilename)
    {
      try
      {
        var str = Path.Combine(Config.GetFolder((Config.Dir) 4), dbFilename);
        if (File.Exists(str))
        {
          if (new FileInfo(str).Length > 0L)
          {
            dbClient = new SQLiteClient(str);
            return true;
          }
        }
      }
      catch
      {
        dbClient = null;
      }
      return false;
    }

    public void Close()
    {
      try
      {
        if (dbClient != null)
          dbClient.Close();
        dbClient = null;
      }
      catch (Exception ex)
      {
        logger.Error("close: " + ex);
      }
    }

    public SQLiteResultSet GetData(Utils.Category category)
    {
      var sqLiteResultSet = (SQLiteResultSet) null;
      try
      {
        var SQL = string.Empty; 
        if (category == Utils.Category.TvSeriesScraped)
          SQL = "SELECT SortName, id FROM online_series;";
        else if (category == Utils.Category.MusicAlbumThumbScraped)
          SQL = "SELECT DISTINCT c.[artist] as Artist, b.[album] as Album, b.[yearreleased] as Year " +
                 "FROM album_info b, album_info__track_info t, artist_info__track_info a, artist_info c " +
                 "WHERE (b.[album] is not null AND Trim(b.[album])<>'') AND "+
                       "(b.[id]=t.[album_info_id] AND t.[track_info_id] = a.[track_info_id] AND a.[artist_info_id] = c.[id]) AND "+
                       "(c.[artist] is not null AND Trim(c.[artist])<>'') "+
                 "GROUP BY c.artist, b.album;";
        else
          SQL = "SELECT DISTINCT artist FROM artist_info WHERE artist is not NULL ORDER BY artist;";
        sqLiteResultSet = dbClient.Execute(SQL);
      }
      catch { }
      return sqLiteResultSet;
    }
  }
}

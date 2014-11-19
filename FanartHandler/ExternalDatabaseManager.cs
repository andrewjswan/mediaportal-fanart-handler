// Type: FanartHandler.ExternalDatabaseManager
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.Configuration;
using NLog;
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
        var str = Config.GetFolder((Config.Dir) 4) + "\\" + dbFilename;
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
        sqLiteResultSet = dbClient.Execute(category != Utils.Category.TvSeriesScraped 
                                             ? "SELECT artist FROM artist_info WHERE artist is not NULL ORDER by artist;" 
                                             : "SELECT SortName, id FROM online_series;"
                                          );
      }
      catch
      {
      }
      return sqLiteResultSet;
    }
  }
}

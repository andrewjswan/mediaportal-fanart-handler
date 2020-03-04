// Type: FanartHandler.UtilsPictures
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using FHNLog.NLog;

using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Picture.Database;

using SQLite.NET;

using System;
using System.Collections.Generic;
using System.IO;

namespace FanartHandler
{
  internal static class UtilsPictures
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static SQLiteClient PicturesDB;

    static UtilsPictures()
    {
    }

    /// <summary>
    /// Initiation of the DatabaseManager.
    /// </summary>
    /// <param name="dbFilename">Database filename</param>
    /// <returns>if database was successfully or not</returns>
    private static bool InitDB()
    {
      string dbFilename = PictureDatabase.DatabaseName;
      try
      {
        if (File.Exists(dbFilename))
        {
          if (new FileInfo(dbFilename).Length > 0)
          {
            PicturesDB = new SQLiteClient(dbFilename);
            DatabaseUtility.SetPragmas(PicturesDB);
            return true;
          }
        }
      }
      catch (Exception e)
      {
        logger.Error("UtilsPictures: InitDB: Could Not Open Database: " + dbFilename + ". " + e.ToString());
      }

      PicturesDB = null;
      return false;
    }

    /// <summary>
    /// Close the database client.
    /// </summary>
    private static void CloseDB()
    {
      try
      {
        if (PicturesDB != null)
        {
          PicturesDB.Close();
        }

        PicturesDB = null;
      }
      catch (Exception ex)
      {
        logger.Error("UtilsPictures: Close: " + ex.ToString());
      }
    }

    internal static List<string> GetSelectedPicturesByPath(string path)
    {
      List<string> pictures = new List<string>();

      if (!Utils.UsePicturesFanart)
      {
        return pictures;
      }

      if (!InitDB())
      {
        return pictures;
      }

      try
      {
        string sqlQuery = "SELECT strFile FROM picture WHERE idPicture IN (SELECT idPicture FROM picture WHERE strFile LIKE '" + 
                                          DatabaseUtility.RemoveInvalidChars(path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar) + "%' " +
                                         "ORDER BY RANDOM() LIMIT " + Utils.LimitNumberFanart + ");";
        SQLiteResultSet resultSet = PicturesDB.Execute(sqlQuery);
        CloseDB();

        if (resultSet != null)
        {
          if (resultSet.Rows.Count > 0)
          {
            for (int i = 0; i < resultSet.Rows.Count; i++)
            {
              pictures.Add(resultSet.GetField(i, 0));
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetSelectedPictures: " + ex);
      }
      return pictures;
    }
  }
}

// Type: FanartHandler.UtilsPictures
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using FHNLog.NLog;

using MediaPortal.GUI.Library;
using MediaPortal.Database;
using MediaPortal.Picture.Database;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPortal.Util;

namespace FanartHandler
{
  internal static class UtilsPictures
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static UtilsPictures()
    {
    }

    internal static List<string> GetSelectedPicturesByPath(string path, out bool inFileView)
    {
      inFileView = true;
      List<string> pictures = new List<string>();

      if (!Utils.UsePicturesFanart)
      {
        return pictures;
      }

      if (!PictureDatabase.DbHealth)
      {
        return pictures;
      }

      try
      {
        string sqlQuery = string.Empty;
        MediaPortal.GUI.Pictures.GUIPictures Pictures = (MediaPortal.GUI.Pictures.GUIPictures)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_PICTURES);
        if (Pictures == null)
        {
          return pictures;
        }

        if (Pictures.GetDisplayMode == MediaPortal.GUI.Pictures.GUIPictures.Display.Files)
        {
          sqlQuery = "SELECT strFile FROM picture WHERE idPicture IN (SELECT idPicture FROM picture WHERE strFile LIKE '" +
                                     DatabaseUtility.RemoveInvalidChars(path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar) + "%'" +
                                    (PictureDatabase.FilterPrivate ? " AND idPicture NOT IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'Private')" : string.Empty) +
                                   " ORDER BY RANDOM() LIMIT " + Utils.LimitNumberFanart + ");";
        }
        else if (Pictures.GetDisplayMode == MediaPortal.GUI.Pictures.GUIPictures.Display.Date)
        {
          inFileView = false;
          path = path.Replace(Path.DirectorySeparatorChar, '-');
          sqlQuery = "SELECT strFile FROM picture WHERE idPicture IN (SELECT idPicture FROM picture WHERE strDateTaken LIKE '" +
                                     DatabaseUtility.RemoveInvalidChars(path) + "%'" +
                                    (PictureDatabase.FilterPrivate ? " AND idPicture NOT IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'Private')" : string.Empty) +
                                   " ORDER BY RANDOM() LIMIT " + Utils.LimitNumberFanart + ");";
        }
        else if (Pictures.GetDisplayMode == MediaPortal.GUI.Pictures.GUIPictures.Display.Keyword)
        {
          inFileView = false;
          sqlQuery = "SELECT strFile FROM picture WHERE idPicture IN (SELECT idPicture FROM picturekeywords WHERE strKeyword = '" +
                                     DatabaseUtility.RemoveInvalidChars(path) + "' ORDER BY RANDOM() LIMIT " + Utils.LimitNumberFanart + ");";
        }
        else if (Pictures.GetDisplayMode == MediaPortal.GUI.Pictures.GUIPictures.Display.Metadata)
        {
          inFileView = false;
          if (!path.Contains(Path.DirectorySeparatorChar))
          {
            string strName = path.ToDBField();
            sqlQuery = "SELECT strFile FROM picturedata WHERE " + strName + " IS NOT NULL" +
                                (PictureDatabase.FilterPrivate ? " AND idPicture NOT IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'Private')" : string.Empty) +
                                " ORDER BY RANDOM() LIMIT " + Utils.LimitNumberFanart + ";";
          }
          else
          {
            string[] metaWhere = path.Split(Path.DirectorySeparatorChar);
            string strName = metaWhere[0].Trim().ToDBField();
            string strValue = strName.Contains("Altitude") ? metaWhere[1].Trim() : "'" + DatabaseUtility.RemoveInvalidChars(metaWhere[1].Trim()) + "'";
            
            sqlQuery = "SELECT strFile FROM picturedata WHERE " + strName + " = " + strValue +
                                (PictureDatabase.FilterPrivate ? " AND idPicture NOT IN (SELECT DISTINCT idPicture FROM picturekeywords WHERE strKeyword = 'Private')" : string.Empty) +
                                " ORDER BY RANDOM() LIMIT " + Utils.LimitNumberFanart + ";";
          }
        }

        if (string.IsNullOrEmpty(sqlQuery))
        {
          return pictures;
        }

        List<PictureData> picsData = PictureDatabase.GetPicturesByFilter(sqlQuery, "pictures");
        if (picsData != null)
        {
          if (picsData.Count > 0)
          {
            for (int i = 0; i < picsData.Count; i++)
            {
              pictures.Add(picsData[i].FileName);
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

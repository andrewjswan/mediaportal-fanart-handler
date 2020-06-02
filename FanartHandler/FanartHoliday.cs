// Type: FanartHandler.FanartSelectedOther
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.GUI.Library;

using FHNLog.NLog;

using System;
using System.Collections;
using System.Collections.Generic;

namespace FanartHandler
{
  internal class FanartHoliday
  {
    // Private
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool DoShowImageOne = true;
    private Hashtable propertiesHoliday;

    private string CurrHoliday;
    private string CurrHolidayFanart;
    private int PrevHoliday;
    private DateTime StoreHoliday;
    private string StoreItem;
    private string CurrHolidayText;
    private DateTime StartTime;
    private bool StoreFlag;

    private bool FanartAvailable;

    private ArrayList ListHoliday;

    private Hashtable CurrentSelectedImageNames;

    /// <summary>
    /// Fanart Control Visible
    /// -1 Unknown, 0 Hiden, 1 Visible
    /// </summary>
    private int ControlVisible;
    /// <summary>
    /// Fanart Image Control Visible
    /// -1 Unknown, 0 Hiden, 1 Visible
    /// </summary>
    private int ControlImageVisible;

    // Public
    public int RefreshTickCount { get; set; }

    public Hashtable WindowsUsingFanartHoliday { get; set; }
    public Hashtable WindowsUsingFanartHolidayText { get; set; }

    public bool IsSelectedHoliday { get; set; }

    static FanartHoliday()
    {
    }

    public FanartHoliday()
    {
      DoShowImageOne = true;
      FanartAvailable = false;

      RefreshTickCount = 0;
      PrevHoliday = -1;

      ListHoliday = new ArrayList();

      propertiesHoliday = new Hashtable();

      WindowsUsingFanartHoliday = new Hashtable();
      WindowsUsingFanartHolidayText = new Hashtable();
      CurrentSelectedImageNames = new Hashtable();

      IsSelectedHoliday = false;

      StoreHoliday = new DateTime();
      StoreItem = string.Empty;
      StoreFlag = false;
      StartTime = new DateTime();

      CurrHolidayText = string.Empty;

      ClearCurrProperties();
    }

    public void ClearCurrProperties()
    {
      CurrHoliday = string.Empty;
      CurrHolidayFanart = string.Empty;

      ControlVisible = -1;
      ControlImageVisible = -1;
    }

    public bool CheckValidWindowIDForFanart()
    {
      return (Utils.ContainsID(WindowsUsingFanartHoliday) || Utils.ContainsID(WindowsUsingFanartHolidayText));
    }

    private string GetCurrHoliday()
    {
      var date = DateTime.Today;
      string holiday = string.Empty;

      if (StoreHoliday != date)
      {
        CurrHolidayText = string.Empty;

        holiday = Utils.GetHolidays(date, ref CurrHolidayText);

        StoreHoliday = date;
        StoreItem = holiday;
        StoreFlag = true;
        StartTime = DateTime.UtcNow;
      }
      else
      {
        holiday = StoreItem;
      }
      return holiday;
    }

    private void SetCurrHolidayIcon()
    {
      var sFile = string.Empty;
      var sFileNames = new List<string>();  
      try
      {
        if (Utils.ContainsID(WindowsUsingFanartHolidayText) && StoreFlag)
        {
          Utils.FillFilesList(ref sFileNames, StoreItem, Utils.OtherPictures.Holiday);
        }

        if (sFileNames.Count == 0)
          sFile = string.Empty;
        else if (sFileNames.Count == 1)
          sFile = sFileNames[0].Trim();
        else if (sFileNames.Count == 2)
          sFile = sFileNames[(DoShowImageOne ? 0 : 1)].Trim();
        else
        {
          var rand = new Random();
          sFile = sFileNames[rand.Next(sFileNames.Count-1)].Trim();
        }
        Utils.SetProperty("holiday.icon", sFile);

        if (sFileNames.Count == 0)
          StoreFlag = false;
      }
      catch (Exception ex)
      {
        logger.Error("SetCurrHolidayIcon: " + ex);
      }
      Utils.SetProperty("holiday.current", CurrHolidayText);
    }

    #region Refresh Holiday Properties
    public void RefreshHolidayProperties()
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        var SelectedItem = GetCurrHoliday();

        if (Utils.HolidayShow > 0)
        {
          TimeSpan breakDuration = TimeSpan.FromMinutes(Utils.HolidayShow);
          if (DateTime.UtcNow - StartTime > breakDuration)
          {
            SelectedItem = string.Empty;
          }
        }

        if (!string.IsNullOrWhiteSpace(SelectedItem))
        {
          var oldFanart = CurrHolidayFanart;
          var newFanart = string.Empty;
          var flag = !CurrHoliday.Equals(SelectedItem, StringComparison.CurrentCulture);

          if (flag || (RefreshTickCount >= Utils.MaxRefreshTickCount))
          {
            if (flag)
            {
              CurrHolidayFanart = string.Empty;
              PrevHoliday = -1;
              SetCurrentSelectedImageNames(null, Utils.Category.Holiday, Utils.SubCategory.None);
              FanartAvailable = false;
              logger.Debug("Holiday: "+CurrHolidayText);
            }

            newFanart = GetFilename(SelectedItem, null, ref CurrHoliday, ref PrevHoliday, Utils.Category.Holiday, Utils.SubCategory.None, flag);

            if (!string.IsNullOrEmpty(newFanart))
            {
              FanartAvailable = true;
              CurrHolidayFanart = newFanart;

              if (newFanart.Equals(oldFanart, StringComparison.CurrentCulture))
              {
                DoShowImageOne = !DoShowImageOne;
              }
              if (DoShowImageOne)
              {
                Utils.AddProperty(ref propertiesHoliday, "holiday.backdrop1", newFanart, ref ListHoliday);
              }
              else
              {
                Utils.AddProperty(ref propertiesHoliday, "holiday.backdrop2", newFanart, ref ListHoliday);
              }
            }
            else
            {
              Utils.AddProperty(ref propertiesHoliday, "holiday.backdrop1", string.Empty, ref ListHoliday);
              Utils.AddProperty(ref propertiesHoliday, "holiday.backdrop2", string.Empty, ref ListHoliday);
            }
            CurrHoliday = SelectedItem;
            ResetRefreshTickCount();
          }
        }
        else
        {
          CurrHoliday = string.Empty;
          CurrHolidayFanart = string.Empty;
          PrevHoliday = -1;

          Utils.AddProperty(ref propertiesHoliday, "holiday.backdrop1", string.Empty, ref ListHoliday);
          Utils.AddProperty(ref propertiesHoliday, "holiday.backdrop2", string.Empty, ref ListHoliday);

          SetCurrentSelectedImageNames(null, Utils.Category.Holiday, Utils.SubCategory.None);

          FanartAvailable = false;
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshHolidayProperties: " + ex);
      }
    }

    public void RefreshHolidayPropertiesText()
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        var SelectedItem = GetCurrHoliday();

        SetCurrHolidayIcon();
      }
      catch (Exception ex)
      {
        logger.Error("RefreshHolidayPropertiesText: " + ex);
      }
    }
    #endregion

    public void RefreshHoliday(RefreshWorker rw, System.ComponentModel.DoWorkEventArgs e)
    {
      try
      {
        var IsIdle = Utils.IsIdle();
        if (Utils.iActiveWindow == (int)GUIWindow.Window.WINDOW_INVALID)
        {
          return;
        }

        #region Holiday
        if (IsIdle)
        {
          if (Utils.ContainsID(WindowsUsingFanartHolidayText))
          {
            RefreshHolidayPropertiesText();
          }
          else
          {
            EmptyHolidayPropertiesText();
          }
        }
        if (rw != null)
          rw.Report(e);

        if (IsIdle)
        {
          if (Utils.ContainsID(WindowsUsingFanartHoliday))
          {
            IsSelectedHoliday = true;
            RefreshHolidayProperties();
          }
          else if (IsSelectedHoliday)
          {
            EmptyHolidayProperties();
          }
        }
        if (rw != null)
          rw.Report(e);
        #endregion

        if (FanartAvailable)
        {
          IncreaseRefreshTickCount();
        }
        else if (IsSelectedHoliday)
        {
          EmptyAllProperties(false);
        }
        if (rw != null)
          rw.Report(e);
      }
      catch (Exception ex)
      {
        logger.Error("RefreshHoliday: " + ex);
      }
    }

    #region Hash
    public Hashtable GetCurrentSelectedImageNames(Utils.Category category, Utils.SubCategory subcategory)
    {
      if (CurrentSelectedImageNames == null)
      {
        CurrentSelectedImageNames = new Hashtable();
      }

      lock (CurrentSelectedImageNames)
      {
        string cat = string.Format("{0}:{1}",category.ToString(), subcategory.ToString());
        if (CurrentSelectedImageNames.ContainsKey(cat))
        {
          return (Hashtable)CurrentSelectedImageNames[cat];
        }
        else
        {
          return null;
        }
      }
    }

    public void SetCurrentSelectedImageNames(Hashtable ht, Utils.Category category, Utils.SubCategory subcategory)
    {
      if (CurrentSelectedImageNames == null)
      {
        CurrentSelectedImageNames = new Hashtable();
      }

      lock (CurrentSelectedImageNames)
      {
        if (category == Utils.Category.Dummy)
        {
          CurrentSelectedImageNames.Clear();
        }
        else
        {
          string cat = string.Format("{0}:{1}",category.ToString(), subcategory.ToString());
          if (CurrentSelectedImageNames.ContainsKey(cat))
          {
            CurrentSelectedImageNames.Remove(cat);
          }
          CurrentSelectedImageNames.Add(cat, ht);
        }
      }
    }
    #endregion

    internal string GetFilename(string key, string key2, ref string currFile, ref int iFilePrev, Utils.Category category, Utils.SubCategory subcategory, bool newArtist)
    {
      var result = string.Empty;
      try
      {
        if (!Utils.GetIsStopping())
        {
          key = Utils.GetArtist(key, category, subcategory);
          var filenames = GetCurrentSelectedImageNames(category, subcategory);

          if (newArtist || filenames == null || filenames.Count == 0)
          {
            Utils.GetFanart(ref filenames, key, key2, category, subcategory, false);
            Utils.Shuffle(ref filenames);

            SetCurrentSelectedImageNames(filenames, category, subcategory);
          }

          if (filenames != null)
          {
            if (filenames.Count > 0)
            {
              var htValues = filenames.Values;
              result = Utils.GetFanartFilename(ref iFilePrev, ref currFile, ref htValues);
            }
          }
        }
        else
          SetCurrentSelectedImageNames(null, category, subcategory);
      }
      catch (Exception ex)
      {
        logger.Error("GetFilename: " + ex);
      }
      return result;
    }

    public void EmptyAllSelectedImages()
    {
      Utils.EmptyAllImages(ref ListHoliday);
    }

    private void EmptyHolidayProperties(bool currClean = true)
    {
      if (currClean)
      {
        CurrHoliday = string.Empty;
        CurrHolidayFanart = string.Empty;
      }
      PrevHoliday = -1;
      EmptyCurrHolidayProperties();
      Utils.EmptyAllImages(ref ListHoliday);
      SetCurrentSelectedImageNames(null, Utils.Category.Holiday, Utils.SubCategory.None);
      IsSelectedHoliday = false;
    }

    private void EmptyHolidayPropertiesText()
    {
      EmptyCurrHolidayTextProperties();
    }

    private void EmptySelectedProperties()
    {
      RefreshTickCount = 0;
      FanartAvailable = false;
      if (Utils.HolidayShowAllDay)
      {
        StoreHoliday = new DateTime();
      }
    }

    public void EmptyAllProperties(bool currClean = true)
    {
      EmptySelectedProperties();

      EmptyHolidayProperties(currClean);
    }

    private void IncreaseRefreshTickCount()
    {
      RefreshTickCount = checked (RefreshTickCount + 1);
    }

    public void ResetRefreshTickCount()
    {
      RefreshTickCount = 0;
    }

    public void ForceRefreshTickCount()
    {
      RefreshTickCount = Utils.MaxRefreshTickCount;
    }

    public void EmptyCurrHolidayProperties()
    {
      Utils.SetProperty("holiday.backdrop1", string.Empty);
      Utils.SetProperty("holiday.backdrop2", string.Empty);
    }

    public void EmptyCurrHolidayTextProperties()
    {
      Utils.SetProperty("holiday.current", string.Empty);
      Utils.SetProperty("holiday.icon", string.Empty);
    }

    public void EmptyAllSelectedProperties()
    {
      EmptyCurrHolidayTextProperties();
      EmptyCurrHolidayProperties();
    }

    public void UpdateProperties()
    {
      Utils.UpdateProperties(ref propertiesHoliday);
    }

    public void ShowImageSelected()
    {
      if (FanartAvailable)
      {
        FanartIsAvailable();
        if (DoShowImageOne)
        {
          ShowImageOne();
        }
        else
        {
          ShowImageTwo();
        }
      }
      else
      {
        FanartIsNotAvailable();
        HideImageSelected();
      }
    }

    public void HideImageSelected()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlImageVisible != 0))
      {
        Utils.HideControl(Utils.iActiveWindow, 91919285);
        Utils.HideControl(Utils.iActiveWindow, 91919286);
        DoShowImageOne = true;
        ControlImageVisible = 0;
        // logger.Debug("*** Hide all fanart [91919285,91919286]... ");
      }
    }

    public void FanartIsAvailable()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlVisible != 1))
      {
        Utils.ShowControl(Utils.iActiveWindow, 91919284);
        ControlVisible = 1;
        // logger.Debug("*** Show fanart [91919284]...");
      }
    }

    public void FanartIsNotAvailable()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlVisible != 0))
      {
        Utils.HideControl(Utils.iActiveWindow, 91919284);
        ControlVisible = 0;
        // logger.Debug("*** Hide fanart [91919284]...");
      }
    }

    public void ShowImageOne()
    {
      if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
      {
        Utils.ShowControl(Utils.iActiveWindow, 91919285);
        Utils.HideControl(Utils.iActiveWindow, 91919286);
        DoShowImageOne = false;
        ControlImageVisible = 1;
        // logger.Debug("*** First fanart [91919285] visible ...");
      }
      else
      {
        RefreshTickCount = 0;
      }
    }

    public void ShowImageTwo()
    {
      if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
      {
        Utils.ShowControl(Utils.iActiveWindow, 91919286);
        Utils.HideControl(Utils.iActiveWindow, 91919285);
        DoShowImageOne = true;
        ControlImageVisible = 1;
        // logger.Debug("*** Second fanart [91919286] visible ...");
      }
      else
      {
        RefreshTickCount = 0;
      }
    }

  }
}
// Type: FanartHandler.FanartSelectedOther
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using MediaPortal.GUI.Library;

using FHNLog.NLog;

using System;
using System.Collections;

namespace FanartHandler
{
  internal class FanartWeather
  {
    // Private
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool DoShowImageOne = true;
    private Hashtable propertiesWeather;

    private string CurrWeather;
    private string CurrWeatherFanart;
    private int PrevWeather;

    private bool FanartAvailable;

    private ArrayList ListWeather;

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

    public Hashtable WindowsUsingFanartWeather { get; set; }

    public bool IsSelectedWeather { get; set; }

    static FanartWeather()
    {
    }

    public FanartWeather()
    {
      DoShowImageOne = true;
      FanartAvailable = false;

      RefreshTickCount = 0;
      PrevWeather = -1;

      ListWeather = new ArrayList();

      propertiesWeather = new Hashtable();

      WindowsUsingFanartWeather = new Hashtable();
      CurrentSelectedImageNames = new Hashtable();

      IsSelectedWeather = false;

      ClearCurrProperties();
    }

    public void ClearCurrProperties()
    {
      CurrWeather = string.Empty;
      CurrWeatherFanart = string.Empty;

      ControlVisible = -1;
      ControlImageVisible = -1;
    }

    public bool CheckValidWindowIDForFanart()
    {
      return (Utils.ContainsID(WindowsUsingFanartWeather));
    }

    #region Refresh Weather Properties
    public void RefreshWeatherProperties()
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        var SelectedItem = Utils.GetWeather(Utils.GetProperty("#WorldWeather.TodayIconNumber"));

        if (!string.IsNullOrWhiteSpace(SelectedItem))
        {
          var oldFanart = CurrWeatherFanart;
          var newFanart = string.Empty;
          var flag = !CurrWeather.Equals(SelectedItem, StringComparison.CurrentCulture);

          if (flag || (RefreshTickCount >= Utils.MaxRefreshTickCount))
          {
            if (flag)
            {
              CurrWeatherFanart = string.Empty;
              PrevWeather = -1;
              SetCurrentSelectedImageNames(null, Utils.Category.Weather);
              FanartAvailable = false;
            }

            newFanart = GetFilename(Utils.GetWeatherCurrentSeason().ToString() + SelectedItem, null, ref CurrWeather, ref PrevWeather, Utils.Category.Weather, flag);
            if (string.IsNullOrEmpty(newFanart))
            {
              newFanart = GetFilename(SelectedItem, null, ref CurrWeather, ref PrevWeather, Utils.Category.Weather, flag);
            }

            if (!string.IsNullOrEmpty(newFanart))
            {
              FanartAvailable = true;
              CurrWeatherFanart = newFanart;

              if (newFanart.Equals(oldFanart, StringComparison.CurrentCulture))
              {
                DoShowImageOne = !DoShowImageOne;
              }
              if (DoShowImageOne)
              {
                Utils.AddProperty(ref propertiesWeather, "weather.backdrop1", newFanart, ref ListWeather);
                // logger.Debug("*** Image 1: " + SelectedItem + " - " + newFanart);
              }
              else
              {
                Utils.AddProperty(ref propertiesWeather, "weather.backdrop2", newFanart, ref ListWeather);
                // logger.Debug("*** Image 2: " + SelectedItem + " - "  + newFanart);
              }
            }
            else
            {
              Utils.AddProperty(ref propertiesWeather, "weather.backdrop1", string.Empty, ref ListWeather);
              Utils.AddProperty(ref propertiesWeather, "weather.backdrop2", string.Empty, ref ListWeather);
              // logger.Debug("*** Image 1,2: " + SelectedItem + " - " + "Empty");
            }
            CurrWeather = SelectedItem;
            ResetRefreshTickCount();
          }
        }
        else
        {
          CurrWeather = string.Empty;
          CurrWeatherFanart = string.Empty;
          PrevWeather = -1;

          Utils.AddProperty(ref propertiesWeather, "weather.backdrop1", string.Empty, ref ListWeather);
          Utils.AddProperty(ref propertiesWeather, "weather.backdrop2", string.Empty, ref ListWeather);
          // logger.Debug("*** Image 1,2: Empty");

          SetCurrentSelectedImageNames(null, Utils.Category.Weather);

          FanartAvailable = false;
        }
      }
      catch (Exception ex)
      {
        logger.Error("RefreshWeatherProperties: " + ex);
      }
    }
    #endregion

    public void RefreshWeather(RefreshWorker rw, System.ComponentModel.DoWorkEventArgs e)
    {
      try
      {
        var IsIdle = Utils.IsIdle();
        if (Utils.iActiveWindow == (int)GUIWindow.Window.WINDOW_INVALID)
        {
          return;
        }

        #region Weather
        if (IsIdle)
        {
          if (Utils.ContainsID(WindowsUsingFanartWeather))
          {
            IsSelectedWeather = true;
            RefreshWeatherProperties();
          }
          else if (IsSelectedWeather)
          {
            EmptyWeatherProperties();
          }
        }
        if (rw != null)
          rw.Report(e);
        #endregion

        if (FanartAvailable)
        {
          IncreaseRefreshTickCount();
        }
        else if (IsSelectedWeather)
        {
          EmptyAllProperties(false);
        }
        if (rw != null)
          rw.Report(e);
      }
      catch (Exception ex)
      {
        logger.Error("RefreshWeather: " + ex);
      }
    }

    #region Hash
    public Hashtable GetCurrentSelectedImageNames(Utils.Category category)
    {
      if (CurrentSelectedImageNames == null)
      {
        CurrentSelectedImageNames = new Hashtable();
      }

      lock (CurrentSelectedImageNames)
      {
        if (CurrentSelectedImageNames.ContainsKey(category))
        {
          return (Hashtable)CurrentSelectedImageNames[category];
        }
        else
        {
          return null;
        }
      }
    }

    public void SetCurrentSelectedImageNames(Hashtable ht, Utils.Category category)
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
          if (CurrentSelectedImageNames.ContainsKey(category))
          {
            CurrentSelectedImageNames.Remove(category);
          }
          CurrentSelectedImageNames.Add(category, ht);
        }
      }
    }
    #endregion

    internal string GetFilename(string key, string key2, ref string currFile, ref int iFilePrev, Utils.Category category, bool newArtist)
    {
      var result = string.Empty;
      try
      {
        if (!Utils.GetIsStopping())
        {
          key = Utils.GetArtist(key, category);
          var filenames = GetCurrentSelectedImageNames(category);

          if (newArtist || filenames == null || filenames.Count == 0)
          {
            Utils.GetFanart(ref filenames, key, key2, category, Utils.SubCategory.None, false);
            if (iFilePrev == -1)
              Utils.Shuffle(ref filenames);

            SetCurrentSelectedImageNames(filenames, category);
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
          SetCurrentSelectedImageNames(null, category);
      }
      catch (Exception ex)
      {
        logger.Error("GetFilename: " + ex);
      }
      return result;
    }

    public void EmptyAllSelectedImages()
    {
      Utils.EmptyAllImages(ref ListWeather);
    }

    private void EmptyWeatherProperties(bool currClean = true)
    {
      if (currClean)
      {
        CurrWeather = string.Empty;
        CurrWeatherFanart = string.Empty;
      }
      PrevWeather = -1;
      EmptyCurrWeatherProperties();
      Utils.EmptyAllImages(ref ListWeather);
      SetCurrentSelectedImageNames(null, Utils.Category.Weather);
      IsSelectedWeather = false;
    }

    private void EmptySelectedProperties()
    {
      RefreshTickCount = 0;
      FanartAvailable = false;
    }

    public void EmptyAllProperties(bool currClean = true)
    {
      EmptySelectedProperties();

      EmptyWeatherProperties(currClean);
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

    public void EmptyCurrWeatherProperties()
    {
      Utils.SetProperty("weather.backdrop1", string.Empty);
      Utils.SetProperty("weather.backdrop2", string.Empty);
    }

    public void EmptyAllSelectedProperties()
    {
      EmptyCurrWeatherProperties();
    }

    public void UpdateProperties()
    {
      Utils.UpdateProperties(ref propertiesWeather);
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
        Utils.HideControl(Utils.iActiveWindow, 91919281);
        Utils.HideControl(Utils.iActiveWindow, 91919282);
        DoShowImageOne = true;
        ControlImageVisible = 0;
        // logger.Debug("*** Hide all weather fanart [91919281,91919282]... ");
      }
    }

    public void FanartIsAvailable()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlVisible != 1))
      {
        Utils.ShowControl(Utils.iActiveWindow, 91919283);
        ControlVisible = 1;
        // logger.Debug("*** Show weather fanart [91919283]...");
      }
    }

    public void FanartIsNotAvailable()
    {
      if ((Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID) && (ControlVisible != 0))
      {
        Utils.HideControl(Utils.iActiveWindow, 91919283);
        ControlVisible = 0;
        // logger.Debug("*** Hide weather fanart [91919283]...");
      }
    }

    public void ShowImageOne()
    {
      if (Utils.iActiveWindow > (int)GUIWindow.Window.WINDOW_INVALID)
      {
        Utils.ShowControl(Utils.iActiveWindow, 91919281);
        Utils.HideControl(Utils.iActiveWindow, 91919282);
        DoShowImageOne = false;
        ControlImageVisible = 1;
        // logger.Debug("*** First weather fanart [91919281] visible ...");
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
        Utils.ShowControl(Utils.iActiveWindow, 91919282);
        Utils.HideControl(Utils.iActiveWindow, 91919281);
        DoShowImageOne = true;
        ControlImageVisible = 1;
        // logger.Debug("*** Second weather fanart [91919282] visible ...");
      }
      else
      {
        RefreshTickCount = 0;
      }
    }

  }
}
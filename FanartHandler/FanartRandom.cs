// Type: FanartHandler.FanartRandom
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using MediaPortal.GUI.Library;
using NLog;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Xml.XPath;

namespace FanartHandler
{
  internal class FanartRandom
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private bool doShowImageOneRandom = true;
    private bool firstRandom = true;

    public ArrayList ListAnyGamesUser;
    public ArrayList ListAnyMoviesScraper;
    public ArrayList ListAnyMoviesUser;
    public ArrayList ListAnyMovingPictures;
    public ArrayList ListAnyMusicScraper;
    public ArrayList ListAnyMusicUser;
    public ArrayList ListAnyPicturesUser;
    public ArrayList ListAnyPluginsUser;
    public ArrayList ListAnyScorecenterUser;
    public ArrayList ListAnyTVSeries;
    public ArrayList ListAnyTVUser;

    public int PrevSelectedGamesUser;
    public int PrevSelectedMoviesScraper;
    public int PrevSelectedMoviesUser;
    public int PrevSelectedMovingPictures;
    public int PrevSelectedMusicScraper;
    public int PrevSelectedMusicUser;
    public int PrevSelectedPicturesUser;
    public int PrevSelectedPluginsUser;
    public int PrevSelectedScorecenterUser;
    public int PrevSelectedTVSeries;
    public int PrevSelectedTVUser;

    private string currAnyGamesUser;
    private string currAnyMoviesScraper;
    private string currAnyMoviesUser;
    private string currAnyMovingPictures;
    private string currAnyMusicScraper;
    private string currAnyMusicUser;
    private string currAnyPicturesUser;
    private string currAnyPluginsUser;
    private string currAnyScorecenterUser;
    private string currAnyTVSeries;
    private string currAnyTVUser;

    private Hashtable htAny;
    private Hashtable propertiesRandomPerm;

    public int CurrCountRandom { get; set; }

    public Random RandAnyGamesUser { get; set; }
    public Random RandAnyMoviesUser { get; set; }
    public Random RandAnyMoviesScraper { get; set; }
    public Random RandAnyMovingPictures { get; set; }
    public Random RandAnyMusicUser { get; set; }
    public Random RandAnyMusicScraper { get; set; }
    public Random RandAnyPicturesUser { get; set; }
    public Random RandAnyScorecenterUser { get; set; }
    public Random RandAnyTVSeries { get; set; }
    public Random RandAnyTVUser { get; set; }
    public Random RandAnyPluginsUser { get; set; }

    public bool WindowOpen { get; set; }

    public int CountSetVisibility { get; set; }
    public int UpdateVisibilityCountRandom { get; set; }

    public Hashtable WindowsUsingFanartRandom { get; set; }

    public Hashtable PropertiesRandom { get; set; }

    public Hashtable PropertiesRandomPerm
    {
      get { return propertiesRandomPerm; }
      set { propertiesRandomPerm = value; }
    }

    public bool DoShowImageOneRandom
    {
      get { return doShowImageOneRandom; }
      set { doShowImageOneRandom = value; }
    }

    public bool FirstRandom
    {
      get { return firstRandom; }
      set { firstRandom = value; }
    }

    static FanartRandom()
    {
    }

    public void RefreshRandomImageProperties(RefreshWorker rw)
    {
      try
      {
        if (Utils.GetIsStopping())
          return;

        var doPerm = false;
        if (PropertiesRandomPerm.Count == 0)
          doPerm = true;

        if (CurrCountRandom >= FanartHandlerSetup.Fh.MaxCountImage || FirstRandom || CurrCountRandom == 0)
        {
          var str = string.Empty;
          #region Movies
          if (SupportsRandomImages("useRandomMoviesUserFanart").Equals("True", StringComparison.CurrentCulture))
          {
            var randomFilename = GetRandomFilename(ref currAnyMoviesUser, Utils.Category.MovieManual, ref PrevSelectedMoviesUser);
            if (!string.IsNullOrEmpty(randomFilename))
            {
              if (DoShowImageOneRandom)
              {
                AddPropertyRandom("#fanarthandler.movie.userdef.backdrop1.any", randomFilename, ref ListAnyMoviesUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.movie.userdef.backdrop2.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.movie.userdef.backdrop2.any", randomFilename, ref ListAnyMoviesUser, doPerm);
              }
              else
              {
                AddPropertyRandom("#fanarthandler.movie.userdef.backdrop2.any", randomFilename, ref ListAnyMoviesUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.movie.userdef.backdrop1.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.movie.userdef.backdrop1.any", randomFilename, ref ListAnyMoviesUser, doPerm);
              }
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
            else
            {
              Utils.SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
              Utils.SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
              PrevSelectedMoviesUser = -1;
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
          }
          else
          {
            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMoviesUser);
            Utils.SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
            PrevSelectedMoviesUser = -1;
            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
              rw.ReportProgress(100, "Updated Properties");
          }
          if (SupportsRandomImages("useRandomMoviesScraperFanart").Equals("True", StringComparison.CurrentCulture))
          {
            var randomFilename = GetRandomFilename(ref currAnyMoviesScraper, Utils.Category.MovieScraped, ref PrevSelectedMoviesScraper);
            if (!string.IsNullOrEmpty(randomFilename))
            {
              if (DoShowImageOneRandom)
              {
                AddPropertyRandom("#fanarthandler.movie.scraper.backdrop1.any", randomFilename, ref ListAnyMoviesScraper, doPerm);
                var property = Utils.GetProperty("#fanarthandler.movie.scraper.backdrop2.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.movie.scraper.backdrop2.any", randomFilename, ref ListAnyMoviesScraper, doPerm);
              }
              else
              {
                AddPropertyRandom("#fanarthandler.movie.scraper.backdrop2.any", randomFilename, ref ListAnyMoviesScraper, doPerm);
                var property = Utils.GetProperty("#fanarthandler.movie.scraper.backdrop1.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.movie.scraper.backdrop1.any", randomFilename, ref ListAnyMoviesScraper, doPerm);
              }
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
            else
            {
              Utils.SetProperty("#fanarthandler.scraper.movie.backdrop1.any", string.Empty);
              Utils.SetProperty("#fanarthandler.scraper.movie.backdrop2.any", string.Empty);
              PrevSelectedMoviesScraper = -1;
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
          }
          else
          {
            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMoviesScraper);
            Utils.SetProperty("#fanarthandler.movie.scraper.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.movie.scraper.backdrop2.any", string.Empty);
            PrevSelectedMoviesScraper = -1;
            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
              rw.ReportProgress(100, "Updated Properties");
          }
          if (SupportsRandomImages("useRandomMovingPicturesFanart").Equals("True", StringComparison.CurrentCulture))
          {
            var randomFilename = GetRandomFilename(ref currAnyMovingPictures, Utils.Category.MovingPictureManual, ref PrevSelectedMovingPictures);
            if (!string.IsNullOrEmpty(randomFilename))
            {
              if (DoShowImageOneRandom)
              {
                AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", randomFilename, ref ListAnyMovingPictures, doPerm);
                var property = Utils.GetProperty("#fanarthandler.movingpicture.backdrop2.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", randomFilename, ref ListAnyMovingPictures, doPerm);
              }
              else
              {
                AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", randomFilename, ref ListAnyMovingPictures, doPerm);
                var property = Utils.GetProperty("#fanarthandler.movingpicture.backdrop1.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", randomFilename, ref ListAnyMovingPictures, doPerm);
              }
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
            else
            {
              Utils.SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
              Utils.SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
              PrevSelectedMovingPictures = -1;
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
          }
          else
          {
            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMovingPictures);
            Utils.SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
            PrevSelectedMovingPictures = -1;
            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
              rw.ReportProgress(100, "Updated Properties");
          }
          #endregion
          #region Music
          if (SupportsRandomImages("useRandomMusicUserFanart").Equals("True", StringComparison.CurrentCulture))
          {
            var randomFilename = GetRandomFilename(ref currAnyMusicUser, Utils.Category.MusicFanartManual, ref PrevSelectedMusicUser);
            if (!string.IsNullOrEmpty(randomFilename))
            {
              if (DoShowImageOneRandom)
              {
                AddPropertyRandom("#fanarthandler.music.userdef.backdrop1.any", randomFilename, ref ListAnyMusicUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.music.userdef.backdrop2.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.music.userdef.backdrop2.any", randomFilename, ref ListAnyMusicUser, doPerm);
              }
              else
              {
                AddPropertyRandom("#fanarthandler.music.userdef.backdrop2.any", randomFilename, ref ListAnyMusicUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.music.userdef.backdrop1.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.music.userdef.backdrop1.any", randomFilename, ref ListAnyMusicUser, doPerm);
              }
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
            else
            {
              Utils.SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
              Utils.SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
              PrevSelectedMusicUser = -1;
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
          }
          else
          {
            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMusicUser);
            Utils.SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
            PrevSelectedMusicUser = -1;
            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
              rw.ReportProgress(100, "Updated Properties");
          }
          if (SupportsRandomImages("useRandomMusicScraperFanart").Equals("True", StringComparison.CurrentCulture))
          {
            var randomFilename = GetRandomFilename(ref currAnyMusicScraper, Utils.Category.MusicFanartScraped, ref PrevSelectedMusicScraper);
            if (!string.IsNullOrEmpty(randomFilename))
            {
              if (DoShowImageOneRandom)
              {
                AddPropertyRandom("#fanarthandler.music.scraper.backdrop1.any", randomFilename, ref ListAnyMusicScraper, doPerm);
                var property = Utils.GetProperty("#fanarthandler.music.scraper.backdrop2.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.music.scraper.backdrop2.any", randomFilename, ref ListAnyMusicScraper, doPerm);
              }
              else
              {
                AddPropertyRandom("#fanarthandler.music.scraper.backdrop2.any", randomFilename, ref ListAnyMusicScraper, doPerm);
                var property = Utils.GetProperty("#fanarthandler.music.scraper.backdrop1.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.music.scraper.backdrop1.any", randomFilename, ref ListAnyMusicScraper, doPerm);
              }
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
            else
            {
              Utils.SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
              Utils.SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
              PrevSelectedMusicScraper = -1;
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
          }
          else
          {
            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMusicScraper);
            Utils.SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
            PrevSelectedMusicScraper = -1;
            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
              rw.ReportProgress(100, "Updated Properties");
          }
          #endregion
          #region TV
          if (SupportsRandomImages("useRandomTVUserFanart").Equals("True", StringComparison.CurrentCulture))
          {
            var randomFilename = GetRandomFilename(ref currAnyTVUser, Utils.Category.TvManual, ref PrevSelectedTVUser);
            if (!string.IsNullOrEmpty(randomFilename))
            {
              if (DoShowImageOneRandom)
              {
                AddPropertyRandom("#fanarthandler.tv.userdef.backdrop1.any", randomFilename, ref ListAnyTVUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.tv.userdef.backdrop2.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.tv.userdef.backdrop2.any", randomFilename, ref ListAnyTVUser, doPerm);
              }
              else
              {
                AddPropertyRandom("#fanarthandler.tv.userdef.backdrop2.any", randomFilename, ref ListAnyTVUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.tv.backdrop1.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.tv.userdef.backdrop1.any", randomFilename, ref ListAnyTVUser, doPerm);
              }
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
            else
            {
              Utils.SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
              Utils.SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
              PrevSelectedTVUser = -1;
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
          }
          else
          {
            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyTVUser);
            Utils.SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
            PrevSelectedTVUser = -1;
            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
              rw.ReportProgress(100, "Updated Properties");
          }
          #endregion
          #region TVSeries
          if (SupportsRandomImages("useRandomTVSeriesFanart").Equals("True", StringComparison.CurrentCulture))
          {
            var randomFilename = GetRandomFilename(ref currAnyTVSeries, Utils.Category.TvSeriesScraped, ref PrevSelectedTVSeries);
            if (!string.IsNullOrEmpty(randomFilename))
            {
              if (DoShowImageOneRandom)
              {
                AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", randomFilename, ref ListAnyTVSeries, doPerm);
                var property = Utils.GetProperty("#fanarthandler.tvseries.backdrop2.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", randomFilename, ref ListAnyTVSeries, doPerm);
              }
              else
              {
                AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", randomFilename, ref ListAnyTVSeries, doPerm);
                var property = Utils.GetProperty("#fanarthandler.tvseries.backdrop1.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", randomFilename, ref ListAnyTVSeries, doPerm);
              }
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
            else
            {
              Utils.SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
              Utils.SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
              PrevSelectedTVSeries = -1;
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
          }
          else
          {
            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyTVSeries);
            Utils.SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
            PrevSelectedTVSeries = -1;
            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
              rw.ReportProgress(100, "Updated Properties");
          }
          #endregion
          #region Pictures
          if (SupportsRandomImages("useRandomPicturesUserFanart").Equals("True", StringComparison.CurrentCulture))
          {
            var randomFilename = GetRandomFilename(ref currAnyPicturesUser, Utils.Category.PictureManual, ref PrevSelectedPicturesUser);
            if (!string.IsNullOrEmpty(randomFilename))
            {
              if (DoShowImageOneRandom)
              {
                AddPropertyRandom("#fanarthandler.picture.userdef.backdrop1.any", randomFilename, ref ListAnyPicturesUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.picture.userdef.backdrop2.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.picture.userdef.backdrop2.any", randomFilename, ref ListAnyPicturesUser, doPerm);
              }
              else
              {
                AddPropertyRandom("#fanarthandler.picture.userdef.backdrop2.any", randomFilename, ref ListAnyPicturesUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.picture.userdef.backdrop1.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.picture.userdef.backdrop1.any", randomFilename, ref ListAnyPicturesUser, doPerm);
              }
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
            else
            {
              Utils.SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
              Utils.SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
              PrevSelectedPicturesUser = -1;
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
          }
          else
          {
            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyPicturesUser);
            Utils.SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
            PrevSelectedPicturesUser = -1;
            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
              rw.ReportProgress(100, "Updated Properties");
          }
          #endregion
          #region Games
          if (SupportsRandomImages("useRandomGamesUserFanart").Equals("True", StringComparison.CurrentCulture))
          {
            var randomFilename = GetRandomFilename(ref currAnyGamesUser, Utils.Category.GameManual, ref PrevSelectedGamesUser);
            if (!string.IsNullOrEmpty(randomFilename))
            {
              if (DoShowImageOneRandom)
              {
                AddPropertyRandom("#fanarthandler.games.userdef.backdrop1.any", randomFilename, ref ListAnyGamesUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.games.userdef.backdrop2.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.games.userdef.backdrop2.any", randomFilename, ref ListAnyGamesUser, doPerm);
              }
              else
              {
                AddPropertyRandom("#fanarthandler.games.userdef.backdrop2.any", randomFilename, ref ListAnyGamesUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.games.userdef.backdrop1.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.games.userdef.backdrop1.any", randomFilename, ref ListAnyGamesUser, doPerm);
              }
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
            else
            {
              Utils.SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
              Utils.SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
              PrevSelectedGamesUser = -1;
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
          }
          else
          {
            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyGamesUser);
            Utils.SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
            PrevSelectedGamesUser = -1;
            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
              rw.ReportProgress(100, "Updated Properties");
          }
          #endregion
          #region ScoreCenter
          if (SupportsRandomImages("useRandomScoreCenterUserFanart").Equals("True", StringComparison.CurrentCulture))
          {
            var randomFilename = GetRandomFilename(ref currAnyScorecenterUser, Utils.Category.SportsManual, ref PrevSelectedScorecenterUser);
            if (!string.IsNullOrEmpty(randomFilename))
            {
              if (DoShowImageOneRandom)
              {
                AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop1.any", randomFilename, ref ListAnyScorecenterUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop2.any", randomFilename, ref ListAnyScorecenterUser, doPerm);
              }
              else
              {
                AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop2.any", randomFilename, ref ListAnyScorecenterUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop1.any", randomFilename, ref ListAnyScorecenterUser, doPerm);
              }
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
            else
            {
              Utils.SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
              Utils.SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
              PrevSelectedScorecenterUser = -1;
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
          }
          else
          {
            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyScorecenterUser);
            Utils.SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
            PrevSelectedScorecenterUser = -1;
            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
              rw.ReportProgress(100, "Updated Properties");
          }
          #endregion
          #region PlugIns
          if (SupportsRandomImages("useRandomPluginsUserFanart").Equals("True", StringComparison.CurrentCulture))
          {
            var randomFilename = GetRandomFilename(ref currAnyPluginsUser, Utils.Category.PluginManual, ref PrevSelectedPluginsUser);
            if (!string.IsNullOrEmpty(randomFilename))
            {
              if (DoShowImageOneRandom)
              {
                AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop1.any", randomFilename, ref ListAnyPluginsUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.plugins.userdef.backdrop2.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop2.any", randomFilename, ref ListAnyPluginsUser, doPerm);
              }
              else
              {
                AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop2.any", randomFilename, ref ListAnyPluginsUser, doPerm);
                var property = Utils.GetProperty("#fanarthandler.plugins.userdef.backdrop1.any");
                if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                  AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop1.any", randomFilename, ref ListAnyPluginsUser, doPerm);
              }
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
            else
            {
              Utils.SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
              Utils.SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
              PrevSelectedPluginsUser = -1;
              if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
                rw.ReportProgress(100, "Updated Properties");
            }
          }
          else
          {
            FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyPluginsUser);
            Utils.SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
            PrevSelectedPluginsUser = -1;
            if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
              rw.ReportProgress(100, "Updated Properties");
          }
          #endregion
          ResetCurrCountRandom();
          FirstRandom = false;
          if (rw != null && FanartHandlerSetup.Fh.FR.WindowOpen)
            rw.ReportProgress(100, "Updated Properties");
        }
        IncreaseCurrCountRandom();
        if (rw == null)
          return;
        rw.ReportProgress(100, "Updated Properties");
      }
      catch (Exception ex)
      {
        logger.Error("RefreshRandomImageProperties: " + ex);
      }
    }

    public void RefreshRandomImagePropertiesPerm()
    {
      try
      {
        var doPerm = false;
        var str1 = string.Empty;

        if (propertiesRandomPerm == null)
          return;

        if (SupportsRandomImages("useRandomMoviesUserFanart").Equals("True", StringComparison.CurrentCulture) && propertiesRandomPerm.Contains("#fanarthandler.movie.userdef.backdrop"))
        {
          var str2 = PropertiesRandomPerm["#fanarthandler.movie.userdef.backdrop"].ToString();
          if (str2 != null && str2.Length > 0)
          {
            if (DoShowImageOneRandom)
            {
              AddPropertyRandom("#fanarthandler.movie.userdef.backdrop1.any", str2, ref ListAnyMoviesUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.movie.userdef.backdrop2.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.movie.userdef.backdrop2.any", str2, ref ListAnyMoviesUser, doPerm);
            }
            else
            {
              AddPropertyRandom("#fanarthandler.movie.userdef.backdrop2.any", str2, ref ListAnyMoviesUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.movie.userdef.backdrop1.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.movie.userdef.backdrop1.any", str2, ref ListAnyMoviesUser, doPerm);
            }
          }
          else
          {
            Utils.SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
            PrevSelectedMoviesUser = -1;
          }
        }
        else
        {
          FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMoviesUser);
          Utils.SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
          Utils.SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
          PrevSelectedMoviesUser = -1;
        }
        if (SupportsRandomImages("useRandomMoviesScraperFanart").Equals("True", StringComparison.CurrentCulture) && propertiesRandomPerm.Contains("#fanarthandler.movie.scraper.backdrop"))
        {
          var str2 = PropertiesRandomPerm["#fanarthandler.movie.scraper.backdrop"].ToString();
          if (str2 != null && str2.Length > 0)
          {
            if (DoShowImageOneRandom)
            {
              AddPropertyRandom("#fanarthandler.movie.scraper.backdrop1.any", str2, ref ListAnyMoviesScraper, doPerm);
              var property = Utils.GetProperty("#fanarthandler.movie.scraper.backdrop2.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.movie.scraper.backdrop2.any", str2, ref ListAnyMoviesScraper, doPerm);
            }
            else
            {
              AddPropertyRandom("#fanarthandler.movie.scraper.backdrop2.any", str2, ref ListAnyMoviesScraper, doPerm);
              var property = Utils.GetProperty("#fanarthandler.movie.scraper.backdrop1.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.movie.scraper.backdrop1.any", str2, ref ListAnyMoviesScraper, doPerm);
            }
          }
          else
          {
            Utils.SetProperty("#fanarthandler.scraper.movie.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.scraper.movie.backdrop2.any", string.Empty);
            PrevSelectedMoviesScraper = -1;
          }
        }
        else
        {
          FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMoviesScraper);
          Utils.SetProperty("#fanarthandler.movie.scraper.backdrop1.any", string.Empty);
          Utils.SetProperty("#fanarthandler.movie.scraper.backdrop2.any", string.Empty);
          PrevSelectedMoviesScraper = -1;
        }
        if (SupportsRandomImages("useRandomMovingPicturesFanart").Equals("True", StringComparison.CurrentCulture) && propertiesRandomPerm.Contains("#fanarthandler.movingpicture.backdrop"))
        {
          var str2 = PropertiesRandomPerm["#fanarthandler.movingpicture.backdrop"].ToString();
          if (str2 != null && str2.Length > 0)
          {
            if (DoShowImageOneRandom)
            {
              AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", str2, ref ListAnyMovingPictures, doPerm);
              var property = Utils.GetProperty("#fanarthandler.movingpicture.backdrop2.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", str2, ref ListAnyMovingPictures, doPerm);
            }
            else
            {
              AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", str2, ref ListAnyMovingPictures, doPerm);
              var property = Utils.GetProperty("#fanarthandler.movingpicture.backdrop1.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", str2, ref ListAnyMovingPictures, doPerm);
            }
          }
          else
          {
            Utils.SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
            PrevSelectedMovingPictures = -1;
          }
        }
        else
        {
          FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMovingPictures);
          Utils.SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
          Utils.SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
          PrevSelectedMovingPictures = -1;
        }
        if (SupportsRandomImages("useRandomMusicUserFanart").Equals("True", StringComparison.CurrentCulture) && propertiesRandomPerm.Contains("#fanarthandler.music.userdef.backdrop"))
        {
          var str2 = PropertiesRandomPerm["#fanarthandler.music.userdef.backdrop"].ToString();
          if (str2 != null && str2.Length > 0)
          {
            if (DoShowImageOneRandom)
            {
              AddPropertyRandom("#fanarthandler.music.userdef.backdrop1.any", str2, ref ListAnyMusicUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.music.userdef.backdrop2.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.music.userdef.backdrop2.any", str2, ref ListAnyMusicUser, doPerm);
            }
            else
            {
              AddPropertyRandom("#fanarthandler.music.userdef.backdrop2.any", str2, ref ListAnyMusicUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.music.userdef.backdrop1.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.music.userdef.backdrop1.any", str2, ref ListAnyMusicUser, doPerm);
            }
          }
          else
          {
            Utils.SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
            PrevSelectedMusicUser = -1;
          }
        }
        else
        {
          FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMusicUser);
          Utils.SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
          Utils.SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
          PrevSelectedMusicUser = -1;
        }
        if (SupportsRandomImages("useRandomMusicScraperFanart").Equals("True", StringComparison.CurrentCulture) && propertiesRandomPerm.Contains("#fanarthandler.music.scraper.backdrop"))
        {
          var str2 = PropertiesRandomPerm["#fanarthandler.music.scraper.backdrop"].ToString();
          if (str2 != null && str2.Length > 0)
          {
            if (DoShowImageOneRandom)
            {
              AddPropertyRandom("#fanarthandler.music.scraper.backdrop1.any", str2, ref ListAnyMusicScraper, doPerm);
              var property = Utils.GetProperty("#fanarthandler.music.scraper.backdrop2.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.music.scraper.backdrop2.any", str2, ref ListAnyMusicScraper, doPerm);
            }
            else
            {
              AddPropertyRandom("#fanarthandler.music.scraper.backdrop2.any", str2, ref ListAnyMusicScraper, doPerm);
              var property = Utils.GetProperty("#fanarthandler.music.scraper.backdrop1.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.music.scraper.backdrop1.any", str2, ref ListAnyMusicScraper, doPerm);
            }
          }
          else
          {
            Utils.SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
            PrevSelectedMusicScraper = -1;
          }
        }
        else
        {
          FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyMusicScraper);
          Utils.SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
          Utils.SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
          PrevSelectedMusicScraper = -1;
        }
        if (SupportsRandomImages("useRandomTVUserFanart").Equals("True", StringComparison.CurrentCulture) && propertiesRandomPerm.Contains("#fanarthandler.tv.userdef.backdrop"))
        {
          var str2 = PropertiesRandomPerm["#fanarthandler.tv.userdef.backdrop"].ToString();
          if (str2 != null && str2.Length > 0)
          {
            if (DoShowImageOneRandom)
            {
              AddPropertyRandom("#fanarthandler.tv.userdef.backdrop1.any", str2, ref ListAnyTVUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.tv.userdef.backdrop2.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.tv.userdef.backdrop2.any", str2, ref ListAnyTVUser, doPerm);
            }
            else
            {
              AddPropertyRandom("#fanarthandler.tv.userdef.backdrop2.any", str2, ref ListAnyTVUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.tv.backdrop1.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.tv.userdef.backdrop1.any", str2, ref ListAnyTVUser, doPerm);
            }
          }
          else
          {
            Utils.SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
            PrevSelectedTVUser = -1;
          }
        }
        else
        {
          FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyTVUser);
          Utils.SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
          Utils.SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
          PrevSelectedTVUser = -1;
        }
        if (SupportsRandomImages("useRandomTVSeriesFanart").Equals("True", StringComparison.CurrentCulture) && propertiesRandomPerm.Contains("#fanarthandler.tvseries.backdrop"))
        {
          var str2 = PropertiesRandomPerm["#fanarthandler.tvseries.backdrop"].ToString();
          if (str2 != null && str2.Length > 0)
          {
            if (DoShowImageOneRandom)
            {
              AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", str2, ref ListAnyTVSeries, doPerm);
              var property = Utils.GetProperty("#fanarthandler.tvseries.backdrop2.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", str2, ref ListAnyTVSeries, doPerm);
            }
            else
            {
              AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", str2, ref ListAnyTVSeries, doPerm);
              var property = Utils.GetProperty("#fanarthandler.tvseries.backdrop1.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", str2, ref ListAnyTVSeries, doPerm);
            }
          }
          else
          {
            Utils.SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
            PrevSelectedTVSeries = -1;
          }
        }
        else
        {
          FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyTVSeries);
          Utils.SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
          Utils.SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
          PrevSelectedTVSeries = -1;
        }
        if (SupportsRandomImages("useRandomPicturesUserFanart").Equals("True", StringComparison.CurrentCulture) && propertiesRandomPerm.Contains("#fanarthandler.picture.userdef.backdrop"))
        {
          var str2 = PropertiesRandomPerm["#fanarthandler.picture.userdef.backdrop"].ToString();
          if (str2 != null && str2.Length > 0)
          {
            if (DoShowImageOneRandom)
            {
              AddPropertyRandom("#fanarthandler.picture.userdef.backdrop1.any", str2, ref ListAnyPicturesUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.picture.userdef.backdrop2.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.picture.userdef.backdrop2.any", str2, ref ListAnyPicturesUser, doPerm);
            }
            else
            {
              AddPropertyRandom("#fanarthandler.picture.userdef.backdrop2.any", str2, ref ListAnyPicturesUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.picture.userdef.backdrop1.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.picture.userdef.backdrop1.any", str2, ref ListAnyPicturesUser, doPerm);
            }
          }
          else
          {
            Utils.SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
            PrevSelectedPicturesUser = -1;
          }
        }
        else
        {
          FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyPicturesUser);
          Utils.SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
          Utils.SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
          PrevSelectedPicturesUser = -1;
        }
        if (SupportsRandomImages("useRandomGamesUserFanart").Equals("True", StringComparison.CurrentCulture) && propertiesRandomPerm.Contains("#fanarthandler.games.userdef.backdrop"))
        {
          var str2 = PropertiesRandomPerm["#fanarthandler.games.userdef.backdrop"].ToString();
          if (str2 != null && str2.Length > 0)
          {
            if (DoShowImageOneRandom)
            {
              AddPropertyRandom("#fanarthandler.games.userdef.backdrop1.any", str2, ref ListAnyGamesUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.games.userdef.backdrop2.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.games.userdef.backdrop2.any", str2, ref ListAnyGamesUser, doPerm);
            }
            else
            {
              AddPropertyRandom("#fanarthandler.games.userdef.backdrop2.any", str2, ref ListAnyGamesUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.games.userdef.backdrop1.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.games.userdef.backdrop1.any", str2, ref ListAnyGamesUser, doPerm);
            }
          }
          else
          {
            Utils.SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
            PrevSelectedGamesUser = -1;
          }
        }
        else
        {
          FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyGamesUser);
          Utils.SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
          Utils.SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
          PrevSelectedGamesUser = -1;
        }
        if (SupportsRandomImages("useRandomScoreCenterUserFanart").Equals("True", StringComparison.CurrentCulture) && propertiesRandomPerm.Contains("#fanarthandler.scorecenter.userdef.backdrop"))
        {
          var str2 = PropertiesRandomPerm["#fanarthandler.scorecenter.userdef.backdrop"].ToString();
          if (str2 != null && str2.Length > 0)
          {
            if (DoShowImageOneRandom)
            {
              AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop1.any", str2, ref ListAnyScorecenterUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop2.any", str2, ref ListAnyScorecenterUser, doPerm);
            }
            else
            {
              AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop2.any", str2, ref ListAnyScorecenterUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.scorecenter.userdef.backdrop1.any", str2, ref ListAnyScorecenterUser, doPerm);
            }
          }
          else
          {
            Utils.SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
            PrevSelectedScorecenterUser = -1;
          }
        }
        else
        {
          FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyScorecenterUser);
          Utils.SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
          Utils.SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
          PrevSelectedScorecenterUser = -1;
        }
        if (SupportsRandomImages("useRandomPluginsUserFanart").Equals("True", StringComparison.CurrentCulture) && propertiesRandomPerm.Contains("#fanarthandler.plugins.userdef.backdrop"))
        {
          var str2 = PropertiesRandomPerm["#fanarthandler.plugins.userdef.backdrop"].ToString();
          if (str2 != null && str2.Length > 0)
          {
            if (DoShowImageOneRandom)
            {
              AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop1.any", str2, ref ListAnyPluginsUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.plugins.userdef.backdrop2.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop2.any", str2, ref ListAnyPluginsUser, doPerm);
            }
            else
            {
              AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop2.any", str2, ref ListAnyPluginsUser, doPerm);
              var property = Utils.GetProperty("#fanarthandler.plugins.userdef.backdrop1.any");
              if (property == null || property.Length < 2 || property.EndsWith("transparent.png", StringComparison.CurrentCulture))
                AddPropertyRandom("#fanarthandler.plugins.userdef.backdrop1.any", str2, ref ListAnyPluginsUser, doPerm);
            }
          }
          else
          {
            Utils.SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
            Utils.SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
            PrevSelectedPluginsUser = -1;
          }
        }
        else
        {
          FanartHandlerSetup.Fh.EmptyAllImages(ref ListAnyPluginsUser);
          Utils.SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
          Utils.SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
          PrevSelectedPluginsUser = -1;
        }
        ResetCurrCountRandom();
        FirstRandom = false;
        IncreaseCurrCountRandom();
      }
      catch (Exception ex)
      {
        logger.Error("RefreshRandomImagePropertiesPerm: " + ex);
      }
    }

    private string SupportsRandomImages(string type)
    {
      var skinFile = (SkinFile) WindowsUsingFanartRandom[GUIWindowManager.ActiveWindow.ToString(CultureInfo.CurrentCulture)];
      if (skinFile != null)
      {
        if (type.Equals("useRandomGamesUserFanart", StringComparison.CurrentCulture))
          return skinFile.UseRandomGamesFanartUser;
        if (type.Equals("useRandomMoviesUserFanart", StringComparison.CurrentCulture))
          return skinFile.UseRandomMoviesFanartUser;
        if (type.Equals("useRandomMoviesScraperFanart", StringComparison.CurrentCulture))
          return skinFile.UseRandomMoviesFanartScraper;
        if (type.Equals("useRandomMovingPicturesFanart", StringComparison.CurrentCulture))
          return skinFile.UseRandomMovingPicturesFanart;
        if (type.Equals("useRandomMusicUserFanart", StringComparison.CurrentCulture))
          return skinFile.UseRandomMusicFanartUser;
        if (type.Equals("useRandomMusicScraperFanart", StringComparison.CurrentCulture))
          return skinFile.UseRandomMusicFanartScraper;
        if (type.Equals("useRandomPicturesUserFanart", StringComparison.CurrentCulture))
          return skinFile.UseRandomPicturesFanartUser;
        if (type.Equals("useRandomScoreCenterUserFanart", StringComparison.CurrentCulture))
          return skinFile.UseRandomScoreCenterFanartUser;
        if (type.Equals("useRandomTVSeriesFanart", StringComparison.CurrentCulture))
          return skinFile.UseRandomTVSeriesFanart;
        if (type.Equals("useRandomTVUserFanart", StringComparison.CurrentCulture))
          return skinFile.UseRandomTVFanartUser;
        if (type.Equals("useRandomPluginsUserFanart", StringComparison.CurrentCulture))
          return skinFile.UseRandomPluginsFanartUser;
      }
      return "False";
    }

    public bool IsPropertyRandomPerm(string value)
    {
      try
      {
        if (value == null)
          value = string.Empty;
        foreach (DictionaryEntry dictionaryEntry in PropertiesRandomPerm)
        {
          if (dictionaryEntry.Value.ToString().Equals(value))
            return true;
        }
      }
      catch (Exception ex)
      {
        logger.Error("IsPropertyRandomPerm: " + ex);
      }
      return false;
    }

    public string GetRandomFilename(ref string prevImage, Utils.Category category, ref int iFilePrev)
    {
      var str = string.Empty;
      try
      {
        if (!Utils.GetIsStopping())
        {
          str = prevImage;
          htAny = Utils.GetDbm().GetAnyFanart(category);
          if (htAny != null)
          {
            if (htAny.Count > 0)
            {
              var values1 = htAny.Values;
              var num1 = 0;
              var num2 = 0;
              foreach (FanartImage fanartImage in values1)
              {
                if ((num1 > iFilePrev || iFilePrev == -1) && 
                    (num2 == 0 && Utils.CheckImageResolution(fanartImage.DiskImage, category, Utils.UseAspectRatio)) && 
                    Utils.IsFileValid(fanartImage.DiskImage))
                {
                  str = fanartImage.DiskImage;
                  iFilePrev = num1;
                  prevImage = fanartImage.DiskImage;
                  num2 = 1;
                  if (CountSetVisibility == 0)
                  {
                    CountSetVisibility = 1;
                    break;
                  }
                  else
                    break;
                }
                else
                  checked { ++num1; }
              }

              if (num2 == 0)
              {
                var values2 = htAny.Values;
                iFilePrev = -1;
                var num3 = 0;
                var num4 = 0;
                foreach (FanartImage fanartImage in values2)
                {
                  if ((num3 > iFilePrev || iFilePrev == -1) && 
                      (num4 == 0 && Utils.CheckImageResolution(fanartImage.DiskImage, category, Utils.UseAspectRatio)) && 
                      Utils.IsFileValid(fanartImage.DiskImage))
                  {
                    str = fanartImage.DiskImage;
                    iFilePrev = num3;
                    prevImage = fanartImage.DiskImage;
                    if (CountSetVisibility == 0)
                    {
                      CountSetVisibility = 1;
                      break;
                    }
                    else
                      break;
                  }
                  else
                    checked { ++num3; }
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.Error("GetRandomFilename: " + ex);
      }
      return str;
    }

    public void ResetCurrCountRandom()
    {
      CurrCountRandom = 0;
      UpdateVisibilityCountRandom = 1;
    }

    private void IncreaseCurrCountRandom()
    {
      CurrCountRandom = checked (CurrCountRandom + 1);
    }

    public int GetPropertiesRandom()
    {
      if (PropertiesRandom == null)
        return 0;
      else
        return PropertiesRandom.Count;
    }

    public void ClearPropertiesRandom()
    {
      if (PropertiesRandom == null)
        return;
      PropertiesRandom.Clear();
    }

    public void ClearPropertiesRandomPerm()
    {
      if (PropertiesRandomPerm == null)
        return;
      PropertiesRandomPerm.Clear();
    }

    private void AddPropertyRandom(string property, string value, ref ArrayList al, bool doPerm)
    {
      try
      {
        if (string.IsNullOrEmpty(value))
          value = string.Empty;

        if (PropertiesRandom.Contains(property))
          PropertiesRandom[property] = value;
        else
          PropertiesRandom.Add(property, value);

        if (doPerm)
        {
          var str = property.Substring(0, checked (property.IndexOf(".any") - 1));
          if (propertiesRandomPerm.Contains(str))
            propertiesRandomPerm[str] = value;
          else
            propertiesRandomPerm.Add(str, value);
        }

        FanartHandlerSetup.Fh.AddPictureToCache(property, value, ref al);
        /*
        if (value == null || value.Length <= 0 || al == null)
          return;

        if (al.Contains(value))
          return;

        try
        {
          al.Add(value);
        }
        catch (Exception ex)
        {
          logger.Error("AddPropertyRandom: " + ex);
        }

        Utils.LoadImage(value);
        */
      }
      catch (Exception ex)
      {
        logger.Error("AddPropertyRandom: " + ex);
      }
    }

    public void UpdatePropertiesRandom()
    {
      try
      {
        var hashtable = new Hashtable();
        var index = 0;
        foreach (DictionaryEntry dictionaryEntry in PropertiesRandom)
        {
          Utils.SetProperty(dictionaryEntry.Key.ToString(), dictionaryEntry.Value.ToString());
          hashtable.Add(index, dictionaryEntry.Key.ToString());
          checked { ++index; }
        }
        index = 0;
        while (index < hashtable.Count)
        {
          PropertiesRandom.Remove(hashtable[index].ToString());
          checked { ++index; }
        }
        if (hashtable != null)
          hashtable.Clear();
      }
      catch (Exception ex)
      {
        logger.Error("UpdatePropertiesRandom: " + ex);
      }
    }

    private string GetNodeValue(XPathNodeIterator myXPathNodeIterator)
    {
      if (myXPathNodeIterator.Count > 0)
      {
        myXPathNodeIterator.MoveNext();
        if (myXPathNodeIterator.Current != null)
          return myXPathNodeIterator.Current.Value;
      }
      return string.Empty;
    }

    public void ShowImageOneRandom(int windowId)
    {
      GUIControl.ShowControl(windowId, 91919297);
      GUIControl.HideControl(windowId, 91919298);
      DoShowImageOneRandom = false ;
    }

    public void ShowImageTwoRandom(int windowId)
    {
      GUIControl.ShowControl(windowId, 91919298);
      GUIControl.HideControl(windowId, 91919297);
      DoShowImageOneRandom = true ;
    }

    public class SkinFile
    {
      public string UseRandomGamesFanartUser;
      public string UseRandomMoviesFanartScraper;
      public string UseRandomMoviesFanartUser;
      public string UseRandomMovingPicturesFanart;
      public string UseRandomMusicFanartScraper;
      public string UseRandomMusicFanartUser;
      public string UseRandomPicturesFanartUser;
      public string UseRandomPluginsFanartUser;
      public string UseRandomScoreCenterFanartUser;
      public string UseRandomTVFanartUser;
      public string UseRandomTVSeriesFanart;
    }
  }
}

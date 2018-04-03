// Type: FanartHandler.Grabbers
// Assembly: FanartHandler, Version=4.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B

extern alias FHNLog;

using CSScriptLibrary;

using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
using MediaPortal.Video.Database;

using FHNLog.NLog;

using System;
using System.IO;

namespace FanartHandler
{
  public class Grabbers
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static string ScriptDirectory = Config.GetSubFolder(Config.Dir.Config, @"FanartHandler\Scripts\");
    public const string Default_Language = "EN";
    public const string Awards_Script = "Awards";

    #region Movies

    public class Movies
    {
      #region Awards Grabber

      public class AwardsGrabbers
      {
        private static IAwardsGrabber _awardsGrabber;
        private static bool _awardsGrabberLoaded;
        private static AsmHelper _asmHelper;

        public static bool AwardsGrabberLoaded()
        {
          return AwardsGrabber != null;
        }

        public static void ResetGrabber()
        {
          if (_asmHelper != null)
          {
            _asmHelper.Dispose();
            _asmHelper = null;
          }

          if (_awardsGrabber != null)
          {
            _awardsGrabber.SafeDispose();
            _awardsGrabber = null;
          }

          _awardsGrabberLoaded = false;
        }

        public static IAwardsGrabber AwardsGrabber
        {
          get
          {
            if (!_awardsGrabberLoaded)
            {
              if (!LoadScript())
              {
                AwardsGrabber = null;
              }
              _awardsGrabberLoaded = true;
            }
            return _awardsGrabber;
          }
          set { _awardsGrabber = value; }
        }

        private static bool LoadScript()
        {
          string localLanguage = Utils.GetLang().ToUpper();
          string scriptFileName = ScriptDirectory + Awards_Script + "_" + localLanguage + ".csscript";
          if (!File.Exists(scriptFileName))
          {
            scriptFileName = ScriptDirectory + Awards_Script + "_" + Default_Language + ".csscript";
          }

          if (!File.Exists(scriptFileName))
          {
            logger.Error("Grabbers LoadScript(): [{1}:{2}] Awards grabber script not found: {0}", scriptFileName, Default_Language, localLanguage);
            return false;
          }

          try
          {
            Environment.CurrentDirectory = Config.GetFolder(Config.Dir.Base);
            _asmHelper = new AsmHelper(CSScript.Load(scriptFileName, null, false));
            _awardsGrabber = (IAwardsGrabber)_asmHelper.CreateObject("AwardsGrabber");
            logger.Debug("Grabbers LoadScript(): Awards grabber: {0}: {1} loaded...", _awardsGrabber.GetName(), scriptFileName);
          }
          catch (Exception ex)
          {
            logger.Error("Grabbers LoadScript(): Awards file: {0}, message : {1}", scriptFileName, ex.Message);
            return false;
          }
          return true;
        }
      }

      #endregion

      #region Awards Grabber Interface

      public interface IAwardsGrabber
      {
        // Name of Grabber
        string GetName();

        // Movies Awards
        string GetMovieAwards(string imdbid, string tmdbid, string localid);
      }

      #endregion
    }

    #endregion
  }

  public class GrabbersLog
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static void Info(string format, params object[] arg)
    {
      logger.Info(format, arg);   
    }

    public static void Error(Exception ex)
    {
      logger.Error(ex);
    }

    public static void Error(string format, params object[] arg)
    {
      logger.Error(format, arg);   
    }

    public static void Warn(string format, params object[] arg)
    {
      logger.Warn(format, arg);   
    }

    public static void Debug(string format, params object[] arg)
    {
      logger.Debug(format, arg);   
    }
  }
}
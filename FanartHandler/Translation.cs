using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;

using MediaPortal.Configuration;
using MediaPortal.GUI.Library;

using NLog;

namespace FanartHandler
{
  internal class Translation
  {
    #region Private variables

    private static Logger logger = LogManager.GetCurrentClassLogger();

    private static Dictionary<string, string> _translations;
    private static Dictionary<string, string> DynamicTranslations = new Dictionary<string, string>();
    private static readonly string _path = string.Empty;
    private static readonly DateTimeFormatInfo _info;

    #endregion

    #region Constructor

    static Translation()
    {
      _info = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentUICulture);
      _path = Config.GetSubFolder(Config.Dir.Language, "FanartHandler");
    }

    #endregion

    #region Public Properties

    public static Dictionary<string, string> FixedTranslations = new Dictionary<string, string>();

    /// <summary>
    /// Gets the translated strings collection in the active language
    /// </summary>
    public static Dictionary<string, string> Strings
    {
      get
      {
        if (_translations == null)
        {
          _translations = new Dictionary<string, string>();
          Type transType = typeof (Translation);
          var fields = transType.GetFields(BindingFlags.Public | BindingFlags.Static).Where(p => p.FieldType == typeof (string));

          foreach (var field in fields)
          {
            if (DynamicTranslations.ContainsKey(field.Name))
            {
              if (field.GetValue(transType).ToString() != string.Empty)
                _translations.Add(field.Name + ":" + DynamicTranslations[field.Name],
                  field.GetValue(transType).ToString());
            }
            else
            {
              if (field.GetValue(transType).ToString() != string.Empty)
                _translations.Add(field.Name, field.GetValue(transType).ToString());
            }
          }
        }
        return _translations;
      }
    }

    #endregion

    #region Public Methods

    public static void Init()
    {
      // reset active translations
      _translations = null;
      FixedTranslations.Clear();

      string lang = string.Empty;
      try
      {
        lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
      }
      catch (Exception)
      {
        lang = CultureInfo.CurrentUICulture.Name;
      }


      if (!System.IO.Directory.Exists(_path))
        System.IO.Directory.CreateDirectory(_path);

      LoadTranslations(lang);
    }

    public static int LoadTranslations(string lang)
    {
      XmlDocument doc = new XmlDocument();
      Dictionary<string, string> TranslatedStrings = new Dictionary<string, string>();
      string langPath = "";

      try
      {
        langPath = Path.Combine(_path, lang + ".xml");
        logger.Debug(string.Format("FanartHandler Translation: Try load Translation file {0}.", langPath));
        doc.Load(langPath);
        logger.Info( string.Format("FanartHandler Translation: Translation file loaded {0}.", langPath));
      }
      catch (Exception e)
      {
        if (lang == "en")
          return 0; // otherwise we are in an endless loop!

        if (e.GetType() == typeof (FileNotFoundException))
        {
          Log.Info( string.Format("FanartHandler Translation: Cannot find translation file {0}.  Failing back to English", langPath));
          logger.Info( string.Format("FanartHandler Translation: Cannot find translation file {0}.  Failing back to English", langPath));
        }
        else
        {
          Log.Info(string.Format("FanartHandler Translation: Error in translation xml file: {0}. Failing back to English", lang));
          Log.Info("FanartHandler Translation:" + e.ToString());
          logger.Info(string.Format("FanartHandler Translation: Error in translation xml file: {0}. Failing back to English", lang));
          logger.Info("FanartHandler Translation:" + e.ToString());
        }

        return LoadTranslations("en");
      }

      foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes)
      {
        if (stringEntry.NodeType == XmlNodeType.Element)
          try
          {
            if (stringEntry.Attributes.GetNamedItem("Field").Value.StartsWith("#"))
            {
              FixedTranslations.Add(stringEntry.Attributes.GetNamedItem("Field").Value, stringEntry.InnerText);
            }
            else
              TranslatedStrings.Add(stringEntry.Attributes.GetNamedItem("Field").Value, stringEntry.InnerText);
          }
          catch (Exception ex)
          {
            Log.Error("FanartHandler Translation: Error in Translation Engine");
            Log.Error("FanartHandler Translation:" + ex.ToString());
            logger.Error("FanartHandler Translation: Error in Translation Engine");
            logger.Error("FanartHandler Translation:" + ex.ToString());
          }
      }

      Type TransType = typeof (Translation);
      var fieldInfos = TransType.GetFields(BindingFlags.Public | BindingFlags.Static).Where(p => p.FieldType == typeof (string));

      foreach (var fi in fieldInfos)
      {
        if (TranslatedStrings != null && TranslatedStrings.ContainsKey(fi.Name))
          TransType.InvokeMember(fi.Name, BindingFlags.SetField, null, TransType,
            new object[] {TranslatedStrings[fi.Name]});
        else
        {
          // There is no hard-coded translation so create one
          Log.Info(string.Format("FanartHandler Translation: Translation not found for field: {0}.  Using hard-coded English default.", fi.Name));
          logger.Info(string.Format("FanartHandler Translation: Translation not found for field: {0}.  Using hard-coded English default.", fi.Name));
        }
      }
      return TranslatedStrings.Count;
    }

    public static string GetByName(string name)
    {
      if (!Strings.ContainsKey(name))
        return name;

      return Strings[name];
    }

    public static string GetByName(string name, params object[] args)
    {
      return String.Format(GetByName(name), args);
    }

    /// <summary>
    /// Takes an input string and replaces all ${named} variables with the proper translation if available
    /// </summary>
    /// <param name="input">a string containing ${named} variables that represent the translation keys</param>
    /// <returns>translated input string</returns>
    public static string ParseString(string input)
    {
      Regex replacements = new Regex(@"\$\{([^\}]+)\}");
      MatchCollection matches = replacements.Matches(input);
      foreach (Match match in matches)
      {
        input = input.Replace(match.Value, GetByName(match.Groups[1].Value));
      }
      return input;
    }

    #endregion

    #region Translations / Strings

    /// <summary>
    /// These will be loaded with the language files content
    /// if the selected lang file is not found, it will first try to load en(us).xml as a backup
    /// if that also fails it will use the hardcoded strings as a last resort.
    /// </summary>
       
    // Scrape
    public static string ScrapeInitial = "Initial Scrape";
    public static string ScrapeNowPlaying = "Now Playing Scrape";
    public static string ScrapeInitializing = "Initializing";

    // CleanUp
    public static string CleanupImages = "Cleanup images";

    // All
    public static string FHArtists = "Artists";
    public static string FHAlbums = "Albums";
    public static string FHVideos = "Videos";

    // Settings
    // public static string PrefsDymmy = "Dymmy";
    #endregion
  }
}

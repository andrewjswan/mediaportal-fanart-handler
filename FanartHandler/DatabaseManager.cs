// Type: FanartHandler.DatabaseManager
// Assembly: FanartHandler, Version=3.1.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 073E8D78-B6AE-4F86-BDE9-3E09A337833B
// Assembly location: D:\Mes documents\Desktop\FanartHandler.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Music.Database;
using NLog;
using SQLite.NET;

namespace FanartHandler
{
    internal class DatabaseManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly object lockObject = new object();
        private readonly Random randNumber = new Random();
        private const string dbFilename = "FanartHandler.db3";
        // private const string dbFilenameOrg = "FanartHandler.org";
        private SQLiteClient dbClient;
        private Hashtable htAnyFanart;
        private MusicDatabase m_db;
        private ArrayList musicDatabaseArtists;
        private List<AlbumInfo> musicDatabaseAlbums;
        private Scraper scraper;

        public bool IsScraping { get; set; }

        public double CurrArtistsBeingScraped { get; set; }

        public double TotArtistsBeingScraped { get; set; }

        public Hashtable HtAnyFanart
        {
            get { return htAnyFanart; }
            set { htAnyFanart = value; }
        }

        public bool StopScraper { get; set; }

        static DatabaseManager()
        {
        }

        public bool GetIsScraping()
        {
            return IsScraping;
        }

        #region DB
        public void InitDB(string type)
        {
            logger.Debug("initDB: Start: "+type);
            try
            {
                IsScraping = false;
                var file1 = Config.GetFile((Config.Dir) 4, dbFilename);
                var flag = false;
                // var file2 = Config.GetFile((Config.Dir) 4, dbFilenameOrg);
                // if (!File.Exists(file1))
                //    File.Copy(file2, file1);
                flag = (!File.Exists(file1));
                dbClient = new SQLiteClient(file1);
                dbClient.Execute("PRAGMA synchronous=OFF;");
                dbClient.Execute("PRAGMA encoding='UTF-8';");
                dbClient.Execute("PRAGMA cache_size=5000;");
                dbClient.Execute("PRAGMA temp_store = MEMORY;");
                if (flag)
                  CreateDBMain() ;

                m_db = MusicDatabase.Instance;
                logger.Info("Successfully Opened Database: "+dbFilename);
                UpgradeDBMain(type);
                if (HtAnyFanart != null)
                    return;
                HtAnyFanart = new Hashtable();
            }
            catch (Exception ex)
            {
                logger.Error("initDB: Could Not Open Database: "+dbFilename+". " + ex);
                dbClient = null;
            }
        }

        public void Close()
        {
            try
            {
                if (dbClient != null)
                {
                    dbClient.Close();
                    dbClient.Dispose();
                }
                dbClient = null;
            }
            catch (Exception ex)
            {
                logger.Error("close: " + ex);
            }
        }

        public void CreateDBMain()
        {
            try
            {
                var date = DateTime.Today.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
                logger.Info("Creating Database, version 3.2");
                lock (lockObject)
                    dbClient.Execute("CREATE TABLE [Image] ([Id] TEXT, "+
                                                           "[Category] TEXT, "+
                                                           "[Provider] TEXT, "+
                                                           "[Key1] TEXT, "+
                                                           "[Key2] TEXT, "+
                                                           "[FullPath] TEXT, "+
                                                           "[SourcePath] TEXT, "+
                                                           "[AvailableRandom] TEXT, "+
                                                           "[Enabled] TEXT, "+
                                                           "[DummyItem] TEXT, "+
                                                           "[MBID] TEXT, "+
                                                           "[Time_Stamp] TEXT, "+
                                                           "CONSTRAINT [iIdProvider] PRIMARY KEY ([Id], [Provider]) ON CONFLICT ROLLBACK);");
                lock (lockObject)
                    dbClient.Execute("CREATE TABLE Version (Id INTEGER PRIMARY KEY, Version TEXT, Time_Stamp TEXT);");
                logger.Info("Create tables [Step 1] - finished");

                lock (lockObject)
                    dbClient.Execute("CREATE INDEX [iCategory] ON [Image] ([Category]);");
                lock (lockObject)
                    dbClient.Execute("CREATE INDEX [iCategoryTimeStamp] ON [Image] ([Category], [Time_Stamp]);");
                lock (lockObject)
                    dbClient.Execute("CREATE INDEX [iEnabledAvailableRandomCategory] ON [Image] ([Enabled], [AvailableRandom], [Category]);");
                lock (lockObject)
                    dbClient.Execute("CREATE INDEX [iKey1CategoryDummyItem] ON [Image] ([Key1], [Category], [DummyItem]);");
                lock (lockObject)
                    dbClient.Execute("CREATE INDEX [iKey1Enabled] ON [Image] ([Key1], [Enabled]);");
                lock (lockObject)
                    dbClient.Execute("CREATE INDEX [iKey1EnabledCategory] ON [Image] ([Key1], [Enabled], [Category]);");
                lock (lockObject)
                    dbClient.Execute("CREATE INDEX [iKey1Key2Category] ON [Image] ([Key1], [Key2], [Category]);");
                lock (lockObject)
                    dbClient.Execute("CREATE INDEX [iMBID] ON [Image] ([MBID] COLLATE NOCASE);");
                lock (lockObject)
                    dbClient.Execute("CREATE INDEX [iKey1MBID] ON [Image] ([Key1], [MBID]);");
                lock (lockObject)
                    dbClient.Execute("CREATE INDEX [iKey1Key2MBID] ON [Image] ([Key1], [Key2], [MBID]);");
                logger.Info("Create indexes [Step 2] - finished");

                lock (lockObject)
                    dbClient.Execute("INSERT INTO Version (Version,Time_Stamp) VALUES ('3.2','"+date+"');");
                lock (lockObject)
                    dbClient.Execute("PRAGMA user_version=32;");
                logger.Info("Create database, version 3.2 - finished");
            }
            catch (Exception ex)
            {
                logger.Error("Error creating database:");
                logger.Error(ex.ToString());
                var num = (int) MessageBox.Show("Error creating database, please see [Fanart Handler Log] for details.","Error");
            }
        }

        public void UpgradeDBMain(string type)
        {
            if (type.Equals("upgrade", StringComparison.CurrentCulture))
              return;

            var DBVersion = string.Empty;
            try
            {
                var date = DateTime.Today.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
                SQLiteResultSet sqLiteResultSet;
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute("SELECT Version FROM Version;");
                var num = 0;
                while (num < sqLiteResultSet.Rows.Count)
                {
                    DBVersion = sqLiteResultSet.GetField(num, 0);
                    checked { ++num; }
                }
                if (DBVersion != null)
                    logger.Info("Database version is: " + DBVersion + " at database initiation");
                if (DBVersion != null && DBVersion.Equals("2.3", StringComparison.CurrentCulture))
                {
                    logger.Info("Upgrading Database to version 2.4");
                    lock (lockObject)
                        dbClient.Execute("DELETE FROM Timestamps WHERE Key LIKE 'Directory -%';");
                    logger.Info("Upgrading Step 1 - finished");

                    DBVersion = "2.4";
                    lock (lockObject)
                        dbClient.Execute("UPDATE Version SET Version = '"+DBVersion+"', Time_Stamp = '"+date+"';");
                    lock (lockObject)
                        dbClient.Execute("PRAGMA user_version="+DBVersion.Replace(".","")+";");
                    logger.Info("Upgraded Database to version "+DBVersion);
                }
                if (DBVersion != null && DBVersion.Equals("2.4", StringComparison.CurrentCulture))
                {
                    logger.Info("Upgrading Database to version 2.5");
                    lock (lockObject)
                        dbClient.Execute("DELETE FROM Timestamps WHERE Key LIKE 'Directory -%';");
                    logger.Info("Upgrading Step 1 - finished");

                    DBVersion = "2.5";
                    lock (lockObject)
                        dbClient.Execute("UPDATE Version SET Version = '"+DBVersion+"', Time_Stamp = '"+date+"';");
                    lock (lockObject)
                        dbClient.Execute("PRAGMA user_version="+DBVersion.Replace(".","")+";");
                    logger.Info("Upgraded Database to version "+DBVersion);
                }
                if (DBVersion != null && DBVersion.Equals("2.5", StringComparison.CurrentCulture))
                {
                    logger.Info("Upgrading Database to version 2.6");
                    lock (lockObject)
                        dbClient.Execute("DELETE FROM tvseries_fanart;");
                    logger.Info("Upgrading Step 1 - finished");
                    lock (lockObject)
                        dbClient.Execute("DELETE FROM Movie_Fanart;");
                    logger.Info("Upgrading Step 2 - finished");
                    lock (lockObject)
                        dbClient.Execute("DELETE FROM MovingPicture_Fanart;");
                    logger.Info("Upgrading Step 3 - finished");

                    DBVersion = "2.6";
                    lock (lockObject)
                        dbClient.Execute("UPDATE Version SET Version = '"+DBVersion+"', Time_Stamp = '"+date+"';");
                    lock (lockObject)
                        dbClient.Execute("PRAGMA user_version="+DBVersion.Replace(".","")+";");
                    logger.Info("Upgraded Database to version "+DBVersion);
                }
                if (DBVersion != null && DBVersion.Equals("2.6", StringComparison.CurrentCulture))
                {
                    logger.Info("Upgrading Database to version 2.7");
                    lock (lockObject)
                        dbClient.Execute("DELETE FROM Timestamps WHERE Key LIKE 'Directory Ext - %';");
                    logger.Info("Upgrading Step 1 - finished");

                    DBVersion = "2.7";
                    lock (lockObject)
                        dbClient.Execute("UPDATE Version SET Version = '"+DBVersion+"', Time_Stamp = '"+date+"';");
                    lock (lockObject)
                        dbClient.Execute("PRAGMA user_version="+DBVersion.Replace(".","")+";");
                    logger.Info("Upgraded Database to version "+DBVersion);
                }
                if (DBVersion != null && DBVersion.Equals("2.7", StringComparison.CurrentCulture))
                {
                    logger.Info("Upgrading Database to version 2.8");
                    lock (lockObject)
                        dbClient.Execute("UPDATE Music_Artist SET Successful_Scrape = 0 WHERE (Successful_Scrape is null or Successful_Scrape = '')");
                    logger.Info("Upgrading Step 1 - finished");
                    lock (lockObject)
                        dbClient.Execute("UPDATE Music_Artist SET successful_thumb_scrape = 0 WHERE (successful_thumb_scrape is null or successful_thumb_scrape = '')");
                    logger.Info("Upgrading Step 2 - finished");

                    DBVersion = "2.8";
                    lock (lockObject)
                        dbClient.Execute("UPDATE Version SET Version = '"+DBVersion+"', Time_Stamp = '"+date+"';");
                    lock (lockObject)
                        dbClient.Execute("PRAGMA user_version="+DBVersion.Replace(".","")+";");
                    logger.Info("Upgraded Database to version "+DBVersion);
                }
                if (DBVersion != null && DBVersion.Equals("2.8", StringComparison.CurrentCulture))
                {
                    logger.Info("Upgrading Database to version 2.9");
                    Close();
                    logger.Info("Upgrading Step 1 - finished");
                    var dbFile = Config.GetFile((Config.Dir) 4, dbFilename);
                    // var dbCleanFile = Config.GetFile((Config.Dir) 4, dbFilenameOrg);
                    if (File.Exists(dbFile))
                    {
                        var backupdate = DateTime.Today.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
                        File.Move(dbFile, dbFile + "_old_" + ((DBVersion != null) ? "v" + DBVersion + "_" : "") + backupdate);
                        logger.Info("Upgrading Step 2 - finished");
                        //File.Copy(dbCleanFile, dbFile);
                        //logger.Info("Upgrading Step 3 - finished");
                    }
                    var musicPath = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\Scraper\\music";
                    var backupPath = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\Scraper_Backup_" + date;
                    if (Directory.Exists(musicPath) && !Directory.Exists(backupPath))
                    {
                        Directory.Move(musicPath, backupPath);
                        logger.Info("Upgrading Step 3 - finished");
                    }
                    if (!Directory.Exists(musicPath))
                    {
                        Directory.CreateDirectory(musicPath);
                        logger.Info("Upgrading Step 4 - finished");
                    }
                    try
                    {
                        File.Copy(backupPath + "\\default.jpg", musicPath + "\\default.jpg");
                        logger.Info("Upgrading Step 5 - finished");
                    }
                    catch {  }
                    // Create New Empty DB ...
                    InitDB("upgrade");
                    logger.Info("Upgrading Step 6 - finished");
                    // Check for New DB Version ...
                    lock (lockObject)
                        sqLiteResultSet = dbClient.Execute("SELECT Version FROM Version;");
                    DBVersion = string.Empty;
                    num = 0;
                    while (num < sqLiteResultSet.Rows.Count)
                    {
                        DBVersion = sqLiteResultSet.GetField(num, 0);
                        checked { ++num; }
                    }
                    if (DBVersion != null && DBVersion.Equals("2.8", StringComparison.CurrentCulture))
                    {
                        DBVersion = "2.9";
                        lock (lockObject)
                            dbClient.Execute("UPDATE Version SET Version = '"+DBVersion+"', Time_Stamp = '"+date+"';");
                        lock (lockObject)
                            dbClient.Execute("PRAGMA user_version="+DBVersion.Replace(".","")+";");
                        logger.Info("Upgraded Database to version "+DBVersion);
                    }
                    else
                    {
                        logger.Info("Upgraded Database to version "+DBVersion);
                    }
                    logger.Debug("Upgrading Step 7 - Fill tables ...");
                    FanartHandlerSetup.Fh.UpdateDirectoryTimer("All", false, "All");
                    logger.Info("Upgrading Step 7 - finished");
                }
                if (DBVersion != null && DBVersion.Equals("2.9", StringComparison.CurrentCulture))
                {
                    logger.Info("Upgrading Database to version 3.0");
                    try
                    {
                        lock (lockObject)
                            dbClient.Execute("CREATE INDEX iKey1Key2Category ON Image (Key1,Key2, Category)");
                        logger.Info("Upgrading Step 1 - finished");
                    }
                    catch { }
                    try
                    {
                        lock (lockObject)
                            dbClient.Execute("CREATE INDEX iKey1CategoryDummyItem ON Image (Key1,Category,DummyItem)");
                        logger.Info("Upgrading Step 2 - finished");
                    }
                    catch { }
                    try
                    {
                        lock (lockObject)
                            dbClient.Execute("CREATE INDEX iCategoryTimeStamp ON Image (Category,Time_Stamp)");
                        logger.Info("Upgrading Step 3 - finished");
                    }
                    catch { }

                    DBVersion = "3.0";
                    lock (lockObject)
                        dbClient.Execute("UPDATE Version SET Version = '"+DBVersion+"', Time_Stamp = '"+date+"';");
                    lock (lockObject)
                        dbClient.Execute("PRAGMA user_version="+DBVersion.Replace(".","")+";");
                    logger.Info("Upgraded Database to version "+DBVersion);
                }
                if (DBVersion != null && DBVersion.Equals("3.0", StringComparison.CurrentCulture))
                {
                    logger.Info("Upgrading Database to version 3.1");
                    try
                    {
                        lock (lockObject)
                            dbClient.Execute("ALTER TABLE [Image] ADD COLUMN [MBID] TEXT;");
                        logger.Info("Upgrading Step 1 - finished");
                    }
                    catch { }
                    try
                    {
                        lock (lockObject)
                            dbClient.Execute("CREATE INDEX [iMBID] ON [Image] ([MBID] COLLATE NOCASE);");
                        logger.Info("Upgrading Step 2 - finished");
                    }
                    catch { }
                    
                    DBVersion = "3.1";
                    lock (lockObject)
                        dbClient.Execute("UPDATE Version SET Version = '"+DBVersion+"', Time_Stamp = '"+date+"';");
                    lock (lockObject)
                        dbClient.Execute("PRAGMA user_version="+DBVersion.Replace(".","")+";");
                    logger.Info("Upgraded Database to version "+DBVersion);
                }
                if (DBVersion != null && DBVersion.Equals("3.1", StringComparison.CurrentCulture))
                {
                    logger.Info("Upgrading Database to version 3.2");
                    try
                    {
                        logger.Debug("Delete Dummy items...");
                        lock (lockObject)
                            dbClient.Execute("DELETE FROM Image WHERE DummyItem = 'True';");

                        logger.Info("Upgrading [Step 1] - finished.");
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Delete Dummy items:");
                        logger.Error(ex);
                    }

                    try
                    {
                    lock (lockObject)
                        logger.Debug("Try to Delete Temp tables...");
                        dbClient.Execute("DROP TABLE ImageN;");
                        logger.Info("Upgrading [Step 2.1] - finished.");
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Delete Temp tables...");
                        logger.Error(ex);
                    }
                    logger.Debug("Create New Table...");
                    lock (lockObject)
                        dbClient.Execute("CREATE TABLE [ImageN] ([Id] TEXT, "+
                                                                "[Category] TEXT, "+
                                                                "[Provider] TEXT, "+
                                                                "[Key1] TEXT, "+
                                                                "[Key2] TEXT, "+
                                                                "[FullPath] TEXT, "+
                                                                "[SourcePath] TEXT, "+
                                                                "[AvailableRandom] TEXT, "+
                                                                "[Enabled] TEXT, "+
                                                                "[DummyItem] TEXT, "+
                                                                "[MBID] TEXT, "+
                                                                "[Time_Stamp] TEXT, "+
                                                                "CONSTRAINT [iIdProvider] PRIMARY KEY ([Id], [Provider]) ON CONFLICT ROLLBACK);");
                    logger.Info("Create tables [Step 2.All] - finished");

                    logger.Debug("Create Indexes...");
                    try {
                    lock (lockObject)
                        dbClient.Execute("CREATE INDEX [iCategory] ON [ImageN] ([Category]);");
                    logger.Debug("Create Indexes [Step 3.1] - finished");
                    } catch (Exception ex) {logger.Error(ex);}
                    try {
                    lock (lockObject)
                        dbClient.Execute("CREATE INDEX [iCategoryTimeStamp] ON [ImageN] ([Category], [Time_Stamp]);");
                    logger.Debug("Create Indexes [Step 3.2] - finished");
                    } catch (Exception ex) {logger.Error(ex);}
                    try {
                    lock (lockObject)
                        dbClient.Execute("CREATE INDEX [iEnabledAvailableRandomCategory] ON [ImageN] ([Enabled], [AvailableRandom], [Category]);");
                    logger.Debug("Create Indexes [Step 3.3] - finished");
                    } catch (Exception ex) {logger.Error(ex);}
                    try {
                    lock (lockObject)
                        dbClient.Execute("CREATE INDEX [iKey1CategoryDummyItem] ON [ImageN] ([Key1], [Category], [DummyItem]);");
                    logger.Debug("Create Indexes [Step 3.4] - finished");
                    } catch (Exception ex) {logger.Error(ex);}
                    try {
                    lock (lockObject)
                        dbClient.Execute("CREATE INDEX [iKey1Enabled] ON [ImageN] ([Key1], [Enabled]);");
                    logger.Debug("Create Indexes [Step 3.5] - finished");
                    } catch (Exception ex) {logger.Error(ex);}
                    try {
                    lock (lockObject)
                        dbClient.Execute("CREATE INDEX [iKey1EnabledCategory] ON [ImageN] ([Key1], [Enabled], [Category]);");
                    logger.Debug("Create Indexes [Step 3.6] - finished");
                    } catch (Exception ex) {logger.Error(ex);}
                    try {
                    lock (lockObject)
                        dbClient.Execute("CREATE INDEX [iKey1Key2Category] ON [ImageN] ([Key1], [Key2], [Category]);");
                    logger.Debug("Create Indexes [Step 3.7] - finished");
                    } catch (Exception ex) {logger.Error(ex);}
                    try {
                    lock (lockObject)
                        dbClient.Execute("CREATE INDEX [iMBID] ON [ImageN] ([MBID] COLLATE NOCASE);");
                    logger.Debug("Create Indexes [Step 3.8] - finished");
                    } catch (Exception ex) {logger.Error(ex);}
                    try {
                    lock (lockObject)
                        dbClient.Execute("CREATE INDEX [iKey1MBID] ON [ImageN] ([Key1], [MBID]);");
                    logger.Debug("Create Indexes [Step 3.9] - finished");
                    } catch (Exception ex) {logger.Error(ex);}
                    try {
                    lock (lockObject)
                        dbClient.Execute("CREATE INDEX [iKey1Key2MBID] ON [ImageN] ([Key1], [Key2], [MBID]);");
                    logger.Debug("Upgrading Indexes [Step 3.10] - finished");
                    } catch (Exception ex) {logger.Error(ex);}
                    logger.Info("Upgrading Indexes [Step 3.All] - finished");

                    logger.Debug("Transfer Data to New table...");
                    lock (lockObject)
                        dbClient.Execute("INSERT INTO [ImageN] SELECT * FROM [Image];");
                    logger.Info("Upgrading [Step 4] - finished.");

                    logger.Debug("Rename and Drop Tables...");
                    lock (lockObject)
                        dbClient.Execute("DROP TABLE Image;");
                    lock (lockObject)
                        dbClient.Execute("ALTER TABLE ImageN RENAME TO Image;");
                    logger.Info("Upgrading [Step 5] - finished.");

                    DBVersion = "3.2";
                    lock (lockObject)
                        dbClient.Execute("UPDATE Version SET Version = '"+DBVersion+"', Time_Stamp = '"+date+"';");
                    lock (lockObject)
                        dbClient.Execute("PRAGMA user_version="+DBVersion.Replace(".","")+";");
                    logger.Info("Upgraded Database to version "+DBVersion);
                }
                logger.Info("Database version is verified: " + DBVersion);
            }
            catch (Exception ex)
            {
                logger.Error("Error upgrading database:");
                logger.Error(ex.ToString());
                var num = (int) MessageBox.Show("Error upgrading database, please see [Fanart Handler Log] for details.","Error");
            }
        }
        #endregion

        public bool ArtistAlbumScrape(string artist, string album)
        {
            try
            {
                logger.Info("ArtistAlbumScrape is starting for Artist: " + artist + ", Album: " + album + ".");
                if (DoScrapeNew(artist, album, true) > 0)
                {
                    logger.Info("ArtistAlbumScrape is done.");
                    return true;
                }
                else
                {
                    logger.Info("ArtistAlbumScrape is done.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("ArtistAlbumScrape: " + ex);
                return false;
            }
        }

        public bool NowPlayingScrape(string artist, string album)
        {
            try
            {
                logger.Info("NowPlayingScrape is starting for Artist: " + artist + ".");
                TotArtistsBeingScraped = 2.0;
                CurrArtistsBeingScraped = 0.0;
                var flag = false;
                if (artist.Contains("|"))
                {
                    var str1 = artist;
                    var chArray = new char[1]
                    {
                        '|'
                    };
                    foreach (var str2 in str1.Split(chArray))
                    {
                        if (!StopScraper) {
                            if (DoScrapeNew(str2.Trim(), album, false) > 0)
                                flag = true;
                        }
                        else
                            break;
                    }
                    logger.Info("NowPlayingScrape is done.");
                    return flag;
                }
                else if (DoScrapeNew(artist, album, false) > 0)
                {
                    logger.Info("NowPlayingScrape is done.");
                    return true;
                }
                else
                {
                    logger.Info("NowPlayingScrape is done.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("NowPlayingScrape: " + ex);
                return false;
            }
        }

        public int GetTotalArtistsInMPMusicDatabase()
        {
            var arrayList = new ArrayList();
            m_db.GetAllArtists(ref arrayList);
            return arrayList.Count;
        }

        public int GetNumberOfFanartImages(string artist)
        {
            try
            {
                var str = "SELECT count(Key1) "+
                           "FROM Image "+
                           "WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND "+
                                 "Category = '" + Utils.PatchSql(((object) Utils.Category.MusicFanartScraped).ToString()) + "' AND "+
                                 "DummyItem = 'False';";
                SQLiteResultSet sqLiteResultSet;
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute(str);
                return int.Parse(sqLiteResultSet.GetField(0, 0), CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                logger.Error("GetNumberOfFanartImages: " + ex);
            }
            return 0;
        }

        public bool HasArtistThumb(string artist)
        {
            var flag = false;
            try
            {
                var str = "SELECT count(Key1) "+
                           "FROM Image "+
                           "WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND "+
                                 "Category = '" + Utils.PatchSql(((object) Utils.Category.MusicArtistThumbScraped).ToString()) + "' AND "+
                                 "DummyItem = 'False';";
                SQLiteResultSet sqLiteResultSet;
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute(str);
                if (int.Parse(sqLiteResultSet.GetField(0, 0), CultureInfo.CurrentCulture) > 0)
                    flag = true;
            }
            catch (Exception ex)
            {
                logger.Error("HasArtistThumb: " + ex);
            }
            return flag;
        }

        public bool HasAlbumThumb(string artist, string album)
        {
            var flag = false;
            try
            {
                var str = "SELECT count(Key1) "+
                           "FROM Image "+
                           "WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND "+
                                 "Key2 = '" + Utils.PatchSql(album) + "' AND "+
                                 "Category = '" + Utils.PatchSql(((object) Utils.Category.MusicAlbumThumbScraped).ToString()) + "' AND "+
                                 "DummyItem = 'False';";
                SQLiteResultSet sqLiteResultSet;
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute(str);
                if (int.Parse(sqLiteResultSet.GetField(0, 0), CultureInfo.CurrentCulture) > 0)
                    flag = true;
            }
            catch (Exception ex)
            {
                logger.Error("HasAlbumThumb: " + ex);
            }
            return flag;
        }

        public SQLiteResultSet GetThumbImages(Utils.Category[] category, int start)
        {
            var arrayList = new ArrayList();
            try
            {
                var str1 = string.Empty;
                var str2 = string.Empty;
                var str3 = string.Empty;
                var index = 0;
                while (index < category.Length)
                {
                    if (str3.Length > 0)
                        str3 = string.Concat(new object[4]
                                            {
                                                str3,
                                                ",'",
                                                category[index],
                                                "'"
                                            });
                    else
                        str3 = "'" + category[index] + "'";
                    checked
                    {
                        ++index;
                    }
                }
                var str4 = "SELECT FullPath, AvailableRandom, Category, Key1 "+
                            "FROM image "+
                            "WHERE Category IN (" +
                //                   (object) str3 + ") AND DummyItem = 'False' order by Key1, FullPath LIMIT " + start + ",500;";
                                                    (object) str3 + ") AND "+
                                  "DummyItem = 'False' "+
                            "ORDER BY Key1, FullPath;";
                SQLiteResultSet sqLiteResultSet;
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute(str4);
                return sqLiteResultSet;
            }
            catch (Exception ex)
            {
                logger.Error("GetThumbImages: " + ex);
            }
            return null;
        }

        public string IsImageProtectedByUser(string diskImage)
        {
            var str1 = "False";
            try
            {
                var str2 = "SELECT AvailableRandom FROM Image WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
                SQLiteResultSet sqLiteResultSet;
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute(str2);
                var num = 0;
                while (num < sqLiteResultSet.Rows.Count)
                {
                    str1 = sqLiteResultSet.GetField(num, 0);
                    checked
                    {
                        ++num;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("IsImageProtectedByUser: " + ex);
            }
            return str1;
        }

        public void SetImageProtectedByUser(string diskImage, bool protect)
        {
            try
            {
                var str1 = string.Empty;
                var str2 = !protect
                    ? "UPDATE Image Set AvailableRandom = 'False' WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';"
                    : "UPDATE Image Set AvailableRandom = 'True' WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
                lock (lockObject)
                    dbClient.Execute(str2);
            }
            catch (Exception ex)
            {
                logger.Error("SetImageProtectedByUser: " + ex);
            }
        }

        public int DeleteRecordsWhereFileIsMissing()
        {
            var num1 = 0;
            try
            {
                SQLiteResultSet sqLiteResultSet;
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute("SELECT FullPath FROM Image WHERE DummyItem = 'False';");
                var num2 = 0;
                while (num2 < sqLiteResultSet.Rows.Count)
                {
                    var field = sqLiteResultSet.GetField(num2, 0);
                    if (!File.Exists(field))
                    {
                        DeleteImage(field);
                        checked
                        {
                            ++num1;
                        }
                    }
                    checked
                    {
                        ++num2;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("DeleteRecordsWhereFileIsMissing: " + ex);
            }
            return num1;
        }

        public int DoScrape(string artist, Hashtable htFanart)
        {
            if (!StopScraper)
            {
                Utils.AllocateDelayStop("FanartHandlerSetup-StartScraper");
                try
                {
                    var artist1 = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
                    scraper = new Scraper();
                    var num1 = 0;
                    if (artist != null && artist.Trim().Length > 0)
                    {
                        lock (lockObject)
                            dbClient.Execute("BEGIN TRANSACTION;");
                        var num2 = int.Parse(Utils.GetScraperMaxImages());
                        var num3 = 0;
                        if (htFanart.Contains(Utils.PatchSql(artist1)))
                            num3 = int.Parse((string) htFanart[Utils.PatchSql(artist1)],CultureInfo.CurrentCulture);
                        if (num3 < 1)
                        {
                            var numberOfFanartImages = GetNumberOfFanartImages(artist1);
                            // DeleteDummyItem(artist1, Utils.Category.MusicFanartScraped);
                            var iMax = checked (Convert.ToInt32(Utils.GetScraperMaxImages(),CultureInfo.CurrentCulture) - numberOfFanartImages);
                            var doScrapeFanart = false;
                            if (numberOfFanartImages < num2)
                                doScrapeFanart = true;
                            num1 = scraper.GetArtistFanart(artist, iMax, this, true, false, false, doScrapeFanart);
                            if (num1 == 0)
                            {
                                logger.Info("No fanart found for Artist: " + artist + ".");
                                // InsertDummyItem(artist1, Utils.Category.MusicFanartScraped, null, null);
                            }
                            if (num1 == 8888 && doScrapeFanart)
                                logger.Info("Artist: " + artist + " has already maximum number of images. Will not download anymore images for this artist.");
                        }
                        lock (lockObject)
                            dbClient.Execute("COMMIT;");
                    }
                    scraper = null;
                    return num1;
                }
                catch (Exception ex)
                {
                    logger.Error("doScrape: " + ex);
                    lock (lockObject)
                        dbClient.Execute("ROLLBACK;");
                }
            }
            return 0;
        }

        /*
        public void InsertDummyItem(string artist, Utils.Category category, string album, string mbid)
        {
            try
            {
                var str1 = string.Empty;
                var str2 = string.Empty;
                var s = "";
                if (category == Utils.Category.MusicArtistThumbScraped) {
                    s = Config.GetFolder((Config.Dir) 6) + "\\Music\\Artists" + "\\" +
                        MediaPortal.Util.Utils.MakeFileName(artist) + ".jpg";
                }
                else if (category == Utils.Category.MusicAlbumThumbScraped) {
                    str1 = Config.GetFolder((Config.Dir) 6) + "\\Music\\Albums";
                    var albumThumbName = MediaPortal.Util.Utils.GetAlbumThumbName(artist, album);
                    if (albumThumbName.IndexOf(".jpg") > 0) {
                        s = albumThumbName.Substring(0, albumThumbName.IndexOf(".jpg")) + ".jpg";
                    }
                    else {
                        return;
                    }
                }
                else {
                    s = Config.GetFolder((Config.Dir) 6) + "\\Skin FanArt\\Scraper\\music" + "\\" +
                        MediaPortal.Util.Utils.MakeFileName(artist) + " (" + randNumber.Next(10000, 99999) + ").jpg";
                }
                var str3 = "DELETE FROM Image WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND Key2 = '" +
                                                    Utils.PatchSql(album) + "' AND Category = '" + Utils.PatchSql((category).ToString()) +
                                                    "' AND DummyItem = 'True';";
                lock (lockObject)
                    dbClient.Execute(str3);
                if (DatabaseUtility.GetAsInt(dbClient.Execute("SELECT COUNT(Key1) FROM Image WHERE Key1 = '" + Utils.PatchSql(artist) +
                                                                      "' AND Key2 = '" + Utils.PatchSql(album) + "' AND Category = '" +
                                             Utils.PatchSql((category).ToString()) + "';"), 0, 0) > 0)
                    return;
                var now = DateTime.Now;
                var str4 = "INSERT INTO Image (Id, Category, Provider, Key1, Key2, FullPath, SourcePath, AvailableRandom, Enabled, DummyItem, Time_Stamp, MBID) VALUES('" +
                                               Utils.PatchSql(s) + "', '" + Utils.PatchSql((category).ToString()) + "','" +
                                               Utils.PatchSql((Utils.Provider.Dummy).ToString()) + "','" + Utils.PatchSql(artist) + "','" +
                                               Utils.PatchSql(album) + "',null, null, 'True', 'True', 'True', '" +
                                               now.ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "','"+Utils.PatchSql(mbid)+"');";
                lock (lockObject)
                    dbClient.Execute(str4);
            }
            catch (Exception ex)
            {
                logger.Error("InsertDummyItem: " + ex);
            }
        }

        public void DeleteDummyItem(string artist, Utils.Category category)
        {
            try
            {
                var str = "DELETE FROM Image WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND Category = '" +
                                                   Utils.PatchSql(((object) category).ToString()) + "' AND DummyItem = 'True';";
                lock (lockObject)
                    dbClient.Execute(str);
            }
            catch (Exception ex)
            {
                logger.Error("DeleteDummyItem: " + ex);
            }
        }
        */

        public int doScrapeThumbs(string artist, bool onlyMissing)
        {
            if (!StopScraper)
            {
                Utils.AllocateDelayStop("FanartHandlerSetup-StartScraper");
                try
                {
                    var artist1 = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
                    scraper = new Scraper();
                    var num = 0;
                    lock (lockObject)
                        dbClient.Execute("BEGIN TRANSACTION;");
                    if (artist != null && artist.Trim().Length > 0)
                    {
                        var str = "SELECT count(Key1) "+
                                   "FROM Image "+
                                   "WHERE Key1 = '" + Utils.PatchSql(artist1) + "' AND "+
                                         "Enabled = 'True' AND "+
                                         "Category in (" + Utils.GetMusicArtistCategoriesInStatement() + ") AND "+
                                         "DummyItem = 'False';";
                        SQLiteResultSet sqLiteResultSet;
                        lock (lockObject)
                            sqLiteResultSet = dbClient.Execute(str);
                        if (int.Parse(sqLiteResultSet.GetField(0, 0), CultureInfo.CurrentCulture) < 1 || !onlyMissing)
                        {
                            num = scraper.GetArtistThumbs(artist, this, onlyMissing);
                            if (num == 0)
                                logger.Info("No fanart found for artist " + artist + ".");
                        }
                        scraper = null;
                    }
                    lock (lockObject)
                        dbClient.Execute("COMMIT;");
                    return num;
                }
                catch (Exception ex)
                {
                    logger.Error("doScrapeThumbs: " + ex);
                    lock (lockObject)
                        dbClient.Execute("ROLLBACK;");
                }
            }
            return 0;
        }

        public int DoScrapeNew(string artist, string album, bool externalAccess)
        {
            if (!StopScraper)
            {
                try
                {
                    var artist1 = Utils.GetArtist(artist, Utils.Category.MusicFanartScraped);
                    scraper = new Scraper();
                    var num1 = 0;
                    lock (lockObject)
                        dbClient.Execute("BEGIN TRANSACTION;");
                    if (artist != null && artist.Trim().Length > 0)
                    {
                        var num2 = int.Parse(dbClient.Execute("SELECT count(Key1) "+
                                                               "FROM Image "+
                                                               "WHERE Key1 = '" + (object) Utils.PatchSql(artist1) + "' AND "+
                                                                     "Enabled = 'True' AND "+
                                                                     "Category IN ('" + Utils.Category.MusicFanartScraped + "') AND "+
                                                                     "DummyItem = 'False';").GetField(0, 0), CultureInfo.CurrentCulture);
                        var doTriggerRefresh = false;
                        if (num2 == 0 && !externalAccess)
                            doTriggerRefresh = true;
                        var iMax = checked (Convert.ToInt32(Utils.GetScraperMaxImages(),CultureInfo.CurrentCulture) - num2);
                        if (iMax < 0) iMax = 0;
                        num1 = scraper.GetArtistFanart(artist, iMax, this, false, doTriggerRefresh, externalAccess, true);
                        switch (num1)
                        {
                            case 0:
                                logger.Info("No fanart found for Artist: " + artist + ".");
                                break;
                            case 8888:
                                logger.Info("Artist: " + artist + " has already maximum number of images. Will not download anymore images for this artist.");
                                break;
                        }
                        if (StopScraper)
                            return num1;
                        // var validUrlLastFmString1 = Scraper.getValidURLLastFMString(Scraper.UndoArtistPrefix(artist));
                        // var validUrlLastFmString2 = Scraper.getValidURLLastFMString(album);
                        // var validUrlLastFmString1 = Scraper.UndoArtistPrefix(artist);
                        // var validUrlLastFmString2 = album;
                        // Utils.GetArtist(validUrlLastFmString1, Utils.Category.MusicFanartScraped);
                        // Utils.GetArtist(validUrlLastFmString2, Utils.Category.MusicFanartScraped);
                        //if (!Utils.GetDbm().HasAlbumThumb(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped),Utils.GetArtist(album, Utils.Category.MusicFanartScraped)) && 
                        if (!Utils.GetDbm().HasAlbumThumb(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped),Utils.GetAlbum(album, Utils.Category.MusicFanartScraped)) && 
                             Utils.ScrapeThumbnailsAlbum.Equals("True", StringComparison.CurrentCulture)
                           )
                        {
                        /*
                            Utils.GetDbm().InsertDummyItem(Utils.GetArtist(artist, Utils.Category.MusicFanartScraped),
                                                           Utils.Category.MusicAlbumThumbScraped,
                        //                                   Utils.GetArtist(album, Utils.Category.MusicFanartScraped));
                                                           Utils.GetAlbum(album, Utils.Category.MusicFanartScraped), 
                                                           null);
                        */
                        //    scraper.LastFMGetTumbnails(Utils.Category.MusicAlbumThumbScraped, artist, album, externalAccess);
                          scraper.GetArtistAlbumThumbs(artist, album, false, externalAccess);
                        }
                        scraper = null;
                    }
                    lock (lockObject)
                        dbClient.Execute("COMMIT;");
                    return num1;
                }
                catch (Exception ex)
                {
                    logger.Error("DoScrapeNew: " + ex);
                    lock (lockObject)
                        dbClient.Execute("ROLLBACK;");
                }
            }
            return 0;
        }

        public void InitialScrape()
        {
            try
            {
                logger.Info("InitialScrape is starting...");
                var flag = true;
                musicDatabaseArtists = new ArrayList();
                musicDatabaseAlbums = new List<AlbumInfo>();
                m_db.GetAllArtists(ref musicDatabaseArtists);
                m_db.GetAllAlbums(ref musicDatabaseAlbums);
                var musicVideoArtists = Utils.GetMusicVideoArtists("mvCentral.db3");
                if (musicVideoArtists != null && musicVideoArtists.Count > 0) {
                    logger.Debug("InitialScrape add Artists from mvCentral ["+musicVideoArtists.Count+"]...");
                    musicDatabaseArtists.AddRange(musicVideoArtists);
                }
                var musicVideoAlbums = Utils.GetMusicVideoAlbums("mvCentral.db3");
                if (musicVideoAlbums != null && musicVideoAlbums.Count > 0) {
                    logger.Debug("InitialScrape add Artists/Albums from mvCentral ["+musicVideoAlbums.Count+"]...");
                    musicDatabaseAlbums.AddRange(musicVideoAlbums);
                }
                if (FanartHandlerSetup.Fh.MyScraperWorker != null)
                    FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(0, "Start");
                TotArtistsBeingScraped = checked (musicDatabaseArtists.Count + musicDatabaseAlbums.Count);
                if (musicDatabaseArtists != null && musicDatabaseArtists.Count > 0)
                {
                    logger.Debug("InitialScrape initiating for Artist...");
                    var htFanart = new Hashtable();
                    var str = "SELECT Key1, count(Key1) "+
                               "FROM Image "+
                               "WHERE Enabled = 'True' AND "+
                                     "Category in ('" + ((object) Utils.Category.MusicFanartScraped).ToString() + "') AND "+
                                     "Time_Stamp >= '" + DateTime.Today.AddDays(-14.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' AND "+
                                     "DummyItem = 'False' "+
                               "GROUP BY Key1;";
                    SQLiteResultSet sqLiteResultSet;
                    lock (lockObject)
                        sqLiteResultSet = dbClient.Execute(str);
                    var num = 0;
                    while (num < sqLiteResultSet.Rows.Count)
                    {
                        if (!htFanart.Contains(sqLiteResultSet.GetField(num, 0)))
                            htFanart.Add(sqLiteResultSet.GetField(num, 0), sqLiteResultSet.GetField(num, 1));
                        checked
                        {
                            ++num;
                        }
                    }
                    var index = 0;
                    while (index < musicDatabaseArtists.Count)
                    {
                        var artist = musicDatabaseArtists[index].ToString();
                        if (!StopScraper && !Utils.GetIsStopping()) 
                        {
                            if (DoScrape(artist, htFanart) > 0 && flag)
                            {
                                AddScapedFanartToAnyHash();
                                if (FanartHandlerSetup.Fh.MyScraperNowWorker != null)
                                {
                                    FanartHandlerSetup.Fh.MyScraperNowWorker.TriggerRefresh = true;
                                    flag = false;
                                }
                            }
                            ++CurrArtistsBeingScraped;
                            if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                              FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Convert.ToInt32(CurrArtistsBeingScraped/TotArtistsBeingScraped*100.0),"Ongoing");
                            checked
                            {
                                ++index;
                            }
                        }
                        else
                            break;
                    }
                    logger.Debug("InitialScrape done for Artist...");
                }
                if (musicDatabaseAlbums != null && musicDatabaseAlbums.Count > 0 &&
                    Utils.ScrapeThumbnailsAlbum.Equals("True", StringComparison.CurrentCulture))
                {
                    logger.Debug("InitialScrape initiating for Artist/Album...");
                    scraper = new Scraper();
                    var hashtable1 = new Hashtable();
                    var str1 = "SELECT Key1, Key2, count(Key1) "+
                                "FROM Image "+
                                "WHERE Category IN ('" + ((object) Utils.Category.MusicAlbumThumbScraped).ToString() + "') AND "+
                                      "Time_Stamp >= '" + DateTime.Today.AddDays(-14.0).ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' AND "+
                                      "DummyItem = 'False' "+
                                "GROUP BY Key1, Key2;";
                    SQLiteResultSet sqLiteResultSet;
                    lock (lockObject)
                        sqLiteResultSet = dbClient.Execute(str1);
                    var num1 = 0;
                    while (num1 < sqLiteResultSet.Rows.Count)
                    {
                        if (!hashtable1.Contains(sqLiteResultSet.GetField(num1, 0) + "-" + sqLiteResultSet.GetField(num1, 1)))
                            hashtable1.Add(sqLiteResultSet.GetField(num1, 0) + "-" + sqLiteResultSet.GetField(num1, 1),sqLiteResultSet.GetField(num1, 2));
                        checked
                        {
                            ++num1;
                        }
                    }
                    var hashtable2 = new Hashtable();
                    var str2 = "SELECT Key1, Key2, count(Key1) "+
                                "FROM Image "+
                                "WHERE Category IN ('" + ((object) Utils.Category.MusicAlbumThumbScraped).ToString() + "') AND "+
                                      "DummyItem = 'False' "+
                                "GROUP BY Key1, Key2;";
                    lock (lockObject)
                        sqLiteResultSet = dbClient.Execute(str2);
                    var num2 = 0;
                    while (num2 < sqLiteResultSet.Rows.Count)
                    {
                        if (!hashtable2.Contains(sqLiteResultSet.GetField(num2, 0) + "-" + sqLiteResultSet.GetField(num2, 1)))
                            hashtable2.Add(sqLiteResultSet.GetField(num2, 0) + "-" + sqLiteResultSet.GetField(num2, 1),sqLiteResultSet.GetField(num2, 2));
                        checked
                        {
                            ++num2;
                        }
                    }
                    var index = 0;
                    while (index < musicDatabaseAlbums.Count)
                    {
                        var album = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Album).Trim();
                        if (album != null && album.Length > 0)
                        {
                            var artist1 = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Artist).Trim();
                            // var validUrlLastFmString1 = Scraper.getValidURLLastFMString(Scraper.UndoArtistPrefix(artist1));
                            // var validUrlLastFmString2 = Scraper.getValidURLLastFMString(album);
                            // var validUrlLastFmString1 = Scraper.UndoArtistPrefix(artist1);
                            // var validUrlLastFmString2 = album;
                            // var artist2 = Utils.GetArtist(validUrlLastFmString1, Utils.Category.MusicFanartScraped);
                            var dbartist = Utils.GetArtist(Scraper.UndoArtistPrefix(artist1), Utils.Category.MusicFanartScraped);
                            // var artist3 = Utils.GetArtist(validUrlLastFmString2, Utils.Category.MusicFanartScraped);
                            // var artist3 = Utils.GetAlbum(validUrlLastFmString2, Utils.Category.MusicFanartScraped);
                            var dbalbum = Utils.GetAlbum(album, Utils.Category.MusicFanartScraped);
                            var num3 = 0;
                            if (hashtable1.Contains(Utils.PatchSql(dbartist) + "-" + Utils.PatchSql(dbalbum)))
                                num3 = int.Parse((string) hashtable1[Utils.PatchSql(dbartist) + "-" + Utils.PatchSql(dbalbum)],CultureInfo.CurrentCulture);
                            if (num3 < 1) {
                                var num4 = 0;
                                if (hashtable2.Contains(Utils.PatchSql(dbartist) + "-" + Utils.PatchSql(dbalbum)))
                                    num4 = int.Parse((string)hashtable2[Utils.PatchSql(dbartist) + "-" + Utils.PatchSql(dbalbum)],CultureInfo.CurrentCulture);
                                if (num4 < 1) {
                                    if (!StopScraper && !Utils.GetIsStopping()) {
                                        // InsertDummyItem(artist2, Utils.Category.MusicAlbumThumbScraped, artist3, null);
                                        // scraper.LastFMGetTumbnails(Utils.Category.MusicAlbumThumbScraped, artist1, album, false);
                                        scraper.GetArtistAlbumThumbs(artist1, album, false, false);
                                    }
                                    else
                                      break;
                                }
                            }
                        }
                        ++CurrArtistsBeingScraped;
                        if (TotArtistsBeingScraped > 0.0 && FanartHandlerSetup.Fh.MyScraperWorker != null)
                            FanartHandlerSetup.Fh.MyScraperWorker.ReportProgress(Convert.ToInt32(CurrArtistsBeingScraped/TotArtistsBeingScraped*100.0),"Ongoing");
                        checked
                        {
                            ++index;
                        }
                    }
                    scraper = null;
                    logger.Debug("InitialScrape done for Artist/Album");
                }
                musicDatabaseArtists = null;
                musicDatabaseAlbums = null;
                AddScapedFanartToAnyHash();
                logger.Info("InitialScrape is done.");
            }
            catch (Exception ex)
            {
                logger.Error("InitialScrape: " + ex);
            }
        }


        public void InitialThumbScrape(bool onlyMissing)
        {
            try
            {
                logger.Info("InitialThumbScrape is starting (only missing=" + (onlyMissing ? 1 : 0) + ")...");
                musicDatabaseArtists = new ArrayList();
                musicDatabaseAlbums = new List<AlbumInfo>();
                m_db.GetAllArtists(ref musicDatabaseArtists);
                m_db.GetAllAlbums(ref musicDatabaseAlbums);
                var musicVideoArtists = Utils.GetMusicVideoArtists("mvCentral.db3");
                if (musicVideoArtists != null && musicVideoArtists.Count > 0){
                    logger.Debug("InitialThumbScrape add Artists from mvCentral ["+musicVideoArtists.Count+"]...");
                    musicDatabaseArtists.AddRange(musicVideoArtists);
                }
                var musicVideoAlbums = Utils.GetMusicVideoAlbums("mvCentral.db3");
                if (musicVideoAlbums != null && musicVideoAlbums.Count > 0) {
                    logger.Debug("InitialThumbScrape add Artists/Albums from mvCentral ["+musicVideoAlbums.Count+"]...");
                    musicDatabaseAlbums.AddRange(musicVideoAlbums);
                }
                TotArtistsBeingScraped = checked (musicDatabaseArtists.Count + musicDatabaseAlbums.Count);
                if (musicDatabaseArtists != null && musicDatabaseArtists.Count > 0)
                {
                    var index = 0;
                    while (index < musicDatabaseArtists.Count)
                    {
                        var artist = musicDatabaseArtists[index].ToString();
                        if (!StopScraper && !Utils.GetIsStopping())
                        {
                            doScrapeThumbs(artist, onlyMissing);
                            ++CurrArtistsBeingScraped;
                            checked
                            {
                                ++index;
                            }
                        }
                        else
                            break;
                    }
                }
                if (musicDatabaseAlbums != null && musicDatabaseAlbums.Count > 0 &&
                    Utils.ScrapeThumbnailsAlbum.Equals("True", StringComparison.CurrentCulture))
                {
                    scraper = new Scraper();
                    var index = 0;
                    while (index < musicDatabaseAlbums.Count)
                    {
                        var album = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Album).Trim();
                        if (album != null && album.Length > 0)
                        {
                            var artist1 = Utils.RemoveMPArtistPipe(musicDatabaseAlbums[index].Artist).Trim();
                            // var validUrlLastFmString1 = Scraper.getValidURLLastFMString(Scraper.UndoArtistPrefix(artist1));
                            // var validUrlLastFmString2 = Scraper.getValidURLLastFMString(album);
                            // var validUrlLastFmString1 = Scraper.UndoArtistPrefix(artist1);
                            // var validUrlLastFmString2 = album;
                            // var artist2 = Utils.GetArtist(validUrlLastFmString1, Utils.Category.MusicFanartScraped);
                            var dbartist = Utils.GetArtist(Scraper.UndoArtistPrefix(artist1), Utils.Category.MusicFanartScraped);
                            // var artist3 = Utils.GetArtist(validUrlLastFmString2, Utils.Category.MusicFanartScraped);
                            // var artist3 = Utils.GetAlbum(validUrlLastFmString2, Utils.Category.MusicFanartScraped);
                            var dbalbum = Utils.GetAlbum(album, Utils.Category.MusicFanartScraped);
                            var str = "SELECT count(Key1) "+
                                       "FROM Image "+
                                       "WHERE Key1 = '" + Utils.PatchSql(dbartist) + "' AND "+
                                             "Key2 = '" + Utils.PatchSql(dbalbum) + "' AND "+
                                             "Category in ('" + ((object) Utils.Category.MusicAlbumThumbScraped).ToString() + "') AND "+
                                             "DummyItem = 'False';";
                            SQLiteResultSet sqLiteResultSet;
                            lock (lockObject)
                                sqLiteResultSet = dbClient.Execute(str);
                            if (int.Parse(sqLiteResultSet.GetField(0, 0), CultureInfo.CurrentCulture) < 1 || !onlyMissing) {
                                if (!StopScraper && !Utils.GetIsStopping()) {
                                    // Utils.GetDbm().InsertDummyItem(artist2, Utils.Category.MusicAlbumThumbScraped, artist3, null);
                                    // scraper.LastFMGetTumbnails(Utils.Category.MusicAlbumThumbScraped, artist1, album, false);
                                    scraper.GetArtistAlbumThumbs(artist1, album, false, false);
                                }
                                else
                                    break;
                            }
                        }
                        ++CurrArtistsBeingScraped;
                        checked
                        {
                            ++index;
                        }
                    }
                    scraper = null;
                }
                logger.Info("InitialThumbScrape is done.");
                musicDatabaseArtists = null;
                musicDatabaseAlbums = null;
            }
            catch (Exception ex)
            {
                logger.Error("InitialThumbScrape: " + ex);
            }
        }

        private void AddScapedFanartToAnyHash()
        {
            if (HtAnyFanart == null)
                HtAnyFanart = new Hashtable();
            var hashtable = !HtAnyFanart.ContainsKey(Utils.Category.MusicFanartScraped)
                ? new Hashtable()
                : (Hashtable) HtAnyFanart[Utils.Category.MusicFanartScraped];
            if (hashtable != null && hashtable.Count >= 1)
                return;
            var filenames = new Hashtable();
            var str = "SELECT Id, Key1, FullPath, SourcePath, Category, Provider "+
                       "FROM Image "+
                       "WHERE Enabled = 'True' AND "+
                             "Category = '" + ((object) Utils.Category.MusicFanartScraped).ToString() + "';";
            SQLiteResultSet sqLiteResultSet;
            lock (lockObject)
                sqLiteResultSet = dbClient.Execute(str);
            var num = 0;
            while (num < sqLiteResultSet.Rows.Count)
            {
                var fanartImage = new FanartImage(sqLiteResultSet.GetField(num, 0), sqLiteResultSet.GetField(num, 1),
                    sqLiteResultSet.GetField(num, 2), sqLiteResultSet.GetField(num, 3), sqLiteResultSet.GetField(num, 4),
                    sqLiteResultSet.GetField(num, 5));
                filenames.Add(num, fanartImage);
                checked
                {
                    ++num;
                }
            }
            Utils.Shuffle(ref filenames);
            HtAnyFanart.Remove(Utils.Category.MusicFanartScraped);
            htAnyFanart.Add(Utils.Category.MusicFanartScraped, filenames);
        }

        public void DeleteAllFanart(Utils.Category category)
        {
            try
            {
                lock (lockObject)
                    dbClient.Execute("DELETE FROM Image WHERE Category = '" + Utils.PatchSql(((object) category).ToString()) + "';");
            }
            catch (Exception ex)
            {
                logger.Error("DeleteAllFanart: " + ex);
            }
        }

        public void EnableImage(string diskImage, bool action)
        {
            try
            {
                var str = !action
                    ? "UPDATE Image SET Enabled = 'False' WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';"
                    : "UPDATE Image SET Enabled = 'True' WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
                lock (lockObject)
                    dbClient.Execute(str);
            }
            catch (Exception ex)
            {
                logger.Error("EnableImage: " + ex);
            }
        }

        public void EnableForRandomImage(string diskImage, bool action)
        {
            try
            {
                var str = !action
                    ? "UPDATE Image SET AvailableRandom = 'False' WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';"
                    : "UPDATE Image SET AvailableRandom = 'True' WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
                lock (lockObject)
                    dbClient.Execute(str);
            }
            catch (Exception ex)
            {
                logger.Error("EnableForRandomImage: " + ex);
            }
        }

        public void DeleteImage(string diskImage)
        {
            try
            {
                var str = "DELETE FROM Image WHERE FullPath = '" + Utils.PatchSql(diskImage) + "';";
                lock (lockObject)
                    dbClient.Execute(str);
            }
            catch (Exception ex)
            {
                logger.Error("DeleteImage: " + ex);
            }
        }

        public SQLiteResultSet GetDataForConfigTable(int start)
        {
            var sqLiteResultSet = (SQLiteResultSet) null;
            try
            {
                var str = "SELECT Key1, Enabled, AvailableRandom, FullPath, ROWID "+
                           "FROM Image "+
                           "WHERE DummyItem = 'False' AND "+
                                 "Category IN ('" + Utils.Category.MusicFanartScraped + "','" + Utils.Category.MusicFanartManual +
                            // "') order by Key1, FullPath LIMIT " + start + ",500;";
                                             "') "+
                           "ORDER BY Key1, FullPath;";
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute(str);
            }
            catch (Exception ex)
            {
                logger.Error("GetDataForConfigTable: " + ex);
            }
            return sqLiteResultSet;
        }

        public SQLiteResultSet GetDataForConfigTableScan(int lastID)
        {
            var sqLiteResultSet = (SQLiteResultSet) null;
            try
            {
                var str = "SELECT Key1, Enabled, AvailableRandom, FullPath, ROWID "+
                           "FROM Image "+
                           "WHERE ROWID > " + lastID + " AND "+
                                 "DummyItem = 'False' AND "+
                                 "Category IN ('" + Utils.Category.MusicFanartScraped + "','" + Utils.Category.MusicFanartManual + "') "+
                           "ORDER BY Key1, FullPath";
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute(str);
            }
            catch (Exception ex)
            {
                logger.Error("GetDataForConfigTable: " + ex);
            }
            return sqLiteResultSet;
        }

        public SQLiteResultSet GetDataForConfigUserManagedTable(int lastID, string category)
        {
            var sqLiteResultSet = (SQLiteResultSet) null;
            try
            {
                var str = "SELECT Category, AvailableRandom, FullPath, ROWID "+
                           "FROM Image "+
                           "WHERE ROWID > " + lastID + " AND DummyItem = 'False' AND "+
                                 "Category IN ('" + category + "') "+
                            "ORDER BY Key1, FullPath";
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute(str);
            }
            catch (Exception ex)
            {
                logger.Error("GetDataForConfigTable: " + ex);
            }
            return sqLiteResultSet;
        }

        public Hashtable GetFanart(string artist, Utils.Category category, bool highDef)
        {
            var filenames = new Hashtable();
            try
            {
                // var str1 = "";
                string str2;
                if (category == Utils.Category.MusicFanartScraped)
                    str2 = "SELECT Id, Key1, FullPath, SourcePath, Category, Provider "+
                            "FROM Image "+
                            "WHERE Key1 IN (" + Utils.HandleMultipleArtistNamesForDBQuery(Utils.PatchSql(artist)) + ") AND "+
                                  "Enabled = 'True' AND "+
                                  "Category in (" + Utils.GetMusicFanartCategoriesInStatement(highDef) + ");";
                else
                    str2 = "SELECT Id, Key1, FullPath, SourcePath, Category, Provider "+
                            "FROM Image "+
                            "WHERE Key1 IN ('" + Utils.PatchSql(artist) + "') AND "+
                                  "Enabled = 'True';";
                SQLiteResultSet sqLiteResultSet;
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute(str2);
                var num = 0;
                while (num < sqLiteResultSet.Rows.Count)
                {
                    var fanartImage = new FanartImage(sqLiteResultSet.GetField(num, 0), sqLiteResultSet.GetField(num, 1),
                        sqLiteResultSet.GetField(num, 2), sqLiteResultSet.GetField(num, 3),
                        sqLiteResultSet.GetField(num, 4), sqLiteResultSet.GetField(num, 5));
                    filenames.Add(num, fanartImage);
                    checked
                    {
                        ++num;
                    }
                }
                Utils.Shuffle(ref filenames);
                // str1 = null;
            }
            catch (Exception ex)
            {
                logger.Error("getFanart: " + ex);
            }
            return filenames;
        }

        public Hashtable GetAnyHashtable(Utils.Category category)
        {
            if (HtAnyFanart == null)
                HtAnyFanart = new Hashtable();
            if (HtAnyFanart.ContainsKey(category))
                return (Hashtable) HtAnyFanart[category];
            else
                return null;
        }

        private void AddToAnyHashtable(Utils.Category category, Hashtable ht)
        {
            if (HtAnyFanart == null)
                HtAnyFanart = new Hashtable();
            if (!HtAnyFanart.ContainsKey(category))
                return;
            HtAnyFanart.Remove(category);
            HtAnyFanart.Add(category, ht);
        }

        public Hashtable GetAnyFanart(Utils.Category category)
        {
            var filenames = GetAnyHashtable(category);
            try
            {
                if (filenames != null)
                    return filenames;
                filenames = new Hashtable();
                var str1 = string.Empty;
                if (category == Utils.Category.MusicFanartScraped)
                {
                    if (FanartHandlerSetup.Fh.UseAlbum.Equals("True", StringComparison.CurrentCulture) &&
                        FanartHandlerSetup.Fh.DisableMPTumbsForRandom.Equals("False", StringComparison.CurrentCulture))
                    {
                        if (str1.Length > 0)
                            str1 = string.Concat(new object[4]
                            {
                                str1,
                                ",'",
                                Utils.Category.MusicAlbumThumbScraped,
                                "'"
                            });
                        else
                            str1 = "'" + Utils.Category.MusicAlbumThumbScraped + "'";
                    }
                    if (FanartHandlerSetup.Fh.UseArtist.Equals("True", StringComparison.CurrentCulture) &&
                        FanartHandlerSetup.Fh.DisableMPTumbsForRandom.Equals("False", StringComparison.CurrentCulture))
                    {
                        if (str1.Length > 0)
                            str1 = string.Concat(new object[4]
                            {
                                str1,
                                ",'",
                                Utils.Category.MusicArtistThumbScraped,
                                "'"
                            });
                        else
                            str1 = "'" + Utils.Category.MusicArtistThumbScraped + "'";
                    }
                    if (FanartHandlerSetup.Fh.UseFanart.Equals("True", StringComparison.CurrentCulture))
                    {
                        string str2;
                        if (str1.Length > 0)
                            str2 = string.Concat(new object[4]
                            {
                                str1,
                                ",'",
                                Utils.Category.MusicFanartScraped,
                                "'"
                            });
                        else
                            str2 = "'" + Utils.Category.MusicFanartScraped + "'";
                        if (str2.Length > 0)
                            str1 = string.Concat(new object[4]
                            {
                                str2,
                                ",'",
                                Utils.Category.MusicFanartManual,
                                "'"
                            });
                        else
                            str1 = "'" + Utils.Category.MusicFanartManual + "'";
                    }
                }
                else
                    str1 = "'" + ((object) category).ToString() + "'";
                var str3 = "SELECT Id, Key1, FullPath, SourcePath, Category, Provider "+
                            "FROM Image "+
                            "WHERE Enabled = 'True' AND "+
                                  "AvailableRandom = 'True' AND "+
                                  "Category IN (" + str1 + ");";
                SQLiteResultSet sqLiteResultSet;
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute(str3);
                var num = 0;
                while (num < sqLiteResultSet.Rows.Count)
                {
                    var fanartImage = new FanartImage(sqLiteResultSet.GetField(num, 0), sqLiteResultSet.GetField(num, 1),
                                                      sqLiteResultSet.GetField(num, 2), sqLiteResultSet.GetField(num, 3),
                                                      sqLiteResultSet.GetField(num, 4), sqLiteResultSet.GetField(num, 5));
                    filenames.Add(num, fanartImage);
                    checked
                    {
                        ++num;
                    }
                }
                Utils.Shuffle(ref filenames);
                AddToAnyHashtable(category, filenames);
                return filenames;
            }
            catch (Exception ex)
            {
                logger.Error("getAnyFanart: " + ex);
                return filenames;
            }
        }

        private string GetImageId(string artist, string diskImage, string sourceImage, Utils.Category category, string album, Utils.Provider provider, string _id)
        {
            var str = string.Empty;
            try
            {
                if (category == Utils.Category.GameManual)
                    str = diskImage;
                else if (category == Utils.Category.MovieManual)
                    str = diskImage;
                else if (category == Utils.Category.MovieScraped)
                    str = diskImage;
                else if (category == Utils.Category.MovingPictureManual)
                    str = diskImage;
                else if (category == Utils.Category.MusicAlbumThumbScraped)
                    str = diskImage;
                else if (category == Utils.Category.MusicArtistThumbScraped)
                    str = diskImage;
                else if (category == Utils.Category.MusicFanartScraped)
                    str = _id;
                else if (category == Utils.Category.MusicFanartManual)
                    str = diskImage;
                else if (category == Utils.Category.PictureManual)
                    str = diskImage;
                else if (category == Utils.Category.PluginManual)
                    str = diskImage;
                else if (category == Utils.Category.SeriesManual)
                    str = diskImage;
                else if (category == Utils.Category.SportsManual)
                    str = diskImage;
                else if (category == Utils.Category.TvManual)
                    str = diskImage;
                else if (category == Utils.Category.TvSeriesScraped)
                    str = diskImage;
            }
            catch (Exception ex)
            {
                logger.Error("GetImageId: " + ex);
            }
            return str;
        }

        public void LoadFanart(string artist, string diskImage, string sourceImage, Utils.Category category, string album, Utils.Provider provider, string _id, string mbid)
        {
            try
            {
                var imageId = GetImageId(artist, diskImage, sourceImage, category, album, provider, _id);
                var SQL = string.Empty;
                var now = DateTime.Now;
                album = (category != Utils.Category.MusicAlbumThumbScraped) ? null : album ;
                SQL = "DELETE FROM Image "+
                       "WHERE Key1 = '" + Utils.PatchSql(artist) + "' AND "+
                             "Category = '" + Utils.PatchSql(((object) category).ToString()) + "' AND "+
                             "DummyItem = 'True';";
                lock (lockObject)
                    dbClient.Execute(SQL);
                if (DatabaseUtility.GetAsInt(dbClient.Execute("SELECT COUNT(Key1) "+
                                                               "FROM Image "+
                                                               "WHERE Id = '" + Utils.PatchSql(imageId) + "' AND "+
                                                                     "Provider = '" + Utils.PatchSql(((object) provider).ToString()) + "';"), 0, 0) > 0)
                {
                    SQL = "UPDATE Image SET Category = '" + Utils.PatchSql(((object) category).ToString()) + "', "+
                                           "Provider = '" + Utils.PatchSql(((object) provider).ToString()) + "', "+
                                           "Key1 = '" + Utils.PatchSql(artist) + "', "+
                                           "Key2 = '" + Utils.PatchSql(album) + "', "+
                                           "FullPath = '" + Utils.PatchSql(diskImage) + "', "+
                                           "SourcePath = '" + Utils.PatchSql(sourceImage) + "', "+
                                           "Enabled = 'True', "+
                                           "Time_Stamp = '" + now.ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' "+
                          "WHERE Id = '" + Utils.PatchSql(imageId) + "' AND "+
                                "Provider = '" + Utils.PatchSql(((object) provider).ToString()) + "';";
                    lock (lockObject)
                        dbClient.Execute(SQL);
                }
                else
                {
                    SQL = "INSERT INTO Image (Id, Category, Provider, Key1, Key2, FullPath, SourcePath, AvailableRandom, Enabled, DummyItem, Time_Stamp, MBID) "+
                           "VALUES('" + Utils.PatchSql(imageId) + "',"+
                                  "'" + Utils.PatchSql(((object) category).ToString()) + "',"+
                                  "'" + Utils.PatchSql(((object) provider).ToString()) + "',"+
                                  "'" + Utils.PatchSql(artist) + "',"+
                                  "'" + Utils.PatchSql(album) + "',"+
                                  "'" + Utils.PatchSql(diskImage) + "',"+
                                  "'" + Utils.PatchSql(sourceImage) + "',"+
                                  "'True', 'True', 'False',"+
                                  "'" + now.ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "',"+
                                  "'"+Utils.PatchSql(mbid)+"');";
                    lock (lockObject)
                        dbClient.Execute(SQL);
                    logger.Info("Importing local fanart into fanart handler database (" + diskImage + ").");
                }
            }
            catch (Exception ex)
            {
                logger.Error("LoadFanart:");
                logger.Error(ex);
            }
        }

        public bool SourceImageExist(string artist, string diskImage, string sourceImage, Utils.Category category, string album, Utils.Provider provider, string _id)
        {
            try
            {
                var imageId = GetImageId(artist, diskImage, sourceImage, category, album, provider, _id);
                if (DatabaseUtility.GetAsInt(dbClient.Execute("SELECT COUNT(Key1) "+
                                                               "FROM Image "+
                                                               "WHERE Id = '" + Utils.PatchSql(imageId) + "' AND "+
                                                                     "Provider = '" + Utils.PatchSql(((object) provider).ToString()) + "';"),0, 0) <= 0)
                    return false;
                lock (lockObject)
                    dbClient.Execute("UPDATE Image "+
                                      "SET Time_Stamp = '" + DateTime.Now.ToString("yyyyMMdd", CultureInfo.CurrentCulture) + "' "+
                                      "WHERE Id = '" + Utils.PatchSql(imageId) + "';");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("SourceImageExist: " + ex);
                return true;
            }
        }

        // Begin: GetDBMuzicBrainzID
        public string GetDBMuzicBrainzID(string artist, string album)
        {
          try
          {
            lock (lockObject)
              return dbClient.Execute("SELECT DISTINCT MBID "+
                                       "FROM Image "+
                                       "WHERE Key1 = '"+Utils.PatchSql(artist)+"'"+
                                              ((album == null) ? "" : " AND Key2 = '"+Utils.PatchSql(album)+"'")+
                                       " LIMIT 0,1;").GetField(0, 0);
          }
          catch (Exception ex)
          {
              logger.Error("GetDBMuzicBrainzID: " + ex);
              return null;
          }
        }
        // End: GetDBMuzicBrainzID

        public Hashtable GetAllFilenames(Utils.Category category)
        {
            var hashtable = new Hashtable();
            try
            {
                var str1 = string.Empty;
                var str2 = string.Empty;
                var str3 = "SELECT FullPath FROM image WHERE Category = '" + category + "';";
                SQLiteResultSet sqLiteResultSet;
                lock (lockObject)
                    sqLiteResultSet = dbClient.Execute(str3);
                var num = 0;
                while (num < sqLiteResultSet.Rows.Count)
                {
                    var field = sqLiteResultSet.GetField(num, 0);
                    if (!hashtable.Contains(field))
                        hashtable.Add(field, field);
                    checked
                    {
                        ++num;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetAllFilenames: " + ex);
            }
            return hashtable;
        }
    }
}
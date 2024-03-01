using FanartHandler;

using System;
using System.Collections;
using System.Globalization;
using System.Xml;

namespace JayMuntzCom
{
  /// <summary>
  /// Summary description for HolidayCalculator.
  /// </summary>
  public class HolidayCalculator
  {

    #region Constructor
    /// <summary>
    /// Returns all of the holidays occuring in the year following the date that is passed in the constructor.  Holidays are defined in an XML file.
    /// </summary>	
    /// <param name="startDate">The starting date for returning holidays.  All holidays for one year after this date are returned.</param>
    /// <param name="xmlPath">The path to the XML file that contains the holiday definitions.</param>
    public HolidayCalculator(System.DateTime startDate, string xmlPath)
    {
      this.startingDate = getFirstDayOfYear(startDate);
      this.checkDate = startDate;
      orderedHolidays = new ArrayList();
      xHolidays = new XmlDocument();
      xHolidays.Load(xmlPath);
      this.processXML();
    }
    #endregion

    #region Private Properties
    private ArrayList orderedHolidays;
    private XmlDocument xHolidays;
    private DateTime startingDate;
    private DateTime checkDate;
    #endregion

    #region Public Properties

    /// <summary>
    /// The holidays occuring after StartDate listed in chronological order;
    /// </summary>
    public ArrayList OrderedHolidays
    {
      get { return this.orderedHolidays; }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Loops through the holidays defined in the XML configuration file, and adds the next occurance into the OrderHolidays collection if it occurs within one year.
    /// </summary>
    private void processXML()
    {
      foreach (XmlNode n in xHolidays.SelectNodes("/Holidays/Holiday"))
      {
        Holiday h = this.processNode(n);
        if (h.Date == checkDate)
        {
          this.orderedHolidays.Add(h);
        }
      }
      orderedHolidays.Sort();
    }
    
    /// <summary>
    /// Processes a Holiday node from the XML configuration file.
    /// </summary>
    /// <param name="n">The Holdiay node to process.</param>
    /// <returns></returns>
    private Holiday processNode(XmlNode n)
    {
      Holiday h = new Holiday();
      if (n.Attributes == null)
      {
        return h;
      }

      h.Name = n.Attributes["name"].Value.ToString();
      if (string.IsNullOrEmpty(h.Name))
      {
        return h;
      }
      if (n.Attributes["shortname"] != null)
      {
        h.ShortName = n.Attributes["shortname"].Value.ToString();
      }
      else
      {
        h.ShortName = h.Name.Replace(" ", string.Empty).Replace("'", string.Empty);
      }

      ArrayList childNodes = new ArrayList();
      foreach (XmlNode o in n.ChildNodes)
      {
        childNodes.Add(o.Name.ToString());
      }

      if (childNodes.Contains("WeekOfMonth"))
      {
        int m = Int32.Parse(n.SelectSingleNode("./Month").InnerXml.ToString());
        int w = Int32.Parse(n.SelectSingleNode("./WeekOfMonth").InnerXml.ToString());
        int wd = Int32.Parse(n.SelectSingleNode("./DayOfWeek").InnerXml.ToString());
        h.Date = this.getDateByMonthWeekWeekday(m, w, wd, this.startingDate);
      }
      else if (childNodes.Contains("DayOfYear"))
      {
        int d = Int32.Parse(n.SelectSingleNode("./DayOfYear").InnerXml.ToString());
        if (d > 366 || d < 0)
          throw new Exception("DOY is greater than 366");
        DateTime dt = new DateTime(this.startingDate.Year, 1, 1).AddDays(d - 1);
        if (dt < this.startingDate)
        {
          dt = dt.AddYears(1);
        }
        h.Date = dt;
      }
      else if (childNodes.Contains("DayOfWeekOnOrAfter"))
      {
        int dow = Int32.Parse(n.SelectSingleNode("./DayOfWeekOnOrAfter/DayOfWeek").InnerXml.ToString());
        if (dow > 6 || dow < 0)
          throw new Exception("DOW is greater than 6");
        int m = Int32.Parse(n.SelectSingleNode("./DayOfWeekOnOrAfter/Month").InnerXml.ToString());
        int d = Int32.Parse(n.SelectSingleNode("./DayOfWeekOnOrAfter/Day").InnerXml.ToString());
        h.Date = this.getDateByWeekdayOnOrAfter(dow, m, d, this.startingDate);
      }
      else if (childNodes.Contains("DayOfWeekOnOrBefore"))
      {
        int dow = Int32.Parse(n.SelectSingleNode("./DayOfWeekOnOrBefore/DayOfWeek").InnerXml.ToString());
        if (dow > 6 || dow < 0)
          throw new Exception("DOW is greater than 6");
        int m = Int32.Parse(n.SelectSingleNode("./DayOfWeekOnOrBefore/Month").InnerXml.ToString());
        int d = Int32.Parse(n.SelectSingleNode("./DayOfWeekOnOrBefore/Day").InnerXml.ToString());
        h.Date = this.getDateByWeekdayOnOrBefore(dow, m, d, this.startingDate);
      }
      else if (childNodes.Contains("WeekdayOnOrAfter"))
      {
        int m = Int32.Parse(n.SelectSingleNode("./WeekdayOnOrAfter/Month").InnerXml.ToString());
        int d = Int32.Parse(n.SelectSingleNode("./WeekdayOnOrAfter/Day").InnerXml.ToString());
        DateTime dt = new DateTime(this.startingDate.Year, m, d);
        if (dt < this.startingDate)
        {
          dt = dt.AddYears(1);
        }
        while (dt.DayOfWeek.Equals(DayOfWeek.Saturday) || dt.DayOfWeek.Equals(DayOfWeek.Sunday))
        {
          dt = dt.AddDays(1);
        }
        h.Date = dt;
      }
      else if (childNodes.Contains("LastFullWeekOfMonth"))
      {
        int m = Int32.Parse(n.SelectSingleNode("./LastFullWeekOfMonth/Month").InnerXml.ToString());
        int weekday = Int32.Parse(n.SelectSingleNode("./LastFullWeekOfMonth/DayOfWeek").InnerXml.ToString());
        DateTime dt = this.getDateByMonthWeekWeekday(m, 5, weekday, this.startingDate);
        if (dt.AddDays(6 - weekday).Month == m)
          h.Date = dt;
        else
          h.Date = dt.AddDays(-7);
      }
      else if (childNodes.Contains("DaysAfterHoliday"))
      {
        XmlNode basis = xHolidays.SelectSingleNode("/Holidays/Holiday[@name='" + n.SelectSingleNode("./DaysAfterHoliday").Attributes["Holiday"].Value.ToString() + "']");
        Holiday bHoliday = this.processNode(basis);
        int days = Int32.Parse(n.SelectSingleNode("./DaysAfterHoliday/Days").InnerXml.ToString());
        h.Date = bHoliday.Date.AddDays(days);
      }
      else if (childNodes.Contains("Easter"))
      {
        h.Date = this.easter();
      }
      else if (childNodes.Contains("ChineseNewYear"))
      {
        h.Date = this.getChineseNewYear();
      }
      else if (childNodes.Contains("DSTStart"))
      {
        h.Date = this.dststart();
      }
      else if (childNodes.Contains("DSTEnd"))
      {
        h.Date = this.dstend();
      }
      else if (childNodes.Contains("NearestWeekday"))
      {
        int m = Int32.Parse(n.SelectSingleNode("./NearestWeekday/Month").InnerXml.ToString());
        int d = Int32.Parse(n.SelectSingleNode("./NearestWeekday/Day").InnerXml.ToString());
        DateTime dt = new DateTime(this.startingDate.Year, m, d);
        if (dt < this.startingDate)
        {
          dt = dt.AddYears(1);
        }
        if (dt.DayOfWeek.Equals(DayOfWeek.Saturday))
        {
          //Make it Friday
          dt = dt.AddDays(-1);
        }
        else if (dt.DayOfWeek.Equals(DayOfWeek.Sunday))
        {
          //Make it Monday
          dt = dt.AddDays(1);
        }
        h.Date = dt;
      }
      else
      {
        if (childNodes.Contains("Month") && childNodes.Contains("Day"))
        {
          int m = Int32.Parse(n.SelectSingleNode("./Month").InnerXml.ToString());
          int d = Int32.Parse(n.SelectSingleNode("./Day").InnerXml.ToString());
          DateTime dt = new DateTime(this.startingDate.Year, m, d);
          if (dt < this.startingDate)
          {
            dt = dt.AddYears(1);
          }
          if (childNodes.Contains("EveryXYears"))
          {
            int yearMult = Int32.Parse(n.SelectSingleNode("./EveryXYears").InnerXml.ToString());
            int startYear = Int32.Parse(n.SelectSingleNode("./StartYear").InnerXml.ToString());
            if (((dt.Year - startYear) % yearMult) == 0)
            {
              h.Date = dt;
            }
          }
          else
          {
            h.Date = dt;
          }
        }
      }

      if (childNodes.Contains("Name"+Utils.HolidayLanguage))
      {
        string localName = n.SelectSingleNode("./Name"+Utils.HolidayLanguage).InnerXml.ToString();
        if (!string.IsNullOrEmpty(localName))
        {
          h.LocalName = localName; 
        }
      }

      if (childNodes.Contains("Year"))
      {
        int y = Int32.Parse(n.SelectSingleNode("./Year").InnerXml.ToString());
        y = h.Date.Year - y;
        h.Name = h.Name.Replace("%A", y.ToString());
        if (!string.IsNullOrEmpty(h.LocalName))
        {
          h.LocalName = h.LocalName.Replace("%A", y.ToString());
        }
      }

      if (childNodes.Contains("CelebratedIn"))
      {
        bool flag = false;
        foreach (XmlNode c in n.SelectNodes("./CelebratedIn"))
        {
          string sCelebratedIn = c.InnerXml.ToString();
          if (!string.IsNullOrEmpty(sCelebratedIn))
          {
            flag = flag || sCelebratedIn.Contains(Utils.HolidayCountry);
          }
        }
        if (!flag)
        {
          
          h.Date = DateTime.MinValue;
        }
      }

      if (childNodes.Contains("NotCelebratedIn"))
      {
        bool flag = false;
        foreach (XmlNode c in n.SelectNodes("./NotCelebratedIn"))
        {
          string sNotCelebratedIn = c.InnerXml.ToString();
          if (!string.IsNullOrEmpty(sNotCelebratedIn))
          {
            flag = flag || sNotCelebratedIn.Contains(Utils.HolidayCountry);
          }
        }
        if (flag)
        {

          h.Date = DateTime.MinValue;
        }
      }

      return h;
    }
    
    /// <summary>
    /// Determines the next occurance of Easter.
    /// </summary>
    /// <returns></returns>
    private DateTime easter()
    {
      DateTime workDate = this.getFirstDayOfMonth(this.startingDate);
      int y = workDate.Year;
      if (workDate.Month > 4)
      {
        y = y + 1;
      }
      return this.easter(y);
    }

    /// <summary>
    /// Determines the occurance of Easter in the given year.  If the result comes before StartDate, recalculates for the following year.
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    private DateTime easter(int y)
    {
      DateTime est;
      int easterMonth;
      int easterDay;

      if (Utils.HolidayEaster == 1)
      {
        // Western - Catholic Easter
        int a = y % 19;
        int b = y / 100;
        int c = y % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        easterMonth = (h + l - 7 * m + 114) / 31;
        int p = (h + l - 7 * m + 114) % 31;
        easterDay = p + 1;
      }
      else if (Utils.HolidayEaster == 2) 
      {
        // Eastern - Orthodox Easter
        int a = y % 19;
        int b = y % 7;
        int c = y % 4;
     
        int d = (19 * a + 16) % 30;
        int e = (2 * c + 4 * b + 6 * d) % 7;
        int f = (19 * a + 16) % 30;
        int key = f + e + 3;
     
        easterMonth = (key > 30) ? 5 : 4;
        easterDay = (key > 30) ? key - 30 : key;
      }
      else // if (Utils.HolidayEaster == 3) 
      {
        // Hebrew - Passover - Pesach 
        int a = (12 * y + 12) % 19;
        int b = y % 4;
        double f = 20.0955877 + 1.5542418 * a + 0.25 * b - 0.003177794 * y;
        int M = (int)f;
        double m = f % 1.0;
        int c = (M + 3 * y + 5 * b + 1) % 7;

        easterMonth = 3;
        easterDay = M;
        if (c == 2 || c == 4 || c == 6)
        {
          easterDay = M + 1;
        }
        else if (c == 1 && a > 6 && m > 0.63287037)
        {
          easterDay = M + 2;
        }
        else if (c == 0 && a > 11 && m > 0.89772376)
        {
          easterDay = M + 1;
        }
        if (M > 31)
        {
          easterMonth = 4;
          easterDay = M - 31;
        }

        DateTime old = new DateTime(y, easterMonth, easterDay);
        old = old.AddDays(13);
        if (old.DayOfWeek == DayOfWeek.Monday || old.DayOfWeek == DayOfWeek.Wednesday || old.DayOfWeek == DayOfWeek.Friday)
        {
          old = old.AddDays(1);
        }

        easterMonth = old.Month;
        easterDay = old.Day;
      }

      est = new DateTime(y, easterMonth, easterDay);
      if (est < this.startingDate)
      {
        return this.easter(y + 1);
      }
      else
      {
        return new DateTime(y, easterMonth, easterDay);
      }
    }

    /// <summary>
    /// Determines the occurance of Chinese New Year Date.
    /// </summary>
    /// <returns></returns>
    private DateTime getChineseNewYear()
    {
      DateTime workDate = this.getFirstDayOfMonth(this.startingDate);
      int y = workDate.Year;
      return this.getChineseNewYear(y);
    }

    /// <summary>
    /// Determines the occurance of Chinese New Year in the given year. If the result comes before StartDate, recalculates for the following year.
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    private DateTime getChineseNewYear(int y)
    {
      ChineseLunisolarCalendar chinese   = new ChineseLunisolarCalendar();
      GregorianCalendar        gregorian = new GregorianCalendar();

      // Get Chinese New Year of current UTC date/time
      DateTime chineseNewYear = chinese.ToDateTime( y, 1, 1, 0, 0, 0, 0 );

      // Convert back to Gregorian (you could just query properties of `chineseNewYear` directly, but I prefer to use `GregorianCalendar` for consistency:
      Int32 year  = gregorian.GetYear( chineseNewYear );
      Int32 month = gregorian.GetMonth( chineseNewYear );
      Int32 day   = gregorian.GetDayOfMonth( chineseNewYear );

      DateTime cny =  new DateTime(year, month, day);
      if (cny < this.startingDate)
      {
        return this.getChineseNewYear(y + 1);
      }
      else
      {
        return new DateTime(y, month, day);
      }
    }

    /// <summary>
    /// Determines the occurance of Daylight Savings Start Date.
    /// </summary>
    /// <returns></returns>
    private DateTime dststart()
    {
      DateTime workDate = this.getFirstDayOfMonth(this.startingDate);
      int y = workDate.Year;
      return this.dststart(y);
    }

    /// <summary>
    /// Determines the occurance of Daylight Savings End Date.
    /// </summary>
    /// <returns></returns>
    private DateTime dstend()
    {
      DateTime workDate = this.getFirstDayOfMonth(this.startingDate);
      int y = workDate.Year;
      return this.dstend(y);
    }

    /// <summary>
    /// Determines the occurance of Daylight Savings Start Date in the given year. If the result comes before StartDate, recalculates for the following year.
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    private DateTime dststart(int y)
    {
      TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
      if (!timeZoneInfo.SupportsDaylightSavingTime )
      {
        return DateTime.MinValue;
      }
      TimeZoneInfo.AdjustmentRule[] adjustmentRules = timeZoneInfo.GetAdjustmentRules();

      DateTime dst = this.GetDaylightSavingsStartDateForYear(adjustmentRules, y);
      if (dst < this.startingDate)
      {
        return this.dststart(y + 1);
      }
      else
      {
        return dst;
      }
    }

    /// <summary>
    /// Determines the occurance of Daylight Savings End Date in the given year. If the result comes before StartDate, recalculates for the following year.
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    private DateTime dstend(int y)
    {
      TimeZoneInfo timeZoneInfo = TimeZoneInfo.Local;
      if (!timeZoneInfo.SupportsDaylightSavingTime )
      {
        return DateTime.MinValue;
      }
      TimeZoneInfo.AdjustmentRule[] adjustmentRules = timeZoneInfo.GetAdjustmentRules();

      DateTime dst = this.GetDaylightSavingsEndDateForYear(adjustmentRules, y);
      if (dst < this.startingDate)
      {
        return this.dstend(y + 1);
      }
      else
      {
        return dst;
      }
    }

    /// <summary>
    /// Determines the occurance of Daylight Savings Start Date in the given year and Adjustment Rules.
    /// </summary>
    /// <param name="adjustmentRules"></param>
    /// <param name="year"></param>
    /// <returns></returns>
    private DateTime GetDaylightSavingsStartDateForYear(TimeZoneInfo.AdjustmentRule[] adjustmentRules, int year)
    {
      DateTime firstOfYear = new DateTime(year, 1, 1);

      foreach (TimeZoneInfo.AdjustmentRule adjustmentRule in adjustmentRules)
      {
        if ((adjustmentRule.DateStart <= firstOfYear) && (firstOfYear <= adjustmentRule.DateEnd))
        {
          return this.GetTransitionDate(adjustmentRule.DaylightTransitionStart, year);
        }
      }
      return DateTime.MinValue;
    }

    /// <summary>
    /// Determines the occurance of Daylight Savings End Date in the given year and Adjustment Rules.
    /// </summary>
    /// <param name="adjustmentRules"></param>
    /// <param name="year"></param>
    /// <returns></returns>
    private DateTime GetDaylightSavingsEndDateForYear(TimeZoneInfo.AdjustmentRule[] adjustmentRules, int year)
    {
      DateTime firstOfYear = new DateTime(year, 1, 1);

      foreach (TimeZoneInfo.AdjustmentRule adjustmentRule in adjustmentRules)
      {
        if ((adjustmentRule.DateStart <= firstOfYear) && (firstOfYear <= adjustmentRule.DateEnd))
        {
          return this.GetTransitionDate(adjustmentRule.DaylightTransitionEnd, year);
        }
      }
      return DateTime.MinValue;
    }

    /// <summary>
    /// Calc Transition DateTime in the given year.
    /// </summary>
    /// <param name="transitionTime"></param>
    /// <param name="year"></param>
    /// <returns></returns>
    private DateTime GetTransitionDate(TimeZoneInfo.TransitionTime transitionTime, int year)
    {
      if (transitionTime.IsFixedDateRule)
      {
        return new DateTime(year, transitionTime.Month, transitionTime.Day);
      }
      else
      {
        if (transitionTime.Week == 5)
        {
          // Special value meaning the last DayOfWeek (e.g., Sunday) in the month.
          DateTime transitionDate = new DateTime(year, transitionTime.Month, 1);
          transitionDate = transitionDate.AddMonths(1);

          transitionDate = transitionDate.AddDays(-1);
          while (transitionDate.DayOfWeek != transitionTime.DayOfWeek)
          {
            transitionDate = transitionDate.AddDays(-1);
          }

          return transitionDate;
        }
        else
        {
          DateTime transitionDate = new DateTime(year, transitionTime.Month, 1);
          transitionDate = transitionDate.AddDays(-1);

          for (int howManyWeeks = 0; howManyWeeks < transitionTime.Week; howManyWeeks++)
          {
            transitionDate = transitionDate.AddDays(1);
            while (transitionDate.DayOfWeek != transitionTime.DayOfWeek)
            {
              transitionDate = transitionDate.AddDays(1);
            }
          }

          return transitionDate;
        }
      }
    }

    /// <summary>
    /// Gets the next occurance of a weekday after a given month and day in the year after StartDate.
    /// </summary>
    /// <param name="weekday">The day of the week (0=Sunday).</param>
    /// <param name="m">The Month</param>
    /// <param name="d">Day</param>
    /// <returns></returns>
    private DateTime getDateByWeekdayOnOrAfter(int weekday, int m, int d, DateTime startDate)
    {
      DateTime workDate = this.getFirstDayOfMonth(startDate);
      while (workDate.Month != m)
      {
        workDate = workDate.AddMonths(1);
      }
      workDate = workDate.AddDays(d - 1);

      while (weekday != (int)(workDate.DayOfWeek))
      {
        workDate = workDate.AddDays(1);
      }

      //It's possible the resulting date is before the specified starting date.  If so we'll calculate again for the next year.
      if (workDate < this.startingDate)
        return this.getDateByWeekdayOnOrAfter(weekday, m, d, startDate.AddYears(1));
      else
        return workDate;
    }

    private DateTime getDateByWeekdayOnOrBefore(int weekday, int m, int d, DateTime startDate)
    {
      DateTime workDate = this.getFirstDayOfMonth(startDate);
      while (workDate.Month != m)
      {
        workDate = workDate.AddMonths(1);
      }
      workDate = workDate.AddDays(d - 1);

      while (weekday != (int)(workDate.DayOfWeek))
      {
        workDate = workDate.AddDays(1);
      }

      //It's possible the resulting date is before the specified starting date.  If so we'll calculate again for the next year.
      if (workDate < this.startingDate)
      {
        return this.getDateByWeekdayOnOrAfter(weekday, m, d, startDate.AddYears(1));
      }
      else
      {
        return workDate;
      }
    }

    /// <summary>
    /// Gets the n'th instance of a day-of-week in the given month after StartDate
    /// </summary>
    /// <param name="month">The month the Holiday falls on.</param>
    /// <param name="week">The instance of weekday that the Holiday falls on (5=last instance in the month).</param>
    /// <param name="weekday">The day of the week that the Holiday falls on.</param>
    /// <returns></returns>
    private DateTime getDateByMonthWeekWeekday(int month, int week, int weekday, DateTime startDate)
    {
      DateTime workDate = this.getFirstDayOfMonth(startDate);
      while (workDate.Month != month)
      {
        workDate = workDate.AddMonths(1);
      }
      while ((int)workDate.DayOfWeek != weekday)
      {
        workDate = workDate.AddDays(1);
      }

      DateTime result;
      if (week == 1)
      {
        result = workDate;
      }
      else
      {
        int addDays = (week * 7) - 7;
        int day = workDate.Day + addDays;
        if (day > DateTime.DaysInMonth(workDate.Year, workDate.Month))
        {
          day = day - 7;
        }
        result = new DateTime(workDate.Year, workDate.Month, day);
      }

      //It's possible the resulting date is before the specified starting date.  If so we'll calculate again for the next year.
      if (result >= this.startingDate)
      {
        return result;
      }
      else
      {
        return this.getDateByMonthWeekWeekday(month, week, weekday, startDate.AddYears(1));
      }
    }

    /// <summary>
    /// Returns the first day of the month for the specified date.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    private DateTime getFirstDayOfMonth(DateTime dt)
    {
      return new DateTime(dt.Year, dt.Month, 1);
    }

    /// <summary>
    /// Returns the first day of the year for the specified date.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    private DateTime getFirstDayOfYear(DateTime dt)
    {
      return new DateTime(dt.Year, 1, 1);
    }
    #endregion

    #region Holiday Object
    public class Holiday : IComparable
    {
      public System.DateTime Date;
      public string Name;
      public string ShortName;
      public string LocalName;

      #region IComparable Members

      public int CompareTo(object obj)
      {
        if (obj is Holiday)
        {
          Holiday h = (Holiday)obj;
          return this.Date.CompareTo(h.Date);
        }
        throw new ArgumentException("Object is not a Holiday");
      }
      #endregion
    }
    #endregion
  }
}

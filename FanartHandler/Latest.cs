//***********************************************************************
// Assembly         : FanartHandler
// Author           : cul8er
// Created          : 05-09-2010
//
// Last Modified By : cul8er
// Last Modified On : 10-05-2010
// Description      : 
//
// Copyright        : Open Source software licensed under the GNU/GPL agreement.
//***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FanartHandler
{
    class Latest
    {
        string dateAdded;
        string thumb;
        string fanart;
        string title;
        string subtitle;
        string artist;
        string album;
        string genre;
        string rating;
        string roundedRating;
        string classification;
        string runtime;
        string year;
        string seasonIndex;
        string episodeIndex;
        string thumbSeries;
        object playable;
        string fanart1;        
        string fanart2;
        string id;
        string summary;

        public string Summary
        {
            get { return summary; }
            set { summary = value; }
        }

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public string Fanart1
        {
            get { return fanart1; }
            set { fanart1 = value; }
        }

        public string Fanart2
        {
            get { return fanart2; }
            set { fanart2 = value; }
        }

        public object Playable
        {
            get { return playable; }
            set { playable = value; }
        }

        public string ThumbSeries
        {
            get { return thumbSeries; }
            set { thumbSeries = value; }
        }

        public string DateAdded
        {
            get { return dateAdded; }
            set { dateAdded = value; }
        }

        public string Thumb
        {
            get { return thumb; }
            set { thumb = value; }
        }

        public string Fanart
        {
            get { return fanart; }
            set { fanart = value; }
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public string Subtitle
        {
            get { return subtitle; }
            set { subtitle = value; }
        }

        public string Artist
        {
            get { return artist; }
            set { artist = value; }
        }

        public string Album
        {
            get { return album; }
            set { album = value; }
        }

        public string Genre
        {
            get { return genre; }
            set { genre = value; }
        }

        public string Rating
        {
            get { return rating; }
            set { rating = value; }
        }

        public string RoundedRating
        {
            get { return roundedRating; }
            set { roundedRating = value; }
        }

        public string Classification
        {
            get { return classification; }
            set { classification = value; }
        }

        public string Runtime
        {
            get { return runtime; }
            set { runtime = value; }
        }

        public string Year
        {
            get { return year; }
            set { year = value; }
        }

        public string SeasonIndex
        {
            get { return seasonIndex; }
            set { seasonIndex = value; }
        }

        public string EpisodeIndex
        {
            get { return episodeIndex; }
            set { episodeIndex = value; }
        }

        public Latest(string dateAdded, string thumb, string fanart, string title, string subtitle, string artist, string album, string genre, string rating, string roundedRating, string classification, string runtime, string year, string seasonIndex, string episodeIndex, string thumbSeries, object playable, string fanart1, string fanart2, string id, string summary)
        {
            this.dateAdded = dateAdded;
            this.thumb = thumb;
            this.fanart = fanart;
            this.title = title;
            this.subtitle = subtitle;
            this.artist = artist;
            this.album = album;
            if (genre != null && genre.Length > 0)
            {
                this.genre = genre.Replace("|",",");
            }
            else
            {
                this.genre = genre;
            }
            this.rating = rating;
            this.roundedRating = roundedRating;
            this.classification = classification;
            this.runtime = runtime;
            this.year = year;
            this.seasonIndex = seasonIndex;
            this.episodeIndex = episodeIndex;
            this.thumbSeries = thumbSeries;
            this.playable = playable;
            this.fanart1 = fanart1;
            this.fanart2 = fanart2;
            this.id = id;
            this.summary = summary;
        }

    }
}

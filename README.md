# mediaportal-fanart-handler
[![MP AnyCPU](https://img.shields.io/badge/MP-AnyCPU-blue?logo=windows&logoColor=white)](https://github.com/andrewjswan/mediaportal-fanart-handler/releases)
[![Build status](https://ci.appveyor.com/api/projects/status/nxaec1dxq8iq5nnb/branch/master?svg=true)](https://ci.appveyor.com/project/andrewjswan79536/mediaportal-fanart-handler/branch/master)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/andrewjswan/mediaportal-fanart-handler/build.yml?logo=github)](https://github.com/andrewjswan/mediaportal-fanart-handler/actions)
[![GitHub](https://img.shields.io/github/license/andrewjswan/mediaportal-fanart-handler?color=blue)](https://github.com/andrewjswan/mediaportal-fanart-handler/blob/master/LICENSE)
[![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/andrewjswan/mediaportal-fanart-handler?include_prereleases)](https://github.com/andrewjswan/mediaportal-fanart-handler/releases)
[![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/downloads-pre/andrewjswan/mediaportal-fanart-handler/latest/total?label=release@downloads)](https://github.com/andrewjswan/mediaportal-fanart-handler/releases)
[![StandWithUkraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://github.com/vshymanskyy/StandWithUkraine/blob/main/docs/README.md)

## Introduction
Fanart Handler is a plugin for **MediaPortal (MP)**. The plugin has two main purposes;

Search and download music fanart (scrape) from the Last.FM/Fanart.TV/TheAudioDB/CoverArtArchive site.
For all artists stored in your MP music database.<br/>
For any artist currently being played (on the fly).<br/>
Push fanart that you store on your local harddrive to the current used MP skin.<br/>
push fanart for now played music<br/>
push fanart for selected music or movie<br/>
push random images from selected folders<br/>
push random images for Latests media<br/>
push weather images for current weather<br/>
push holiday images for current holiday<br/>
and more ...

## Scope
The scope of this plugin is to:

scrape sites for music fanart for all artists stored in your MP music database.<br/>
scrape sites for music fanart for currently played artist.<br/>
push music fanart for now played music track (artist images)<br/>
push music fanart for browsed music artist in myMusic plugin<br/>
push music fanart for browsed/played music artist in myMusicPlaylist plugin<br/>
push music fanart for browsed music artist in GlobalSearch plugin<br/>
push music fanart for browsed music artist in Music Videos plugin<br/>
push music fanart for browsed music artist in mVidsplugin<br/>
push music fanart for browsed music artist in YouTube.FM plugin<br/>
push movie fanart for browsed video title in myVideo plugin<br/>
push scorecenter fanart for browsed category in myScoreCenter plugin<br/>
push weather fanart for current weather from World Weather / World Weather Lite<br/>
push fanart for current Holiday<br/>
push fanart for pictures from Windows 10 Spotlight<br/>
push cycling fanart from the following folders for use anywhere in a skin for<br/>
- thumbs\Skin Fanart\UserDef\games
- thumbs\Skin Fanart\UserDef\movies
- thumbs\Skin Fanart\UserDef\music
- thumbs\Skin Fanart\UserDef\albums
- thumbs\Skin Fanart\UserDef\pictures
- thumbs\Skin Fanart\UserDef\plugins
- thumbs\Skin Fanart\UserDef\tv
- thumbs\Skin Fanart\UserDef\scorecenter
- thumbs\Skin Fanart\UserDef\Holidays
- thumbs\Skin Fanart\Scraper\movies
- thumbs\Skin Fanart\Scraper\music
- thumbs\MovingPictures\Backdrops\FullSize
- thumbs\MyFilms\Fanart
- thumbs\Fan Art\fanart\original
- thumbs\Skin Fanart\Media\Weather\Backdrops
- thumbs\Music\Artists
- thumbs\Music\Albums
- thumbs\mvCentral\Artists\FullSize
- thumbs\mvCentral\Albums\FullSize
- Theme|Skin|\Media\Logos\Genres - Thumb\Skin Fanart\Media\Logos\Genres
- Theme|Skin|\Media\Logos\Genres\Music - Thumb\Skin Fanart\Media\Logos\Genres\Music
- Theme|Skin|\Media\Logos\Genres\Characters - Thumb\Skin Fanart\Media\Logos\Genres\Characters
- Theme|Skin|\Media\Logos\Studios - Thumb\Skin Fanart\Media\Logos\Studios
- Theme|Skin|\Media\Logos\Awards - Thumb\Skin Fanart\Media\Logos\Awards
- Theme|Skin|\Media\Logos\Holidays - Thumb\Skin Fanart\Media\Logos\Holidays

## Guides
- Installation Guide http://code.google.com/p/fanart-handler/wiki/InstallationGuide
- User Guide http://code.google.com/p/fanart-handler/wiki/UserGuide
- Skinners Guide http://code.google.com/p/fanart-handler/wiki/SkinnersGuide

## Requirements
- MediaPortal
- A skin that supports the Fanart Handler.
NOTE! This plugin only supports JPG images.

## Additional for Skin
- Studios - https://github.com/andrewjswan/mediaportal.images.studios
- Genres - https://github.com/andrewjswan/mediaportal.images.genres
- Awards - https://github.com/andrewjswan/mediaportal.images.awards
- Weather - https://github.com/andrewjswan/mediaportal.images.weather
- Holiday - https://github.com/andrewjswan/mediaportal.images.holidays

## Supported Skins
Titan ( full support in ajs Theme for Titan - https://github.com/andrewjswan/mediaportal-skin-themes )
DWHD
aMPed
X-Factor
Please let me know if your skin supports the Fanart Handler and I will add it to the list.

## Installation (MPE1 FILE)
Please see the installation guide (link).

## Forum
A sub-forum is located on the MediaPortal website. For up-to-date information and community support please use this forum.
Link: http://forum.team-mediaportal.com/forums/fanart-handler.535/

## Credits
To the team behind the MP Moving Picture plugin. From who I have used some of the utils code and also got the idea for using two images for smoth image transitions.

## Properties
<pre>
#fanarthandler.scraper.task
#fanarthandler.scraper.percent.completed
#fanarthandler.scraper.percent.sign

#fanarthandler.games.userdef.available
#fanarthandler.games.userdef.backdrop1.any
#fanarthandler.games.userdef.backdrop2.any

#fanarthandler.movie.latests.available
#fanarthandler.movie.userdef.available
#fanarthandler.movie.scraper.available
#fanarthandler.movie.userdef.backdrop1.any
#fanarthandler.movie.userdef.backdrop2.any
#fanarthandler.movie.scraper.backdrop1.any
#fanarthandler.movie.scraper.backdrop2.any
#fanarthandler.movie.latests.backdrop1.any
#fanarthandler.movie.latests.backdrop2.any
#fanarthandler.movie.backdrop1.selected
#fanarthandler.movie.backdrop2.selected
#fanarthandler.movie.studios.selected.single
#fanarthandler.movie.studios.selected.all
#fanarthandler.movie.studios.selected.verticalall
#fanarthandler.movie.genres.selected.single
#fanarthandler.movie.genres.selected.all
#fanarthandler.movie.genres.selected.verticalall
#fanarthandler.movie.awards.selected.single
#fanarthandler.movie.awards.selected.all
#fanarthandler.movie.awards.selected.verticalall
#fanarthandler.movie.awards.selected.text
#fanarthandler.movie.animated.selected.thumb
#fanarthandler.movie.animated.selected.background
#fanarthandler.movie.clearart.selected
#fanarthandler.movie.clearlogo.selected
#fanarthandler.movie.cd.selected

#fanarthandler.movingpicture.latests.available
#fanarthandler.movingpicture.available
#fanarthandler.movingpicture.backdrop1.any
#fanarthandler.movingpicture.backdrop2.any
#fanarthandler.movingpicture.latests.backdrop1.any
#fanarthandler.movingpicture.latests.backdrop2.any

#fanarthandler.music.latests.available
#fanarthandler.music.userdef.available
#fanarthandler.music.scraper.available
#fanarthandler.music.userdef.backdrop1.any
#fanarthandler.music.userdef.backdrop2.any
#fanarthandler.music.scraper.backdrop1.any
#fanarthandler.music.scraper.backdrop2.any
#fanarthandler.music.latests.backdrop1.any
#fanarthandler.music.latests.backdrop2.any
#fanarthandler.music.overlay.play
#fanarthandler.music.artisthumb.play
#fanarthandler.music.artistclearart.play
#fanarthandler.music.artistbanner.play
#fanarthandler.music.albumcd.play
#fanarthandler.music.backdrop1.play
#fanarthandler.music.backdrop2.play
#fanarthandler.music.backdrop1.selected
#fanarthandler.music.backdrop2.selected
#fanarthandler.music.artistclearart.selected
#fanarthandler.music.artistbanner.selected

#fanarthandler.music.labels.play
#fanarthandler.music.labels.selected

#fanarthandler.music.genres.play.single
#fanarthandler.music.genres.play.all
#fanarthandler.music.genres.play.verticalall
#fanarthandler.music.genres.selected.single
#fanarthandler.music.genres.selected.all
#fanarthandler.music.genres.selected.verticalall

#fanarthandler.mvcentral.latests.available
#fanarthandler.mvcentral.latests.backdrop1.any
#fanarthandler.mvcentral.latests.backdrop2.any

#fanarthandler.pictures.slideshow.translation
#fanarthandler.pictures.slideshow.enabled

#fanarthandler.picture.userdef.available
#fanarthandler.picture.backdrop1.selected
#fanarthandler.picture.backdrop2.selected
#fanarthandler.picture.userdef.backdrop1.any
#fanarthandler.picture.userdef.backdrop2.any

#fanarthandler.scorecenter.userdef.available
#fanarthandler.scorecenter.backdrop1.selected
#fanarthandler.scorecenter.backdrop2.selected
#fanarthandler.scorecenter.userdef.backdrop1.any
#fanarthandler.scorecenter.userdef.backdrop2.any

#fanarthandler.tvseries.latests.available
#fanarthandler.tvseries.available
#fanarthandler.tvseries.backdrop1.any
#fanarthandler.tvseries.backdrop2.any
#fanarthandler.tvseries.latests.backdrop1.any
#fanarthandler.tvseries.latests.backdrop2.any
#fanarthandler.tvseries.clearart.selected
#fanarthandler.tvseries.clearlogo.selected

#fanarthandler.tv.userdef.available
#fanarthandler.tv.userdef.backdrop1.any
#fanarthandler.tv.userdef.backdrop2.any

#fanarthandler.myfilms.latests.available
#fanarthandler.myfilms.available
#fanarthandler.myfilms.backdrop1.any
#fanarthandler.myfilms.backdrop2.any
#fanarthandler.myfilms.latests.backdrop1.any
#fanarthandler.myfilms.latests.backdrop2.any

#fanarthandler.plugins.userdef.available
#fanarthandler.plugins.userdef.backdrop1.any
#fanarthandler.plugins.userdef.backdrop2.any

#fanarthandler.showtimes.available
#fanarthandler.showtimes.backdrop1.any
#fanarthandler.showtimes.backdrop2.any

#fanarthandler.spotlight.available
#fanarthandler.spotlight.backdrop1.any
#fanarthandler.spotlight.backdrop2.any

#fanarthandler.weather.season
#fanarthandler.weather.backdrop1
#fanarthandler.weather.backdrop2

#fanarthandler.holiday.backdrop1
#fanarthandler.holiday.backdrop2
#fanarthandler.holiday.current - Holiday name
#fanarthandler.holiday.icon - Holiday icon
</pre>

## Defines
<pre>
#useRandomGamesUserFanart:Yes
#useRandomMoviesUserFanart:Yes
#useRandomMoviesScraperFanart:Yes
#useRandomMovingPicturesFanart:Yes
#useRandomMusicUserFanart:Yes
#useRandomMusicScraperFanart:Yes
#useRandomPicturesUserFanart:Yes
#useRandomPluginsUserFanart:Yes
#useRandomScoreCenterUserFanart:Yes 
#useRandomTVUserFanart:Yes
#useRandomTVSeriesFanart:Yes
#useRandomMyFilmsFanart:Yes

#usePlayFanart:Yes

#useRandomLatestsMusicFanart:Yes
#useRandomLatestsMvCentralFanart:Yes
#useRandomLatestsMovieFanart:Yes
#useRandomLatestsMovingPicturesFanart:Yes
#useRandomLatestsTVSeriesFanart:Yes
#useRandomLatestsMyFilmsFanart:Yes

#useRandomSpotLightsFanart:Yes
</pre>

## Window Control IDs
<pre>
91919297 - Random/Random Latests Fanart 1 Showed
91919298 - Random/Random Latests Fanart 2 Showed
91919299 - Random/Random Latests Fanart Available

91919291 - Selected Fanart 1 Showed
91919292 - Selected Fanart 2 Showed
91919293 - Selected Fanart Available

91919295 - Playing Fanart 1 Showed
91919296 - Playing Fanart 2 Showed
91919294 - Playing Fanart Available

91919283 - Current Weather Fanart 1 Showed 
91919281 - Current Weather Fanart 2 Showed 
91919282 - Current Weather Fanart Available

91919285 - Holiday Fanart 1 Showed
91919286 - Holiday Fanart 2 Showed
91919284 - Holiday Fanart Available
</pre>

## Additional settings for Holidays. (FanartHandler.xml)
```xml
  <section name="FanartHandler">
    <entry name="HolidayCountry">UA</entry>
  </section>
```
- HolidayCountry - The country of the holiday coincides, defaults to the language selected in the Mediaportal. (UA, DE, HE, RU, etc)

## Additional settings for displaying pictures. (FanartHandler.xml)
```xml
  <section name="OtherPicturesView">
    <entry name="MaxAwards">10</entry>
    <entry name="MaxGenres">10</entry>
    <entry name="MaxStudios">10</entry>
    <entry name="MaxRandomFanart">10</entry>
  </section>
```
- MaxAwards - Max awards images per stack
- MaxGenres - Max genres images per stack
- MaxStudios - Max studios images per stack
- MaxRandomFanart - Max Random Fanart pack images for show, ie if MaxRandomFanart = 10, then Random images for show = 10 * ScraperMaxImages.

```xml
  <section name="SpotLight">
    <entry name="Max">10</entry>
  </section>
```
- Max - Max Windows 10 SpotLight images in MP Thumb folder, The oldest ones are deleted, the new ones are added.

## Additional settings for MBID search Providers. (FanartHandler.xml)
```xml
  <section name="MBID">
    <entry name="ArtistProviders">TheAudioDB|LastFM|MusicBrainz</entry>
    <entry name="AlbumProviders">MusicBrainz|TheAudioDB|LastFM</entry>
  </section>
```
- Allows you to change the priority of the search MBID by providers, as well as turn off the necessary ones (remove from the list).

## Additional settings for Artists. (FanartHandler.xml)
```xml
  <section name="Advanced">
    <entry name="SkipFeatArtist">yes</entry>
  </section>
```
- SkipFeatArtist - When processing, skip feat Artists (Default: no - Don't skip)

## Advanced debug Info (FanartHandler.xml)
```xml
  <section name="Debug">
    <entry name="AdvancedDebug">yes</entry>
  </section>
```
- Displays the maximum debug information in log (the amount of information depends on the plugin version).

# Experemental function. (FanartHandler.xml)
## :warning: CAUTION: Use at your own risk

### Clean old unused pictures (not in MP music/music video DB) 
Suitable for cleaning images that were downloaded while listening to the radio. Or when deleting music from the collection. If the image has not been used for **more than 100 days**, it is subject to cleaning.
```xml
  <section name="CleanUp">
    <entry name="CleanUpOldFiles">yes</entry>
    <entry name="CleanUpDelete">yes</entry>
  </section>
```
- CleanUpOldFiles - Look for outdated image files.
- CleanUpDelete - Delete outdated image files, if value **no**, just output them to the FanartHandler log file.

### Find information about Actors, Albums and update the music database. Replacement for MusicInfoHandler 
```xml
  <section name="MusicInfo">
    <entry name="GetArtistInfo">yes</entry>
    <entry name="GetAlbumInfo">yes</entry>
    <entry name="InfoLanguage">EN</entry>
    <entry name="FullScanInfo">yes</entry>
  </section>
```
- GetArtistInfo - Get info for Artists.
- GetAlbumInfo - Get info for Albums.
- InfoLanguage - Info language.
- FullScanInfo - Make full scan of Music DB (Once in 30 days).

### Find information about Movies Awards 
```xml
  <section name="MoviesInfo">
    <entry name="GetMoviesAwards">yes</entry>
  </section>
```
- GetMoviesAwards - Get info for Movies Awards.
Used scripts from MP Config folder (..\Team MediaPortal\MediaPortal\FanartHandler\Scripts)

# mediaportal-fanart-handler

#Introduction
Fanart Handler is a plugin for MediaPortal (MP). The plugin has two main purposes;

Search and download music fanart (scrape) from the htbackdrops/last.fm/fanart.tv site.
for all artists stored in your MP music database.<br/>
for any artist currently being played (on the fly).<br/>
Push fanart that you store on your local harddrive to the current used MP skin.<br/>
push fanart for now played music<br/>
push fanart for selected music or movie<br/>
push random images from selected folders<br/>
push random images for Latests media<br/>
push weather images for current weather<br/>
push holiday images for current holiday<br/>
and more ...

#Scope
The scope of this plugin is to:

scrape htbackdrops site for music fanart for all artists stored in your MP music database.<br/>
scrape htbackdrops site for music fanart for currently played artist.<br/>
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

#Guides
- Installation Guide http://code.google.com/p/fanart-handler/wiki/InstallationGuide
- User Guide http://code.google.com/p/fanart-handler/wiki/UserGuide
- Skinners Guide http://code.google.com/p/fanart-handler/wiki/SkinnersGuide

#Requirements
- MediaPortal
- A skin that supports the Fanart Handler.
NOTE! This plugin only supports JPG images.

#Additional for Skin
Studios - https://github.com/andrewjswan/mediaportal.images.studios
Genres - https://github.com/andrewjswan/mediaportal.images.genres
Awards - https://github.com/andrewjswan/mediaportal.images.awards
Weather - https://github.com/andrewjswan/mediaportal.images.weather
Holiday - https://github.com/andrewjswan/mediaportal.images.holiday

#Supported Skins
Titan ( full support in ajs Theme for Titan - https://github.com/andrewjswan/mediaportal-skin-themes )
DWHD
aMPed
X-Factor
Please let me know if your skin supports the Fanart Handler and I will add it to the list

#Installation (MPE1 FILE)
Please see the installation guide (link).

#Forum
A sub-forum is located on the MediaPortal website. For up-to-date information and community support please use this forum.
Link: http://forum.team-mediaportal.com/forums/fanart-handler.535/

#Credits
To the team behind the MP Moving Picture plugin. From who I have used some of the utils code and also got the idea for using two images for smoth image transitions.

#Properties
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

#fanarthandler.weather.backdrop1
#fanarthandler.weather.backdrop2

#fanarthandler.holiday.backdrop1
#fanarthandler.holiday.backdrop2
#fanarthandler.holiday.current - Holiday name
#fanarthandler.holiday.icon - Holiday icon
</pre>

#Defines
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
</pre>

#Window Control IDs
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

Automatically exported from code.google.com/p/mediaportal-fanart-handler
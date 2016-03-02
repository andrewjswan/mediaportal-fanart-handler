# mediaportal-fanart-handler

#Introduction
Fanart Handler is a plugin for MediaPortal (MP). The plugin has two main purposes;

Search and download music fanart (scrape) from the htbackdrops site.
for all artists stored in your MP music database.<br/>
for any artist currently being played (on the fly).<br/>
Push fanart that you store on your local harddrive to the current used MP skin.<br/>
push fanart for now played music<br/>
push fanart for selected music or movie<br/>
push random images from selected folders<br/>

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
push cycling fanart from the following folders for use anywhere in a skin for<br/>
- thumbs\MovingPictures\Backdrops\FullSize
- thumbs\Fan Art\fanart\original
- thumbs\Skin Fanart\games
- thumbs\Skin Fanart\movies
- thumbs\Skin Fanart\music
- thumbs\Skin Fanart\pictures
- thumbs\Skin Fanart\plugins
- thumbs\Skin Fanart\tv

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

#Credits
To the team behind the MP Moving Picture plugin. From who I have used some of the utils code and also got the idea for using two images for smoth image transitions.

#Properties
<pre>
#fanarthandler.scraper.task
#fanarthandler.scraper.percent.completed
#fanarthandler.scraper.percent.sign

#fanarthandler.games.userdef.backdrop1.any
#fanarthandler.games.userdef.backdrop2.any

#fanarthandler.movie.userdef.backdrop1.any
#fanarthandler.movie.userdef.backdrop2.any
#fanarthandler.movie.scraper.backdrop1.any
#fanarthandler.movie.scraper.backdrop2.any
#fanarthandler.movie.backdrop1.selected
#fanarthandler.movie.backdrop2.selected
#fanarthandler.movie.studios.selected
#fanarthandler.movie.studios.selected.all
#fanarthandler.movie.studios.selected.verticalall
#fanarthandler.movie.genres.selected
#fanarthandler.movie.genres.selected.all
#fanarthandler.movie.genres.selected.verticalall

#fanarthandler.movingpicture.backdrop1.any
#fanarthandler.movingpicture.backdrop2.any

#fanarthandler.music.userdef.backdrop1.any
#fanarthandler.music.userdef.backdrop2.any
#fanarthandler.music.scraper.backdrop1.any
#fanarthandler.music.scraper.backdrop2.any
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
#fanarthandler.music.genres.play
#fanarthandler.music.genres.play.all
#fanarthandler.music.genres.play.verticalall
#fanarthandler.music.genres.selected
#fanarthandler.music.genres.selected.all
#fanarthandler.music.genres.selected.verticalall

#fanarthandler.pictures.slideshow.translation
#fanarthandler.pictures.slideshow.enabled

#fanarthandler.picture.backdrop1.selected
#fanarthandler.picture.backdrop2.selected
#fanarthandler.picture.userdef.backdrop1.any
#fanarthandler.picture.userdef.backdrop2.any

#fanarthandler.scorecenter.backdrop1.selected
#fanarthandler.scorecenter.backdrop2.selected
#fanarthandler.scorecenter.userdef.backdrop1.any
#fanarthandler.scorecenter.userdef.backdrop2.any

#fanarthandler.tvseries.backdrop1.any
#fanarthandler.tvseries.backdrop2.any
#fanarthandler.tv.userdef.backdrop1.any
#fanarthandler.tv.userdef.backdrop2.any

#fanarthandler.plugins.userdef.backdrop1.any
#fanarthandler.plugins.userdef.backdrop2.any
</pre>
Automatically exported from code.google.com/p/mediaportal-fanart-handler

@echo off
cls
Title Creating MediaPortal FanartHandler Installer

:: Check for modification
svn status ..\source | findstr "^M"
if ERRORLEVEL 1 (
	echo No modifications in source folder.
) else (
	echo There are modifications in source folder. Aborting.
	pause
	exit 1
)

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%	
:CONT

:: Get version from DLL
FOR /F "tokens=1-3" %%i IN ('Tools\sigcheck.exe "..\FanartHandler\bin\Release\FanartHandler.dll"') DO ( IF "%%i %%j"=="File version:" SET version=%%k )

:: trim version
SET version=%version:~0,-1%

:: Temp xmp2 file
copy fanarthandler.xmp2 fanarthandlerTemp.xmp2

:: Sed "fanarthandler-{VERSION}.xml" from xmp2 file
Tools\sed.exe -i "s/update-{VERSION}.xml/update-%version%.xml/g" fanarthandlerTemp.xmp2

:: Build MPE1
"%PROGS%\Team MediaPortal\MediaPortal\MPEMaker.exe" fanarthandlerTemp.xmp2 /B /V=%version% /UpdateXML

:: Cleanup
del fanarthandlerTemp.xmp2

:: Sed "FanartHandler-{VERSION}.MPE1" from update.xml
Tools\sed.exe -i "s/FanartHandler-{VERSION}.MPE1/FanartHandler-%version%.MPE1/g" update-%version%.xml

:: Parse version (Might be needed in the futute)
FOR /F "tokens=1-4 delims=." %%i IN ("%version%") DO ( 
	SET major=%%i
	SET minor=%%j
	SET build=%%k
	SET revision=%%l
)

:: Rename MPE1
if exist "..\builds\FanartHandler-%major%.%minor%.%build%.%revision%.MPE1" del "..\builds\FanartHandler-%major%.%minor%.%build%.%revision%.MPE1"
rename ..\builds\FanartHandler-MAJOR.MINOR.BUILD.REVISION.MPE1 "FanartHandler-%major%.%minor%.%build%.%revision%.MPE1"



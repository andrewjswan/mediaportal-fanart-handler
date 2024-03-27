@echo off
cls
Title Creating MediaPortal FanartHandler Installer

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
    :: 64-bit
    set PROGS=%programfiles(x86)%
    goto CONT
:32BIT
    set PROGS=%ProgramFiles%
:CONT

IF NOT EXIST "%PROGS%\Team MediaPortal\MediaPortal\" SET PROGS=C:

:: Get version from DLL
FOR /F "tokens=*" %%i IN ('Tools\sigcheck.exe /accepteula /nobanner /n "..\FanartHandler\bin\Release\FanartHandler.dll"') DO (SET version=%%i)

:: Temp xmp2 file
copy /Y fanarthandler.xmp2 fanarthandlertemp.xmp2

:: Build MPE1
"%PROGS%\Team MediaPortal\MediaPortal\MPEMaker.exe" fanarthandlertemp.xmp2 /B /V=%version% /UpdateXML

:: Cleanup
del fanarthandlertemp.xmp2


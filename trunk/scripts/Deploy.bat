@echo off
cls
Title Deploying MediaPortal FanartHandler (RELEASE)
cd ..

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%	
:CONT

copy /y "FanartHandler\bin\Release\FanartHandler.dll" "%PROGS%\Team MediaPortal\MediaPortal\plugins\process\"

cd scripts
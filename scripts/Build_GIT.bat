@echo off
cls
Title Building MediaPortal Fanart Handler (RELEASE)
cd ..

setlocal enabledelayedexpansion

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%
:CONT
IF NOT EXIST "%PROGS%\Team MediaPortal\MediaPortal\" SET PROGS=C:

: Prepare version
for /f "tokens=*" %%a in ('git rev-list HEAD --count') do set REVISION=%%a 
set REVISION=%REVISION: =%
"scripts\Tools\sed.exe" -i -r "s/(Assembly(File)?Version\(.[0-9]+\.[0-9]+\.[0-9]+\.)[0-9]+(.\))/\1%REVISION%\3/g" FanartHandler\Properties\AssemblyInfo.cs
	
:: Build
"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\MSBUILD.exe" /target:Rebuild /property:Configuration=RELEASE /fl /flp:logfile=FanartHandler.log;verbosity=diagnostic FanartHandler.sln

: Revert version
git checkout FanartHandler\Properties\AssemblyInfo.cs

cd scripts

pause


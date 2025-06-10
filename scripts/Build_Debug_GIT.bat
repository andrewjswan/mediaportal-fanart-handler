@ECHO OFF
CLS
TITLE Building MediaPortal Fanart Handler (DEBUG)
CD ..

SETLOCAL ENABLEDELAYEDEXPANSION

:: Prepare version
FOR /f "tokens=*" %%a IN ('git rev-list HEAD --count') DO SET REVISION=%%a 
SET REVISION=%REVISION: =%
"scripts\Tools\sed.exe" -i -r "s/(Assembly(File)?Version\(.[0-9]+\.[0-9]+\.[0-9]+\.)[0-9]+(.\))/\1%REVISION%\3/g" FanartHandler\Properties\AssemblyInfo.cs
	
:: Build
FOR %%p IN ("%PROGRAMFILES(x86)%" "%PROGRAMFILES%") DO (
  FOR %%s IN (2019 2022) DO (
    FOR %%e IN (Community Professional Enterprise BuildTools) DO (
      SET PF=%%p
      SET PF=!PF:"=!
      SET MSBUILD_PATH="!PF!\Microsoft Visual Studio\%%s\%%e\MSBuild\Current\Bin\MSBuild.exe"
      IF EXIST "!MSBUILD_PATH!" GOTO :BUILD
    )
  )
)

:BUILD

%MSBUILD_PATH% /target:Rebuild /property:Configuration=DEBUG /fl /flp:logfile=FanartHandler.log;verbosity=diagnostic FanartHandler.sln

:: Revert version
git checkout FanartHandler\Properties\AssemblyInfo.cs

CD scripts

PAUSE

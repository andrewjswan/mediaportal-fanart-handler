@echo off

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
    :: 64-bit
    set PROGS=%programfiles(x86)%
    goto CONT
:32BIT
    set PROGS=%ProgramFiles%
:CONT

if exist FanartHandler_UNMERGED.dll del FanartHandler_UNMERGED.dll
ren FanartHandler.dll FanartHandler_UNMERGED.dll 
ilmerge.exe /out:FanartHandler.dll FanartHandler_UNMERGED.dll Nlog.dll /target:dll /targetplatform:"v4,%PROGS%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2" /wildcards

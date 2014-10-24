@echo off
IF EXIST FanartHandler_UNMERGED.dll del FanartHandler_UNMERGED.dll
ren FanartHandler.dll FanartHandler_UNMERGED.dll
ilmerge /out:FanartHandler.dll FanartHandler_UNMERGED.dll NLog.dll

@echo off
REM Install ARR v2.5
SET WebPICmd=%~dp0..\webpicmd.exe
SET APPCMD=%windir%\system32\inetsrv\appcmd.exe

if not exist %WebPICmd% goto error

%webpicmd% /Install /AcceptEula /SuppressReboot  /Products:ARRv2_5
%appcmd% unlock config /section:system.webServer/proxy
exit /b 0

:error
echo Could not find webpicmd
exit /b 1
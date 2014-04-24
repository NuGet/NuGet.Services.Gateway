@echo off
REM Install ARR v2.5
SET WebPICmd=%~dp0..\webpicmd.exe

if not exist %WebPICmd% goto error

%~dp0..\webpicmd /Install /AcceptEula /SuppressReboot  /Products:ARRv2_5 >> installlog.txt 2>&1
exit /b 0

:error
echo Could not find webpicmd
exit /b 1
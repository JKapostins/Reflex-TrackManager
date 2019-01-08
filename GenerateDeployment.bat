@echo off
rem Embed admin manifest into the exe
call "C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin\mt.exe" -nologo -manifest %~dp0\trackmanager\TrackManager\app1.manifest -outputresource:%~dp0\local\windows-x86\trackmanager\TrackManager\Publish\TrackManager.exe

rem Copy track manager binaries into the publish folder
for %%e in (exe dll json) do (
    XCOPY "%~dp0\local\windows-x86\trackmanager\TrackManager\Publish\*.%%e" %~dp0\local\TrackManager /S /F /H /i /y
)

rem Copy the updater binaries into the publish folder
for %%e in (exe dll config) do (
    XCOPY "%~dp0\local\windows-x86\trackmanager\TrackManagerUpdater\Release\*.%%e" %~dp0\local\TrackManager\Update /S /F /H /i /y
)

rem Copy the injector into the publish folder
XCOPY "%~dp0\local\windows-x86\injector\Release\*.exe" %~dp0\local\TrackManager\Overlay /S /F /H /i /y

rem Copy the overlay into the publish folder
XCOPY "%~dp0\local\windows-x86\Indicium-Supra\Release\x86\*.dll" %~dp0\local\TrackManager\Overlay /S /F /H /i /y
XCOPY "%~dp0\local\windows-x86\overlay\Release\*.dll" %~dp0\local\TrackManager\Overlay /S /F /H /i /y
pause

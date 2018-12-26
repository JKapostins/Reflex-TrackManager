@echo off
set "buildRoot=%~dp0\local\windows-x86"

if not exist %buildRoot% (
mkdir %buildRoot%
)

cd %buildRoot%
call cmake -G "Visual Studio 15" ../../

pause

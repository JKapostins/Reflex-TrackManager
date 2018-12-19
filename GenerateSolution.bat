@echo off
set "buildRoot=%~dp0\build\"

set "awsRoot=%~dp0\third-party\aws-sdk-cpp"
if not exist %awsRoot% (
call git submodule init
call git submodule update
)

if not exist %buildRoot% (
mkdir %buildRoot%
)

cd %buildRoot%
call cmake -G "Visual Studio 15" ../

pause

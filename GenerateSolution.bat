@echo off
set "buildRoot=%~dp0\local\windows-x86"
set "vcpkgExe=%buildRoot%\third-party\vcpkg\vcpkg.exe"

if not exist %buildRoot% (
mkdir %buildRoot%
)

call git submodule init
call git submodule update

if not exist %vcpkgExe% (
call %buildRoot%\third-party\vcpkg\bootstrap-vcpkg.bat
)

call %vcpkgExe% install grpc:x86-windows
call %vcpkgExe% install libjpeg-turbo:x86-windows

rem call %buildRoot%\

cd %buildRoot%
call cmake -G "Visual Studio 15" -DCMAKE_TOOLCHAIN_FILE=%buildRoot%\third-party\vcpkg\scripts\buildsystems\vcpkg.cmake ../../

pause

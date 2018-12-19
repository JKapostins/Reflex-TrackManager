## # vcpkg_install_cmake
##
## Build and install a cmake project.
##
## ## Usage:
## ```cmake
## vcpkg_install_cmake(...)
## ```
##
## ## Parameters:
## See [`vcpkg_build_cmake()`](vcpkg_build_cmake.md).
##
## ## Notes:
## This command transparently forwards to [`vcpkg_build_cmake()`](vcpkg_build_cmake.md), adding a `TARGET install`
## parameter.
##
## ## Examples:
##
## * [zlib](https://github.com/Microsoft/vcpkg/blob/master/ports/zlib/portfile.cmake)
## * [cpprestsdk](https://github.com/Microsoft/vcpkg/blob/master/ports/cpprestsdk/portfile.cmake)
## * [poco](https://github.com/Microsoft/vcpkg/blob/master/ports/poco/portfile.cmake)
## * [opencv](https://github.com/Microsoft/vcpkg/blob/master/ports/opencv/portfile.cmake)
function(vcpkg_install_cmake)
    vcpkg_build_cmake(LOGFILE_ROOT install TARGET install ${ARGN})
endfunction()

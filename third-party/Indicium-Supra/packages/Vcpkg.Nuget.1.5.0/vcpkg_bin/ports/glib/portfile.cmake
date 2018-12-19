 # Glib uses winapi functions not available in WindowsStore
if(VCPKG_CMAKE_SYSTEM_NAME STREQUAL WindowsStore)
    message(FATAL_ERROR "Error: UWP builds are currently not supported.")
endif()

# Glib relies on DllMain on Windows
if(NOT VCPKG_CMAKE_SYSTEM_NAME)
    if(VCPKG_LIBRARY_LINKAGE STREQUAL "static")
        message("Glib relies on DllMain and therefore cannot be built statically")
        set(VCPKG_LIBRARY_LINKAGE "dynamic")
    endif()
    if(VCPKG_CRT_LINKAGE STREQUAL "static")
        message(FATAL_ERROR "Glib only supports dynamic library and crt linkage")
    endif()
endif()

include(vcpkg_common_functions)
set(GLIB_VERSION 2.52.3)
set(SOURCE_PATH ${CURRENT_BUILDTREES_DIR}/src/glib-${GLIB_VERSION})
vcpkg_download_distfile(ARCHIVE
    URLS "https://ftp.gnome.org/pub/gnome/sources/glib/2.52/glib-${GLIB_VERSION}.tar.xz"
    FILENAME "glib-${GLIB_VERSION}.tar.xz"
    SHA512 a068f2519cfb82de8d4b7f004e7c1f15e841cad4046430a83b02b359d011e0c4077cdff447a1687ed7c68f1a11b4cf66b9ed9fc23ab5f0c7c6be84eb0ddc3017)

vcpkg_extract_source_archive(${ARCHIVE})
vcpkg_apply_patches(
    SOURCE_PATH ${SOURCE_PATH}
    PATCHES
        ${CMAKE_CURRENT_LIST_DIR}/use-libiconv-on-windows.patch)

file(COPY ${CMAKE_CURRENT_LIST_DIR}/CMakeLists.txt DESTINATION ${SOURCE_PATH})
file(COPY ${CMAKE_CURRENT_LIST_DIR}/cmake DESTINATION ${SOURCE_PATH})
file(REMOVE_RECURSE ${SOURCE_PATH}/glib/pcre)
file(WRITE ${SOURCE_PATH}/glib/pcre/Makefile.in)
file(REMOVE ${SOURCE_PATH}/glib/win_iconv.c)

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA
    OPTIONS
        -DGLIB_VERSION=${GLIB_VERSION}
    OPTIONS_DEBUG
        -DGLIB_SKIP_HEADERS=ON
        -DGLIB_SKIP_TOOLS=ON
)

vcpkg_install_cmake()
vcpkg_fixup_cmake_targets(CONFIG_PATH share/unofficial-glib TARGET_PATH share/unofficial-glib)

vcpkg_copy_pdbs()
vcpkg_copy_tool_dependencies(${CURRENT_PACKAGES_DIR}/tools/glib)

file(COPY ${SOURCE_PATH}/COPYING DESTINATION ${CURRENT_PACKAGES_DIR}/share/glib)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/glib/COPYING ${CURRENT_PACKAGES_DIR}/share/glib/copyright)


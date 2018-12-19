# Glib uses winapi functions not available in WindowsStore, so glibmm also
if (VCPKG_CMAKE_SYSTEM_NAME STREQUAL WindowsStore)
    message(FATAL_ERROR "Error: UWP builds are currently not supported.")
endif()

include(vcpkg_common_functions)
set(SOURCE_PATH ${CURRENT_BUILDTREES_DIR}/src/glibmm-2.52.1)
vcpkg_download_distfile(ARCHIVE
    URLS "http://ftp.gnome.org/pub/GNOME/sources/glibmm/2.52/glibmm-2.52.1.tar.xz"
    FILENAME "glibmm-2.52.1.tar.xz"
    SHA512 702158762cb28972b315ab98dc00a62e532bda08b6e76dc2a2556e8cb381c2021290891887a4af2fbff5a62bab4d50581be73037dc8e0dc47d5febd6cbeb7bda
)
vcpkg_extract_source_archive(${ARCHIVE})

vcpkg_apply_patches(
    SOURCE_PATH ${SOURCE_PATH}
    PATCHES ${CMAKE_CURRENT_LIST_DIR}/glibmm-api-variant.patch
)

file(COPY ${CMAKE_CURRENT_LIST_DIR}/CMakeLists.txt DESTINATION ${SOURCE_PATH})

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA
    OPTIONS
        -DWARNINGS_HEADER=${CMAKE_CURRENT_LIST_DIR}/msvc_recommended_pragmas.h
    OPTIONS_DEBUG
        -DDISABLE_INSTALL_HEADERS=ON
)

vcpkg_install_cmake()

vcpkg_copy_pdbs()

# Handle copyright and readme
file(INSTALL ${SOURCE_PATH}/COPYING DESTINATION ${CURRENT_PACKAGES_DIR}/share/glibmm RENAME copyright)
file(INSTALL ${SOURCE_PATH}/README DESTINATION ${CURRENT_PACKAGES_DIR}/share/glibmm RENAME readme.txt)

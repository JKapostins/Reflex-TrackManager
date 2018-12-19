include(vcpkg_common_functions)

vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO glennrp/libpng
    REF v1.6.35
    SHA512 1b6e2664bd80726e8f6ef7ea4f23d311a883841086a4a60700dfd11621130808c24487c744c8a942219980eb29b244e81965aebfc4ab7f637693f537f4e8e148
    HEAD_REF master
)

vcpkg_apply_patches(
    SOURCE_PATH ${SOURCE_PATH}
    PATCHES
        ${CMAKE_CURRENT_LIST_DIR}/use-abort-on-all-platforms.patch
        ${CMAKE_CURRENT_LIST_DIR}/skip-install-symlink.patch
)

if(VCPKG_LIBRARY_LINKAGE STREQUAL dynamic)
    set(PNG_STATIC_LIBS OFF)
    set(PNG_SHARED_LIBS ON)
else()
    set(PNG_STATIC_LIBS ON)
    set(PNG_SHARED_LIBS OFF)
endif()

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA
    OPTIONS
        -DPNG_STATIC=${PNG_STATIC_LIBS}
        -DPNG_SHARED=${PNG_SHARED_LIBS}
        -DPNG_TESTS=OFF
        -DSKIP_INSTALL_PROGRAMS=ON
        -DSKIP_INSTALL_EXECUTABLES=ON
        -DSKIP_INSTALL_FILES=ON
        -DSKIP_INSTALL_SYMLINK=ON
    OPTIONS_DEBUG
        -DSKIP_INSTALL_HEADERS=ON
)

vcpkg_install_cmake()

if(VCPKG_LIBRARY_LINKAGE STREQUAL "static")
    if(EXISTS ${CURRENT_PACKAGES_DIR}/lib/libpng16_static.lib)
        file(RENAME ${CURRENT_PACKAGES_DIR}/lib/libpng16_static.lib ${CURRENT_PACKAGES_DIR}/lib/libpng16.lib)
    endif()
    if(EXISTS ${CURRENT_PACKAGES_DIR}/debug/lib/libpng16_staticd.lib)
        file(RENAME ${CURRENT_PACKAGES_DIR}/debug/lib/libpng16_staticd.lib ${CURRENT_PACKAGES_DIR}/debug/lib/libpng16d.lib)
    endif()
endif()

# Remove CMake config files as they are incorrectly generated and everyone uses built-in FindPNG anyway.
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/lib/libpng ${CURRENT_PACKAGES_DIR}/debug/lib/libpng)
file(COPY ${SOURCE_PATH}/LICENSE DESTINATION ${CURRENT_PACKAGES_DIR}/share/libpng)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/libpng/LICENSE ${CURRENT_PACKAGES_DIR}/share/libpng/copyright)

vcpkg_copy_pdbs()

file(COPY ${CMAKE_CURRENT_LIST_DIR}/usage DESTINATION ${CURRENT_PACKAGES_DIR}/share/${PORT})

if(VCPKG_CMAKE_SYSTEM_NAME AND NOT VCPKG_CMAKE_SYSTEM_NAME STREQUAL "WindowsStore")
    file(COPY ${CMAKE_CURRENT_LIST_DIR}/vcpkg-cmake-wrapper.cmake DESTINATION ${CURRENT_PACKAGES_DIR}/share/png)
endif()

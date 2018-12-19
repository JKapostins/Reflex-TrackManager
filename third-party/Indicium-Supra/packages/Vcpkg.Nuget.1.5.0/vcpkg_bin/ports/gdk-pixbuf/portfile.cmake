if (VCPKG_LIBRARY_LINKAGE STREQUAL static)
    message(STATUS "Warning: Static building not supported. Building dynamic.")
    set(VCPKG_LIBRARY_LINKAGE dynamic)
endif()

set(GDK_PIXBUF_VERSION 2.36)
set(GDK_PIXBUF_PATCH 9)
include(vcpkg_common_functions)
set(SOURCE_PATH ${CURRENT_BUILDTREES_DIR}/src/gdk-pixbuf-${GDK_PIXBUF_VERSION}.${GDK_PIXBUF_PATCH})
vcpkg_download_distfile(ARCHIVE
    URLS "http://ftp.gnome.org/pub/GNOME/sources/gdk-pixbuf/${GDK_PIXBUF_VERSION}/gdk-pixbuf-${GDK_PIXBUF_VERSION}.${GDK_PIXBUF_PATCH}.tar.xz"
    FILENAME "gdk-pixbuf-${GDK_PIXBUF_VERSION}.${GDK_PIXBUF_PATCH}.tar.xz"
    SHA512 ab8f2cda4490012936b094a1321e64b85e1fa1f8d070fae135a514f87f695201b845f4192e4a02954e2767d44314c0a95d727118853528182952d15890130261)

vcpkg_extract_source_archive(${ARCHIVE})
file(COPY ${CMAKE_CURRENT_LIST_DIR}/CMakeLists.txt DESTINATION ${SOURCE_PATH})

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA
    OPTIONS_DEBUG
        -DGDK_SKIP_HEADERS=ON
        -DGDK_SKIP_TOOLS=ON)

vcpkg_install_cmake()
vcpkg_copy_pdbs()
vcpkg_copy_tool_dependencies(${CURRENT_PACKAGES_DIR}/tools/gdk-pixbuf)

file(COPY ${SOURCE_PATH}/COPYING DESTINATION ${CURRENT_PACKAGES_DIR}/share/gdk-pixbuf)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/gdk-pixbuf/COPYING ${CURRENT_PACKAGES_DIR}/share/gdk-pixbuf/copyright)

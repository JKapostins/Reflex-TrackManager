# Common Ambient Variables:
#   CURRENT_BUILDTREES_DIR    = ${VCPKG_ROOT_DIR}\buildtrees\${PORT}
#   CURRENT_PACKAGES_DIR      = ${VCPKG_ROOT_DIR}\packages\${PORT}_${TARGET_TRIPLET}
#   CURRENT_PORT_DIR          = ${VCPKG_ROOT_DIR}\ports\${PORT}
#   PORT                      = current port name (zlib, etc)
#   TARGET_TRIPLET            = current triplet (x86-windows, x64-windows-static, etc)
#   VCPKG_CRT_LINKAGE         = C runtime linkage type (static, dynamic)
#   VCPKG_LIBRARY_LINKAGE     = target library linkage type (static, dynamic)
#   VCPKG_ROOT_DIR            = <C:\path\to\current\vcpkg>
#   VCPKG_TARGET_ARCHITECTURE = target architecture (x64, x86, arm)
#

include(vcpkg_common_functions)
set(SOURCE_PATH ${CURRENT_BUILDTREES_DIR}/src/libmikmod-3.3.11.1)
vcpkg_download_distfile(ARCHIVE
    URLS "https://downloads.sourceforge.net/project/mikmod/libmikmod/3.3.11.1/libmikmod-3.3.11.1.tar.gz"
    FILENAME "libmikmod-3.3.11.1.tar.gz"
    SHA512 f2439e2b691613847cd0787dd4e050116683ce7b05c215b8afecde5c6add819ea6c18e678e258c0a80786bef463f406072de15127f64368f694287a5e8e1a9de
)
vcpkg_extract_source_archive(${ARCHIVE})

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA # Disable this option if project cannot be built with Ninja
    OPTIONS
        -DENABLE_DOC=OFF
        -DENABLE_THREADS=ON
        -DDISABLE_HQMIXER=OFF
        -DENABLE_AF=ON
        -DENABLE_AIFF=ON
        -DENABLE_NAS=ON
        -DENABLE_OPENAL=ON
        -DENABLE_PIPE=ON
        -DENABLE_RAW=ON
        -DENABLE_STDOUT=ON
        -DENABLE_WAV=ON
        -DOPENAL_INCLUDE_DIR=${CURRENT_INSTALLED_DIR}/include
    OPTIONS_RELEASE -DENABLE_SIMD=ON
    OPTIONS_DEBUG -DENABLE_SIMD=OFF
)

vcpkg_install_cmake()

# Handle copyright
file(COPY ${SOURCE_PATH}/COPYING.LESSER DESTINATION ${CURRENT_PACKAGES_DIR}/share/libmikmod)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/libmikmod/COPYING.LESSER ${CURRENT_PACKAGES_DIR}/share/libmikmod/copyright)

file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/include)

if(VCPKG_LIBRARY_LINKAGE STREQUAL static)
    file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/bin ${CURRENT_PACKAGES_DIR}/debug/bin)
endif()
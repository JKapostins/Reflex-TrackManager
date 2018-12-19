# Common Ambient Variables:
#   VCPKG_ROOT_DIR = <C:\path\to\current\vcpkg>
#   TARGET_TRIPLET is the current triplet (x86-windows, etc)
#   PORT is the current port name (zlib, etc)
#   CURRENT_BUILDTREES_DIR = ${VCPKG_ROOT_DIR}\buildtrees\${PORT}
#   CURRENT_PACKAGES_DIR  = ${VCPKG_ROOT_DIR}\packages\${PORT}_${TARGET_TRIPLET}
#

include(vcpkg_common_functions)
set(SOURCE_PATH ${CURRENT_BUILDTREES_DIR}/src/parmetis-4.0.3)
vcpkg_download_distfile(ARCHIVE
    URLS "http://glaros.dtc.umn.edu/gkhome/fetch/sw/parmetis/parmetis-4.0.3.tar.gz"
    FILENAME "parmetis-4.0.3.tar.gz"
    SHA512 454a91921ca35c981df11c9846a11963ff8fd8407a25179453af33f8fe69493f6dd7f2a0b8feed9a7d3f121e45b715749dd7a94873eaac2bae4cad1e535ca132
)
vcpkg_extract_source_archive(${ARCHIVE})

vcpkg_apply_patches(
    SOURCE_PATH ${SOURCE_PATH}
    PATCHES
        ${CMAKE_CURRENT_LIST_DIR}/fix-metis-vs14-math.patch
        ${CMAKE_CURRENT_LIST_DIR}/fix-gklib-vs14-math.patch
        ${CMAKE_CURRENT_LIST_DIR}/fix-root-cmakelist.patch
        ${CMAKE_CURRENT_LIST_DIR}/fix-libparmetis-cmakelist.patch
)

if(VCPKG_LIBRARY_LINKAGE STREQUAL dynamic)
  set(ADDITIONAL_OPTIONS -DSHARED=ON -DCMAKE_WINDOWS_EXPORT_ALL_SYMBOLS=ON)
else()
  set(ADDITIONAL_OPTIONS -DSHARED=OFF)
endif()

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    OPTIONS
        ${ADDITIONAL_OPTIONS}
)

vcpkg_install_cmake()
vcpkg_copy_pdbs()

file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/include)

# Handle copyright
file(COPY ${SOURCE_PATH}/LICENSE.txt DESTINATION ${CURRENT_PACKAGES_DIR}/share/parmetis)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/parmetis/LICENSE.txt ${CURRENT_PACKAGES_DIR}/share/parmetis/copyright)

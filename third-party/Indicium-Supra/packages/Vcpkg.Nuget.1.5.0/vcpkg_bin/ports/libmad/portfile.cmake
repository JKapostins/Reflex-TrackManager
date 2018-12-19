include(vcpkg_common_functions)
set(SOURCE_PATH ${CURRENT_BUILDTREES_DIR}/src/libmad-0.15.1b)
vcpkg_download_distfile(ARCHIVE
    URLS "http://download.sourceforge.net/mad/libmad-0.15.1b.tar.gz"
    FILENAME "libmad-0.15.1b.tar.gz"
    SHA512 2cad30347fb310dc605c46bacd9da117f447a5cabedd8fefdb24ab5de641429e5ec5ce8af7aefa6a75a3f545d3adfa255e3fa0a2d50971f76bc0c4fc0400cc45
)
vcpkg_extract_source_archive(${ARCHIVE})

vcpkg_apply_patches(
  SOURCE_PATH ${SOURCE_PATH}
  PATCHES "${CMAKE_CURRENT_LIST_DIR}/use_fpm_default.patch"
)

#The archive only contains a Visual Studio 6.0 era DSP project file, so use a custom CMakeLists.txt
file(COPY ${CMAKE_CURRENT_LIST_DIR}/CMakeLists.txt DESTINATION ${SOURCE_PATH})

#Use the msvc++ config.h and mad.h header
file(COPY ${SOURCE_PATH}/msvc++/config.h DESTINATION ${SOURCE_PATH})
file(COPY ${SOURCE_PATH}/msvc++/mad.h DESTINATION ${SOURCE_PATH})

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
)

vcpkg_install_cmake()
vcpkg_copy_pdbs()

file(COPY ${SOURCE_PATH}/COPYING DESTINATION ${CURRENT_PACKAGES_DIR}/share/libmad)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/libmad/COPYING ${CURRENT_PACKAGES_DIR}/share/libmad/copyright)

file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/include)

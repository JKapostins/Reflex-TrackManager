include(vcpkg_common_functions)
if(VCPKG_LIBRARY_LINKAGE STREQUAL dynamic)
  message(WARNING "Dynamic not supported building static")
  set(VCPKG_LIBRARY_LINKAGE static)
endif()
set(SOURCE_PATH ${CURRENT_BUILDTREES_DIR}/src/libstemmer_c)
vcpkg_download_distfile(ARCHIVE
    URLS "http://snowball.tartarus.org/dist/libstemmer_c.tgz"
    FILENAME "libstemmer_c.tgz"
    SHA512 9ab5b8bfd5b4071dbbd63d769e09fae3971b49ee441ad970aa95d90b3297f5ffc9deed1613d99974d1485bf3b69292663591957f52bbeddcadbf9d9a4af432f2
)
vcpkg_extract_source_archive(${ARCHIVE})

file(COPY ${CMAKE_CURRENT_LIST_DIR}/CMakeLists.txt DESTINATION ${SOURCE_PATH})

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA
    OPTIONS_DEBUG -DDISABLE_INSTALL_HEADERS=ON
)

vcpkg_install_cmake()

file(INSTALL ${CMAKE_CURRENT_LIST_DIR}/LICENSE DESTINATION ${CURRENT_PACKAGES_DIR}/share/libstemmer RENAME copyright)

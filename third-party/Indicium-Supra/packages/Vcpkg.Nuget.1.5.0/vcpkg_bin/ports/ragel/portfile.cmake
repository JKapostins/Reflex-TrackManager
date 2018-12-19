include(vcpkg_common_functions)
set(SOURCE_PATH ${CURRENT_BUILDTREES_DIR}/src/ragel-6.10)
vcpkg_download_distfile(ARCHIVE
    URLS "http://www.colm.net/files/ragel/ragel-6.10.tar.gz"
    FILENAME "ragel-6.10.tar.gz"
    SHA512 6c1fe4f6fa8546ae28b92ccfbae94355ff0d3cea346b9ae8ce4cf6c2bdbeb823e0ccd355332643ea72d3befd533a8b3030ddbf82be7ffa811c2c58cbb01aaa38
)
vcpkg_extract_source_archive(${ARCHIVE})

file(COPY ${CMAKE_CURRENT_LIST_DIR}/CMakeLists.txt DESTINATION ${SOURCE_PATH})
file(COPY ${CMAKE_CURRENT_LIST_DIR}/config.h DESTINATION ${SOURCE_PATH}/ragel)

vcpkg_apply_patches(
    SOURCE_PATH ${CURRENT_BUILDTREES_DIR}/src
    PATCHES
        "${CMAKE_CURRENT_LIST_DIR}/0001-Remove-unistd.h-include-1.patch"
        "${CMAKE_CURRENT_LIST_DIR}/0002-Remove-unistd.h-include-2.patch"
)

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
)

vcpkg_install_cmake()

# Allow empty include directory
set(VCPKG_POLICY_EMPTY_INCLUDE_FOLDER enabled)

# Handle copyright
file(COPY ${SOURCE_PATH}/COPYING DESTINATION ${CURRENT_PACKAGES_DIR}/share/ragel)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/ragel/COPYING ${CURRENT_PACKAGES_DIR}/share/ragel/copyright)

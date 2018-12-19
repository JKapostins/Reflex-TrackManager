if(VCPKG_LIBRARY_LINKAGE STREQUAL "dynamic")
    message("Note: plib only supports static library linkage")
    set(VCPKG_LIBRARY_LINKAGE static)
endif()

include(vcpkg_common_functions)
set(SOURCE_PATH ${CURRENT_BUILDTREES_DIR}/src/plib-1.8.5)
vcpkg_download_distfile(ARCHIVE
    URLS "http://plib.sourceforge.net/dist/plib-1.8.5.tar.gz"
    FILENAME "plib-1.8.5.tar.gz"
    SHA512 17154cc77243fe576c2bcbcb0285b98aef1a0634658f5473e95fe0ac8fa3ed477dbe5620e44ccf0b7cc616f812af0cd44d6fcbba0c563180d3b61c9d6f158e1d
)
vcpkg_extract_source_archive(${ARCHIVE})

file(COPY ${CMAKE_CURRENT_LIST_DIR}/CMakeLists.txt DESTINATION ${SOURCE_PATH})

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA
)

vcpkg_install_cmake()

file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/include)

# Handle copyright
file(INSTALL ${SOURCE_PATH}/COPYING DESTINATION ${CURRENT_PACKAGES_DIR}/share/plib RENAME copyright)

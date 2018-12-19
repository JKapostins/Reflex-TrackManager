include(vcpkg_common_functions)

if(VCPKG_LIBRARY_LINKAGE STREQUAL "static")
    message("treehopper only supports dynamic library linkage")
    set(VCPKG_LIBRARY_LINKAGE dynamic)
endif()
if(VCPKG_CRT_LINKAGE STREQUAL "static")
    message(FATAL_ERROR "treehopper does not support static crt linkage")
endif()

vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO treehopper-electronics/treehopper-sdk
    REF 1.11.3
    SHA512 65b748375b798787c8b59f9657151f340920c939c3f079105b9b78f4e3b775125598106c6dfa4feba111a64d30f007003a70110ac767802a7dd1127a25c9fb14
    HEAD_REF master)

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}/C++/API/
    PREFER_NINJA
)

vcpkg_install_cmake()
vcpkg_copy_pdbs()

vcpkg_fixup_cmake_targets(CONFIG_PATH share/treehopper)

file(INSTALL ${SOURCE_PATH}/C++/API/inc/ DESTINATION ${CURRENT_PACKAGES_DIR}/include/Treehopper/)

file(INSTALL ${SOURCE_PATH}/LICENSE DESTINATION ${CURRENT_PACKAGES_DIR}/share/treehopper RENAME copyright)

file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/share)
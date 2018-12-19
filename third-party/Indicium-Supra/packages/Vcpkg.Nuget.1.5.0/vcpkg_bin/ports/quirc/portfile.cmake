include(vcpkg_common_functions)

vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO dlbeer/quirc
    REF v1.0
    SHA512 a556b08f2e2c710648b342fd06a855aa577b2b8c047c45a1c47cf54cde9963faf612978afba80bfd60a6f4f63156566f549ea303f09ed6e5348c1c30f5d77c13
    HEAD_REF master
)

file(COPY ${CMAKE_CURRENT_LIST_DIR}/CMakeLists.txt DESTINATION ${SOURCE_PATH})

vcpkg_apply_patches(
    SOURCE_PATH ${SOURCE_PATH}
    PATCHES
        ${CMAKE_CURRENT_LIST_DIR}/patch-for-msvc.patch
)

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA
)

vcpkg_install_cmake()
vcpkg_copy_pdbs()

file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/include)

# Handle copyright
file(COPY ${SOURCE_PATH}/license DESTINATION ${CURRENT_PACKAGES_DIR}/share/quirc)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/quirc/license ${CURRENT_PACKAGES_DIR}/share/quirc/copyright)

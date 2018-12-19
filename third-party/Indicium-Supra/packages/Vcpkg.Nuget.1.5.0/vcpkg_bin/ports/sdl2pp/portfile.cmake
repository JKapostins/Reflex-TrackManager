include(vcpkg_common_functions)
vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO libSDL2pp/libSDL2pp
    REF 0.16.0
    SHA512 36603a0b1c3ba9294fffa5368357866e5689ceed9743352ff52c096d8b0070cc3f8708a5e837c10c871b410b6bda3ed7e8e3b95cb9afc136d91afb035cde6361
    HEAD_REF master
    PATCHES "${CMAKE_CURRENT_LIST_DIR}/find-debug-libs.patch"
)

string(COMPARE EQUAL "${VCPKG_LIBRARY_LINKAGE}" "static" SDL2PP_STATIC)

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA
    OPTIONS
        -DSDL2PP_WITH_EXAMPLES=OFF
        -DSDL2PP_WITH_TESTS=OFF
        -DSDL2PP_STATIC=${SDL2PP_STATIC}
)

vcpkg_install_cmake()

# Handle copyright
file(INSTALL ${SOURCE_PATH}/COPYING.txt DESTINATION ${CURRENT_PACKAGES_DIR}/share/sdl2pp RENAME copyright)

file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/include)
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/share)

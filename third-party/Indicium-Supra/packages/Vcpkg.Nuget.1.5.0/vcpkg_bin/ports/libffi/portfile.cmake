if(NOT VCPKG_TARGET_ARCHITECTURE STREQUAL x86 AND NOT VCPKG_TARGET_ARCHITECTURE STREQUAL x64)
    message(FATAL_ERROR "Architecture not supported")
endif()

include(vcpkg_common_functions)
vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO libffi/libffi
    REF v3.1
    SHA512 b214e4a876995f44e0a93bad5bf1b3501ea1fbedafbf33ea600007bd08c9bc965a1f0dd90ea870281c3add6c051febd19aa6cdce36f3ee8ba535ba2c0703153c
    HEAD_REF master
)

file(COPY ${CMAKE_CURRENT_LIST_DIR}/CMakeLists.txt DESTINATION ${SOURCE_PATH})

vcpkg_apply_patches(
    SOURCE_PATH ${SOURCE_PATH}
    PATCHES
        ${CMAKE_CURRENT_LIST_DIR}/export-global-data.patch
)

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    OPTIONS
        -DFFI_CONFIG_FILE=${CMAKE_CURRENT_LIST_DIR}/fficonfig.h
    OPTIONS_DEBUG
        -DFFI_SKIP_HEADERS=ON
)

vcpkg_install_cmake()
vcpkg_copy_pdbs()

if(VCPKG_LIBRARY_LINKAGE STREQUAL static)
    vcpkg_apply_patches(
        SOURCE_PATH ${CURRENT_PACKAGES_DIR}/include
        PATCHES
            ${CMAKE_CURRENT_LIST_DIR}/auto-define-static-macro.patch
    )
endif()

file(COPY ${SOURCE_PATH}/LICENSE DESTINATION ${CURRENT_PACKAGES_DIR}/share/libffi)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/libffi/LICENSE ${CURRENT_PACKAGES_DIR}/share/libffi/copyright)

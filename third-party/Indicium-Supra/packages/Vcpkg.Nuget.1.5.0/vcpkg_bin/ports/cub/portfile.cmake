include(vcpkg_common_functions)
vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO NVlabs/cub
    REF v1.8.0
    SHA512 c4ba51ca4e369c8ef87bc411aa011470478fcc2526b667f088e9ac8c62e6532dc2368e869f5147b56f22b3e8718c7276434c3294d9d67cb3a92c700d163e8fa7
    HEAD_REF master
)

file(COPY ${SOURCE_PATH}/cub/ DESTINATION ${CURRENT_PACKAGES_DIR}/include/cub)

configure_file(${SOURCE_PATH}/LICENSE.TXT ${CURRENT_PACKAGES_DIR}/share/cub/copyright COPYONLY)

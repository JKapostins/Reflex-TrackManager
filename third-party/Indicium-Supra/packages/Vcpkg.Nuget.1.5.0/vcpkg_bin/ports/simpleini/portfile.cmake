# header-only library
include(vcpkg_common_functions)

vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO brofield/simpleini
    REF fe082fa81f4a55ddceb55056622136be616b3c6f
    SHA512 9ba3fc0e5d4d426a7943a6783f3e66203d3b822a9ac6bc2b261e877f70e099495ad22e03fd6ad3dd7aab422192701b2b450ace750ebd3bc6b4e6266c6d15184d
    HEAD_REF master
)

file(COPY ${SOURCE_PATH}/simpleini.h DESTINATION ${CURRENT_PACKAGES_DIR}/include)

file(INSTALL ${SOURCE_PATH}/LICENCE.txt DESTINATION ${CURRENT_PACKAGES_DIR}/share/simpleini RENAME copyright)

include(${CMAKE_TRIPLET_FILE})
include(vcpkg_common_functions)
set(SOURCE_PATH ${CURRENT_BUILDTREES_DIR}/src/FreeImage)
vcpkg_download_distfile(ARCHIVE
    URLS "http://downloads.sourceforge.net/freeimage/FreeImage3180.zip"
    FILENAME "FreeImage3180.zip"
    SHA512 9d9cc7e2d57552c3115e277aeb036e0455204d389026b17a3f513da5be1fd595421655488bb1ec2f76faebed66049119ca55e26e2a6d37024b3fb7ef36ad4818
)
vcpkg_extract_source_archive(${ARCHIVE})


file(COPY ${CMAKE_CURRENT_LIST_DIR}/CMakeLists.txt DESTINATION ${SOURCE_PATH})
file(COPY ${CMAKE_CURRENT_LIST_DIR}/FreeImageConfig-static.h DESTINATION ${SOURCE_PATH})
file(COPY ${CMAKE_CURRENT_LIST_DIR}/FreeImageConfig-dynamic.h DESTINATION ${SOURCE_PATH})

# Copy some useful Find***.cmake modules 
file(COPY ${CMAKE_CURRENT_LIST_DIR}/cmake DESTINATION ${SOURCE_PATH})

# This is not strictly necessary, but to make sure 
# that no "internal" libraries are used by removing them
file(REMOVE_RECURSE ${SOURCE_PATH}/Source/LibJPEG)
file(REMOVE_RECURSE ${SOURCE_PATH}/Source/LibPNG)
file(REMOVE_RECURSE ${SOURCE_PATH}/Source/LibTIFF4)
file(REMOVE_RECURSE ${SOURCE_PATH}/Source/ZLib)
file(REMOVE_RECURSE ${SOURCE_PATH}/Source/LibOpenJPEG)
file(REMOVE_RECURSE ${SOURCE_PATH}/Source/LibJXR)
file(REMOVE_RECURSE ${SOURCE_PATH}/Source/LibWebP)
file(REMOVE_RECURSE ${SOURCE_PATH}/Source/LibRawLite)
file(REMOVE_RECURSE ${SOURCE_PATH}/Source/OpenEXR)

vcpkg_apply_patches(
    SOURCE_PATH ${SOURCE_PATH}
    PATCHES "${CMAKE_CURRENT_LIST_DIR}/disable-plugins-depending-on-internal-third-party-libraries.patch"
            "${CMAKE_CURRENT_LIST_DIR}/use-external-jpeg.patch"
            "${CMAKE_CURRENT_LIST_DIR}/use-external-jxrlib.patch"
            "${CMAKE_CURRENT_LIST_DIR}/use-external-libtiff.patch"
            "${CMAKE_CURRENT_LIST_DIR}/use-external-openjpeg.patch"
            "${CMAKE_CURRENT_LIST_DIR}/use-external-png-zlib.patch"
            "${CMAKE_CURRENT_LIST_DIR}/use-external-rawlib.patch"
            "${CMAKE_CURRENT_LIST_DIR}/use-external-webp.patch"
            "${CMAKE_CURRENT_LIST_DIR}/use-external-openexr.patch"
            "${CMAKE_CURRENT_LIST_DIR}/use-freeimage-config-include.patch"
            "${CMAKE_CURRENT_LIST_DIR}/fix-function-overload.patch"
)

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA
    OPTIONS_DEBUG -DINSTALL_HEADERS=OFF
)

vcpkg_install_cmake()

# Handle copyright
file(COPY ${SOURCE_PATH}/license-fi.txt DESTINATION ${CURRENT_PACKAGES_DIR}/share/freeimage)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/freeimage/license-fi.txt ${CURRENT_PACKAGES_DIR}/share/freeimage/copyright)

vcpkg_copy_pdbs()

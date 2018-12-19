if (VCPKG_LIBRARY_LINKAGE STREQUAL dynamic)
    message(STATUS "Warning: Dynamic building not supported yet. Building static.")
    set(VCPKG_LIBRARY_LINKAGE static)
endif()

if(VCPKG_TARGET_ARCHIECTURE STREQUAL "x86")
    message(FATAL_ERROR "theia requires ceres[suitesparse] which depends on suitesparse which depends on openblas which is unavailable on x86.")
endif()

include(vcpkg_common_functions)

vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO sweeneychris/TheiaSfM
    REF d15154a6c30ea48e7d135be126e2936802e476ad
    SHA512 aaf6e9737d04499f0ffd453952380f2e1aa3aab2a75487d6806bfab60aa972719d7349730eab3d1b37088e99cf6c9076ae1cdea276f48532698226c69ac48977
    HEAD_REF master
)

vcpkg_apply_patches(
    SOURCE_PATH ${SOURCE_PATH}
    PATCHES
        ${CMAKE_CURRENT_LIST_DIR}/fix-cmakelists.patch
        ${CMAKE_CURRENT_LIST_DIR}/fix-vlfeat-static.patch
        ${CMAKE_CURRENT_LIST_DIR}/fix-glog-error.patch
        ${CMAKE_CURRENT_LIST_DIR}/fix-find-suitesparse.patch
        ${CMAKE_CURRENT_LIST_DIR}/fix-oiio.patch
)

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    OPTIONS
        -DBUILD_TESTING=OFF
        -DTHEIA_USE_EXTERNAL_CEREAL=ON
        -DTHEIA_USE_EXTERNAL_FLANN=ON
)

vcpkg_install_cmake()

vcpkg_fixup_cmake_targets(CONFIG_PATH "CMake")

# Changes target search path
file(READ ${CURRENT_PACKAGES_DIR}/share/theia/TheiaConfig.cmake THEIA_TARGETS)
string(REPLACE "get_filename_component(CURRENT_ROOT_INSTALL_DIR\n  \${THEIA_CURRENT_CONFIG_INSTALL_DIR}/../ ABSOLUTE)"
               "get_filename_component(CURRENT_ROOT_INSTALL_DIR\n  \${THEIA_CURRENT_CONFIG_INSTALL_DIR}/../../ ABSOLUTE)" THEIA_TARGETS "${THEIA_TARGETS}")
file(WRITE ${CURRENT_PACKAGES_DIR}/share/theia/TheiaConfig.cmake "${THEIA_TARGETS}")

vcpkg_copy_pdbs()

# Clean
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/include)
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/optimo)
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/share)
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/include/theia/libraries/akaze/cimg/cmake-modules)
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/include/theia/libraries/akaze/cmake)
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/include/theia/libraries/akaze/datasets)
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/include/theia/libraries/spectra/doxygen)
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/optimo)

# Handle copyright
file(COPY ${SOURCE_PATH}/license.txt DESTINATION ${CURRENT_PACKAGES_DIR}/share/theia)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/theia/license.txt ${CURRENT_PACKAGES_DIR}/share/theia/copyright)
file(COPY ${SOURCE_PATH}/data/camera_sensor_database_license.txt DESTINATION ${CURRENT_PACKAGES_DIR}/share/theia)

include(vcpkg_common_functions)
vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO mosra/magnum-extras
    REF v2018.10
    SHA512 2f1164e321ceff9ae526cb2bae961147f52f56b35ea837c29ba0153f418a269056f469415ca0393dfb8fa5d916e92130aaccc61105d18e3f242820d13478eb33
    HEAD_REF master
)

string(COMPARE EQUAL "${VCPKG_LIBRARY_LINKAGE}" "static" BUILD_STATIC)

# Handle features
set(_COMPONENT_FLAGS "")
foreach(_feature IN LISTS ALL_FEATURES)
    # Uppercase the feature name and replace "-" with "_"
    string(TOUPPER "${_feature}" _FEATURE)
    string(REPLACE "-" "_" _FEATURE "${_FEATURE}")

    # Turn "-DWITH_*=" ON or OFF depending on whether the feature
    # is in the list.
    if(_feature IN_LIST FEATURES)
        list(APPEND _COMPONENT_FLAGS "-DWITH_${_FEATURE}=ON")
    else()
        list(APPEND _COMPONENT_FLAGS "-DWITH_${_FEATURE}=OFF")
    endif()
endforeach()

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA # Disable this option if project cannot be built with Ninja
    OPTIONS
        ${_COMPONENT_FLAGS}
        -DBUILD_STATIC=${BUILD_STATIC}
        -DMAGNUM_PLUGINS_DEBUG_DIR=${CURRENT_INSTALLED_DIR}/debug/bin/magnum-d
        -DMAGNUM_PLUGINS_RELEASE_DIR=${CURRENT_INSTALLED_DIR}/bin/magnum
)

vcpkg_install_cmake()

vcpkg_fixup_cmake_targets(CONFIG_PATH share/cmake/MagnumExtras TARGET_PATH share/MagnumExtras)

# Messages to the user
if("ui" IN_LIST FEATURES)
    message(WARNING "It is recommended to install one of magnum-plugins[freetypefont,harfbuzzfont,stbtruetypefont] to have the UI library working out of the box")
endif()

# Debug includes and share are the same as release
file(REMOVE_RECURSE
    ${CURRENT_PACKAGES_DIR}/debug/include
    ${CURRENT_PACKAGES_DIR}/debug/share
)

# Clean up empty directories
if("${FEATURES}" STREQUAL "core")
    file(REMOVE_RECURSE
        ${CURRENT_PACKAGES_DIR}/bin
        ${CURRENT_PACKAGES_DIR}/lib
        ${CURRENT_PACKAGES_DIR}/debug
    )
    set(VCPKG_POLICY_EMPTY_INCLUDE_FOLDER enabled)
endif()

if(VCPKG_LIBRARY_LINKAGE STREQUAL "static")
   file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/bin)
   file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/bin)
endif()

# Handle copyright
file(COPY ${SOURCE_PATH}/COPYING DESTINATION ${CURRENT_PACKAGES_DIR}/share/magnum-extras)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/magnum-extras/COPYING ${CURRENT_PACKAGES_DIR}/share/magnum-extras/copyright)

vcpkg_copy_pdbs()

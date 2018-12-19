include(vcpkg_common_functions)
vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO jbeder/yaml-cpp
    REF yaml-cpp-0.6.2
    SHA512 fea8ce0a20a00cbc75023d1db442edfcd32d0ac57a3c41b32ec8d56f87cc1d85d7dd7a923ce662f5d3a315f91a736d6be0d649997acd190915c1d68cc93795e4
    HEAD_REF master
    PATCHES
        ${CMAKE_CURRENT_LIST_DIR}/0001-noexcept.patch
)

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA
    OPTIONS
        -DYAML_CPP_BUILD_TOOLS=OFF
        -DYAML_CPP_BUILD_TESTS=OFF
)

vcpkg_install_cmake()
vcpkg_copy_pdbs()
if(EXISTS ${CURRENT_PACKAGES_DIR}/CMake)
    vcpkg_fixup_cmake_targets(CONFIG_PATH CMake)
endif()
if(EXISTS ${CURRENT_PACKAGES_DIR}/lib/cmake/yaml-cpp)
    vcpkg_fixup_cmake_targets(CONFIG_PATH lib/cmake/yaml-cpp)
endif()

# Adjust paths and remove hardcoded ones from the config files
file(READ ${CURRENT_PACKAGES_DIR}/share/yaml-cpp/yaml-cpp-config.cmake YAML_CONFIG)
string(REPLACE "set(YAML_CPP_INCLUDE_DIR \"\${YAML_CPP_CMAKE_DIR}/../include\")"
               "set(YAML_CPP_INCLUDE_DIR \"\${YAML_CPP_CMAKE_DIR}/../../include\")" YAML_CONFIG "${YAML_CONFIG}")
file(WRITE ${CURRENT_PACKAGES_DIR}/share/yaml-cpp/yaml-cpp-config.cmake "${YAML_CONFIG}")

file(READ ${CURRENT_PACKAGES_DIR}/share/yaml-cpp/yaml-cpp-targets.cmake YAML_CONFIG)
string(REPLACE "set(_IMPORT_PREFIX \"${CURRENT_PACKAGES_DIR}\")"
"get_filename_component(_IMPORT_PREFIX \"\${CMAKE_CURRENT_LIST_FILE}\" PATH)
get_filename_component(_IMPORT_PREFIX \"\${_IMPORT_PREFIX}\" PATH)
get_filename_component(_IMPORT_PREFIX \"\${_IMPORT_PREFIX}\" PATH)" YAML_CONFIG "${YAML_CONFIG}")
file(WRITE ${CURRENT_PACKAGES_DIR}/share/yaml-cpp/yaml-cpp-targets.cmake "${YAML_CONFIG}")

set(_targets_cmake_conf)
if(NOT DEFINED VCPKG_BUILD_TYPE OR VCPKG_BUILD_TYPE STREQUAL "debug")
    list(APPEND _targets_cmake_conf "debug")
endif()
if(NOT DEFINED VCPKG_BUILD_TYPE OR VCPKG_BUILD_TYPE STREQUAL "release")
    list(APPEND _targets_cmake_conf "release")
endif()
foreach(CONF ${_targets_cmake_conf})
    file(READ ${CURRENT_PACKAGES_DIR}/share/yaml-cpp/yaml-cpp-targets-${CONF}.cmake YAML_CONFIG)
    string(REPLACE "${CURRENT_PACKAGES_DIR}" "\${_IMPORT_PREFIX}" YAML_CONFIG "${YAML_CONFIG}")
    file(WRITE ${CURRENT_PACKAGES_DIR}/share/yaml-cpp/yaml-cpp-targets-${CONF}.cmake "${YAML_CONFIG}")
endforeach()

# Remove debug include files
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/include)

file(READ ${CURRENT_PACKAGES_DIR}/include/yaml-cpp/dll.h DLL_H)
if(VCPKG_LIBRARY_LINKAGE STREQUAL "dynamic")
    string(REPLACE "#ifdef YAML_CPP_DLL" "#if 1" DLL_H "${DLL_H}")
else()
    string(REPLACE "#ifdef YAML_CPP_DLL" "#if 0" DLL_H "${DLL_H}")
endif()
file(WRITE ${CURRENT_PACKAGES_DIR}/include/yaml-cpp/dll.h "${DLL_H}")

# Handle copyright
file(COPY ${SOURCE_PATH}/LICENSE DESTINATION ${CURRENT_PACKAGES_DIR}/share/yaml-cpp)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/yaml-cpp/LICENSE ${CURRENT_PACKAGES_DIR}/share/yaml-cpp/copyright)

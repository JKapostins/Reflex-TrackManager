include(vcpkg_common_functions)

if(VCPKG_CMAKE_SYSTEM_NAME STREQUAL WindowsStore)
  message(FATAL_ERROR "WindowsStore not supported")
endif()

vcpkg_from_github(
  OUT_SOURCE_PATH SOURCE_PATH
  REPO madler/zlib
  REF v1.2.11
  SHA512 104c62ed1228b5f1199bc037081861576900eb0697a226cafa62a35c4c890b5cb46622e399f9aad82ee5dfb475bae26ae75e2bd6da3d261361b1c8b996970faf
  HEAD_REF master
)

file(COPY ${CMAKE_CURRENT_LIST_DIR}/CMakeLists.txt DESTINATION ${SOURCE_PATH})

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    PREFER_NINJA
    OPTIONS_DEBUG -DDISABLE_INSTALL_HEADERS=ON -DDISABLE_INSTALL_TOOLS=ON
)

vcpkg_install_cmake()
vcpkg_copy_pdbs()

vcpkg_copy_tool_dependencies(${CURRENT_PACKAGES_DIR}/tools/minizip)
file(GLOB HEADERS "${CURRENT_PACKAGES_DIR}/include/minizip/*.h")
foreach(HEADER ${HEADERS})
  file(READ "${HEADER}" _contents)
  string(REPLACE "#ifdef HAVE_BZIP2" "#if 1" _contents "${_contents}")
  file(WRITE "${HEADER}" "${_contents}")
endforeach()

file(INSTALL ${SOURCE_PATH}/contrib/minizip/MiniZip64_info.txt DESTINATION ${CURRENT_PACKAGES_DIR}/share/minizip RENAME copyright)

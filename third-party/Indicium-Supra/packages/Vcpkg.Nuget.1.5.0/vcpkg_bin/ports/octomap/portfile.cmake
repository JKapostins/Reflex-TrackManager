if(VCPKG_LIBRARY_LINKAGE STREQUAL static)
    message("Octomap does not currently support building purely static. Building dynamic instead.")
    set(VCPKG_LIBRARY_LINKAGE dynamic)
endif()

include(vcpkg_common_functions)
vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO OctoMap/octomap
    REF cefed0c1d79afafa5aeb05273cf1246b093b771c
    SHA512 8fdea8b33680488d41e570d55ff88c20b923efb9d48238031f9b96d2e3917dbe7e49699769de63794f4b1d24e40a99615151e72487f30de340a3abf6522ea156
    HEAD_REF master
)

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    OPTIONS
        -DBUILD_OCTOVIS_SUBPROJECT=OFF
        -DBUILD_DYNAMICETD3D_SUBPROJECT=OFF
        -DCMAKE_WINDOWS_EXPORT_ALL_SYMBOLS=ON
)

vcpkg_install_cmake()

file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/include)

file(MAKE_DIRECTORY ${CURRENT_PACKAGES_DIR}/tools/octomap)
file(RENAME ${CURRENT_PACKAGES_DIR}/bin/binvox2bt.exe ${CURRENT_PACKAGES_DIR}/tools/octomap/binvox2bt.exe)
file(RENAME ${CURRENT_PACKAGES_DIR}/bin/bt2vrml.exe ${CURRENT_PACKAGES_DIR}/tools/octomap/bt2vrml.exe)
file(RENAME ${CURRENT_PACKAGES_DIR}/bin/compare_octrees.exe ${CURRENT_PACKAGES_DIR}/tools/octomap/compare_octrees.exe)
file(RENAME ${CURRENT_PACKAGES_DIR}/bin/convert_octree.exe ${CURRENT_PACKAGES_DIR}/tools/octomap/convert_octree.exe)
file(RENAME ${CURRENT_PACKAGES_DIR}/bin/edit_octree.exe ${CURRENT_PACKAGES_DIR}/tools/octomap/edit_octree.exe)
file(RENAME ${CURRENT_PACKAGES_DIR}/bin/eval_octree_accuracy.exe ${CURRENT_PACKAGES_DIR}/tools/octomap/eval_octree_accuracy.exe)
file(RENAME ${CURRENT_PACKAGES_DIR}/bin/graph2tree.exe ${CURRENT_PACKAGES_DIR}/tools/octomap/graph2tree.exe)
file(RENAME ${CURRENT_PACKAGES_DIR}/bin/log2graph.exe ${CURRENT_PACKAGES_DIR}/tools/octomap/log2graph.exe)

file(REMOVE ${CURRENT_PACKAGES_DIR}/debug/bin/binvox2bt.exe)
file(REMOVE ${CURRENT_PACKAGES_DIR}/debug/bin/bt2vrml.exe)
file(REMOVE ${CURRENT_PACKAGES_DIR}/debug/bin/compare_octrees.exe)
file(REMOVE ${CURRENT_PACKAGES_DIR}/debug/bin/convert_octree.exe)
file(REMOVE ${CURRENT_PACKAGES_DIR}/debug/bin/edit_octree.exe)
file(REMOVE ${CURRENT_PACKAGES_DIR}/debug/bin/eval_octree_accuracy.exe)
file(REMOVE ${CURRENT_PACKAGES_DIR}/debug/bin/graph2tree.exe)
file(REMOVE ${CURRENT_PACKAGES_DIR}/debug/bin/log2graph.exe)

vcpkg_fixup_cmake_targets(CONFIG_PATH share/octomap)
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/share)

file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/lib/pkgconfig)
file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/lib/pkgconfig)

# Handle copyright
file(COPY ${SOURCE_PATH}/octomap/LICENSE.txt DESTINATION ${CURRENT_PACKAGES_DIR}/share/octomap)
file(RENAME ${CURRENT_PACKAGES_DIR}/share/octomap/LICENSE.txt ${CURRENT_PACKAGES_DIR}/share/octomap/copyright)

vcpkg_copy_pdbs()
vcpkg_copy_tool_dependencies(${CURRENT_PACKAGES_DIR}/tools/octomap)

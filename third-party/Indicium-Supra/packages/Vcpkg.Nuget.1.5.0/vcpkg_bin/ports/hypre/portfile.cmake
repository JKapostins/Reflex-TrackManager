include(vcpkg_common_functions)
set(SOURCE_PATH ${CURRENT_BUILDTREES_DIR}/src/hypre-2.11.2/src)
vcpkg_download_distfile(ARCHIVE
    URLS "http://computation.llnl.gov/projects/hypre-scalable-linear-solvers-multigrid-methods/download/hypre-2.11.2.tar.gz"
    FILENAME "hypre-2.11.2.tar.gz"
    SHA512 a06321028121e5420fa944ce4fae5f9b96e6021ec2802e68ec3c349f19a20543ed7eff774a4735666c5807ce124eb571b3f86757c67e91faa1c683c3f657469f
)
vcpkg_extract_source_archive(${ARCHIVE})

vcpkg_apply_patches(
    SOURCE_PATH ${SOURCE_PATH}
    PATCHES
        ${CMAKE_CURRENT_LIST_DIR}/fix-root-cmakelists.patch
        ${CMAKE_CURRENT_LIST_DIR}/fix-macro-to-template.patch
        ${CMAKE_CURRENT_LIST_DIR}/fix-blas-vs14-math.patch
        ${CMAKE_CURRENT_LIST_DIR}/fix-lapack-vs14-math.patch
        ${CMAKE_CURRENT_LIST_DIR}/fix-export-global-data-symbols.patch
)

if(VCPKG_LIBRARY_LINKAGE STREQUAL dynamic)
  set(OPTIONS -DHYPRE_SHARED=ON)
else()
  set(OPTIONS -DHYPRE_SHARED=OFF)
endif()

vcpkg_configure_cmake(
    SOURCE_PATH ${SOURCE_PATH}
    OPTIONS
        ${OPTIONS}
    OPTIONS_RELEASE
        -DHYPRE_BUILD_TYPE=Release
        -DHYPRE_INSTALL_PREFIX=${CURRENT_PACKAGES_DIR}
    OPTIONS_DEBUG
        -DHYPRE_BUILD_TYPE=Debug
        -DHYPRE_INSTALL_PREFIX=${CURRENT_PACKAGES_DIR}/debug
)

vcpkg_install_cmake()
vcpkg_copy_pdbs()

file(REMOVE_RECURSE ${CURRENT_PACKAGES_DIR}/debug/include)

# Handle copyright
file(COPY ${SOURCE_PATH}/../COPYRIGHT DESTINATION ${CURRENT_PACKAGES_DIR}/share/hypre/copyright)

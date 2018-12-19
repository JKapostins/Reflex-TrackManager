## # vcpkg_apply_patches
##
## Apply a set of patches to a source tree.
##
## ## Usage
## ```cmake
## vcpkg_apply_patches(
##     SOURCE_PATH <${SOURCE_PATH}>
##     [QUIET]
##     PATCHES <patch1.patch>...
## )
## ```
##
## ## Parameters
## ### SOURCE_PATH
## The source path in which apply the patches. By convention, this is usually set in the portfile as the variable `SOURCE_PATH`.
##
## ### PATCHES
## A list of patches that are applied to the source tree.
##
## Generally, these take the form of `${CMAKE_CURRENT_LIST_DIR}/some.patch` to select patches in the `port\<port>\` directory.
##
## ### QUIET
## Disables the warning message upon failure.
##
## This should only be used for edge cases, such as patches that are known to fail even on a clean source tree.
##
## ## Examples
##
## * [boost](https://github.com/Microsoft/vcpkg/blob/master/ports/boost/portfile.cmake)
## * [freetype](https://github.com/Microsoft/vcpkg/blob/master/ports/freetype/portfile.cmake)
## * [libpng](https://github.com/Microsoft/vcpkg/blob/master/ports/libpng/portfile.cmake)

function(vcpkg_apply_patches)
    cmake_parse_arguments(_ap "QUIET" "SOURCE_PATH" "PATCHES" ${ARGN})

    find_program(GIT NAMES git git.cmd)
    set(PATCHNUM 0)
    foreach(PATCH ${_ap_PATCHES})
        get_filename_component(ABSOLUTE_PATCH "${PATCH}" ABSOLUTE BASE_DIR "${CURRENT_PORT_DIR}")
        message(STATUS "Applying patch ${PATCH}")
        set(LOGNAME patch-${TARGET_TRIPLET}-${PATCHNUM})
        execute_process(
            COMMAND ${GIT} --work-tree=. --git-dir=.git apply "${ABSOLUTE_PATCH}" --ignore-whitespace --whitespace=nowarn --verbose
            OUTPUT_FILE ${CURRENT_BUILDTREES_DIR}/${LOGNAME}-out.log
            ERROR_FILE ${CURRENT_BUILDTREES_DIR}/${LOGNAME}-err.log
            WORKING_DIRECTORY ${_ap_SOURCE_PATH}
            RESULT_VARIABLE error_code
        )

        if(error_code AND NOT _ap_QUIET)
            message(STATUS "Applying patch failed. This is expected if this patch was previously applied.")
        endif()

        math(EXPR PATCHNUM "${PATCHNUM}+1")
    endforeach()
endfunction()

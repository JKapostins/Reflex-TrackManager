cmake_minimum_required (VERSION 3.13.0)

include (scripts/SourceFiles.cmake)

set(projectName "TrackManager")

project (${projectName})

add_library (${projectName} SHARED ${SOURCE_FILES})

target_include_directories(${projectName} PRIVATE "../third-party/Indicium-Supra/include")
target_include_directories(${projectName} PRIVATE "../third-party/minhook/include")
include_external_msproject(libMinHook "../third-party/minhook/build/VC15/libMinHook.vcxproj" PLATFORM Win32)
include_external_msproject(Indicium-Supra "../third-party/Indicium-Supra/src/Indicium-Supra.vcxproj" PLATFORM Win32)

target_link_libraries(${projectName} debug ${CMAKE_BINARY_DIR}/libMinHook/Debug/libMinHook.x86.lib)
target_link_libraries(${projectName} optimized ${CMAKE_BINARY_DIR}/libMinHook/Release/libMinHook.x86.lib)

target_link_libraries(${projectName} debug ${CMAKE_BINARY_DIR}/Indicium-Supra/Debug/x86/Indicium-Supra.lib)
target_link_libraries(${projectName} optimized ${CMAKE_BINARY_DIR}/libMinHook/Release/libMinHook.x86.lib)

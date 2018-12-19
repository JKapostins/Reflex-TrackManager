cmake_minimum_required (VERSION 3.13.0)

set(projectName "ReflexCustoms")
project (${projectName})

set(headerFiles
	"EnsureCleanup.h"
	"Injector.h"
)

set(sourceFiles 
	"Injector.cpp"
	"Main.cpp"
)

configure_file (
  "${PROJECT_SOURCE_DIR}/BuildDefines.h.in"
  "${PROJECT_BINARY_DIR}/BuildDefines.h"
  )

include_directories(${PROJECT_BINARY_DIR})

set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} -DGNARLY_DEBUG")

add_executable (${projectName} ${headerFiles} ${sourceFiles})
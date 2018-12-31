cmake_minimum_required (VERSION 3.13.0)

if(NOT MSVC)
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")
else()
  add_definitions(-D_WIN32_WINNT=0x600)
endif()


include (scripts/SourceFiles.cmake)

set(projectName "TrackManager-Overlay")

project (${projectName})

# Find Protobuf installation
# Looks for protobuf-config.cmake file installed by Protobuf's cmake installation.
set(protobuf_MODULE_COMPATIBLE TRUE)
find_package(Protobuf CONFIG REQUIRED)
message(STATUS "Using protobuf ${protobuf_VERSION}")

set(_PROTOBUF_LIBPROTOBUF protobuf::libprotobuf)
set(_PROTOBUF_PROTOC $<TARGET_FILE:protobuf::protoc>)

# Find gRPC installation
# Looks for gRPCConfig.cmake file installed by gRPC's cmake installation.
find_package(gRPC CONFIG REQUIRED)
message(STATUS "Using gRPC ${gRPC_VERSION}")

set(_GRPC_GRPCPP_UNSECURE gRPC::grpc++_unsecure)
set(_GRPC_CPP_PLUGIN_EXECUTABLE $<TARGET_FILE:gRPC::grpc_cpp_plugin>)

get_filename_component(trackManagementProto "../protos/trackmanagement.proto" ABSOLUTE)
get_filename_component(trackManagementProtoPath "${trackManagementProto}" PATH)

set(proto_srcs "${CMAKE_CURRENT_BINARY_DIR}/trackmanagement.pb.cc")
set(proto_hdrs "${CMAKE_CURRENT_BINARY_DIR}/trackmanagement.pb.h")
set(grpc_srcs "${CMAKE_CURRENT_BINARY_DIR}/trackmanagement.grpc.pb.cc")
set(grpc_hdrs "${CMAKE_CURRENT_BINARY_DIR}/trackmanagement.grpc.pb.h")

# Generated sources
add_custom_command(
      OUTPUT "${proto_srcs}" "${proto_hdrs}" "${grpc_srcs}" "${grpc_hdrs}"
      COMMAND ${_PROTOBUF_PROTOC}
      ARGS --grpc_out "${CMAKE_CURRENT_BINARY_DIR}"
        --cpp_out "${CMAKE_CURRENT_BINARY_DIR}"
        -I "${trackManagementProtoPath}"
        --plugin=protoc-gen-grpc="${_GRPC_CPP_PLUGIN_EXECUTABLE}"
        "${trackManagementProto}"
      DEPENDS "${trackManagementProto}")

# Include generated *.pb.h files
include_directories("${CMAKE_CURRENT_BINARY_DIR}")

SOURCE_GROUP(grpc\\generated FILES ${proto_srcs} ${grpc_srcs} ${proto_hdrs} ${grpc_hdrs})

add_library (${projectName} SHARED ${SOURCE_FILES} ${proto_srcs} ${grpc_srcs} ${proto_hdrs} ${grpc_hdrs})

target_include_directories(${projectName} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/../third-party/Indicium-Supra/include")
target_include_directories(${projectName} PRIVATE "../third-party/minhook/include")
include_external_msproject(libMinHook "${CMAKE_CURRENT_SOURCE_DIR}/../third-party/minhook/build/VC15/libMinHook.vcxproj" PLATFORM Win32)
include_external_msproject(Indicium-Supra "${CMAKE_CURRENT_SOURCE_DIR}/../third-party/Indicium-Supra/src/Indicium-Supra.vcxproj" PLATFORM Win32)

target_link_libraries(${projectName} debug ${CMAKE_BINARY_DIR}/libMinHook/Debug/libMinHook.x86.lib)
target_link_libraries(${projectName} optimized ${CMAKE_BINARY_DIR}/libMinHook/Release/libMinHook.x86.lib)

target_link_libraries(${projectName} debug ${CMAKE_BINARY_DIR}/Indicium-Supra/Debug/x86/Indicium-Supra.lib)
target_link_libraries(${projectName} optimized ${CMAKE_BINARY_DIR}/libMinHook/Release/libMinHook.x86.lib)
target_link_libraries(${projectName} ${_GRPC_GRPCPP_UNSECURE} ${_PROTOBUF_LIBPROTOBUF} )

add_dependencies(${projectName} libMinHook Indicium-Supra)

cmake_minimum_required (VERSION 3.13.0)

set(imguiFiles
imgui/imconfig.h
imgui/imgui.cpp
imgui/imgui.h
imgui/imgui_demo.cpp
imgui/imgui_draw.cpp
imgui/imgui_impl_dx9.cpp
imgui/imgui_impl_dx9.h
imgui/imgui_internal.h
imgui/stb_rect_pack.h
imgui/stb_textedit.h
imgui/stb_truetype.h
)

set(trackManagerFiles
dllmain.cpp
dllmain.h
)

set(grpcFiles
grpc/testclient.h
grpc/testclient.cpp
)

set(grpcGenerated
${CMAKE_CURRENT_BINARY_DIR}/helloworld.pb.cc
${CMAKE_CURRENT_BINARY_DIR}/helloworld.pb.h
${CMAKE_CURRENT_BINARY_DIR}/helloworld.grpc.pb.cc
${CMAKE_CURRENT_BINARY_DIR}/helloworld.grpc.pb.h
)

set(SOURCE_FILES ${imguiFiles} ${trackManagerFiles} ${grpcFiles} ${grpcGenerated})

SOURCE_GROUP(grpc FILES ${grpcFiles})
SOURCE_GROUP(grpc\\generated FILES ${grpcGenerated})
SOURCE_GROUP(imgui FILES ${imguiFiles})
SOURCE_GROUP(TrackManager FILES ${trackManagerFiles})
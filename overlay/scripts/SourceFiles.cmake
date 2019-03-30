cmake_minimum_required (VERSION 3.13.0)

set(imguiFiles
imgui/imconfig.h
imgui/imgui.cpp
imgui/imgui.h
imgui/imgui_demo.cpp
imgui/imgui_draw.cpp
imgui/imgui_widgets.cpp
imgui/imgui_impl_dx9.cpp
imgui/imgui_impl_dx9.h
imgui/imgui_internal.h
imgui/stb_rect_pack.h
imgui/stb_textedit.h
imgui/stb_truetype.h
)

set(entryPointFiles
entry/dllmain.cpp
entry/dllmain.h
)

set(reflexFiles
reflex/GameWindow.h
reflex/GameWindow.cpp
reflex/Log.h
reflex/Log.cpp
reflex/InstalledTracks.h
reflex/InstalledTracks.cpp
reflex/OverlayKernel.h
reflex/OverlayKernel.cpp
reflex/SharedTracks.h
reflex/SharedTracks.cpp
reflex/TrackSelection.h
reflex/TrackSelection.cpp
)

set(grpcFiles
grpc/TrackManagementClient.h
grpc/TrackManagementClient.cpp
)

set(SOURCE_FILES ${imguiFiles} ${entryPointFiles} ${grpcFiles} ${reflexFiles})

SOURCE_GROUP(entry FILES ${entryPointFiles})
SOURCE_GROUP(grpc FILES ${grpcFiles})
SOURCE_GROUP(imgui FILES ${imguiFiles})
SOURCE_GROUP(reflex FILES ${reflexFiles})



#include "dllmain.h"

// 
// MinHook
// 
#include <MinHook.h>

// 
// STL
// 
#include <mutex>

// 
// ImGui includes
// 
#include "imgui/imgui.h"
#include "imgui/imgui_impl_dx9.h"

#include "grpc/testclient.h"

t_WindowProc OriginalDefWindowProc = nullptr;
t_WindowProc OriginalWindowProc = nullptr;
PINDICIUM_ENGINE engine = nullptr;


static void grpcTest()
{
	// Instantiate the client. It requires a channel, out of which the actual RPCs
	// are created. This channel models a connection to an endpoint (in this case,
	// localhost at port 50051). We indicate that the channel isn't authenticated
	// (use of InsecureChannelCredentials()).
	/*GreeterClient greeter(grpc::CreateChannel("localhost:50051", grpc::InsecureChannelCredentials()));
	std::string user("world");
	std::string reply = greeter.SayHello(user);
	std::cout << "Greeter received: " << reply << std::endl;*/
}

BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID)
{
	//
	// We don't need to get notified in thread attach- or detachments
	// 
	DisableThreadLibraryCalls(static_cast<HMODULE>(hInstance));

	INDICIUM_D3D9_EVENT_CALLBACKS d3d9;
	INDICIUM_D3D9_EVENT_CALLBACKS_INIT(&d3d9);
	d3d9.EvtIndiciumD3D9PreEndScene = EvtIndiciumD3D9PreEndScene;
	d3d9.EvtIndiciumD3D9PreReset = EvtIndiciumD3D9PreReset;
	d3d9.EvtIndiciumD3D9PostReset = EvtIndiciumD3D9PostReset;
	d3d9.EvtIndiciumD3D9PrePresentEx = EvtIndiciumD3D9PresentEx;
	d3d9.EvtIndiciumD3D9PreResetEx = EvtIndiciumD3D9PreResetEx;
	d3d9.EvtIndiciumD3D9PostResetEx = EvtIndiciumD3D9PostResetEx;

	INDICIUM_ERROR err;

	switch (dwReason)
	{
	case DLL_PROCESS_ATTACH:

		if (!engine)
		{
			//
			// Get engine handle
			// 
			engine = IndiciumEngineAlloc();

			//
			// Register render pipeline callbacks
			// 
			IndiciumEngineSetD3D9EventCallbacks(engine, &d3d9);

			// 
			// TODO: cover failure
			// 
			err = IndiciumEngineInit(engine, EvtIndiciumGameHooked);
		}

		break;
	case DLL_PROCESS_DETACH:

		if (engine)
		{
			IndiciumEngineShutdown(engine, EvtIndiciumGameUnhooked);
			IndiciumEngineFree(engine);
		}

		break;
	default:
		break;
	}

	return TRUE;
}

void EvtIndiciumGameHooked(const INDICIUM_D3D_VERSION GameVersion)
{
	MH_STATUS status = MH_Initialize();

	if (status != MH_OK && status != MH_ERROR_ALREADY_INITIALIZED)
	{
		//GNARLY_TODO: Add logging
	}

	// Setup Dear ImGui binding
	IMGUI_CHECKVERSION();
	ImGui::CreateContext();
	ImGuiIO& io = ImGui::GetIO(); (void)io;
	//io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;  // Enable Keyboard Controls

	// Setup style
	ImGui::StyleColorsDark();
}

void EvtIndiciumGameUnhooked()
{

	if (MH_DisableHook(MH_ALL_HOOKS) != MH_OK)
	{
		//GNARLY_TODO: Add logging
		return;
	}

	if (MH_Uninitialize() != MH_OK)
	{
		//GNARLY_TODO: Add logging
		return;
	}
}

#pragma region D3D9(Ex)

void EvtIndiciumD3D9PreEndScene(
	LPDIRECT3DDEVICE9   pDevice
)
{
	static auto initialized = false;
	static bool show_overlay = true;
	static std::once_flag init;

	//
	// This section is only called once to initialize ImGui
	// 
	std::call_once(init, [&](LPDIRECT3DDEVICE9 pd3dDevice)
	{
		D3DDEVICE_CREATION_PARAMETERS params;

		auto hr = pd3dDevice->GetCreationParameters(&params);
		if (FAILED(hr))
		{
			//GNARLY_TODO: Add logging
			return;
		}

		ImGui_ImplDX9_Init(params.hFocusWindow, pd3dDevice);

		HookWindowProc(params.hFocusWindow);

		initialized = true;
		//This is a test to communicate with an external process.
		//grpcTest();

	}, pDevice);

	if (!initialized)
		return;

	TOGGLE_STATE(VK_F12, show_overlay);
	if (!show_overlay) 
		return;

	ImGui_ImplDX9_NewFrame();
	RenderScene();
	ImGui_ImplDX9_RenderDrawData(ImGui::GetDrawData());
}

void EvtIndiciumD3D9PreReset(
	LPDIRECT3DDEVICE9       pDevice,
	D3DPRESENT_PARAMETERS   *pPresentationParameters
)
{
	ImGui_ImplDX9_InvalidateDeviceObjects();
}

void EvtIndiciumD3D9PostReset(
	LPDIRECT3DDEVICE9       pDevice,
	D3DPRESENT_PARAMETERS   *pPresentationParameters
)
{
	ImGui_ImplDX9_CreateDeviceObjects();
}

void EvtIndiciumD3D9PresentEx(
	LPDIRECT3DDEVICE9EX     pDevice,
	const RECT              *pSourceRect,
	const RECT              *pDestRect,
	HWND                    hDestWindowOverride,
	const RGNDATA           *pDirtyRegion,
	DWORD                   dwFlags
)
{
	static auto initialized = false;
	static bool show_overlay = true;
	static std::once_flag init;

	//
	// This section is only called once to initialize ImGui
	// 
	std::call_once(init, [&](LPDIRECT3DDEVICE9EX pd3dDevice)
	{
		D3DDEVICE_CREATION_PARAMETERS params;

		auto hr = pd3dDevice->GetCreationParameters(&params);
		if (FAILED(hr))
		{
			//GNARLY_TODO: Add logging
			return;
		}

		ImGui_ImplDX9_Init(params.hFocusWindow, pd3dDevice);

		//GNARLY_TODO: Add logging

		HookWindowProc(params.hFocusWindow);

		initialized = true;

	}, pDevice);

	if (!initialized)
		return;

	TOGGLE_STATE(VK_F12, show_overlay);
	if (!show_overlay) 
		return;

	ImGui_ImplDX9_NewFrame();
	RenderScene();
	ImGui_ImplDX9_RenderDrawData(ImGui::GetDrawData());
}

void EvtIndiciumD3D9PreResetEx(
	LPDIRECT3DDEVICE9EX     pDevice,
	D3DPRESENT_PARAMETERS   *pPresentationParameters,
	D3DDISPLAYMODEEX        *pFullscreenDisplayMode
)
{
	ImGui_ImplDX9_InvalidateDeviceObjects();
}

void EvtIndiciumD3D9PostResetEx(
	LPDIRECT3DDEVICE9EX     pDevice,
	D3DPRESENT_PARAMETERS   *pPresentationParameters,
	D3DDISPLAYMODEEX        *pFullscreenDisplayMode
)
{
	ImGui_ImplDX9_CreateDeviceObjects();
}

#pragma endregion


#pragma region WNDPROC Hooking

void HookWindowProc(HWND hWnd)
{

	MH_STATUS ret;

	if ((ret = MH_CreateHook(
		&DefWindowProcW,
		&DetourDefWindowProc,
		reinterpret_cast<LPVOID*>(&OriginalDefWindowProc))
		) != MH_OK)
	{
		//GNARLY_TODO: Add logging
		return;
	}

	if (ret == MH_OK && MH_EnableHook(&DefWindowProcW) != MH_OK)
	{
		//GNARLY_TODO: Add logging
	}

	if ((ret = MH_CreateHook(
		&DefWindowProcA,
		&DetourDefWindowProc,
		reinterpret_cast<LPVOID*>(&OriginalDefWindowProc))
		) != MH_OK)
	{
		//GNARLY_TODO: Add logging
		return;
	}

	if (ret == MH_OK && MH_EnableHook(&DefWindowProcA) != MH_OK)
	{
		//GNARLY_TODO: Add logging
	}

	auto lptrWndProc = reinterpret_cast<t_WindowProc>(GetWindowLongPtr(hWnd, GWLP_WNDPROC));

	if (MH_CreateHook(lptrWndProc, &DetourWindowProc, reinterpret_cast<LPVOID*>(&OriginalWindowProc)) != MH_OK)
	{
		//GNARLY_TODO: Add logging
		return;
	}

	if (MH_EnableHook(lptrWndProc) != MH_OK)
	{
		//GNARLY_TODO: Add logging
	}
}

LRESULT WINAPI DetourDefWindowProc(
	_In_ HWND hWnd,
	_In_ UINT Msg,
	_In_ WPARAM wParam,
	_In_ LPARAM lParam
)
{
	ImGui_ImplWin32_WndProcHandler(hWnd, Msg, wParam, lParam);

	return OriginalDefWindowProc(hWnd, Msg, wParam, lParam);
}

LRESULT WINAPI DetourWindowProc(
	_In_ HWND hWnd,
	_In_ UINT Msg,
	_In_ WPARAM wParam,
	_In_ LPARAM lParam
)
{
	ImGui_ImplWin32_WndProcHandler(hWnd, Msg, wParam, lParam);

	return OriginalWindowProc(hWnd, Msg, wParam, lParam);
}

#pragma endregion

#pragma region Main content rendering

void RenderScene()
{
	ImGui::ShowMetricsWindow();

	{
		ImGui::SetNextWindowPos(ImVec2(1400, 60));
		ImGui::Begin(
			"Some plots =)",
			nullptr,
			ImGuiWindowFlags_AlwaysAutoResize
		);

		static bool animate = true;
		ImGui::Checkbox("Animate", &animate);

		static float arr[] = { 0.6f, 0.1f, 1.0f, 0.5f, 0.92f, 0.1f, 0.2f };
		ImGui::PlotLines("Frame Times", arr, IM_ARRAYSIZE(arr));

		// Create a dummy array of contiguous float values to plot
		// Tip: If your float aren't contiguous but part of a structure, you can pass a pointer to your first float and the sizeof() of your structure in the Stride parameter.
		static float values[90] = { 0 };
		static int values_offset = 0;
		static float refresh_time = 0.0f;
		if (!animate || refresh_time == 0.0f)
			refresh_time = ImGui::GetTime();
		while (refresh_time < ImGui::GetTime()) // Create dummy data at fixed 60 hz rate for the demo
		{
			static float phase = 0.0f;
			values[values_offset] = cosf(phase);
			values_offset = (values_offset + 1) % IM_ARRAYSIZE(values);
			phase += 0.10f*values_offset;
			refresh_time += 1.0f / 60.0f;
		}
		ImGui::PlotLines("Lines", values, IM_ARRAYSIZE(values), values_offset, "avg 0.0", -1.0f, 1.0f, ImVec2(0, 80));
		ImGui::PlotHistogram("Histogram", arr, IM_ARRAYSIZE(arr), 0, NULL, 0.0f, 1.0f, ImVec2(0, 80));

		// Use functions to generate output
		// FIXME: This is rather awkward because current plot API only pass in indices. We probably want an API passing floats and user provide sample rate/count.
		struct Funcs
		{
			static float Sin(void*, int i) { return sinf(i * 0.1f); }
			static float Saw(void*, int i) { return (i & 1) ? 1.0f : -1.0f; }
		};
		static int func_type = 0, display_count = 70;
		ImGui::Separator();
		ImGui::PushItemWidth(100); ImGui::Combo("func", &func_type, "Sin\0Saw\0"); ImGui::PopItemWidth();
		ImGui::SameLine();
		ImGui::SliderInt("Sample count", &display_count, 1, 400);
		float(*func)(void*, int) = (func_type == 0) ? Funcs::Sin : Funcs::Saw;
		ImGui::PlotLines("Lines", func, NULL, display_count, 0, NULL, -1.0f, 1.0f, ImVec2(0, 80));
		ImGui::PlotHistogram("Histogram", func, NULL, display_count, 0, NULL, -1.0f, 1.0f, ImVec2(0, 80));
		ImGui::Separator();

		// Animate a simple progress bar
		static float progress = 0.0f, progress_dir = 1.0f;
		if (animate)
		{
			progress += progress_dir * 0.4f * ImGui::GetIO().DeltaTime;
			if (progress >= +1.1f) { progress = +1.1f; progress_dir *= -1.0f; }
			if (progress <= -0.1f) { progress = -0.1f; progress_dir *= -1.0f; }
		}

		// Typically we would use ImVec2(-1.0f,0.0f) to use all available width, or ImVec2(width,0.0f) for a specified width. ImVec2(0.0f,0.0f) uses ItemWidth.
		ImGui::ProgressBar(progress, ImVec2(0.0f, 0.0f));
		ImGui::SameLine(0.0f, ImGui::GetStyle().ItemInnerSpacing.x);
		ImGui::Text("Progress Bar");

		float progress_saturated = (progress < 0.0f) ? 0.0f : (progress > 1.0f) ? 1.0f : progress;
		char buf[32];
		sprintf(buf, "%d/%d", (int)(progress_saturated * 1753), 1753);
		ImGui::ProgressBar(progress, ImVec2(0.f, 0.f), buf);

		ImGui::End();
	}

	ImGui::Render();
}

#pragma endregion

#pragma region ImGui-specific (taken from their examples unmodified)

bool ImGui_ImplWin32_UpdateMouseCursor()
{
	ImGuiIO& io = ImGui::GetIO();
	if (io.ConfigFlags & ImGuiConfigFlags_NoMouseCursorChange)
		return false;

	ImGuiMouseCursor imgui_cursor = io.MouseDrawCursor ? ImGuiMouseCursor_None : ImGui::GetMouseCursor();
	if (imgui_cursor == ImGuiMouseCursor_None)
	{
		// Hide OS mouse cursor if imgui is drawing it or if it wants no cursor
		::SetCursor(NULL);
	}
	else
	{
		// Hardware cursor type
		LPTSTR win32_cursor = IDC_ARROW;
		switch (imgui_cursor)
		{
		case ImGuiMouseCursor_Arrow:        win32_cursor = IDC_ARROW; break;
		case ImGuiMouseCursor_TextInput:    win32_cursor = IDC_IBEAM; break;
		case ImGuiMouseCursor_ResizeAll:    win32_cursor = IDC_SIZEALL; break;
		case ImGuiMouseCursor_ResizeEW:     win32_cursor = IDC_SIZEWE; break;
		case ImGuiMouseCursor_ResizeNS:     win32_cursor = IDC_SIZENS; break;
		case ImGuiMouseCursor_ResizeNESW:   win32_cursor = IDC_SIZENESW; break;
		case ImGuiMouseCursor_ResizeNWSE:   win32_cursor = IDC_SIZENWSE; break;
		}
		::SetCursor(::LoadCursor(NULL, win32_cursor));
	}
	return true;
}

// Allow compilation with old Windows SDK. MinGW doesn't have default _WIN32_WINNT/WINVER versions.
#ifndef WM_MOUSEHWHEEL
#define WM_MOUSEHWHEEL 0x020E
#endif

// Process Win32 mouse/keyboard inputs. 
// You can read the io.WantCaptureMouse, io.WantCaptureKeyboard flags to tell if dear imgui wants to use your inputs.
// - When io.WantCaptureMouse is true, do not dispatch mouse input data to your main application.
// - When io.WantCaptureKeyboard is true, do not dispatch keyboard input data to your main application.
// Generally you may always pass all inputs to dear imgui, and hide them from your application based on those two flags.
// PS: In this Win32 handler, we use the capture API (GetCapture/SetCapture/ReleaseCapture) to be able to read mouse coordinations when dragging mouse outside of our window bounds.
// PS: We treat DBLCLK messages as regular mouse down messages, so this code will work on windows classes that have the CS_DBLCLKS flag set. Our own example app code doesn't set this flag.
IMGUI_API LRESULT ImGui_ImplWin32_WndProcHandler(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	if (ImGui::GetCurrentContext() == NULL)
		return 0;

	ImGuiIO& io = ImGui::GetIO();
	switch (msg)
	{
	case WM_LBUTTONDOWN: case WM_LBUTTONDBLCLK:
	case WM_RBUTTONDOWN: case WM_RBUTTONDBLCLK:
	case WM_MBUTTONDOWN: case WM_MBUTTONDBLCLK:
	{
		int button = 0;
		if (msg == WM_LBUTTONDOWN || msg == WM_LBUTTONDBLCLK) button = 0;
		if (msg == WM_RBUTTONDOWN || msg == WM_RBUTTONDBLCLK) button = 1;
		if (msg == WM_MBUTTONDOWN || msg == WM_MBUTTONDBLCLK) button = 2;
		if (!ImGui::IsAnyMouseDown() && ::GetCapture() == NULL)
			::SetCapture(hwnd);
		io.MouseDown[button] = true;
		return 0;
	}
	case WM_LBUTTONUP:
	case WM_RBUTTONUP:
	case WM_MBUTTONUP:
	{
		int button = 0;
		if (msg == WM_LBUTTONUP) button = 0;
		if (msg == WM_RBUTTONUP) button = 1;
		if (msg == WM_MBUTTONUP) button = 2;
		io.MouseDown[button] = false;
		if (!ImGui::IsAnyMouseDown() && ::GetCapture() == hwnd)
			::ReleaseCapture();
		return 0;
	}
	case WM_MOUSEWHEEL:
		io.MouseWheel += GET_WHEEL_DELTA_WPARAM(wParam) > 0 ? +1.0f : -1.0f;
		return 0;
	case WM_MOUSEHWHEEL:
		io.MouseWheelH += GET_WHEEL_DELTA_WPARAM(wParam) > 0 ? +1.0f : -1.0f;
		return 0;
	case WM_MOUSEMOVE:
		io.MousePos.x = (signed short)(lParam);
		io.MousePos.y = (signed short)(lParam >> 16);
		return 0;
	case WM_KEYDOWN:
	case WM_SYSKEYDOWN:
		if (wParam < 256)
			io.KeysDown[wParam] = 1;
		return 0;
	case WM_KEYUP:
	case WM_SYSKEYUP:
		if (wParam < 256)
			io.KeysDown[wParam] = 0;
		return 0;
	case WM_CHAR:
		// You can also use ToAscii()+GetKeyboardState() to retrieve characters.
		if (wParam > 0 && wParam < 0x10000)
			io.AddInputCharacter((unsigned short)wParam);
		return 0;
	case WM_SETCURSOR:
		if (LOWORD(lParam) == HTCLIENT && ImGui_ImplWin32_UpdateMouseCursor())
			return 1;
		return 0;
	}
	return 0;
}

#pragma endregion

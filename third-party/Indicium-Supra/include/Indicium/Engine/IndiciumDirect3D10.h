#ifndef IndiciumDirect3D10_h__
#define IndiciumDirect3D10_h__

#include <dxgi.h>
#include <d3d10_1.h>

typedef
_Function_class_(EVT_INDICIUM_D3D10_PRESENT)
VOID
EVT_INDICIUM_D3D10_PRESENT(
    IDXGISwapChain  *pSwapChain,
    UINT            SyncInterval,
    UINT            Flags
);

typedef EVT_INDICIUM_D3D10_PRESENT *PFN_INDICIUM_D3D10_PRESENT;

typedef
_Function_class_(EVT_INDICIUM_D3D10_RESIZE_TARGET)
VOID
EVT_INDICIUM_D3D10_RESIZE_TARGET(
    IDXGISwapChain          *pSwapChain,
    const DXGI_MODE_DESC    *pNewTargetParameters
);

typedef EVT_INDICIUM_D3D10_RESIZE_TARGET *PFN_INDICIUM_D3D10_RESIZE_TARGET;

typedef
_Function_class_(EVT_INDICIUM_D3D10_RESIZE_BUFFERS)
VOID
EVT_INDICIUM_D3D10_RESIZE_BUFFERS(
    IDXGISwapChain  *pSwapChain,
    UINT            BufferCount,
    UINT            Width,
    UINT            Height,
    DXGI_FORMAT     NewFormat,
    UINT            SwapChainFlags
);

typedef EVT_INDICIUM_D3D10_RESIZE_BUFFERS *PFN_INDICIUM_D3D10_RESIZE_BUFFERS;

HRESULT
FORCEINLINE
D3D10_DEVICE_FROM_SWAPCHAIN(
    IDXGISwapChain *pSwapChain,
    ID3D10Device **ppDevice
)
{
    return pSwapChain->GetDevice(__uuidof(ID3D10Device), (PVOID*)ppDevice);
}


typedef struct _INDICIUM_D3D10_EVENT_CALLBACKS
{
    PFN_INDICIUM_D3D10_PRESENT          EvtIndiciumD3D10PrePresent;
    PFN_INDICIUM_D3D10_PRESENT          EvtIndiciumD3D10PostPresent;

    PFN_INDICIUM_D3D10_RESIZE_TARGET    EvtIndiciumD3D10PreResizeTarget;
    PFN_INDICIUM_D3D10_RESIZE_TARGET    EvtIndiciumD3D10PostResizeTarget;

    PFN_INDICIUM_D3D10_RESIZE_BUFFERS   EvtIndiciumD3D10PreResizeBuffers;
    PFN_INDICIUM_D3D10_RESIZE_BUFFERS   EvtIndiciumD3D10PostResizeBuffers;

} INDICIUM_D3D10_EVENT_CALLBACKS, *PINDICIUM_D3D10_EVENT_CALLBACKS;

VOID
FORCEINLINE
INDICIUM_D3D10_EVENT_CALLBACKS_INIT(
    _Out_ PINDICIUM_D3D10_EVENT_CALLBACKS Callbacks
)
{
    RtlZeroMemory(Callbacks, sizeof(INDICIUM_D3D10_EVENT_CALLBACKS));
}

#endif // IndiciumDirect3D10_h__

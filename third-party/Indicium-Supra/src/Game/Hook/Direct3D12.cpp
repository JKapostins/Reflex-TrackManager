/*
MIT License

Copyright (c) 2018 Benjamin H�glinger

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#include "Direct3D12.h"
#include <dxgi.h>
#include <dxgi1_4.h>
#include <d3d12.h>
#include "DXGI.h"
#include <Poco/Exception.h>


Direct3D12Hooking::Direct3D12::Direct3D12() : pd3dDevice(nullptr), pQueue(nullptr), pSwapChain(nullptr)
{
    temp_window = new Window("TempDirect3D12OverlayWindow");

    auto hModDXGI = GetModuleHandle("DXGI.dll");
    auto hModD3D12 = GetModuleHandle("D3D12.dll");

    if (hModDXGI == nullptr)
    {
        throw Poco::RuntimeException("Couldn't get handle to DXGI.dll");
    }

    if (hModD3D12 == nullptr)
    {
        throw Poco::RuntimeException("Couldn't get handle to D3D12.dll");
    }

    auto hD3D12CreateDevice = static_cast<LPVOID>(GetProcAddress(hModD3D12, "D3D12CreateDevice"));

    if (hD3D12CreateDevice == nullptr)
    {
        throw Poco::RuntimeException("Couldn't get handle to D3D12CreateDevice");
    }

    auto hCreateDXGIFactory1 = static_cast<LPVOID>(GetProcAddress(hModDXGI, "CreateDXGIFactory1"));
    if (hCreateDXGIFactory1 == nullptr)
    {
        throw Poco::RuntimeException("Couldn't get handle to CreateDXGIFactory1");
    }

    IDXGIFactory4* pFactory;
    auto hr = static_cast<HRESULT(WINAPI *)(
        REFIID,
        void**)>(hCreateDXGIFactory1)(IID_PPV_ARGS(&pFactory));

    if (FAILED(hr))
    {
        throw Poco::RuntimeException("Couldn't create DXGI factory");
    }

    auto hr12 = static_cast<HRESULT(WINAPI *)(
        IUnknown*,
        D3D_FEATURE_LEVEL,
        REFIID,
        void**)>(hD3D12CreateDevice)(
            nullptr,
            D3D_FEATURE_LEVEL_11_0,
            IID_PPV_ARGS(&pd3dDevice));

    if (FAILED(hr12))
    {
        throw Poco::RuntimeException("Couldn't create D3D12 device");
    }

    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
    queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;

    hr = pd3dDevice->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&pQueue));

    if (FAILED(hr))
    {
        throw Poco::RuntimeException("Command queue creation failed");
    }

    DXGI_SWAP_CHAIN_DESC scDesc;
    ZeroMemory(&scDesc, sizeof(scDesc));
    scDesc.BufferCount = 2;
    scDesc.BufferDesc.Width = 0;
    scDesc.BufferDesc.Height = 0;
    scDesc.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    scDesc.BufferDesc.RefreshRate.Numerator = 60;
    scDesc.BufferDesc.RefreshRate.Denominator = 1;
    scDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    scDesc.OutputWindow = temp_window->windowHandle();
    scDesc.SampleDesc.Count = 1;
    scDesc.SampleDesc.Quality = 0;
    scDesc.Windowed = TRUE;
    scDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;

    hr = pFactory->CreateSwapChain(
        pQueue,
        &scDesc,
        &pSwapChain
    );

    if (FAILED(hr))
    {
        throw Poco::RuntimeException("Swap chain creation failed");
    }
}

std::vector<UINTX> Direct3D12Hooking::Direct3D12::vtable() const
{
    UINTX vtbl[DXGIHooking::DXGI::SwapChainVTableElements];
    memcpy(vtbl, *reinterpret_cast<UINTX **>(pSwapChain), DXGIHooking::DXGI::SwapChainVTableElements * sizeof(UINTX));
    return std::vector<UINTX>(vtbl, vtbl + sizeof vtbl / sizeof vtbl[0]);
}


Direct3D12Hooking::Direct3D12::~Direct3D12()
{
    if (pSwapChain)
        pSwapChain->Release();

    if (pd3dDevice)
        pd3dDevice->Release();

    if (pQueue)
        pQueue->Release();
}

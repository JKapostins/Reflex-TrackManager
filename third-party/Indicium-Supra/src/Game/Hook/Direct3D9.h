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

#pragma once
#include <d3d9.h>
#include <intarch.h>
#include "Direct3DBase.h"

namespace Direct3D9Hooking
{
    enum Direct3DDevice9FunctionOrdinals : short
    {
        QueryInterface = 0,
        AddRef = 1,
        Release = 2,
        TestCooperativeLevel = 3,
        GetAvailableTextureMem = 4,
        EvictManagedResources = 5,
        GetDirect3D = 6,
        GetDeviceCaps = 7,
        GetDisplayMode = 8,
        GetCreationParameters = 9,
        SetCursorProperties = 10,
        SetCursorPosition = 11,
        ShowCursor = 12,
        CreateAdditionalSwapChain = 13,
        GetSwapChain = 14,
        GetNumberOfSwapChains = 15,
        Reset = 16,
        Present = 17,
        GetBackBuffer = 18,
        GetRasterStatus = 19,
        SetDialogBoxMode = 20,
        SetGammaRamp = 21,
        GetGammaRamp = 22,
        CreateTexture = 23,
        CreateVolumeTexture = 24,
        CreateCubeTexture = 25,
        CreateVertexBuffer = 26,
        CreateIndexBuffer = 27,
        CreateRenderTarget = 28,
        CreateDepthStencilSurface = 29,
        UpdateSurface = 30,
        UpdateTexture = 31,
        GetRenderTargetData = 32,
        GetFrontBufferData = 33,
        StretchRect = 34,
        ColorFill = 35,
        CreateOffscreenPlainSurface = 36,
        SetRenderTarget = 37,
        GetRenderTarget = 38,
        SetDepthStencilSurface = 39,
        GetDepthStencilSurface = 40,
        BeginScene = 41,
        EndScene = 42,
        Clear = 43,
        SetTransform = 44,
        GetTransform = 45,
        MultiplyTransform = 46,
        SetViewport = 47,
        GetViewport = 48,
        SetMaterial = 49,
        GetMaterial = 50,
        SetLight = 51,
        GetLight = 52,
        LightEnable = 53,
        GetLightEnable = 54,
        SetClipPlane = 55,
        GetClipPlane = 56,
        SetRenderState = 57,
        GetRenderState = 58,
        CreateStateBlock = 59,
        BeginStateBlock = 60,
        EndStateBlock = 61,
        SetClipStatus = 62,
        GetClipStatus = 63,
        GetTexture = 64,
        SetTexture = 65,
        GetTextureStageState = 66,
        SetTextureStageState = 67,
        GetSamplerState = 68,
        SetSamplerState = 69,
        ValidateDevice = 70,
        SetPaletteEntries = 71,
        GetPaletteEntries = 72,
        SetCurrentTexturePalette = 73,
        GetCurrentTexturePalette = 74,
        SetScissorRect = 75,
        GetScissorRect = 76,
        SetSoftwareVertexProcessing = 77,
        GetSoftwareVertexProcessing = 78,
        SetNPatchMode = 79,
        GetNPatchMode = 80,
        DrawPrimitive = 81,
        DrawIndexedPrimitive = 82,
        DrawPrimitiveUP = 83,
        DrawIndexedPrimitiveUP = 84,
        ProcessVertices = 85,
        CreateVertexDeclaration = 86,
        SetVertexDeclaration = 87,
        GetVertexDeclaration = 88,
        SetFVF = 89,
        GetFVF = 90,
        CreateVertexShader = 91,
        SetVertexShader = 92,
        GetVertexShader = 93,
        SetVertexShaderConstantF = 94,
        GetVertexShaderConstantF = 95,
        SetVertexShaderConstantI = 96,
        GetVertexShaderConstantI = 97,
        SetVertexShaderConstantB = 98,
        GetVertexShaderConstantB = 99,
        SetStreamSource = 100,
        GetStreamSource = 101,
        SetStreamSourceFreq = 102,
        GetStreamSourceFreq = 103,
        SetIndices = 104,
        GetIndices = 105,
        CreatePixelShader = 106,
        SetPixelShader = 107,
        GetPixelShader = 108,
        SetPixelShaderConstantF = 109,
        GetPixelShaderConstantF = 110,
        SetPixelShaderConstantI = 111,
        GetPixelShaderConstantI = 112,
        SetPixelShaderConstantB = 113,
        GetPixelShaderConstantB = 114,
        DrawRectPatch = 115,
        DrawTriPatch = 116,
        DeletePatch = 117,
        CreateQuery = 118,
    };

    class Direct3D9 : public Direct3DHooking::Direct3DBase
    {
        IDirect3D9* d3d9;
        IDirect3DDevice9* d3d9_device;
    public:
        Direct3D9();

        static const int VTableElements = 119;

        std::vector<UINTX> vtable() const override;
    protected:
        ~Direct3D9();
    };
}

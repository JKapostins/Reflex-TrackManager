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

#include "Window.h"

#include <Poco/Exception.h>
#include <utility>


Window::Window(std::string windowClassName) : temp_window(nullptr), window_class_name(std::move(windowClassName))
{
    ZeroMemory(&window_class, sizeof(WNDCLASSEX));

    window_class.cbSize = sizeof(WNDCLASSEX);
    window_class.style = CS_HREDRAW | CS_VREDRAW;
    window_class.lpfnWndProc = DefWindowProc;
    window_class.lpszClassName = window_class_name.c_str();

    window_class.hInstance = GetModuleHandle(nullptr);
    if (window_class.hInstance == nullptr)
    {
        throw Poco::RuntimeException("Could not get the instance handle", GetLastError());
    }

    if (!RegisterClassEx(&window_class))
    {
        throw Poco::RuntimeException("Could not get register the window class", GetLastError());
    }

    temp_window = CreateWindow(window_class.lpszClassName, "Temporary DirectX Overlay Window",
        WS_OVERLAPPEDWINDOW, 0, 0, 100, 100, NULL, NULL, window_class.hInstance, NULL);
    if (temp_window == nullptr)
    {
        throw Poco::RuntimeException("Could not get create the temporary window", GetLastError());
    }
}

Window::~Window()
{
    if (temp_window)
    {
        DestroyWindow(temp_window);
        UnregisterClass(window_class.lpszClassName, window_class.hInstance);
    }
}

HWND Window::windowHandle() const
{
    return temp_window;
}

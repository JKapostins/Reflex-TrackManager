#include "GameWindow.h"

namespace game_window
{
	ImVec2 ScaleWindowSize(ImVec2 imguiWindowSize, ImVec2 totalWindosSize)
	{
		const int baseResolutionX = 1920;
		const int baseResolutionY = 1080;

		if (totalWindosSize.x < baseResolutionX)
		{
			return ImVec2(imguiWindowSize.x * (totalWindosSize.x / baseResolutionX), imguiWindowSize.y * (totalWindosSize.y / baseResolutionY));
		}

		return imguiWindowSize;
	}
}
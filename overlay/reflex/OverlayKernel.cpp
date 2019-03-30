#include "reflex/OverlayKernel.h"

#include "grpc/TrackManagementClient.h"
#include "imgui/imgui.h"
#include "reflex/GameWindow.h"
#include "reflex/Log.h"
#include "reflex/InstalledTracks.h"
#include "reflex/SharedTracks.h"
#include "reflex/TrackSelection.h"

OverlayKernel::OverlayKernel(int width, int height)
	: m_trackManagementClient(std::make_shared<TrackManagementClient>(width, height, grpc::CreateChannel("localhost:50051", grpc::InsecureChannelCredentials())))
	, m_trackSelection(std::make_unique<TrackSelection>(m_trackManagementClient))
	, m_log(std::make_unique<Log>(m_trackManagementClient))
	, m_installedNationals(std::make_unique<InstalledTracks>(m_trackManagementClient, "National"))
	, m_installedSupercross(std::make_unique<InstalledTracks>(m_trackManagementClient, "Supercross"))
	, m_installedFreeRide(std::make_unique<InstalledTracks>(m_trackManagementClient, "FreeRide"))
	, m_sharedTracks(std::make_unique<SharedTracks>(m_trackManagementClient))
	, m_overlayVisible(true)
{
}

OverlayKernel::~OverlayKernel()
{
}

void OverlayKernel::render(LPDIRECT3DDEVICE9 device)
{
	drawHelp();

	m_trackSelection->render(device);

	ImVec2 windowDimensions = game_window::ScaleWindowSize(ImVec2(934, 244), m_trackManagementClient->getGameWindowRect());
	ImGui::SetNextWindowPos(game_window::ScaleWindowSize(ImVec2(976, 295), m_trackManagementClient->getGameWindowRect()), ImGuiCond_Always);
	ImGui::SetNextWindowSize(windowDimensions, ImGuiCond_Always);
	
	bool adjustContentSize = m_trackManagementClient->getGameWindowRect().x < 1920;
	static float offset = 5;
	float contentHeight = windowDimensions.y + offset;
	
	if (adjustContentSize)
	{
		ImGui::SetNextWindowContentSize(ImVec2(0, contentHeight));
	}
	m_installedNationals->render();

	ImGui::SetNextWindowPos(game_window::ScaleWindowSize(ImVec2(976, 541), m_trackManagementClient->getGameWindowRect()), ImGuiCond_Always);
	ImGui::SetNextWindowSize(windowDimensions, ImGuiCond_Always);
	if (adjustContentSize)
	{
		ImGui::SetNextWindowContentSize(ImVec2(0, contentHeight));
	}
	m_installedSupercross->render();

	ImGui::SetNextWindowPos(game_window::ScaleWindowSize(ImVec2(1444, 49), m_trackManagementClient->getGameWindowRect()), ImGuiCond_Always);
	ImGui::SetNextWindowSize(game_window::ScaleWindowSize(ImVec2(466, 244), m_trackManagementClient->getGameWindowRect()), ImGuiCond_Always);
	if (adjustContentSize)
	{
		ImGui::SetNextWindowContentSize(ImVec2(0, contentHeight));
	}
	m_installedFreeRide->render();

	m_sharedTracks->render();
	m_log->render();
}

void OverlayKernel::setVisibility(bool visible)
{
	m_overlayVisible = visible;
	m_trackManagementClient->setOverlayVisible(visible);
}

void OverlayKernel::drawHelp()
{
	ImGui::SetNextWindowPos(game_window::ScaleWindowSize(ImVec2(733, 12), m_trackManagementClient->getGameWindowRect()), ImGuiCond_Always);
	ImGui::SetNextWindowSize(ImVec2(374, 24), ImGuiCond_Always);
	ImGui::SetNextWindowBgAlpha(0.3f); // Transparent background
	if (ImGui::Begin("ToggleHelp", nullptr,   ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoResize))
	{
		ImGui::Text("Press f11 on your keyboard to show/hide the overlay");
	}
	ImGui::End();
}

void OverlayKernel::invalidateDeviceObjects()
{
	m_trackSelection->invalidateDeviceObjects();
}
void OverlayKernel::createDeviceObjects(LPDIRECT3DDEVICE9 device)
{
	m_trackSelection->createDeviceObjects(device);
}


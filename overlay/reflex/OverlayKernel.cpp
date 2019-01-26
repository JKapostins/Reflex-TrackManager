#include "reflex/OverlayKernel.h"

#include "grpc/TrackManagementClient.h"
#include "imgui/imgui.h"
#include "reflex/Log.h"
#include "reflex/InstalledTracks.h"
#include "reflex/SharedTracks.h"
#include "reflex/TrackSelection.h"

OverlayKernel::OverlayKernel()
	: m_trackManagementClient(std::make_shared<TrackManagementClient>(grpc::CreateChannel("localhost:50051", grpc::InsecureChannelCredentials())))
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

	ImGui::SetNextWindowPos(ImVec2(976, 295), ImGuiCond_Always);
	ImGui::SetNextWindowSize(ImVec2(934, 244), ImGuiCond_Always);
	m_installedNationals->render();

	ImGui::SetNextWindowPos(ImVec2(976, 541), ImGuiCond_Always);
	ImGui::SetNextWindowSize(ImVec2(934, 244), ImGuiCond_Always);
	m_installedSupercross->render();

	ImGui::SetNextWindowPos(ImVec2(1444, 49), ImGuiCond_Always);
	ImGui::SetNextWindowSize(ImVec2(466, 244), ImGuiCond_Always);
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
	ImGui::SetNextWindowPos(ImVec2(733,12), ImGuiCond_Always);
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


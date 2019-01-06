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
{
}

OverlayKernel::~OverlayKernel()
{
}

void OverlayKernel::render(LPDIRECT3DDEVICE9 device)
{
	m_trackSelection->render(device);

	ImGui::SetNextWindowPos(ImVec2(976, 295), ImGuiCond_FirstUseEver);
	ImGui::SetNextWindowSize(ImVec2(934, 244), ImGuiCond_FirstUseEver);	
	m_installedNationals->render();

	ImGui::SetNextWindowPos(ImVec2(976, 541), ImGuiCond_FirstUseEver);
	ImGui::SetNextWindowSize(ImVec2(934, 244), ImGuiCond_FirstUseEver);
	m_installedSupercross->render();

	ImGui::SetNextWindowPos(ImVec2(1444, 49), ImGuiCond_FirstUseEver);
	ImGui::SetNextWindowSize(ImVec2(466, 244), ImGuiCond_FirstUseEver);
	m_installedFreeRide->render();

	m_sharedTracks->render();
	m_log->render();
}

void OverlayKernel::setVisibility(bool visible)
{
	m_trackManagementClient->setOverlayVisible(visible);
}

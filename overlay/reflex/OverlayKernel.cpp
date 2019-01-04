#include "reflex/OverlayKernel.h"

#include "grpc/TrackManagementClient.h"
#include "reflex/Log.h"
#include "reflex/InstalledTracks.h"
#include "reflex/TrackSelection.h"

OverlayKernel::OverlayKernel()
	: m_trackManagementClient(std::make_shared<TrackManagementClient>(grpc::CreateChannel("localhost:50051", grpc::InsecureChannelCredentials())))
	, m_trackSelection(std::make_unique<TrackSelection>(m_trackManagementClient))
	, m_log(std::make_unique<Log>(m_trackManagementClient))
	, m_installedNationals(std::make_unique<InstalledTracks>(m_trackManagementClient, "National"))
	, m_installedSupercross(std::make_unique<InstalledTracks>(m_trackManagementClient, "Supercross"))
	, m_installedFreeRide(std::make_unique<InstalledTracks>(m_trackManagementClient, "FreeRide"))
{
}

OverlayKernel::~OverlayKernel()
{
}

void OverlayKernel::render(LPDIRECT3DDEVICE9 device)
{
	m_trackSelection->render(device);
	m_installedNationals->render();
	m_installedSupercross->render();
	m_installedFreeRide->render();
	m_log->render();
}

#include "reflex/OverlayKernel.h"

#include "grpc/TrackManagementClient.h"
#include "reflex/TrackSelection.h"

OverlayKernel::OverlayKernel()
	: m_trackManagementClient(std::make_shared<TrackManagementClient>(grpc::CreateChannel("localhost:50051", grpc::InsecureChannelCredentials())))
	, m_trackSelection(std::make_unique<TrackSelection>(m_trackManagementClient))
{
}

OverlayKernel::~OverlayKernel()
{
}

void OverlayKernel::render(LPDIRECT3DDEVICE9 device)
{
	m_trackSelection->render(device);
}

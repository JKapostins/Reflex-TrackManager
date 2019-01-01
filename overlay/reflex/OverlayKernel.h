#pragma once

#include <string>

class TrackSelection;
class TrackManagementClient;

class OverlayKernel
{
public:
	OverlayKernel();
	~OverlayKernel();
	void render();

private:
	//GRPC client
	std::shared_ptr<TrackManagementClient> m_trackManagementClient;

	//UI objects
	std::unique_ptr<TrackSelection> m_trackSelection;
};
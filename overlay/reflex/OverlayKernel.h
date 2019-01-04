#pragma once
#include <d3d9.h>
#include <string>

class Log;
class TrackSelection;
class TrackManagementClient;

class OverlayKernel
{
public:
	OverlayKernel();
	~OverlayKernel();
	void render(LPDIRECT3DDEVICE9 device);

private:
	//GRPC client
	std::shared_ptr<TrackManagementClient> m_trackManagementClient;

	//UI objects
	std::unique_ptr<TrackSelection> m_trackSelection;
	std::unique_ptr<Log> m_log;
};
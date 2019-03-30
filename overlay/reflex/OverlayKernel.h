#pragma once
#include <d3d9.h>
#include <string>

class Log;
class InstalledTracks;
class SharedTracks;
class TrackSelection;
class TrackManagementClient;

class OverlayKernel
{
public:
	OverlayKernel(int width, int height);
	~OverlayKernel();
	void render(LPDIRECT3DDEVICE9 device);
	void setVisibility(bool visible);
	bool getVisibility() const { return m_overlayVisible; }
	void invalidateDeviceObjects();
	void createDeviceObjects(LPDIRECT3DDEVICE9 device);

private:
	void drawHelp();
	
	bool m_overlayVisible;

	//GRPC client
	std::shared_ptr<TrackManagementClient> m_trackManagementClient;

	//UI objects
	std::unique_ptr<TrackSelection> m_trackSelection;
	std::unique_ptr<Log> m_log;
	std::unique_ptr<InstalledTracks> m_installedNationals;
	std::unique_ptr<InstalledTracks> m_installedSupercross;
	std::unique_ptr<InstalledTracks> m_installedFreeRide;
	std::unique_ptr<SharedTracks> m_sharedTracks;
};
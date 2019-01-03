#pragma once
#include <d3d9.h>
#include <string>

class TrackManagementClient;

namespace trackmanagement
{
	class Track;
}

class TrackSelection
{
public:
	TrackSelection(std::shared_ptr<TrackManagementClient> client);
	~TrackSelection();
	void render(LPDIRECT3DDEVICE9 device);

private:
	void drawPreviewImage(LPDIRECT3DDEVICE9 device, const trackmanagement::Track& selected);
	LPDIRECT3DTEXTURE9 createTextureFromFile(LPDIRECT3DDEVICE9 device, const char* fileName);

	int m_previeImageWidth;
	int m_previewImageHeight;
	LPDIRECT3DTEXTURE9 m_previewImage;
	std::string m_selectedTrack;
	std::string m_previouslySelectedTrack;
	std::shared_ptr<TrackManagementClient> m_trackManagementClient;
};
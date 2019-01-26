#pragma once
#include <d3d9.h>
#include <string>

#include "grpc/TrackManagementClient.h"

class TrackManagementClient;

class TrackSelection
{
public:
	TrackSelection(std::shared_ptr<TrackManagementClient> client);
	~TrackSelection();
	void render(LPDIRECT3DDEVICE9 device);
	void invalidateDeviceObjects();
	void createDeviceObjects(LPDIRECT3DDEVICE9 device);

private:
	void drawPreviewImage(LPDIRECT3DDEVICE9 device, const trackmanagement::Track& selected);
	void drawComboBoxes();
	void drawTableBody();
	void drawActionButtons();
	void setTableColumnWidth();
	void flushVisibleTracks(trackmanagement::SortRequest* sortInfo, int displayStart, int displayEnd);
	LPDIRECT3DTEXTURE9 createTextureFromFile(LPDIRECT3DDEVICE9 device, const char* fileName);

	bool m_loadNewImage;
	int m_previeImageWidth;
	int m_previewImageHeight;
	int m_trackTypeFilterIndex;
	int m_slotFilterIndex;
	int m_sortByIndex;
	long long m_lastTimeStamp;
	trackmanagement::Track m_selectedTrack;
	trackmanagement::Track m_firstTrackInList;
	LPDIRECT3DTEXTURE9 m_previewImage;
	std::string m_selectedTrackName;
	std::string m_previouslySelectedTrackName;
	std::shared_ptr<TrackManagementClient> m_trackManagementClient;
	std::vector<trackmanagement::Track> m_visibleTracks;
};
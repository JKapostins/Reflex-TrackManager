#pragma once
#include <memory>
#include "grpc/TrackManagementClient.h"

class TrackManagementClient;

class InstalledTracks
{
public:
	InstalledTracks(std::shared_ptr<TrackManagementClient> client, const char* trackType);
	~InstalledTracks();
	void render();

private:
	void drawTable(const std::vector<trackmanagement::Track>& tracks);
	void drawActions(bool favorite);
	void setTableColumnWidth();
	std::string m_selectedTrackName;
	std::string m_trackType;
	std::shared_ptr<TrackManagementClient> m_trackManagementClient;
};
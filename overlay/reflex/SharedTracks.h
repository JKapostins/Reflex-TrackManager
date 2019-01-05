#pragma once
#include <memory>
#include <string>

class TrackManagementClient;

class SharedTracks
{
public:
	SharedTracks(std::shared_ptr<TrackManagementClient> client);
	~SharedTracks();
	void render();

private:
	void drawTable();
	void drawActions();
	std::string m_selectedTrackName;
	std::shared_ptr<TrackManagementClient> m_trackManagementClient;
};
#pragma once

#include <string>

class TrackManagementClient;

class TrackSelection
{
public:
	TrackSelection(std::shared_ptr<TrackManagementClient> client);
	~TrackSelection();
	void render();

private:
	std::string m_selectedTrack;
	std::shared_ptr<TrackManagementClient> m_trackManagementClient;
};
#pragma once
#include <memory>

class TrackManagementClient;

class Log
{
public:
	Log(std::shared_ptr<TrackManagementClient> client);
	~Log();
	void render();

private:
	std::shared_ptr<TrackManagementClient> m_trackManagementClient;
};
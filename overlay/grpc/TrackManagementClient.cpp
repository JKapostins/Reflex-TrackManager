#include "TrackManagementClient.h"

TrackManagementClient::TrackManagementClient(std::shared_ptr<grpc::Channel> channel)
	: m_stub(new trackmanagement::TrackManager::Stub(channel))
{
}
TrackManagementClient::~TrackManagementClient()
{
	//GNARLY_TODO: Send disconnect message to the server.

	//Note: Leaking on purpose here because attempting to delete grpc generated object causes hang.
	//This code only gets called when reflex is closed so the OS will clean it up.
	//delete m_stub;
}

std::vector<trackmanagement::Track> TrackManagementClient::getTracks(const trackmanagement::TrackRequest& request) const
{
	trackmanagement::TrackResponse reply;
	grpc::ClientContext context;
	std::vector<trackmanagement::Track> tracks;

	grpc::Status status = m_stub->GetTracks(&context, request, &reply);

	if (status.ok())
	{
		auto trackResponse = reply.tracks();
		tracks.reserve(trackResponse.size());

		for (auto& track : trackResponse)
		{
			tracks.push_back(track);
		}
	}
	return tracks;
}

std::vector<trackmanagement::LogMessage> TrackManagementClient::getLogMessages() const
{
	trackmanagement::Empty request;
	trackmanagement::LogResponse reply;
	grpc::ClientContext context;
	std::vector<trackmanagement::LogMessage> logMessages;

	grpc::Status status = m_stub->GetLogMessages(&context, request, &reply);
	if (status.ok())
	{
		auto logResponse = reply.messages();
		logMessages.reserve(logResponse.size());

		for (auto& message : logResponse)
		{
			logMessages.push_back(message);
		}
	}

	return logMessages;
}

std::string TrackManagementClient::getInstallStatus() const
{
	
	trackmanagement::Empty request;
	trackmanagement::InstallStatusResponse reply;
	grpc::ClientContext context;
	std::string installStatus = "";

	grpc::Status status = m_stub->GetInstallStatus(&context, request, &reply);
	if (status.ok())
	{
		installStatus = reply.installstatus();
	}

	return installStatus;
}

void TrackManagementClient::installRandomNationals()
{
	trackmanagement::Empty request;
	trackmanagement::Empty reply;
	grpc::ClientContext context;

	m_stub->InstallRandomNationals(&context, request, &reply);
}

void TrackManagementClient::installRandomSupercross()
{
	trackmanagement::Empty request;
	trackmanagement::Empty reply;
	grpc::ClientContext context;

	m_stub->InstallRandomSupercross(&context, request, &reply);
}

void TrackManagementClient::installRandomFreeRides()
{
	trackmanagement::Empty request;
	trackmanagement::Empty reply;
	grpc::ClientContext context;

	m_stub->InstallRandomFreeRides(&context, request, &reply);
}

void TrackManagementClient::installSelectedTrack(const char* trackName)
{
	trackmanagement::InstallTrackRequest request;
	request.set_trackname(trackName);

	trackmanagement::Empty reply;
	grpc::ClientContext context;

	m_stub->InstallSelectedTrack(&context, request, &reply);
}
